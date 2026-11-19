# Lig 4

Lig 4 (Connect Four) resolvido em tabuleiros pequenos, em **F#**, sem dependência nenhuma além do xUnit nos testes. Para cada tabuleiro de L colunas por A linhas, o motor diz se quem começa ganha, empata ou perde com os dois jogando o melhor possível, e o que vale cada primeira coluna.

As pedras caem por gravidade. Ganha quem alinhar 4 pedras suas na horizontal, na vertical ou numa diagonal. Tabuleiro cheio sem ninguém alinhar é empate.

---

## Por que F#

O tabuleiro inteiro é um `uint64`: cada coluna ocupa A + 1 bits, e o bit de cima de cada coluna fica sempre vazio, de sentinela, para que um alinhamento não "dê a volta" de uma coluna para a outra. Achar 4 em linha numa direção é `m = p &&& (p >>> s)` e depois `m &&& (m >>> 2s)`, e jogar numa coluna é uma soma: `mascara + bitDeBaixo`. F# tem os operadores de bit nativos e o `uint64` sem sinal, e o resultado é um tipo enumerado (`Ganha`, `Empata`, `Perde`) em vez de um número solto.

---

## Os quatro achados

**1. De todos os tabuleiros medidos, só o 6x4 não empata — e nele quem começa perde, jogue onde jogar.** São 32 tabuleiros de até 20 casas, mais 4x5, 6x4, 4x6, 5x5, 7x4, 4x7, 6x5, 5x6 e 7x5. Todos empatam, menos o 6x4, em que as seis primeiras colunas perdem.

**2. No 7x4, só a coluna do meio segura o empate.** As outras seis primeiras jogadas perdem.

**3. No 5x4, os dois cantos perdem.** As três colunas do meio empatam.

**4. A poda corta 24,6 vezes as posições do 5x4.** O motor guarda 55.659 posições no 5x4; o mesmo motor sem a poda alfa-beta guarda 1.368.715. No 5x5 a razão é 22,1, e no 6x4, o tabuleiro que não empata, só 9,4. O 7x5, o maior medido, precisa de 68.530.265 posições.

---

## A tabela

O resultado para quem começa e o de cada primeira coluna, da esquerda para a direita (E empata, P perde).

| tabuleiro | quem começa | primeiras colunas | posições guardadas |
|---|---|---|---:|
| 4x4 | empata | E E E E | 8.351 |
| 5x4 | empata | P E E E P | 55.659 |
| 4x5 | empata | E E E E | 33.342 |
| 6x4 | **perde** | P P P P P P | 741.371 |
| 4x6 | empata | E E E E | 333.011 |
| 5x5 | empata | E E E E E | 731.792 |
| 7x4 | empata | P P P E P P P | 6.603.739 |
| 4x7 | empata | E E E E | 903.831 |
| 6x5 | empata | E E E E E E | 6.132.928 |
| 5x6 | empata | E E E E E | 3.606.277 |
| 7x5 | empata | | 68.530.265 |

Os 32 tabuleiros de até 20 casas empatam todos; os menores que 4x4 nem aparecem aqui, porque neles alinhar já é difícil ou impossível. O 7x5 leva cerca de um minuto e meio e uns 2 GB; o 6x6 e o 7x6, o tabuleiro oficial, não foram medidos.

Cada número está preso num teste: a tabela leve em `MedidasTestes.fs`, o 7x5 em `PesadosTestes.fs`.

---

## Os juízes

**A grade ingênua.** Um Lig 4 escrito sem bitboard: uma matriz `int[,]`, a vitória conferida casa a casa nas quatro direções a partir da pedra que caiu, minimax que não ordena colunas e só corta quando acha uma vitória, memória pela grade lida como número na base 3. Concorda com o motor nos **28 tabuleiros de até 16 casas**, e no 6x4 também diz que quem começa perde (em cerca de dois minutos).

**O motor sem poda.** O motor de antes da poda alfa-beta, com memória de valor exato e sem bloqueio obrigatório. Concorda com o motor nos **32 tabuleiros de até 20 casas** e foi o primeiro a dizer que o 6x4 é perdido.

**O espelho.** Uma posição e o seu reflexo (a coluna c vira a L − 1 − c) têm de valer o mesmo. Em 300 posições sorteadas no 5x4 e 300 no 6x4, **0 discordâncias**.

---

## O que as medidas me corrigiram

**O primeiro motor não passou do 7x4.** Ele guardava o valor exato de cada posição e só cortava ao achar uma vitória. Resolveu o 5x5 em 18 s, mas ficou minutos no 7x4 sem terminar, e eu parei. A troca foi a poda alfa-beta com memória de intervalo (piso e teto) em vez de valor, mais duas regras que não mudam o resultado: quem enfrenta duas ameaças perde, e quem enfrenta uma é obrigado a bloquear. Com isso o 7x4 sai em 6 s. O motor antigo não foi jogado fora: virou um dos juízes.

---

## Como usar

.NET 8 ou mais novo.

```bash
dotnet test testes/Lig4.Testes/Lig4.Testes.fsproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.fsproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.fsproj -c Release -- longo
dotnet fsi exemplos/uso.fsx
```

```fsharp
open Conde.Lig4.Nucleo

let m = Motor(tabuleiro 6 4)
m.Abertura()           // Perde
m.PrimeirasJogadas()   // [| Perde; Perde; Perde; Perde; Perde; Perde |]
```

## Os arquivos

- `src/Lig4/Nucleo.fs` — o bitboard, o alinhamento e o motor com poda.
- `src/Lig4/Juiz.fs` — a grade ingênua, o motor sem poda e o espelho.
- `src/Lig4/Medidas.fs` — as tabelas do README, cada uma calculada uma vez só.
- `ferramentas/Medidor/Program.fs` — imprime as tabelas.
- `exemplos/uso.fsx` — o pacote em uso, sem compilar.
- `testes/` — 18 testes, com os números do README presos.

## Licença

MIT.
