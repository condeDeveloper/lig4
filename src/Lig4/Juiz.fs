/// Os juizes. Nenhum deles usa o bitboard.
module Conde.Lig4.Juiz

open System.Collections.Generic

/// Grade ingenua: grade.[col, lin], 0 vazio, 1 e 2 os jogadores. A linha 0 e a de baixo.
type Grade = int[,]

/// Alguem tem 4 em linha passando pela casa (col, lin)? Olha as 4 direcoes, casa a casa.
let alinhouEm (g: Grade) col lin =
    let l = Array2D.length1 g
    let a = Array2D.length2 g
    let quem = g.[col, lin]
    let conta dc dl =
        let mutable n = 0
        let mutable c = col + dc
        let mutable r = lin + dl
        while c >= 0 && c < l && r >= 0 && r < a && g.[c, r] = quem do
            n <- n + 1
            c <- c + dc
            r <- r + dl
        n
    quem <> 0
    && [ (1, 0); (0, 1); (1, 1); (1, -1) ]
       |> List.exists (fun (dc, dl) -> 1 + conta dc dl + conta -dc -dl >= 4)

/// Minimax completo sobre a grade, com memoria indexada pela grade lida como um
/// numero na base 3 (cabe em 64 bits ate 39 casas). Nao ordena nada e so corta quando
/// acha uma vitoria: e lento de proposito.
let resolveNaGrade largura altura : int =
    let g: Grade = Array2D.zeroCreate largura altura
    let altas = Array.zeroCreate largura
    if largura * altura > 39 then invalidArg "largura" "a grade cabe em 64 bits so ate 39 casas"
    let memoria = Dictionary<int64, int>()
    let numero () =
        let mutable k = 0L
        for c in 0 .. largura - 1 do
            for r in 0 .. altura - 1 do
                k <- k * 3L + int64 g.[c, r]
        k
    let rec valor quem jogadas =
        if jogadas = largura * altura then 0
        else
            let k = numero ()
            match memoria.TryGetValue k with
            | true, v -> v
            | _ ->
                let mutable melhor = -1
                for col in 0 .. largura - 1 do
                    if melhor < 1 && altas.[col] < altura then
                        let lin = altas.[col]
                        g.[col, lin] <- quem
                        altas.[col] <- lin + 1
                        let v = if alinhouEm g col lin then 1 else -(valor (3 - quem) (jogadas + 1))
                        if v > melhor then melhor <- v
                        altas.[col] <- lin
                        g.[col, lin] <- 0
                memoria.[k] <- melhor
                melhor
    valor 1 0

/// Espelha uma lista de jogadas: a coluna c vira a coluna L - 1 - c.
let espelha largura (colunas: int list) = colunas |> List.map (fun c -> largura - 1 - c)

/// Sorteia `quantas` posicoes por partidas aleatorias (semente fixa) e confere que a
/// posicao e o seu espelho tem o mesmo valor. Devolve (conferidas, discordam).
let confereEspelho largura altura quantas semente =
    let t = Nucleo.tabuleiro largura altura
    let m = Nucleo.Motor t
    let rnd = System.Random(semente: int)
    let mutable conferidas = 0
    let mutable discordam = 0
    let mutable tentativas = 0
    while conferidas < quantas && tentativas < 50 * quantas do
        tentativas <- tentativas + 1
        let n = rnd.Next(0, t.Casas)
        let mutable jogadas = []
        let mutable ok = true
        for _ in 1 .. n do
            if ok then
                let tenta = jogadas @ [ rnd.Next(0, largura) ]
                try
                    Nucleo.posicao t tenta |> ignore
                    jogadas <- tenta
                with _ -> ok <- false
        let a, b, k = Nucleo.posicao t jogadas
        let a', b', k' = Nucleo.posicao t (espelha largura jogadas)
        if m.Valor(a, b, k) <> m.Valor(a', b', k') then discordam <- discordam + 1
        conferidas <- conferidas + 1
    conferidas, discordam

/// O motor de antes da poda: negamax com memoria de valor EXATO, que so corta quando
/// acha uma vitoria. Usa as mesmas regras do bitboard, mas nenhuma das podas do
/// motor: nem alfa-beta, nem limites, nem bloqueio obrigatorio.
type MotorSemPoda(t: Nucleo.Tabuleiro) =
    let memoria = Dictionary<uint64, sbyte>()
    let ordem = Nucleo.ordemDoMeio t.Largura

    member _.Posicoes = memoria.Count

    member this.Valor(atual: uint64, mascara: uint64, jogadas: int) : int =
        let mutable ganhaJa = false
        for col in ordem do
            if not ganhaJa && Nucleo.cabe t mascara col then
                if Nucleo.alinhou t (atual ||| Nucleo.casaQueCai t mascara col) then ganhaJa <- true
        if ganhaJa then 1
        elif jogadas + 1 >= t.Casas then 0
        else
            let chave = atual + mascara
            match memoria.TryGetValue chave with
            | true, v -> int v
            | _ ->
                let mutable melhor = -1
                let mutable i = 0
                while i < ordem.Length && melhor < 1 do
                    let col = ordem.[i]
                    if Nucleo.cabe t mascara col then
                        let nova = mascara ||| (mascara + (1UL <<< (col * (t.Altura + 1))))
                        let v = -(this.Valor(atual ^^^ mascara, nova, jogadas + 1))
                        if v > melhor then melhor <- v
                    i <- i + 1
                memoria.[chave] <- sbyte melhor
                melhor
