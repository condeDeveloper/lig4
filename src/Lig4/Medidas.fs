/// As tabelas do README. Calculadas aqui, e em lugar nenhum mais.
module Conde.Lig4.Medidas

open System.Collections.Concurrent
open Conde.Lig4.Nucleo

let private guardadas = ConcurrentDictionary<string, obj>()

let private umaVez<'T> (chave: string) (f: unit -> 'T) : 'T =
    guardadas.GetOrAdd(chave, fun _ -> box (f ())) :?> 'T

/// Uma linha da tabela: o resultado de quem comeca, o de cada primeira coluna, e o
/// tamanho da busca.
type Linha =
    { Largura: int
      Altura: int
      Resultado: Resultado
      Primeiras: Resultado[]
      Posicoes: int
      Nos: int64 }

let linha largura altura : Linha =
    umaVez (sprintf "l:%dx%d" largura altura) (fun () ->
        let m = Motor(tabuleiro largura altura)
        let r = m.Abertura()
        let p = m.Posicoes
        let n = m.Nos
        { Largura = largura
          Altura = altura
          Resultado = r
          Primeiras = m.PrimeirasJogadas()
          Posicoes = p
          Nos = n })

/// Os tabuleiros de 4 a 7 colunas e 4 a 6 linhas que terminam em segundos.
let leves = [ (4, 4); (5, 4); (4, 5); (6, 4); (4, 6); (5, 5); (7, 4); (4, 7); (6, 5); (5, 6) ]

/// Quantas posicoes o motor sem poda guarda no mesmo tabuleiro.
let semPoda largura altura =
    umaVez (sprintf "s:%dx%d" largura altura) (fun () ->
        let s = Juiz.MotorSemPoda(tabuleiro largura altura)
        let v = s.Valor(0UL, 0UL, 0)
        v, s.Posicoes)

/// Todos os tabuleiros pequenos: (largura, altura, motor, sem poda, grade ou None).
let todosPequenos () =
    umaVez "pequenos" (fun () ->
        [ for l in 1 .. 7 do
              for a in 1 .. 6 do
                  if l * a <= 20 then
                      let m = Motor(tabuleiro l a)
                      let v = m.Valor(0UL, 0UL, 0)
                      let s, _ = semPoda l a
                      let g = if l * a <= 16 then Some(Juiz.resolveNaGrade l a) else None
                      yield l, a, v, s, g ])
