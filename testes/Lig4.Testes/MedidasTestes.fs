/// Os numeros do README, presos.
module Conde.Lig4.MedidasTestes

open Xunit
open Conde.Lig4.Nucleo
open Conde.Lig4.Medidas

let E = Resultado.Empata
let P = Resultado.Perde

let esperado =
    [ (4, 4), E, [| E; E; E; E |], 8351
      (5, 4), E, [| P; E; E; E; P |], 55659
      (4, 5), E, [| E; E; E; E |], 33342
      (6, 4), P, [| P; P; P; P; P; P |], 741371
      (4, 6), E, [| E; E; E; E |], 333011
      (5, 5), E, [| E; E; E; E; E |], 731792
      (7, 4), E, [| P; P; P; E; P; P; P |], 6603739
      (4, 7), E, [| E; E; E; E |], 903831
      (6, 5), E, [| E; E; E; E; E; E |], 6132928
      (5, 6), E, [| E; E; E; E; E |], 3606277 ]

[<Fact>]
let ``a tabela dos tabuleiros leves`` () =
    for (l, a), r, primeiras, posicoes in esperado do
        let x = linha l a
        Assert.True((x.Resultado = r), sprintf "%dx%d" l a)
        Assert.Equal<Resultado[]>(primeiras, x.Primeiras)
        Assert.Equal(posicoes, x.Posicoes)

[<Fact>]
let ``no 7x4 so a coluna do meio segura o empate`` () =
    let x = linha 7 4
    Assert.Equal(1, x.Primeiras |> Array.filter ((=) Resultado.Empata) |> Array.length)
    Assert.Equal(Resultado.Empata, x.Primeiras.[3])

[<Fact>]
let ``todo tabuleiro de ate 20 casas empata, nos dois motores e na grade`` () =
    let todos = todosPequenos ()
    Assert.Equal(32, todos.Length)
    for l, a, v, s, g in todos do
        Assert.True((v = 0 && s = 0), sprintf "%dx%d" l a)
        match g with
        | Some x -> Assert.Equal(0, x)
        | None -> ()
    Assert.Equal(28, todos |> List.filter (fun (_, _, _, _, g) -> g.IsSome) |> List.length)

[<Fact>]
let ``a poda guarda 24,6 vezes menos posicoes no 5x4`` () =
    let _, s = semPoda 5 4
    Assert.Equal(1368715, s)
    Assert.Equal(55659, (linha 5 4).Posicoes)
    Assert.Equal(87691, snd (semPoda 4 4))
    Assert.Equal((-1, 6987625), semPoda 6 4)
