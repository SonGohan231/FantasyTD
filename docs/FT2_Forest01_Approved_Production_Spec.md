# Forest01 — zatwierdzona specyfikacja produkcyjna
**Identyfikator:** FT2-FOR-ART-LOCK-001  
**Wersja:** 1.0 — 2026-10-22  
**Status:** **APPROVED VISUAL DIRECTION / IMPLEMENTATION IN PROGRESS / UNITY NOT YET VERIFIED**  
**Produkt:** FantasyTD, Unity 6 (`6000.0.26f1`), Android landscape, touch-first.  
**Repozytorium:** `SonGohan231/FantasyTD`, roboczy PR #33, gałąź `codex/ft2-forest01-isometric-terrain-20261022`.  
**Powiązany kontrakt geometrii:** `docs/FT2_Forest01_28x20_Layout.md`.  
**Bezpieczeństwo zmian:** oryginalna grywalna `Assets/FantasyTD/V2/Scenes/Forest01.unity` pozostaje nietknięta; testujemy odrębnie `Forest01_TerrainTest.unity`.

## 0. Zakres zatwierdzenia użytkownika — co jest wzorcem, a co nie
Użytkownik zatwierdził **trzy wizualizacje koncepcyjne Forest01** przygotowane w rozmowie: (A) czystą leśną planszę, (B) mapę z opisem trasy i stanowisk, (C) poglądowy ekran rozgrywki z wieżami i przeciwnikami. Zatwierdzenie dotyczy *kierunku artystycznego, ogólnej kompozycji mapy i poziomu czytelności*, a nie potwierdzenia, że te kadry zostały wyrenderowane w Unity albo istnieją jako sprite'y gotowe do użycia.

**Zaakceptowane referencje z rozmowy (pliki koncepcyjne — jeszcze niezaimportowane do Unity):**
- `izometryczna_mapa_leśnej_krainy_z_mostem.png` — wzorzec docelowego wyglądu planszy.
- `plan_terenu_leśnej_wieży_obronnej.png` — oznaczenia wejścia/wyjścia, trasy i stanowisk.
- `kolorowa_leśna_kraina_wież_obronnych.png` — wzorzec czytelności rozgrywki, wież, przeciwników i HUD.
- `Forest01_28x20_schemat_ukladu.png` — strukturalna podstawa projektowa; **konkretne współrzędne w kodzie** mają pierwszeństwo przed niedokładnościami ilustracji AI.

Wspomniane obrazy są *referencjami artystycznymi*, nie atlasami sprite'ów, nie odtwarzalną sceną 3D i nie dowodem testów gry. Źródło rozmowy: https://chatgpt.com/share/6ad9c85d-2df8-83ed-80f5-6f2eadd166e5?ogimg=plain

**Nie są zatwierdzone jako wartości mechaniki:** ilustracyjne `WAVE 3/10`, 20 serc, 230 monet, ilość widocznych wrogów, ataki wież czy rozmieszczenie wież zajętych podczas przykładowej walki. Obowiązują dane testowane w `StarterLevel.cs` i `BattleSimulator`, aż do osobnego zatwierdzenia zmian balansu.

## 1. Elementy Forest01, które należy odtworzyć

### 1.1. Kontrakt geometrii — bez swobodnej reinterpretacji
| Element | Wymaganie | Aktualny punkt odniesienia |
|---|---|---|
| Siatka | `28 × 20`, **560** pól; współrzędne `x=0..27`, `y=0..19` | `Forest01TerrainLayout.Width/Height` |
| Projekcja | Unity `GridLayout.CellLayout.Isometric`, **kamera ortograficzna**, ujęcie 3/4 mapy | `Forest01TerrainGenerator` |
| Trawa | 560 edytowalnych komórek; płynna wizualnie darń bez czarnych szpar | `Ground_560_Tiles` |
| Droga | **40 kolejnych, unikalnych pól**, jedna trasa, brak teleportacji i przerw | `Road_Continuous` |
| Stanowiska | **10** osobnych stanowisk B01–B10, poza drogą/wodą, wybieralne dotykiem | `Forest01TerrainBuildSite` |
| Woda | Oczko wodne **18** komórek w sektorze (15..20, 2..5) | `Water_Pond` |
| Most | Dekoracyjny most w rejonie `(18,4)`; nie udawać przejścia trasy wroga | `Bridge_Decoration` |
| Wejście/wyjście | Dwie czytelne bramy, `(0,13)` i `(27,9)` | `Start_Gate`, `Exit_Gate` |
| Drzewa / skały | Minimum **12** niezależnych drzew i **6** skał jako edytowalne obiekty bazowe; gęste leśne obramowanie docelowo przez dodatkowe dekoracje | `Trees`, `Rocks` |
| Grywalna oryginalna scena | **Zachować** poprzednie 12 stanowisk i sprawdzony prototyp 3 fal; **nie nadpisywać** na etapie nowej geometrii | `V2/Scenes/Forest01.unity` |

