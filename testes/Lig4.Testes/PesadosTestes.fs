/// Os caros: a grade ingenua no 6x4 (~2 min) e o 7x5 (~1,5 min, ~2 GB).
/// O CI roda num passo separado (filtro `FullyQualifiedName~Pesados`).
module Conde.Lig4.PesadosTestes

open Xunit
open Conde.Lig4

[<Fact>]
let ``a grade ingenua tambem diz que no 6x4 quem comeca perde`` () =
    Assert.Equal(-1, Juiz.resolveNaGrade 6 4)

[<Fact>]
let ``o 7x5 empata, com 68.530.265 posicoes guardadas`` () =
    let m = Nucleo.Motor(Nucleo.tabuleiro 7 5)
    Assert.Equal(Nucleo.Resultado.Empata, m.Abertura())
    Assert.Equal(68530265, m.Posicoes)
