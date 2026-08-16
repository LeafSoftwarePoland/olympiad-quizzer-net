# Olympiad Quizzer

Aplikacja do samodzielnych ćwiczeń przed olimpiadami i konkursami informatycznymi dla uczniów.
Zawiera pytania z poprzednich edycji — wybierasz zakres, rozwiązujesz, od razu widzisz wynik
i wyjaśnienie.

Nie zastępuje nauczyciela. Ma pozwolić przećwiczyć zadania samodzielnie, w dowolnym momencie.

## Założenia

- **Działa na telefonie i na komputerze.** Ten sam interfejs, bez osobnej aplikacji mobilnej.
- **Bezpłatna, bez kont i bez logowania.** Nie zakładasz konta, nie podajesz adresu e-mail.
- **Dostępna.** Obsługa czytników ekranu, opisy alternatywne obrazków, sterowanie klawiaturą,
  regulacja wielkości liter.
- **Działa po swojej stronie.** Interfejs uruchamia się w przeglądarce i sam pamięta swój stan.
  Wyczyszczenie danych przeglądarki usuwa aplikację i wszystko, co zapamiętała.

## Prywatność

**Nie używamy ciasteczek. Nie zbieramy danych o użytkowniku. Nie ma statystyk ani śledzenia.**

W pamięci Twojej przeglądarki zapisujemy tylko to, bez czego aplikacja nie działa:

| Co | Po co |
|---|---|
| Ustawienia — motyw, wielkość liter | Żeby nie ustawiać ich przy każdym wejściu |
| Sesja quizu — wylosowane pytania, Twoje odpowiedzi, postęp, czas | Żeby dało się wrócić do przerwanego quizu |

Te dane **nie opuszczają Twojego urządzenia** — nie są nigdzie wysyłane. Wyczyszczenie danych
przeglądarki kasuje je bezpowrotnie.

Serwer z pytaniami zapisuje techniczne logi zapytań (data, adres IP), tak jak każdy serwer
w internecie. Nie ma w nich nic, co pozwoliłoby Cię rozpoznać, i nie są z niczym łączone.

## Pytania i źródła

**Nie roszczę sobie praw do treści pytań.** Pochodzą z materiałów publicznie udostępnianych przez
organizatorów — obecnie z [oij.edu.pl](https://oij.edu.pl). Kolejne źródła będą dochodzić.

Jeśli jesteś autorem lub organizatorem i chcesz, żeby coś zostało poprawione albo usunięte —
załóż zgłoszenie w zakładce Issues.

## Jak powstała

Aplikacja została zbudowana z użyciem Claude, jako ćwiczenie z programowania agentowego i własnego
procesu wytwarzania oprogramowania. Kod, decyzje architektoniczne i standardy są w repozytorium
jawne — łącznie z uzasadnieniami, dlaczego coś zrobiono tak, a nie inaczej.

## Dokumentacja

| Gdzie | Co znajdziesz |
|---|---|
| [docs/README.md](docs/README.md) | Mapa całej dokumentacji — zacznij tutaj |
| [docs/development.md](docs/development.md) | Uruchomienie lokalne, struktura projektu, wdrożenie |
| [docs/adl/](docs/adl/) | Dziennik decyzji architektonicznych wraz z uzasadnieniami |
| [docs/standards/](docs/standards/) | Standardy pisania kodu |
| [docs/rules/](docs/rules/) | Zasady konkursów — format etapów, sposób oceniania |

## Współpraca

Repozytorium nie przyjmuje zmian wypychanych bezpośrednio — pracujemy przez forka.

1. Zrób fork repozytorium.
2. Utwórz gałąź i wprowadź zmiany.
3. Otwórz pull request — szablon podpowie, co opisać.

Błąd lub pomysł zgłaszasz przez **Issues**; są osobne szablony zgłoszenia błędu i propozycji
funkcji.

Przed zmianami w kodzie warto przejrzeć [docs/standards/](docs/standards/). Automat sprawdza tylko
to, czy projekt się kompiluje i czy przechodzą testy — zgodność ze standardami sprawdzam ręcznie
przy przeglądaniu pull requestów, więc trzymanie się ich skraca drogę do scalenia.

---

Tomasz Mankin &copy; 2026
