/// Lig 4 (Connect Four) num tabuleiro de L colunas por A linhas.
///
/// As pedras caem por gravidade: jogar numa coluna poe a pedra na casa livre mais
/// baixa dela. Ganha quem alinhar 4 pedras suas na horizontal, na vertical ou numa
/// diagonal. Tabuleiro cheio sem ninguem alinhar e empate.
///
/// O tabuleiro e um bitboard de 64 bits: cada coluna ocupa A + 1 bits, de baixo para
/// cima, e o bit de cima de cada coluna fica SEMPRE vazio. E esse bit sentinela que
/// impede um alinhamento de "dar a volta" de uma coluna para a outra.
///
///   mascara  = as casas ocupadas
///   atual    = as pedras de quem vai jogar
///
/// Jogar troca os papeis: as pedras do outro sao `atual ^^^ mascara`.
module Conde.Lig4.Nucleo

open System.Collections.Generic

/// O resultado para quem esta para jogar.
type Resultado =
    | Perde = -1
    | Empata = 0
    | Ganha = 1

type Tabuleiro =
    { Largura: int
      Altura: int }

    member t.Casas = t.Largura * t.Altura

let tabuleiro largura altura =
    if largura < 1 || altura < 1 then invalidArg "largura" "o tabuleiro precisa de pelo menos uma casa"
    if largura * (altura + 1) > 63 then
        invalidArg "largura" (sprintf "%dx%d nao cabe em 64 bits com o sentinela" largura altura)
    { Largura = largura; Altura = altura }

let inline private bitDeBaixo (t: Tabuleiro) col = 1UL <<< (col * (t.Altura + 1))
let inline private bitDeCima (t: Tabuleiro) col = 1UL <<< (t.Altura - 1 + col * (t.Altura + 1))
let inline private colunaInteira (t: Tabuleiro) col = ((1UL <<< t.Altura) - 1UL) <<< (col * (t.Altura + 1))

let inline cabe (t: Tabuleiro) (mascara: uint64) col = (mascara &&& bitDeCima t col) = 0UL

/// A casa onde a pedra cai se jogarem na coluna `col`.
let inline casaQueCai (t: Tabuleiro) (mascara: uint64) col =
    (mascara + bitDeBaixo t col) &&& colunaInteira t col

/// Ha 4 em linha nas pedras `p`?
let alinhou (t: Tabuleiro) (p: uint64) =
    let h = t.Altura
    let inline direcao (s: int) =
        let m = p &&& (p >>> s)
        (m &&& (m >>> (2 * s))) <> 0UL
    direcao 1 || direcao (h + 1) || direcao h || direcao (h + 2)

/// As colunas do meio para as bordas: 3, 2, 4, 1, 5, 0, 6 num tabuleiro de 7.
let ordemDoMeio (largura: int) =
    [| 0 .. largura - 1 |] |> Array.sortBy (fun c -> abs (2 * c - (largura - 1)), c)