Punkty zwrotne ścieżki z kodu:

```text
(0,13) → (4,13) → (4,11) → (9,11) → (9,13) → (13,13)
       → (13,9) → (18,9) → (18,7) → (22,7) → (22,9) → (27,9)
```

Dziesięć stanowisk, zgodnych z istniejącą klasą `Forest01TerrainLayout`:

```text
B01 (2,16)   B02 (5,15)   B03 (7,9)    B04 (10,16)  B05 (11,10)
B06 (15,15)  B07 (15,7)   B08 (20,11)  B09 (23,5)   B10 (25,12)
```

Współrzędne komórek są wartościami logicznymi siatki; pozycję świata obliczać przez jeden wspólny interfejs Grid (np. `GetCellCenterWorld`). Nie przenosić ręcznie współrzędnych wizualizacji AI do `BattleSimulator`.

### 1.2. Zatwierdzona stylistyka artystyczna
- **Świat:** ciepły, kolorowy, organiczny fantasy forest; łagodne modelowanie światła i przyjazne, czytelne sylwetki.
- **Rzut:** czytelna izometria 2D/2.5D, nie realistyczna gra z kamerą perspektywiczną. Styl „unoszącej się wyspy” z urwiskiem może być dodatkową *warstwą wizualną*, lecz nie wolno zwiększać siatki logicznej bez osobnej decyzji.
- **Paleta:** nasycone, zróżnicowane zielenie; ciepły piasek; chłodny błękit/turkus wody; naturalne brązy drewna; kamienie szarozielone. Ciemne, nieinwazyjne tło poza mapą.
- **Droga:** jasnopiaskowa, łatwa do prześledzenia wzrokiem; osobne warianty prostych, zakrętów, narożników, obrzeży, łączeń. Żadnych widocznych szwów lub ciemnych dziur między kafelkami.
- **Roślinność:** świerki/sosny i drzewa liściaste, kwiaty, trawy, krzewy, mchy, pniaki, omszałe skały; większa gęstość na obrzeżach, środek zostawić czytelny.
- **Woda:** turkusowa sadzawka z brzegiem i odbiciami, kamienie, trzciny/lilie; oddzielna animacja efektów jest opcjonalna w późniejszym etapie.
- **Most i bramy:** czytelne drewniane elementy, bramy z delikatnym ognistym akcentem; nie zasłaniać drogi/stanowisk.
- **Stanowiska:** jednolity język okrągłych kamiennych podstaw. Domyślnie nie mogą przypominać istniejącej wieży; wyraźny stan wolne/zaznaczone/zajęte/niedostępne.
- **Wieże:** wyłącznie roślinne/żywe, różne sylwetki i kolory według funkcji. Nie wprowadzać działek SF, robotów ani technologicznych dział.
- **Przeciwnicy:** małe fantastyczne stworzenia o sylwetkach możliwych do rozpoznania na ekranie telefonu; pasek zdrowia czytelny, ale nie przytłaczający.
- **HUD:** oszczędny, kontrastowy, nie zasłania mapy i stanowisk; ilustracyjny HUD z grafiki nie jest wzorcem ustalonych statystyk.

