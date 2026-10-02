# Dyrygent — README dla projektu Fantasy TD

## Co to jest

Dyrygent jest warstwą orkiestracji dla Fantasy TD. Użytkownik opisuje rezultat jednym promptem, a Dyrygent dobiera wykonawców:
- GPT-6 Astra — decyzje wysokiego poziomu, krytyczne reasoning i finalny audyt,
- GPT-6.1 Sol — cięższa analiza techniczna,
- GPT-6 Luna — lekka/przepisowa praca,
- GPT-5.3-Codex — implementacja, debugging i praca kodowa,
- Unity Executor — realna walidacja Unity przez GitHub Actions na persistent Google Cloud VM,
- Higgsfield — generacja grafiki przez hostową wtyczkę ChatGPT.

## Zasada nadrzędna

Nie twórz nowego projektu, jeżeli zadanie dotyczy istniejącego Fantasy TD.

Repo:
`SonGohan231/FantasyTD`

Unity root:
`UnityProject/`

Unity:
`6000.0.26f1`

Najpierw zawsze respektuj:
1. `AGENTS.md`
2. `docs/Repo_Current_State.md`
3. `docs/Tickets.md`
4. aktywny ticket

Obowiązuje one-ticket rule.

## Routing

### Kod / repo
Codex.

### Prawdziwe sprawdzenie Unity
Unity Executor.

Dotyczy m.in.:
- importu projektu,
- compile,
- Console/build logs,
- Build Settings,
- testów Unity,
- Android build,
- faktycznego APK.

Nigdy nie uznawaj "powinno działać" za dowód Unity.

### Grafika
Każda jawna prośba o stworzenie grafiki gry ma obowiązkowy handoff do Higgsfield, niezależnie od trybu ECO/BALANCED/MAX.

Przykłady:
- mapa / mapka,
- wieżyczka / roślina obronna,
- przeciwnik,
- boss,
- bohater,
- sprite,
- ikona,
- UI art,
- tło,
- environment,
- concept art.

Dla zadania mieszanego:
1. Higgsfield tworzy grafikę,
2. Codex integruje plik do repo,
3. Unity Executor weryfikuje import/compile/runtime.

Dyrygent nie może zastąpić Higgsfield opisem promptu, jeśli host ChatGPT ma aktywną wtyczkę Higgsfield.

## Kierunek wizualny Fantasy TD

Nie kopiować konkretnych chronionych assetów z innych gier.

Docelowa energia:
- mobilny, czytelny tower defense,
- kolorowe organiczne fantasy,
- silne i proste sylwetki,
- humorystyczne/cartoonowe postacie,
- rośliny, korzenie, kwiaty, grzyby, ciernie, lód, pyłek, naturalna magia,
- czytelność z daleka na ekranie telefonu.

Punkt odniesienia gatunkowego: energia Kingdom Rush + Plants vs. Zombies, ale własne IP, własne postacie, własne assety i własny język wizualny.

## Workflow przy tworzeniu funkcji

Typowy prompt:
`@Dyrygent dodaj [funkcja] do FantasyTD zgodnie z aktywnym ticketem. Użyj Codex do implementacji, a po zmianach sprawdź wynik przez Unity Executor. Jeśli potrzebna jest nowa grafika, użyj Higgsfield.`

Oczekiwany przebieg:
```text
użytkownik
  ↓
Dyrygent
  ↓
GPT-6 Astra (jeśli potrzebna)
  ↓
Codex / Sol / Luna
  ↓
Higgsfield (jeśli potrzebna grafika)
  ↓
GitHub
  ↓
Unity Executor
  ↓
evidence
  ↓
Dyrygent final
```

## Tryby

### ECO
Do prostych pytań i małych zadań. Minimalizuje tokeny.

### BALANCED
Domyślny do tworzenia gry. Astra może zaplanować, Codex implementuje, Unity Executor weryfikuje.

### MAX
Do trudnych awarii, dużych zmian architektury i problemów obejmujących wiele systemów.

## Evidence policy

Nie wolno twierdzić, że:
- Unity się skompilowało,
- scena działa,
- Play Mode działa,
- test przeszedł,
- APK powstało,
- APK uruchomiło się,

jeśli nie ma realnego wyniku Unity Executor / build / test / urządzenie.

## Aktualny milestone

MAP-A Android v2 vertical slice.

Aktualny ticket:
`T0001 — Unity Project Baseline`

Nie przechodź do T0002, dopóki T0001 nie zostanie zakończony albo użytkownik jawnie nie zmieni zakresu.

## Gotowy prompt startowy

```text
@Dyrygent
Kontynuujemy SonGohan231/FantasyTD.

Przeczytaj AGENTS.md, docs/Repo_Current_State.md i docs/Tickets.md.
Wykonuj tylko ACTIVE ticket.
BALANCED.

Używaj:
- Codex do implementacji,
- Unity Executor do prawdziwej walidacji Unity na self-hosted runnerze `dyrygent-unity`,
- Higgsfield automatycznie dla każdej potrzebnej grafiki,
- Astry tylko tam, gdzie jej jakość wnosi realną wartość.

Nie deklaruj PASS bez dowodu z Unity Executor.
Na końcu podaj status ticketu, zmienione pliki, realnie wykonane testy i następny najmniejszy krok.
```


## Środowisko wykonawcze Unity

Unity Executor korzysta z persistent Google Compute Engine VM `dyrygent-unity` z Ubuntu 22.04, Unity 6000.0.26f1 i Android toolchain. Unity Personal jest aktywowane w Unity Hub na tej maszynie; nie przechowujemy danych logowania Unity w GitHub Secrets.