/// O motor: negamax com poda alfa-beta e memoria de LIMITES.
///
/// Como o valor so pode ser -1, 0 ou 1, a poda corta muito: depois de achar um
/// empate, as outras colunas so precisam responder "ganha?", e nao "quanto?". Por
/// isso a memoria nao guarda um valor exato, guarda um intervalo [piso, teto] que a
/// busca foi estreitando.
///
/// Mais duas regras que nao mudam o resultado, so o caminho:
///   - se o outro ameaca alinhar em duas colunas, eu perco (so bloqueio uma);
///   - se ameaca em uma, eu sou obrigado a bloquear ali.
type Motor(t: Tabuleiro) =
    let memoria = Dictionary<uint64, byte>()
    let ordem = ordemDoMeio t.Largura
    let mutable nos = 0L

    member _.Tabuleiro = t
    member _.Posicoes = memoria.Count
    member _.Nos = nos

    member this.Busca(atual: uint64, mascara: uint64, jogadas: int, alfa0: int, beta0: int) : int =
        nos <- nos + 1L
        let mutable ganhaJa = false
        for col in ordem do
            if not ganhaJa && cabe t mascara col && alinhou t (atual ||| casaQueCai t mascara col) then
                ganhaJa <- true
        if ganhaJa then 1
        elif jogadas + 1 >= t.Casas then 0 // a ultima pedra cabe e nao alinha: empate
        else
            let outro = atual ^^^ mascara
            let mutable ameacas = 0
            let mutable forcada = -1
            for col in ordem do
                if cabe t mascara col && alinhou t (outro ||| casaQueCai t mascara col) then
                    ameacas <- ameacas + 1
                    forcada <- col
            if ameacas >= 2 then -1
            else
                let chave = atual + mascara
                let mutable piso = -1
                let mutable teto = 1
                match memoria.TryGetValue chave with
                | true, b ->
                    piso <- int (b &&& 3uy) - 1
                    teto <- int (b >>> 2) - 1
                | _ -> ()
                if piso >= beta0 || piso = teto then piso
                elif teto <= alfa0 then teto
                else
                    let alfaBusca = max alfa0 piso
                    let betaBusca = min beta0 teto
                    let mutable alfa = alfaBusca
                    let mutable melhor = -2
                    let colunas = if ameacas = 1 then [| forcada |] else ordem
                    let mutable i = 0
                    while i < colunas.Length && alfa < betaBusca do
                        let col = colunas.[i]
                        if cabe t mascara col then
                            let nova = mascara ||| (mascara + bitDeBaixo t col)
                            let v = -(this.Busca(outro, nova, jogadas + 1, -betaBusca, -alfa))
                            if v > melhor then melhor <- v
                            if v > alfa then alfa <- v
                        i <- i + 1
                    if melhor <= alfaBusca then teto <- min teto melhor
                    elif melhor >= betaBusca then piso <- max piso melhor
                    else
                        piso <- melhor
                        teto <- melhor
                    memoria.[chave] <- byte (piso + 1) ||| (byte (teto + 1) <<< 2)
                    melhor

    /// O valor da posicao para quem esta para jogar: 1 ganha, 0 empata, -1 perde.
    member this.Valor(atual: uint64, mascara: uint64, jogadas: int) : int =
        this.Busca(atual, mascara, jogadas, -1, 1)

    /// O tabuleiro vazio.
    member this.Abertura() : Resultado = enum<Resultado> (this.Valor(0UL, 0UL, 0))

    /// O valor de cada primeira jogada, para quem a fez (None se a coluna nao existe).
    member this.PrimeirasJogadas() : Resultado[] =
        [| for col in 0 .. t.Largura - 1 ->
               let mascara = bitDeBaixo t col
               if t.Casas = 1 then Resultado.Empata
               elif alinhou t mascara then Resultado.Ganha
               else enum<Resultado> (-(this.Valor(0UL, mascara, 1))) |]

/// Uma posicao montada a partir de uma lista de colunas jogadas (0 = a da esquerda).
/// Devolve (atual, mascara, jogadas). Falha se a coluna estiver cheia ou se alguem
/// ja tiver alinhado antes do fim da lista.
let posicao (t: Tabuleiro) (colunas: int list) =
    let mutable atual = 0UL
    let mutable mascara = 0UL
    let mutable n = 0
    for col in colunas do
        if col < 0 || col >= t.Largura || not (cabe t mascara col) then
            invalidArg "colunas" (sprintf "a coluna %d nao aceita pedra" col)
        let nova = mascara ||| (mascara + bitDeBaixo t col)
        let minhas = atual ||| (nova ^^^ mascara)
        if alinhou t minhas then invalidArg "colunas" "alguem ja alinhou 4"
        atual <- minhas ^^^ nova
        mascara <- nova
        n <- n + 1
    atual, mascara, n

/// Resolve um tabuleiro vazio: (resultado para quem comeca, posicoes guardadas).
let resolve largura altura =
    let m = Motor(tabuleiro largura altura)
    let r = m.Abertura()
    r, m.Posicoes
