module Conde.Lig4.JuizTestes

open Xunit
open Conde.Lig4

[<Fact>]
let ``a grade ingenua concorda com o motor em todo tabuleiro de ate 16 casas`` () =
    for l in 1 .. 7 do
        for a in 1 .. 6 do
            if l * a <= 16 then
                let m = Nucleo.Motor(Nucleo.tabuleiro l a)
                Assert.True((m.Valor(0UL, 0UL, 0) = Juiz.resolveNaGrade l a), sprintf "%dx%d" l a)

[<Fact>]
let ``o motor sem poda concorda com o motor em todo tabuleiro de ate 20 casas`` () =
    for l in 1 .. 7 do
        for a in 1 .. 6 do
            if l * a <= 20 then
                let t = Nucleo.tabuleiro l a
                let m = Nucleo.Motor t
                let s = Juiz.MotorSemPoda t
                Assert.True((m.Valor(0UL, 0UL, 0) = s.Valor(0UL, 0UL, 0)), sprintf "%dx%d" l a)

[<Fact>]
let ``a grade enxerga as quatro direcoes`` () =
    let g: Juiz.Grade = Array2D.zeroCreate 5 4
    for i in 0 .. 3 do g.[i + 1, i] <- 1
    Assert.True(Juiz.alinhouEm g 3 2)
    g.[4, 3] <- 2
    Assert.False(Juiz.alinhouEm g 4 3)

[<Fact>]
let ``espelhar troca a coluna c pela L - 1 - c`` () =
    Assert.Equal<int list>([ 6; 3; 0 ], Juiz.espelha 7 [ 0; 3; 6 ])

[<Fact>]
let ``uma posicao e o seu espelho valem o mesmo`` () =
    Assert.Equal((300, 0), Juiz.confereEspelho 5 4 300 7)
    Assert.Equal((300, 0), Juiz.confereEspelho 6 4 300 7)