### 1.3. Priorytet jakości
**Czytelność > możliwość edycji > wierny klimat > liczba ozdób.** W razie konfliktu należy zmniejszyć zagęszczenie dekoracji, zamiast zasłaniać miejsca interaktywne, wrogów lub trasę. Każda warstwa musi pozostać niezależna, aby korekty stylu nie wymagały przebudowy logiki poziomu.

## 2. Wymagania wdrożeniowe 1:1 w Unity 6

### 2.1. Dane i struktura sceny
Zachować testowy generator i rozdzielić odpowiedzialności:

```text
Forest01_TerrainTest
├── Terrain_Test_Orthographic_Camera
└── Forest01_TerrainRoot (Grid / TerrainInteraction)
    ├── Ground_560_Tiles (Tilemap + TilemapRenderer)
    ├── Water_Pond (Tilemap + TilemapRenderer)
    ├── Road_Continuous (Tilemap + TilemapRenderer)
    ├── Decorations_Editable
    │   ├── Trees / Rocks / Flowers / Shrubs / CliffDetails
    │   ├── Bridge_Decoration
    │   ├── Start_Gate
    │   └── Exit_Gate
    └── BuildSites_10_Interactive (B01...B10)
```

- `Forest01TerrainLayout.cs` pozostaje autorytatywnym źródłem współrzędnych, a nie edytowane ręcznie pozycje prefabów.
- Generator pozostaje deterministyczny i bezpieczny przy ponownym uruchomieniu; przy generowaniu nie wolno utracić ręcznie dopracowanych źródeł artystycznych — nowe arkusze mają własną ścieżkę z kontrolą wersji.
- `Forest01_TerrainTest.unity` musi zostać fizycznie wygenerowana i otwarta w Unity Editor. Sam kod generatora nie jest sceną końcową.
- Pracować na osobnej gałęzi i w osobnym katalogu; nie ingerować w `MAP_A_ANDROID_V2`, starą `Forest01.unity`, oryginalny balans ani pakiety niepowiązane z biletem.

### 2.2. Produkcja assetów do jakości z wizualizacji
Obecny `Forest01TerrainGenerator` tworzy uproszczone kafelki PNG i sprite'y generowane kodem. Jest to **placeholder do testu Tilemap**, a nie akceptacja finalnego wyglądu.

Do realizacji zaakceptowanego wzorca należy przygotować **oryginalne, osobno edytowalne** zestawy:
1. **Ground atlas** — 2–4 warianty trawy, obrzeża, fragmenty nierównego podłoża; bez widocznego powtarzania w mozaice.
2. **Road atlas** — proste, zakręty, wewnętrzne/zewnętrzne krawędzie, połączenia; komórka drogi ma naturalnie pasować do sąsiednich.
3. **Water atlas** — wnętrze, brzegi, narożniki, detale i odseparowany most.
4. **Forest props** — min. warianty iglaków, liściastych, skał, krzewów, kwiatów, pniaków; osobne przezroczyste sprite'y/prefaby.
5. **Build pads + gates** — wolna/zajęta/zaznaczona podstawa, wejściowa i wyjściowa brama, efekt aktywacji.
6. **Ground edge / cliff band** — estetyczne obramowanie całej planszy w stylu zaakceptowanej wizualizacji (jeśli mieści się w budżecie rysowania i czytelności).
7. **Sorting + lighting** — warstwy i kolejność renderowania gwarantują, że postaci na ścieżce nie znikają pod przypadkowymi obiektami. Roślinność może przesłaniać obrzeża, ale nie cele dotykowe.
8. **Animacje i efekty gry** — dopiero po weryfikacji nieruchomej mapy: proste animacje, projectiles/VFX bez maskowania przeciwników.

Format wejściowy: oryginalne PNG z przezroczystością i stabilnymi ustawieniami importu sprite/Texture2D; piksele-na-jednostkę, pivot oraz rozmiar prefabów muszą być konsekwentne dla całego atlasu. Nie wycinać bezmyślnie elementów z jednego obrazu ilustracyjnego i nie traktować go jako kafelkowalnego tilesetu. Zachować dokumentację źródeł i praw do assetów w `docs/FT2_Forest01_Art_Sources.md`.

