module Conde.Lig4.NucleoTestes

open System
open Xunit
open Conde.Lig4.Nucleo

let t74 = tabuleiro 7 4

[<Fact>]
let ``quatro na horizontal, na vertical e nas duas diagonais alinham`` () =
    // Pedras do mesmo jogador, posicionadas a mao: bit = col * (A + 1) + lin.
    let bit c l = 1UL <<< (c * 5 + l)
    Assert.True(alinhou t74 (bit 0 0 ||| bit 1 0 ||| bit 2 0 ||| bit 3 0))
    Assert.True(alinhou t74 (bit 6 0 ||| bit 6 1 ||| bit 6 2 ||| bit 6 3))
    Assert.True(alinhou t74 (bit 1 0 ||| bit 2 1 ||| bit 3 2 ||| bit 4 3))
    Assert.True(alinhou t74 (bit 4 0 ||| bit 3 1 ||| bit 2 2 ||| bit 1 3))
    Assert.False(alinhou t74 (bit 0 0 ||| bit 1 0 ||| bit 2 0 ||| bit 4 0))

[<Fact>]
let ``o bit sentinela impede a vertical de dar a volta para a coluna seguinte`` () =
    // Topo da coluna 0 (linhas 2 e 3) e base da coluna 1 (linhas 0 e 1): nao e uma linha.
    let bit c l = 1UL <<< (c * 5 + l)
    Assert.False(alinhou t74 (bit 0 2 ||| bit 0 3 ||| bit 1 0 ||| bit 1 1))

[<Fact>]
let ``a pedra cai na casa livre mais baixa`` () =
    let _, mascara, _ = posicao t74 [ 3; 3; 3 ]
    Assert.Equal(1UL <<< (3 * 5 + 3), casaQueCai t74 mascara 3)
    Assert.Equal(1UL <<< (2 * 5), casaQueCai t74 mascara 2)

[<Fact>]
let ``coluna cheia e jogada depois de alinhar sao recusadas`` () =
    Assert.Throws<ArgumentException>(fun () -> posicao t74 [ 0; 0; 0; 0; 0 ] |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () -> posicao t74 [ 0; 1; 0; 1; 0; 1; 0; 2 ] |> ignore) |> ignore

[<Fact>]
let ``as colunas vao do meio para as bordas`` () =
    Assert.Equal<int[]>([| 3; 2; 4; 1; 5; 0; 6 |], ordemDoMeio 7)
    Assert.Equal<int[]>([| 2; 3; 1; 4; 0; 5 |], ordemDoMeio 6)

[<Fact>]
let ``o tabuleiro tem de caber em 64 bits com o sentinela`` () =
    Assert.Throws<ArgumentException>(fun () -> tabuleiro 8 7 |> ignore) |> ignore
    Assert.Equal(42, (tabuleiro 7 6).Casas)

[<Fact>]
let ``quem tem tres em pe e a vez ganha; quem nao tem como bloquear duas ameacas perde`` () =
    let m = Motor t74
    // O primeiro jogador tem 3 em pe na coluna 0 e e a vez dele.
    let a, b, k = posicao t74 [ 0; 6; 0; 6; 0; 5 ]
    Assert.Equal(1, m.Valor(a, b, k))
    // O segundo tem duas ameacas na linha de baixo (colunas 1 e 5), e e a vez do primeiro.
    let a, b, k = posicao t74 [ 6; 2; 6; 3; 0; 4 ]
    Assert.Equal(-1, m.Valor(a, b, k))
