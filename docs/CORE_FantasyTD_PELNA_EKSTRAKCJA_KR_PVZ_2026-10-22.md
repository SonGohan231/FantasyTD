# FantasyTD — ekstrakcja projektowa z Kingdom Rush Genesis i Plants vs. Zombies

**Data:** 22.10.2026  
**Źródła:** [udostępniona konwersacja użytkownika](https://chatgpt.com/share/6ad9ad3d-c254-83eb-8c7b-6af5620aea05?ogimg=plain), odczytane na telefonie przez TelefonMCP2 archiwa zainstalowanych APK Kingdom Rush Genesis (`com.ironhidegames.android.kingdomrush6.genesis`) i PvZ FREE (`com.ea.game.pvzfree_row`), plik OBB zagnieżdżony w PvZ APK, pliki konfiguracyjne i nazwy symboli znalezione w skompilowanych modułach LuaJIT.

## 0. Jak czytać ten raport

**[P] Potwierdzone w artefakcie** — bezpośrednio odczytana struktura plików, konkretna wartość z tabeli lub tekstowy symbol widoczny w bytecode.  
**[W] Wniosek projektowy** — mechanika wywiedziona z połączenia nazwy/konfiguracji i wiedzy o rozgrywce; nie jest dowodem na szczegóły algorytmu.  
**[R] Rekomendacja dla FantasyTD** — zaprojektowany przez nas system, którego nie przypisujemy bez dowodów implementacji pierwowzoru.  
**[N] Niepotwierdzone** — wymagające dalszej dekompilacji, analizy natywnej lub testu gry.

Raport **nie** zawiera skopiowanego kodu, grafik ani dźwięków gier; ma służyć opracowaniu własnych mechanik. Dokument nie twierdzi, że wykonano pełną dekompilację LuaJIT lub `libpvz.so`.

## 1. Warunki ukończenia projektu — na podstawie udostępnionej konwersacji

1. **Forest01 ma być gotową pionową próbką produktu**, a nie tylko demonstracją: czytelna mapa, dobre animacje, rozsądny balans i wyraźny feedback. Żadnej nowej mapy, zanim Forest01 przejdzie kryteria akceptacyjne.
2. **Naprawa podstawowych błędów przed rozbudową**: wybieranie już postawionej wieży, zakup ulepszenia L2/L3, aktualizacja obrażeń i grafiki, poprawny koszt i waluta, brak awarii przy tapnięciach.
3. **Android-first**: UI dotykowe, różne proporcje ekranów, stabilne FPS, test prawdziwego APK, nie tylko edytora.
4. **Powtarzalna metoda pracy**: test → poprawka → test regresyjny → review → build → instalacja i test → dopiero następna funkcja.
5. **Koszty kontrolowane**: użytkownik nie chce zależności od drogich usług cloud. Środowisko w konwersacji to Chromebook/Linux ARM64; Unity 6 uruchamiany jest poprzez FEX, lecz to nie oznacza automatycznie zweryfikowanego pipeline'u Android Build Support.
6. **Nie kopiować cudzego IP**: własna identyfikacja wizualna, własne jednostki, efekty i balans, mechaniki projektowane na podstawie obserwacji wzorców.

## 2. Weryfikacja rzeczywistych zasobów źródłowych

### Kingdom Rush Genesis [P]

- Główny APK: **754 735 596 bajtów**, **5491** pozycji w ZIP.
- W całym archiwum wcześniej naliczono **1896 plików `.lua`**; w poddrzewie `assets/gc32/` wyliczono **868** (liczniki odnoszą się do innego zakresu, nie są sprzeczne).
- W poddrzewie `assets/gc32/kr6/data/waves/` stwierdzono **128** pozycji podczas węższego testu, a pełne przeszukanie ścieżek zawierających `/data/waves/` znalazło **218 tabel `.tsv`**: `blitz` 36, `campaign` 38, `extra_heroes` 36, `iron` 36, `kr1` 36, `no_hero` 36. **To są pliki, nie liczby unikalnych poziomów.**
- Zasoby obejmują `assets/gc32/all/wave_db.lua`, `grid_a_star.lua`, `path_db.lua`, `difficulty.lua`, `templates.lua`; `assets/gc32/kr6/data/balance/balance.lua`, pliki wież, bohaterów, przeciwników oraz setki modułów UI `kui_templates`.
- Pliki `.lua` rozpoczynają się bajtami `1b 4c 4a`, wskazującymi **skompilowany bytecode LuaJIT**. Nazwy modułów i tekstowe symbole są dostępne, lecz bez pełnej rekonstrukcji instrukcji nie przypisujemy im konkretnej implementacji.
- W bytecode modułu balansu wykryto m.in. identyfikatory `damage_min`, `damage_max`, `damage_factor`, `cooldown`, `armor`, `magic_armor`, `rally_range_factor`, `miniboss_damage_cap`, `boss_damage_cap`, `stun`, `speed`, `health_factor` — dowód na obecność odpowiednich pól/nazw, **nie na ich rzeczywiste wartości lub algorytmy**.
- **Poziom 01, kampania, odczyt z TSV**: `lives=20`, `gold=175`, `gems=150`, `default_increment=1`, `default_interval=20`. Fala 1 wskazuje 8 jednostek `bandit`, a konfiguracja zawiera jawne odstępy np. `2`, `0.8`, `0.4`, `12`. Nie interpretujemy tych liczb jako uniwersalnych wartości dla FantasyTD.
- W tej samej tabeli skrypt tutorialu korzysta z komend `manual_wave`, `signal`, `wait`; nazwy sygnałów odnoszą się do blokowania wejścia, sterowania kamerą, wskazówek i startu spawnów.
- Źródła: `assets/gc32/kr6/data/waves/level01_waves_campaign.tsv`, `assets/gc32/kr6/data/waves/level08_waves_iron.tsv`, `assets/gc32/kr6/data/balance/balance.lua`, `assets/gc32/all/wave_db.lua`.

### Plants vs. Zombies FREE [P]

- Główny APK: **136 518 835 bajtów**; pakiet split ARM64: **15 226 006 bajtów**.
- Główny APK zawiera `assets/main.0.com.ea.game.pvzfree_row.obb` o wielkości **45 895 764 bajtów**. Plik OBB jest ZIP z **465** wpisami: m.in. animacje, efekty cząsteczkowe, muzyka i `main.rsb`.
- Około **240** plików `reanim/` oraz **208** `particles/` (liczone według ścieżek z obu wariantów compiled32/compiled64; **nie oznacza 240 niezależnych animacji**).
- Rozpoznawalne zasoby: `mindcontrol`, `buttersplat`, `icetrap`, `potatomine`, `mower`, `sunflower`, `twinsunflower`, `starfruit`, `garlic`, `fume`, `gloom`, `portal`, `zombie_balloon`, `zombie_ladder`, `zombie_pogo`, `zombie_catapult`, `bossiceballtrail`. Pliki potwierdzają obecność **zasobów** określonych mechanik, ale bez dekompilacji `libpvz.so` nie potwierdzają ich reguł.
- `split_config.arm64_v8a.apk` zawiera natywną bibliotekę **`lib/arm64-v8a/libpvz.so` o wielkości 50 117 680 bajtów**.
- `assets/ConfigFile.json` ma klucze m.in. `Global_property`, `Mini_Game`, `Bonus_Game`, `StoreMetadata`, `KillSwitches`, `RuntimeNotifications`. W `Global_property[0]` znaleziono wartości sklepu: Gatling Pea 500, Twin Sunflower 500, Gloom-shroom 750, Cattail 1000, Winter Melon 1000, Spikerock 750, Cob Cannon 2000, Imitater 3000, dodatkowy slot pakietu 75. **To ceny z konkretnej konfiguracji, a nie obowiązkowe parametry balansu FantasyTD**.
- OBB zawiera pliki `music/day.sps`, `night.sps`, `fog.sps`, `pool.sps`, `roof.sps`; potwierdzają rozdzielenie warstw klimatu audio.

## 3. Najbardziej przydatne mechaniki — katalog implementacyjny

**Priorytety:** P0 = konieczne dla Forest01; P1 = wymagane przed uznaniem pionowego wycinka za profesjonalny; P2 = następna fala funkcji; P3 = opcjonalne.

### A. Silnik i projektowanie rozgrywki

| ID | Mechanika | Źródło i status | Jak zbudować w FantasyTD | Priorytet |
|---|---|---|---|---|
| TD-01 | Dane fal poza kodem | KR TSV [P] | `WaveDefinition` jako ScriptableObject/JSON, z eventami spawnu i ścieżką | P0 |
| TD-02 | Wielotorowe fale | kolumny ścieżek KR TSV [P] | Każdy spawn ma `laneId`/`pathId`, start i opóźnienie | P0 |
| TD-03 | Rozdzielenie sekwencji fal od spawnera | `wave_db` + TSV [P/W] | `WaveDirector` czyta plan, `SpawnService` tylko tworzy jednostki | P0 |
| TD-04 | Konfigurowalny balans | KR `balance.lua` [P] | Jeden katalog statystyk `TowerStats/EnemyStats`; brak liczb hardcode w `MonoBehaviour` | P0 |
| TD-05 | System bazowych statystyk i modyfikatorów | symbole KR [P/W] | Atak, typ obrażeń, pancerz, rezysty, prędkość, cooldown, zasięg, mnożniki | P0 |
| TD-06 | Rozdział tras i zachowania przeciwników | `path_db`, `grid_a_star` [P/W] | W Forest01 stałe splajny; A* tylko gdy pojawią się dynamiczne przeszkody | P0 |
| TD-07 | Obsługa wielu trybów tej samej mapy | nazwy `campaign/iron/blitz/no_hero` [P] | Jedna mapa + `GameModeModifier`: modyfikacje fal, budżetu, reguł | P2 |
| TD-08 | Komendy wydarzeń niezależne od samych przeciwników | `signal`, `wait`, `manual_wave` KR [P] | Typowane `WaveEvent` (`Spawn`, `Wait`, `ShowHint`, `ActivateRoute`, `PlayCue`) | P1 |
| TD-09 | Skalowanie endless | `wave_db` — nazwy `endless`, `health`, `gold` [P/W] | Po ukończeniu kampanii: skalowanie ograniczone formułami i limitami | P3 |
| TD-10 | Wymiar strategiczny trudności | `difficulty.lua`, różne TSV [P] | Easy/Normal/Hard wpływa na falę i ekonomię, nie tylko HP | P2 |

### B. Wieże, ulepszenia i interakcja dotykowa

| ID | Mechanika | Źródło i status | Projekt FantasyTD | Priorytet |
|---|---|---|---|---|
| TD-11 | Kupowanie i stawianie wież | KR `tower_build` [P/W] | Bezpieczna maszyna stanów: `Empty → Preview → Placed → Upgrading` | P0 |
| TD-12 | Wybór **już postawionej** wieży | potrzeba użytkownika [konwersacja] | Tap raycast po colliderze wieży, panel aktualnego stanu | P0 |
| TD-13 | Ulepszenia L1–L3 | potrzeba użytkownika + KR `button_tower_upgrade` [P/W] | Atomowy zakup: sprawdź środki, pobierz walutę, zmień level, przelicz statystyki/wygląd | P0 |
| TD-14 | Zasięg wieży | KR `range_circle`, `range_base` [P/W] | Koło zasięgu przy zaznaczeniu i podglądzie przed budową | P0 |
| TD-15 | Rozgałęzienia ewolucji | KR UI level tree [P/W] | Po L3 wybór 1 z 2 specjalizacji; wyraźne trade-offy, nie monotoniczny bonus | P2 |
| TD-16 | Sprzedaż/przebudowa | KR `tower_sell_dust` [P/W] | Zwrot części ceny, aktualizacja ekonomii i slotu | P1 |
| TD-17 | Priorytety celowania | gatunki jednostek [W/R] | First, Last, Strong, Air; zachowanie deterministyczne i testowalne | P1 |
| TD-18 | Wieże przechwytujące/walczące wręcz | KR `barrack`, `rally` [P/W] | Blokowanie i rally point jako osobny typ jednostek | P2 |
| TD-19 | Mocne różnice archetypów | KR wieże + PvZ zasoby [W/R] | Snajper, splash, DoT, slow, generator zasobów, wsparcie | P1 |
| TD-20 | Rozpoznawalna ewolucja wizualna | wymóg profesjonalnej gry [konwersacja] | Każdy poziom ma inny sprite, animację, feedback i siluetkę | P0 |

### C. Ekonomia i decyzje strategiczne

| ID | Mechanika | Źródło i status | Projekt FantasyTD | Priorytet |
|---|---|---|---|---|
| TD-21 | Budżet startowy i nagrody fal | KR TSV [P] | Przydział złota, nagroda za zabicie i ewentualny bonus za wcześniejsze wezwanie fali | P0 |
| TD-22 | Oddzielne koszty jednostek i meta-progresji | PvZ `ConfigFile.json` [P] | Waluta bojowa resetowana po poziomie; inna waluta/odblokowanie poza walką, bez IAP | P1 |
| TD-23 | Generatory zasobów | PvZ zasób `sunflower` [P/W] | Własna wieża generująca zasób; wydajność ograniczona zajętym slotem | P2 |
| TD-24 | System kart/slotów ekwipunku | PvZ `seedpacket`/`STORE_ITEM_PACKET_UPGRADE` [P/W] | Loadout 4–6 wież wybierany przed misją; na Forest01 opcjonalnie stały | P2 |
| TD-25 | Wyraźny koszt alternatywny | połączenie PvZ/KR [R] | Każda wieża ma przynajmniej jedną strategiczną słabość | P0 |
| TD-26 | Ochrona przed błędną transakcją | wymóg jakości [konwersacja] | Nie można mieć ujemnego złota, podwójnego zakupu ani wielokrotnego zwrotu | P0 |

### D. Przeciwnicy i kontrmechaniki

| ID | Mechanika | Dowód i poziom | Projekt FantasyTD | Priorytet |
|---|---|---|---|---|
| TD-27 | Szybki słaby przeciwnik | KR pliki wrogów [P/W] | Test wymagający szybkiego celowania | P0 |
| TD-28 | Opancerzony tank | KR `armor`, `magic_armor` [P/W] | Zmusza do ataku magicznego/prawdziwego | P0 |
| TD-29 | Latający wróg | KR motyw + PvZ `zombie_balloon` asset [P/W] | Celowanie wyłącznie wybranych typów wież | P1 |
| TD-30 | Leczenie/wzmocnienia grupowe | wrogowie `shaman`, symbole aury [P/W] | Jednostka wsparcia z czytelnym aura VFX | P2 |
| TD-31 | Rozmnażanie/przywoływanie | KR `necromancer`, PvZ zasoby [P/W] | Spawn dodatkowych jednostek z limitami, bez kaskad nieskończonych | P2 |
| TD-32 | Skakanie, omijanie obrony | PvZ `zombie_pogo`, `zombie_ladder` [P/W] | Wrogowie, którzy czasowo ignorują blokadę, telegraphed | P2 |
| TD-33 | Przeciwnik zmieniający trasę | PvZ `garlic`, `portal` [P/W] | Przekierowania tylko gdy dostępne i z góry zdefiniowane | P3 |
| TD-34 | Wielofazowy boss | KR stage bosses + PvZ boss resources [P/W] | Fazy HP, okresy nietykalności tylko z ostrzeżeniem, wyraźne sygnały | P1 |
| TD-35 | Ograniczenie efektów CC na bossach | KR `boss_damage_cap`, `miniboss_damage_cap` [P/W] | Osobne reguły limitu stun/slow/DOT dla bossów | P1 |
| TD-36 | System odporności i statusów | KR `DAMAGE_POISON/TRUE/MAGICAL/PHYSICAL` + PvZ ice/fire [P/W] | `StatusEffectSystem`: slow, poison, burn, freeze, stun, shield | P1 |

### E. Mechaniki roślinne PvZ możliwe do twórczej adaptacji

| ID | Inspiracja | Co naprawdę wykryto | Oryginalny pomysł dla FantasyTD | Priorytet |
|---|---|---|---|---|
| TD-37 | Pułapka z opóźnionym uzbrajaniem | `potatomine` asset [P] | „Pąk minowy” układany w punkcie obronnym, arm-time i AoE | P2 |
| TD-38 | Lodowa kontrola tłumu | `icetrap`, `snowpea`, boss ice [P] | Kryształowa liana stopniowo spowalniająca i krusząca pancerz | P1 |
| TD-39 | Krótkodystansowy gaz/chmura | `fumecloud`, `gloomcloud` [P] | Grzyb zarodnikowy atakujący obszar stożkowy | P2 |
| TD-40 | Ogień na całej linii | `jalapeno` asset [P] | Jednorazowa aktywna umiejętność „ognisty korzeń” | P2 |
| TD-41 | Wielokierunkowe pociski | `starfruit` asset [P] | Wieża „gwiezdny cierń” strzela po kilku wektorach | P2 |
| TD-42 | Kontrola umysłu | `mindcontrol` particle [P] | Krótka dezorientacja: przeciwnik atakuje swoich, z limitem celów | P3 |
| TD-43 | Ochrona przeciw powietrzu | `balloon`, `cattail` sklep [P/W] | Kwiat wiatru samonaprowadzający przeciw lotnym | P2 |
| TD-44 | Strzały obszarowe i parabola | `catapultexplosion` [P] | Artyleryjska wieża nasion z trafieniem obszarowym | P1 |
| TD-45 | Ratunek przed utratą życia | `lawnmower`, `poolcleaner` [P] | Jednorazowy „strażnik bramy”, odkupywalny rzadko | P2 |
| TD-46 | Środowiska ograniczające strategie | audio/assets day/night/fog/pool/roof [P/W] | Modyfikatory map: mgła, zmienna widoczność, bagnisty grunt, noc | P3 |
| TD-47 | Ulepszenie istniejącej jednostki w unikatową | nazwy sklepowe Gatling/Twin etc. [P/W] | Drzewko awansu o unikatowych funkcjach, nie tylko +DPS | P2 |

### F. Bohater, zdolności i progresja

| ID | Mechanika | Źródło | FantasyTD | Priorytet |
|---|---|---|---|---|
| TD-48 | Bohater i jego zdolności | KR `go_hero_*`, `button_hero_skill` [P] | Pojedyncza postać pod kontrolą, 1 aktywna i 1 pasywna umiejętność | P2 |
| TD-49 | Umiejętność globalna | KR `go_power_*` [P] | Jedno aktywne zaklęcie z cooldownem i wizualnym telegraph | P2 |
| TD-50 | Rally point | KR `rally_range_factor`, `rally_range` [P] | Przenoszenie wspierających jednostek bez przerywania gry | P2 |
| TD-51 | Wyzwania/map challenge | KR `challenges.lua` i tryby TSV [P] | Modyfikatory „bez bohatera”, „tylko 3 typy wież”, „zwiększony pancerz” | P3 |
| TD-52 | Odblokowania niezależne od płatności | PvZ `StoreMetadata`, KR progression [P/W] | Gwiazdki za cele, odblokowanie wież; ekonomia bez agresywnego sklepiku | P2 |
| TD-53 | Encyklopedia przeciwników i wież | KR `button_encyclopedia_thumb_*` [P] | Bestiariusz z odpornościami i skutecznymi kontrami | P2 |

### G. UX, edukacja gracza i warstwa techniczna

| ID | Mechanika | Źródło | FantasyTD | Priorytet |
|---|---|---|---|---|
| TD-54 | Tutorial sterowany zdarzeniami | KR TSV `signal/wait/manual_wave` [P] | 3–5 kroków w Forest01: budowa, przeciwnik, wybór, upgrade, zdolność | P0 |
| TD-55 | Czytelność na małym ekranie | problem użytkownika [konwersacja] | Rozdzielenie planu gry od HUD, skala czcionek, kontrast statusów | P0 |
| TD-56 | Responsywny panel wieży | KR liczne `kui_templates` [P/W] | Koszt, obecny DPS, DPS po upgrade, statystyki z ikonami, aktywne przyciski | P0 |
| TD-57 | Feedback akcji | zasoby VFX obu gier [P/W] | Wybór: obwód; zakup: pop i animacja; trafienie: hit flash; brak środków: sygnał | P0 |
| TD-58 | Wydzielone warstwy FX i animacji | PvZ `particles/reanim`, KR `animations/` [P] | Oddzielny `VFXService`, pooling cząsteczek, atlas sprite'ów | P1 |
| TD-59 | Muzyka zależna od klimatu | PvZ `day.sps`, `night.sps`, `fog.sps` [P] | Płynne przejścia między ambient/calm/combat/boss (własna muzyka) | P2 |
| TD-60 | Ustawienia i accessibility | KR `UI_settings.lua` [P] | Miks audio, wibracja, skala HUD, redukcja efektów, pauza | P1 |
| TD-61 | Zapis/odtwarzanie poziomu | KR `storage.lua` [P] | Zachowanie ukończeń, odblokowań; później zapis przebiegu w połowie poziomu | P1 |
| TD-62 | Symulacja deterministyczna | niezależny wymóg niezawodności [R] | Seed RNG, fixed timestep, stabilna kolejność zdarzeń; możliwość odtwarzania testu | P0 |
| TD-63 | Diagnostyka jakości i wydajności | problem użytkownika [konwersacja] | Log błędów, FPS, spawn count, RAM, czas fali, błędy ekonomi | P0 |

## 4. „Forest01 — produkt, nie prototyp”: zamknięty zakres P0

1. **Jedna piękna, czytelna plansza** z drogą, punktami wież, oznaczeniem startu i celu; bez nakładającego się HUD.
2. **4 archetypy wież**: szybka punktowa, ciężka obszarowa, kontrola tłumu, przeciwpancerna. Każda ma trzy poziomy i widoczną zmianę wyglądu. Na potrzeby istniejącego projektu można zachować już dobre wieże zamiast tworzyć zamienniki.
3. **3 typy przeciwników**: zwykły, szybki, pancerny + silniejszy miniboss.
4. **10–12 zaplanowanych fal** — jawne czasy i tory; zmiana rytmu między zagrożeniami, bez przypadkowego spamowania.
5. **Pełny cykl ekonomiczny**: start, zakup, ulepszenie, zabicie, nagroda, przegrana, zwycięstwo i restart.
6. **Sterowanie dotykowe**: tap na slot / tap na stojącą wieżę / upgrade / zamknięcie; przycisk jednoznacznie sygnalizuje, że upgrade jest dostępny.
7. **Tutorial** pokazuje postawienie pierwszej wieży, jej wybranie i ulepszenie bez psucia standardowej fali.
8. **Feedback audiowizualny** i wyraźne typy obrażeń/statusów. Efekt graficzny nigdy nie ukrywa krytycznych informacji.
9. **Pauza, przyspieszenie, retry, wynik i zapis ukończenia**.
10. **Testy red–green–refactor, Android smoke-test i regresja** — warunek zakończenia.

### Zakazy zakresowe do czasu akceptacji Forest01

Nie zaczynać nowej mapy, rozbudowanego ekwipunku, pełnego systemu bohaterów, monetyzacji, endless i 30 nowych wież. Każda taka funkcja podnosi ryzyko kolejnych regresji.

## 5. Proponowana architektura Unity 6

```text
FantasyTD
  GameBootstrap
  GameStateMachine              // Menu, Tutorial, Playing, Paused, Win, Lose
  DefinitionCatalog             // read-only definitions
    TowerDefinition
    TowerUpgradeDefinition
    EnemyDefinition
    WaveDefinition
    LevelDefinition
    StatusEffectDefinition
  Runtime
    WaveDirector                 // emits scheduled events
    SpawnService                 // object pool + path binding
    EconomyService               // atomic gold transactions
    TowerPlacementService        // slot and preview validation
    TowerUpgradeService          // purchase + level + stats + visuals
    TargetSelectionService       // deterministic priorities
    CombatService                // physical/magical/true etc.
    StatusEffectService          // unique stacking policies
    EnemyPathFollower
    LevelResultService
    SaveService
  Presentation
    TouchInputRouter             // tap disambiguation
    TowerSelectionPanel          // source-of-truth UI
    HUDController
    VFXPool
    AudioDirector
    AccessibilitySettings
  QA
    DeterministicSimRunner
    BalanceReport
    AndroidSmokeChecklist
```

**Kluczowe granice odpowiedzialności:** `TowerUpgradeService` jest jedynym modułem odejmującym walutę przy zakupie ulepszenia; `EconomyService` kontroluje saldo; `TowerSelectionPanel` czyta model, zamiast utrzymywać własne statystyki. Jedna definicja reguł działa w rozgrywce i testach.

## 6. Testy regresyjne — obowiązkowe przed każdą następną mechaniką

| Kod | Test | Oczekiwany wynik |
|---|---|---|
| F01-01 | Tap na pustym slocie | Otwiera menu budowy, nie wybiera innej wieży |
| F01-02 | Kupno wieży bez środków | Brak spawnu i brak zmiany waluty |
| F01-03 | Kupno wieży z wystarczającymi środkami | Jedna wieża, jeden koszt, jeden update HUD |
| F01-04 | Tap w istniejącą wieżę | Wybiera właściwy obiekt i pokazuje poziom oraz koszt upgrade |
| F01-05 | Upgrade L1→L2 | Waluta pobrana raz, statystyki przeliczone, sprite/animacja zmienione |
| F01-06 | Upgrade L2→L3 | Jak wyżej, a przy max level przycisk nieaktywny |
| F01-07 | Dwa szybkie tapnięcia w upgrade | Tylko jedna transakcja przy jednym stanie wejściowym |
| F01-08 | Wieża strzela w poprawnego wroga | Poprawny priorytet, cooldown i zakres |
| F01-09 | Przeciwnik ma pancerz/magic resist | Obrażenia wyliczane na podstawie jawnych reguł |
| F01-10 | Slow + DoT + stun | Reguły nakładania statusów nie powodują locka ani nieskończonego stackowania |
| F01-11 | Wrogowie przecinają cel mapy | Właściwa utrata życia, usunięcie jednostki, bez podwójnego naliczenia |
| F01-12 | Zakończenie fali | Brak zdublowanego spawnu; następna fala i nagroda są zgodne z tabelą |
| F01-13 | Pauza i przyspieszenie | Wszystkie zegary rozgrywki zachowują spójność |
| F01-14 | Reload/retry | Czyszczenie starych pocisków/eventów i pełny reset stanu |
| F01-15 | Tutorial i normalna fala | Zdarzenia tutorialu nie blokują ani nie dublują spawnu |
| F01-16 | Android dotyk 16:9, 20:9 | Przycisk upgrade nie jest zasłonięty przez UI; target touch ma odpowiedni rozmiar |
| F01-17 | Długa rozgrywka | Brak narastania pamięci przez pociski i cząsteczki |
| F01-18 | Powtarzalność | Ten sam seed, te same decyzje → te same zdarzenia i wynik |

**Warunek akceptacji:** przechodzą testy jednostkowe + integracyjne + instalacja i test rzeczywistego APK na telefonie, a wszystkie błędy P0 są zamknięte. Jedna plansza nie jest „gotowa”, jeśli działa tylko w edytorze.

## 7. Plan wdrażania — z kontrolą zakresu i kosztów

### Etap A — stabilizacja istniejącego Forest01 (P0)
- Audyt aktualnego kodu i istniejących assetów; zachować to, co działa.
- Odizolować walutę/upgrade/wybór wieży; dodać testy F01-01…07.
- Zamknąć system `WaveDefinition`, precyzyjną sekwencję spawnu i testy F01-08…12.
- Poprawić HUD, dotyk, grafikę L1/L2/L3, animacje i czytelność.
- Zbudować i zainstalować APK; zapisać dowody ze zrzutami, logami i commitem.

### Etap B — jakość produktowa Forest01 (P1)
- Efekty statusów, boss/miniboss, balans kontr i ekonomii.
- Dodać dźwięk, ekran wyników, ustawienia, zapis i tryb pauzy.
- Regresja i profilowanie na Androidzie; dopracowanie do akceptacji.

### Etap C — zawartość po odbiorze (P2/P3)
- Nowe archetypy, rozgałęzienia, hero, generatory, specjalne tryby, endless.
- Dopiero wtedy tworzyć Forest02 lub nowy biom.

## 8. Rejestr dowodów i otwartych kwestii technicznych

| Artefakt / kanał | Stan | Co można wywnioskować | Czego nie można uczciwie stwierdzić |
|---|---|---|---|
| KR TSV `level01_waves_campaign.tsv` | Odczytany w całości w poprzedniej analizie, fragmenty sprawdzone | Startowe zasoby, plan jednostek, czas między spawnami, tutorial oparty na sygnałach | Pełny scheduler runtime bez analizy engine |
| KR bytecode `balance.lua` | Nagłówki i tekstowe symbole odczytane; kopia przesłana do prywatnego magazynu Rea, SHA zweryfikowany | Kategorie parametrów balansu | Konkretne wyliczenia DPS i wzory bez dekompilacji |
| KR `grid_a_star.lua` / `path_db.lua` | Nazwy i bytecode znalezione | Moduły tras/nawigacji | Sposób podziału tras i koszt A* w runtime |
| KR 218 TSV fal | Zindeksowane ścieżki i tryby | Architektura data-driven i warianty mapy | Pełen balans wszystkich fal bez ich parsowania |
| PvZ `ConfigFile.json` | Odczytany, przesłany do Rea, SHA zweryfikowany | Parametry sklepu i kilka sekcji konfiguracji | Przebieg całej walki; część pól dotyczy usług, reklam i sklepu |
| PvZ OBB | Otwarty ZIP wewnątrz APK; 465 pozycji | Obecne assets efektów, reanimacji, muzyki | Dokładny kod zachowania roślin z samej nazwy pliku |
| PvZ `libpvz.so` | Zlokalizowana biblioteka 50 MB | Miejsce istotnej logiki natywnej | Nie wykonano analizy funkcji ani pseudokodu |
| Rea lokalny dekompilator | Osobny host, bez dostępu do `/data/app` Androida | Konieczna wymiana danych lub ADB na hoście | Nie można uznać pełnej dekompilacji za wykonaną |

## 9. Ostatni krok przed automatycznym wdrożeniem

Przed tworzeniem PR do `SonGohan231/FantasyTD` konieczny jest **odczyt bieżących plików repozytorium** (scen, prefabów, kodu wież, ekonomii, testów i istniejących issue). Ten dokument jest **specyfikacją**, nie twierdzi, że zmieniono kod lub zbudowano nowy APK. Konkretne nazwy klas w pkt 5 są projektowanym interfejsem, nie potwierdzonym stanem repo.

W pierwszej kolejności należy wdrażać **TD-01, TD-04, TD-11–14, TD-20–21, TD-26, TD-54–58, TD-62–63**. Ich suma eliminuje zgłoszone w konwersacji problemy: słabą czytelność, niedziałające ulepszenia i regresje przy każdej funkcji.