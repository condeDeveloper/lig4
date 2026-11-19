// Exemplo de uso, sem compilar nada:
//
//   dotnet fsi exemplos/uso.fsx
//
// Resolve alguns tabuleiros e mostra o que cada primeira coluna vale.
#load "../src/Lig4/Nucleo.fs"

open Conde.Lig4.Nucleo

for l, a in [ (4, 4); (5, 4); (6, 4) ] do
    let m = Motor(tabuleiro l a)
    printfn "%dx%d: quem comeca %A; coluna por coluna: %A" l a (m.Abertura()) (m.PrimeirasJogadas())

// Uma posicao no meio da partida: as colunas jogadas, alternando, a partir do primeiro.
let t = tabuleiro 7 4
let atual, mascara, jogadas = posicao t [ 6; 2; 6; 3; 0; 4 ]
printfn "depois de 6, 2, 6, 3, 0, 4 no 7x4, quem joga: %d" (Motor(t).Valor(atual, mascara, jogadas))