### 2.3. Implementacja interakcji i rozgrywki
Po przejściu testu terenu połączyć geometrię z istniejącym deterministycznym `BattleSimulator` przez **adapter danych poziomu**, nie przez przepisywanie walki.

- Adapter mapy przelicza `Cell` → world position dla 40 pól trasy, 10 stanowisk, początku i końca.
- Stara scena 12 stanowisk nadal działa na swoich danych; nowa scena używa 10 stanowisk. Nie wolno po cichu nadpisać starego `StarterLevel`.
- Dotyk: pojedyncze tapnięcie poprawnie wskazuje wolne stanowisko, odróżnia obszar UI i teren; na 16:9 oraz 20:9 nie ma niecelnych hitboxów.
- Budowa: koszt, odmowa budowania na zajętym polu, widoczny stan dostępności, potwierdzenie ustawienia i aktualizacja waluty.
- Wieże: w pierwszym kroku wykorzystać istniejące typy roślinnych obrońców. Ulepszenia to **oddzielny, jawny bilet** (wcześniej użytkownik zgłaszał niedziałające ulepszanie).
- Wrogowie: korzystają z jednej trasy i przechodzą kolejne komórki bez skoków; `spawn` i `exit` zgodne z bramami, zachowane wyniki obrażeń/walki.
- Pierwsza grywalna sesja: start → wybór pola → postawienie rośliny → fala → obrażenia i nagrody → następna fala / porażka / zwycięstwo / restart.
- Nie rozszerzać teraz rosteru, kampanii, ekonomii, bossów ani liczby fal wyłącznie na podstawie koncepcyjnego zrzutu rozgrywki.

### 2.4. Kamera, interfejs i czytelność Android
- Orientacja pozioma (landscape), główne testy **16:9 i 20:9**, z uwzględnieniem wycięć/bezpiecznych obszarów.
- Cała trasa i oba końce poziomu muszą pozostać widoczne; jeśli na telefonie nie da się utrzymać czytelnych obiektów, zaprojektować świadomy wariant framingu, zamiast arbitrarnie pomniejszać całość.
- Stanowiska i wrogowie rozpoznawalni bez ciągłego przybliżania; interfejs nie przykrywa interakcji.
- Przyciski z obszarami dotykowymi odpowiadającymi mobilnym standardom; testować na urządzeniu, a nie wyłącznie myszą w edytorze.
- Docelowo **60 FPS** na urządzeniu reprezentatywnym dla celu projektu; raportować rzeczywiste FPS, czas klatki, draw calls i pamięć. Brak pomiaru nie oznacza PASS.

## 3. Harmonogram wykonania — jedno aktywne zadanie naraz

| Bilet / etap | Prace | Obiektywny dowód zakończenia | Status na dzień sporządzenia |
|---|---|---|---|
| **TERR-01 / test sceny** | Import i uruchomienie generatora 28×20 w prawdziwym Unity 6, zrzut kamery i archiwum plików | Zero błędów C#; 560 pól trawy, 40 pól drogi, 18 wody, 10 stanowisk; PNG z Unity oraz log z markerem PASS na tym samym SHA | **BLOCKED / Unity runner queued** |
| **TERR-02 / kafelki produkcyjne** | Zamienić proceduralny wygląd na edytowalne atlasowe sprite'y trawy, piasku i brzegów | Realny screenshot Unity obok zatwierdzonego konceptu; brak widocznych szwów, no-overlap errors; bez zmian logiki | PLANNED |
| **TERR-03 / scenografia** | Woda, most, las, klify, bramy, sorting i spójne światło | Każdy obiekt jest osobny i edytowalny; brak zasłoniętych celów, poprawne renderowanie | PLANNED |
| **TERR-04 / integracja geometrii z walką** | Adapter 40-cell path / 10-site do istniejącego simulatora; interakcja dotykowa | Jedna trasa, wszystkie 10 pól wybieralne, brak regresji starszej sceny 12-site | PLANNED |
| **TERR-05 / grywalna Forest01** | Obrońcy, spawning, trzy fale prototypowe, HUD, obrażenia, zwycięstwo/porażka, restart | Realny Play Mode test na Unity oraz automat deterministyczny; pełna sesja bez krytycznych błędów | PLANNED |
| **TERR-06 / Android acceptance** | Debug APK, instalacja na rzeczywistym Androidzie, dotyk, 16:9/20:9, profilowanie | Build SHA, log, APK hash, test instalacji/uruchomienia, screenshot, wydajność i regresja | PLANNED |
| **TERR-07 / jakość produktu** | Wizualne poprawki i gameplay UX, osobno system ulepszeń; dopracować dopiero po akceptacji sesji | Porównanie zaakceptowanego wzorca vs screen Unity vs telefon; komplet QA krytycznych interakcji | PLANNED |

