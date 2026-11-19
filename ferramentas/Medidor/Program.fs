/// O medidor. Roda as tabelas e cospe os numeros que vao para o README.
///
///   dotnet run --project ferramentas/Medidor/Medidor.fsproj -c Release              os tabuleiros leves e os juizes
///   dotnet run --project ferramentas/Medidor/Medidor.fsproj -c Release -- longo     a grade no 6x4 (~2 min) e o 7x5 (~1,5 min e ~2 GB)
open Conde.Lig4.Nucleo
open Conde.Lig4.Medidas

let nome (r: Resultado) =
    match r with
    | Resultado.Ganha -> "ganha"
    | Resultado.Perde -> "perde"
    | _ -> "empata"

let imprime (l: Linha) (segundos: float) =
    printfn "   %dx%d: quem comeca %-7s  primeiras: %-45s  %d posicoes, %d nos  (%.1f s)"
        l.Largura l.Altura (nome l.Resultado)
        (l.Primeiras |> Array.map nome |> String.concat " ") l.Posicoes l.Nos segundos

let leve () =
    printfn "\n== largura x altura: o resultado para quem comeca e cada primeira coluna =="
    for l, a in leves do
        let sw = System.Diagnostics.Stopwatch.StartNew()
        let x = linha l a
        imprime x sw.Elapsed.TotalSeconds
    printfn "\n== a poda: posicoes guardadas com e sem ela =="
    for l, a in [ (4, 4); (5, 4); (6, 4); (5, 5) ] do
        let _, s = semPoda l a
        let c = (linha l a).Posicoes
        printfn "   %dx%d: %d sem poda, %d com poda (%.1f vezes menos)" l a s c (float s / float c)
    printfn "\n== os juizes =="
    let todos = todosPequenos ()
    let comGrade = todos |> List.filter (fun (_, _, _, _, g) -> g.IsSome)
    let discordaSem = todos |> List.filter (fun (_, _, v, s, _) -> v <> s)
    let discordaGrade = comGrade |> List.filter (fun (_, _, v, _, g) -> Some v <> g)
    printfn "   tabuleiros de ate 20 casas: %d; contra o motor sem poda discordam %d" todos.Length discordaSem.Length
    printfn "   contra a grade ingenua (ate 16 casas): %d tabuleiros, discordam %d" comGrade.Length discordaGrade.Length
    let ganhos = todos |> List.filter (fun (_, _, v, _, _) -> v <> 0)
    printfn "   tabuleiros de ate 20 casas que nao empatam: %A" (ganhos |> List.map (fun (l, a, v, _, _) -> l, a, v))
    for l, a in [ (5, 4); (6, 4) ] do
        let n, d = Conde.Lig4.Juiz.confereEspelho l a 300 7
        printfn "   espelho no %dx%d: %d posicoes sorteadas, %d discordam" l a n d

let longo () =
    printfn "\n== os caros =="
    let sg = System.Diagnostics.Stopwatch.StartNew()
    printfn "   a grade ingenua no 6x4: %d  (%.1f s)" (Conde.Lig4.Juiz.resolveNaGrade 6 4) sg.Elapsed.TotalSeconds
    let sw = System.Diagnostics.Stopwatch.StartNew()
    let m = Motor(tabuleiro 7 5)
    let r = m.Abertura()
    printfn "   7x5: quem comeca %s, %d posicoes, %d nos  (%.1f s)" (nome r) m.Posicoes m.Nos sw.Elapsed.TotalSeconds

[<EntryPoint>]
let main argv =
    match argv with
    | [||] -> leve (); 0
    | [| "longo" |] -> longo (); 0
    | _ -> eprintfn "nao conheco %A. conheco: longo" argv; 1
