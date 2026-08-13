# Tag Taxonomy

Standardized tags for the question bank. All tags are strings. Multi-value where noted.

## Identifier convention

Tags use Polish words, Latin characters only. Drop diacritics: ą→a, ę→e, ó→o, ś→s, ł→l, ź/ż→z, ć→c, ń→n.
Example: `złożoność` → `zlozonosc`, `śledzenie` → `sledzenie`.

This is an explicit exception to ADR-019 (which requires English for machine identifiers). Tag vocabulary is human-facing Polish domain concepts — English equivalents would be less recognizable to Polish educators. Documented in ADR-019 amendment.

---

## `category[]` — mandatory, multi-value

Every question must have at least one category. Vocabulary is standardized — do not invent new values without updating this file.

| tag | definition |
|-----|-----------|
| `programowanie_python` | Python language mechanics: syntax, operators, data types, control flow, I/O, chr/ord, list operations |
| `sledzenie_kodu` | Trace code to predict output, count iterations, fill a missing line, identify equivalent code |
| `rekurencja` | Recursive function tracing: factorial, Fibonacci, GCD, digit-sum, binary search, tree traversal |
| `sortowanie` | Sorting algorithms: pass counting, comparisons, time complexity, LIS |
| `zlozonosc` | Big-O / Θ notation, memory estimation, time analysis of loops and algorithms |
| `struktury_danych` | List vs set, array memory layout, stack, queue, dictionary — properties and selection |
| `teoria_liczb` | GCD/LCM, primes, divisors, prime factorization, modular arithmetic, digit sums |
| `matematyka_dyskretna` | Counting/combinatorics: sequences, subsets, paths, Pascal's triangle, Catalan numbers |
| `systemy_liczbowe` | Binary, hex, octal, arbitrary base conversion, digit extraction via modulo/div |
| `napisy_tekst` | Strings: indexing, slicing, palindromes, Caesar cipher, prefix-free codes, RLE, compression |
| `operacje_bitowe` | XOR, AND, OR, NOT, bit counting, bit manipulation |
| `grafy_drzewa` | BFS pathfinding, grid path counting, binary tree traversal and path sums |
| `programowanie_dynamiczne` | DP: Kadane's, subset sum, grid path DP, optimal substructure recognition |
| `algorytmy_zachlanne` | Greedy: coin change, Frobenius/stamp problem, greedy vs optimal analysis |
| `logika_automaty` | Boolean logic, logical equivalences, finite automata / state machines |
| `schematy_blokowe` | Reading and tracing algorithm flowcharts |

### VEA categories — identified, future scope

Not needed until VEA questions are imported. Do not use on OIJ questions.

`sprzet_komputerowy`, `sieci_internet`, `bezpieczenstwo_cyfrowe`, `licencje_prawo`,
`arkusz_kalkulacyjny`, `edytor_tekstu_html`, `grafika_komputerowa`, `sztuczna_inteligencja`,
`druk_3d`, `programowanie_scratch`, `historia_informatyki`, `multimedia_urzadzenia`,
`programowanie_cpp`, `kultura_cyfrowa_etyka`

---

## `algorithms[]` — optional, multi-value

Add only when a specific named algorithm is the subject of the question, not merely used incidentally.
If `algorithms[]` is set, the matching `category[]` must also be set.

| tag | algorithm |
|-----|-----------|
| `sortowanie_babelkowe` | Bubble sort |
| `sortowanie_przez_wybieranie` | Selection sort |
| `sortowanie_przez_zliczanie` | Counting sort |
| `algorytm_euklidesa` | Euclid's GCD — subtraction variant |
| `algorytm_euklidesa_reszta` | Euclid's GCD — remainder variant (while b!=0: r=a%b...) |
| `algorytm_kadane` | Kadane's maximum subarray |
| `szyfr_cezara` | Caesar cipher |
| `kodowanie_prefiksowe` | Prefix-free / Huffman codes |
| `kompresja_rle` | Run-length encoding |
| `przeszukiwanie_bfs` | BFS grid pathfinding |
| `wyszukiwanie_liniowe` | Linear search |
| `mnozenie_rosyjskie` | Russian peasant / binary multiplication |
| `problem_frobeniusa` | Frobenius / coin-denominations problem |
| `rozklad_na_czynniki_pierwsze` | Prime factorization |
| `wieze_hanoi` | Tower of Hanoi |
| `liczby_catalana` | Catalan numbers |
| `trojkat_pascala` | Pascal's triangle / binomial coefficients |
| `automat_skonczony` | Finite automaton |

---

## `source` — recommended, nullable

Format: `{OLYMPIAD}-{YEAR}-{STAGE}[-{PART}]`

| segment | values | notes |
|---------|--------|-------|
| `OLYMPIAD` | `OIJ` | Expands when more olympiads are added |
| `YEAR` | `2019`…`2025` | Calendar year |
| `STAGE` | `E1` `E2` `E3` | Etap I (Szkolny), Etap II (Okręgowy), Etap III (Ogólnopolski) |
| `PART` | `p1` `p2` `mock` | Optional — use when one stage produced multiple files |

Examples: `OIJ-2024-E1`, `OIJ-2019-E1-p1`, `OIJ-2023-E1-mock`

Keep the raw PDF filename in a separate `source_raw` field for traceability.

---

## `year` — recommended, nullable

Integer. Derives from `source`. `null` if unknown.

Current corpus: `2019`, `2020`, `2021`, `2023`, `2024`, `2025`. No 2022 data.

---

## `difficulty` — optional, 1–5

One question may have multiple difficulty values if different approaches yield different cognitive loads.

| value | label | typical content |
|-------|-------|----------------|
| 1 | Podstawy | Pure syntax recognition, no computation |
| 2 | Łatwe | Simple loop tracing, basic arithmetic output |
| 3 | Średnie | Recursion tracing, base conversion, modular arithmetic |
| 4 | Trudne | Algorithm analysis, non-trivial combinatorics, graph paths |
| 5 | Olimpijskie | Named hard algorithms, complex DP, Catalan/Frobenius, finite automata |