**Bramka do kolejnego biletu:** aktualny bilet musi mieć dowody, nie sam napis w PR. Test `TERR-01` wykonywany jest przez `.github/workflows/ft2-terrain-smoke.yml`, statyczny kontrakt przez `.github/workflows/ft2-terrain-static.yml`. Używać wyników z **tego samego commita**, który podlega ocenie.

## 4. Checklista odbiorowa (konieczne)
- [x] Zaakceptowany przez użytkownika kierunek wyglądu: trzy obrazy referencyjne z rozmowy.
- [x] Układ 28×20, logiczna trasa i stanowiska opisane w kodzie.
- [x] Statyczne testy geometrii **5/5 PASS** dla SHA `4da14ec295c8108c5a51e89bf41bc34e519883a9`, run `39750512078`.
- [ ] Realne wykonanie generatora i import osobnej sceny na Unity 6 — **niepotwierdzone**, run `39750512081` ostatnio `queued`.
- [ ] Render Unity `Forest01_TerrainTest_Preview.png` i porównanie z zatwierdzonym konceptem.
- [ ] Docelowy atlas kafelków i nieproceduralne dekoracje (obecny generator to placeholder).
- [ ] Wybór 10 pól w Play Mode, a potem na telefonie.
- [ ] Wszystkie trzy fale grywalne na nowej geometrii bez zepsucia starej Forest01.
- [ ] APK z tej gałęzi + weryfikacja na urządzeniu.
- [ ] Test 16:9/20:9, czytelność, mierzalna wydajność.

**Znane blokady na dzień wpisu:** podłączony ComputerMCP zgłasza `Session terminated`; kolejka Unity x64 może wymagać przywrócenia runnera. Praca nad dokumentacją/kodem przez GitHub nie oznacza, że lokalne Unity jest aktywne. PR #33 pozostaje **draft**, bez scalania do `main`.

## 5. Zasady prowadzenia przez ChatGPT + ComputerMCP
1. Jedynym interfejsem do zgłaszania prac dla użytkownika jest rozmowa ChatGPT; preferowanym wykonawcą Linux jest `ComputerMCP`, jeśli jego sesja działa. Alternatywnie GitHub Actions na zgodnym runnerze x64 Unity.
2. Każdorazowo sprawdzać rzeczywiste narzędzia, status sesji, commit i uruchomienia CI. Nie zakładać, że uprawnienia wtyczki zastępują uprawnienia systemowe.
3. Zmiany na jednej gałęzi/PR, w kolejności biletów. Testy regresyjne przed i po zmianie, bez nadpisywania działających wersji.
4. Każdy bilet kończy się raportem: **zmienione pliki, commit, testy i ich status, zrzut z Unity jeśli możliwy, pozostałe blokady**.
5. Nie generować kolejnych ilustracji lub poziomów, dopóki `Forest01` nie przejdzie wspólnie testów jakości terenu i podstawowej rozgrywki.

### Linki do dowodów
- PR: https://github.com/SonGohan231/FantasyTD/pull/33
- Zakończone statyczne testy: https://github.com/SonGohan231/FantasyTD/actions/runs/39750512078
- Unity Editor terrain job: https://github.com/SonGohan231/FantasyTD/actions/runs/39750512081
- Oryginalny, wcześniej testowany slice: https://github.com/SonGohan231/FantasyTD/pull/27
