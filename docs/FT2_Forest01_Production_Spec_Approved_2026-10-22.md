# FantasyTD 2.0 — Forest01 | Specyfikacja produkcyjna
**Wersja:** 1.0 · **Data:** 2026-10-22 · **Status:** ZATWIERDZONY KIERUNEK WIZUALNY / IMPLEMENTACJA W TOKU  
**Ticket nadrzędny:** FT2-TERRAIN-001 · **Gałąź robocza:** `codex/ft2-forest01-isometric-terrain-20261022` · **PR:** #33  
**Istotna granica:** akceptacja ilustracji koncepcyjnej nie jest potwierdzeniem zgodności sceny Unity ani ukończenia gry Android.

## 1. Decyzje zatwierdzone przez właściciela

Właściciel zaakceptował w rozmowie 2026-10-22 trzy kierunki przedstawienia jednej mapy:
1. **Czysta plansza** — izometryczna leśna wyspa z czytelną piaszczystą drogą, stawem i drewnianym mostkiem, roślinnością, skałami, bramami oraz kamiennymi stanowiskami na wieże; brak HUD i oznaczeń.
2. **Plansza techniczna** — ten sam układ, czytelnie wyróżniona trasa ruchu, dziesięć stanowisk budowy, start i meta, numeracja i legenda.
3. **Widok rozgrywki / marketingowy** — ten sam krajobraz z roślinnymi wieżami, czytelnymi przeciwnikami, pociskami/efektami i skromnym HUD mobilnym.

**Referencje źródłowe (zatwierdzone obrazy koncepcyjne, nie renderowane z Unity):**
- `izometryczna_mapa_leśnej_krainy_z_mostem.png` — czysta plansza.
- `plan_terenu_leśnej_wieży_obronnej.png` — opisany widok techniczny.
- `kolorowa_leśna_kraina_wież_obronnych.png` — wizualizacja gameplay.
- `Forest01_28x20_schemat_ukladu.png` — diagram współrzędnych/geometrii.
- Rozmowa referencyjna użytkownika: https://chatgpt.com/share/6ad9c85d-2df8-83ed-80f5-6f2eadd166e5?ogimg=plain

**Uwaga do przekazania zespołowi:** obrazy koncepcyjne powstały za pomocą AI i są materiałem kierunkowym. Zatwierdzone pliki PNG znajdują się w artefaktach rozmowy, **nie** są aktualnie wersjonowane w `UnityProject/Assets`. Nie należy twierdzić, że ich elementy są już osobnymi sprite'ami, tile'ami czy prefabami. Nie traktować rozkładu ikon/numeracji w AI-grafice jako dokładnego źródła współrzędnych: autorytatywny plan siatki jest w `Forest01TerrainLayout.cs`.

## 2. Cel i produkt końcowy

W ramach Forest01 uzyskać **grywalną, czytelną na telefonie** pierwszą mapę na Androida, przypominającą zatwierdzony obraz, ale z rzeczywistymi edytowalnymi obiektami Unity 6 i działającymi mechanikami. Priorytet: dopracować jeden poziom produktu, nie produkować dodatkowych map i ilustracji.

Wymagany efekt składa się z trzech oddzielnie odbieranych warstw:
- **Geometria / technologia:** powtarzalnie generowana, modyfikowalna siatka i ścieżka, poprawny import i test.
- **Prezentacja:** spójne ręcznie dopracowane/oryginalne sprite'y, interfejs, efekty oraz powtarzalne ujęcie kamery zgodne z zatwierdzonym kierunkiem.
- **Rozgrywka:** rzeczywiste rozstawianie/wybór wież, ruch przeciwników, fale, ekonomiа, HP bazy, wynik, pauza i restart. Ta warstwa **nie jest częścią testu samego terenu**.

## 3. Geometria i zamrożony kontrakt pierwszej iteracji

| Parametr | Wartość | Źródło prawdy |
|---|---|---|
| Widok | 2D izometryczny / 2.5D wrażenie głębi | `GridLayout.CellLayout.Isometric` |
| Rozmiar | 28 kolumn × 20 wierszy = 560 komórek | `Forest01TerrainLayout.Width/Height` |
| Układ współrzędnych | x=0–27, y=0–19 | `Forest01TerrainLayout.cs` |
| Podłoże | 560 trawiastych tile'i, bez brakujących pól | `Ground_560_Tiles` |
| Droga | jedna ciągła, nierozgałęziona, 40 komórek | `Road_Continuous` |
| Wejście | (0,13), lewa strona planszy | `Road[0]` |
| Wyjście | (27,9), prawa strona planszy | `Road[Road.Length-1]` |
| Stanowiska | dokładnie 10 różnych, niezachodzących na drogę i wodę | `Sites[]` |
| Woda | 18 komórek, okolice x=15–20 i y=2–5 | `Water[]` |
| Most | środek (18,4), wyłącznie ozdobny w obecnym etapie | `Bridge` |
| Drzewa / skały | 12 / 6 obiektów testowych | `Trees[] / Rocks[]` |

**Punkty zwrotne trasy (łączenie tylko ortogonalne w przestrzeni komórek):**
```text
(0,13) → (4,13) → (4,11) → (9,11) → (9,13) → (13,13)
       → (13,9) → (18,9) → (18,7) → (22,7) → (22,9) → (27,9)
```

**Stanowiska budowy:**

| ID | (x,y) | ID | (x,y) |
|---|---|---|---|
| B01 | (2,16) | B06 | (15,15) |
| B02 | (5,15) | B07 | (15,7) |
| B03 | (7,9) | B08 | (20,11) |
| B04 | (10,16) | B09 | (23,5) |
| B05 | (11,10) | B10 | (25,12) |

Kolejne rewizje rozmieszczenia dopuszczalne wyłącznie jako jawna zmiana wersji danych mapy z aktualizacją testów. Rysunek koncepcyjny może zawierać nieścisłości perspektywy, numeracji albo rozmiarów; w razie kolizji pierwszeństwo mają jawnie przyjęte współrzędne, a dopiero później korekta artystyczna zatwierdzona z nowym layoutem.

## 4. Wygląd 1:1 — wymagania artystyczne (co ma zobaczyć gracz)

| Element | Zatwierdzony wygląd | Odpowiedzialna warstwa | Obecny stan |
|---|---|---|---|
| Teren | Żywozielona, organiczna łąka; zróżnicowane plamy trawy, kwiaty, ślady ziemi; podniesiona skalno-ziemna krawędź wyspy | Ground / Edge / Detail | generator dwóch prostych wariantów trawy; **potrzebna wymiana grafiki** |
| Droga | Jasnopiaskowa szeroka ścieżka, wyraźny kamienny brzeg, płynne wizualnie zakręty bez szczelin; cały przebieg widoczny | Road base / borders / corners | jeden kafel piasku; **brak autorskich zakrętów i brzegów** |
| Drzewa | Gęstszy pierścień drzew wokół planszy; świerki i okrągłe korony w różnych skalach i odcieniach, utrzymane prześwity wokół drogi | TallDecor / canopy sorting | proste zastępcze sprite'y; **brak atlasu i detalu** |
| Woda | Niebiesko-turkusowy staw o nieregularnym obrysie, brzegi skalne, trzciny i lilie, czytelna tafla | Water base / shore / reeds | jeden rodzaj tile'a wody; **brak brzegów i roślin wodnych** |
| Most | Niewielki drewniany mostek z deskami i słupkami nad stawem; część dekoracyjna | Bridge prefab | placeholder; bez połączenia z drogą |
| Skały | Kamienie o kilku wielkościach, naturalne bryły, mchy, nierównomierna dystrybucja | LowDecor | sześć prostych placeholderów |
| Bramy | Dwie drewniane palisady z pochodniami i proporcami; wejście i wyjście rozpoznawalne bez tekstu | Gates / VFX light | proste placeholdery |
| Miejsca wież | Okrągłe kamienne podesty, równy wizualny rozmiar, odróżnienie wolny/zajęty/wybrany/niedostępny także bez samego koloru | BuildSites / interaction overlay | 10 oddzielnych obiektów z zaznaczaniem; brak stanów produkcyjnych |
| Kamera | Pełna plansza czytelna na landscape; zachować UI safe areas i nie obcinać bram; odpowiednia przestrzeń pod HUD | Camera / MobileViewport | ortograficzne wycentrowanie; wymaga realnego porównania screenshotów |
| Atmosfera | Barwne organiczne fantasy, przyjazne, szczegółowe, bez militarnego/sci-fi charakteru | Art Bible / palette / shadows | zaakceptowany koncept, **brak pełnego wdrożenia** |

**Podstawowa hierarchia czytelności:** 1. przebieg drogi i kierunek wrogów, 2. stanowiska wież i ich stany, 3. sylwetki wież oraz wrogów, 4. efekt walki, 5. ornamenty. Żaden detal dekoracyjny, korona drzewa ani efekt nie może zasłaniać wskaźnika wyboru lub kluczowego odcinka drogi.

**Język graficzny:** oryginalne, kolorowe organiczne fantasy; ręcznie malowany / ilustracyjny modułowy teren; miękkie światło i spójny sposób cieniowania, odróżnialne sylwetki. Zgodność z `UnityProject/Docs/ART_DIRECTION.md` i `docs/FantasyTD_2_0_GDD.md`. Bez naśladowania postaci, nazw i chronionych zasobów komercyjnych gier.

### Założenia produkcji grafiki
- Nie używać jednego obrazu AI jako „gotowej gry”: droga, trawa, brzegi stawu, rośliny, most, podesty, bramy i jednostki muszą być edytowalnymi sprite'ami/Tilemapami/prefabami.
- Arkusze/atlasy sprite'ów: jednolite rozmiary źródłowe, pivots, Pixels Per Unit, sortowanie i deterministyczny import w Unity. Dla większych obiektów oddzielny collider/visual root.
- Dla piasku i wody przewidzieć co najmniej: środek, krawędź, narożnik wewnętrzny i zewnętrzny, zakończenie/wariant przejścia; użyć RuleTile lub własnego deterministycznego resolvera sąsiedztwa.
- Osobne warstwy wizualne: `Ground`, `Road`, `RoadBorders`, `Water`, `WaterShore`, `CliffEdges`, `LowDecor`, `TallDecor`, `BuildSites`, `CombatUnits`, `VFX`, `HUD`. Nie wszystkie muszą być osobnymi Tilemapami: dekoracje 2.5D preferują prefab obiektowy.
- Każdy źródłowy plik z manifestem pochodzenia (nazwa, autor/generator, prawo do użycia, data, format, wymiary, pivot, PPU, atlas i sposób integracji). Grafika zatwierdzona wizualnie = kierunek, nie automatycznie licencjonowany gotowy sprite.

## 5. Wymagania techniczne Unity 6

**Nie nadpisywać działającego prototypu.** Jedyny docelowy plik etapu terenu to nowa scena:
`Assets/FantasyTD/V2/Scenes/Forest01_TerrainTest.unity`.

**Istniejące pliki:**
- `UnityProject/Assets/FantasyTD/V2/Terrain/Forest01TerrainLayout.cs` — parametry i walidator geometrii;
- `UnityProject/Assets/FantasyTD/V2/Terrain/Forest01TerrainInteraction.cs` — prototyp wybierania 10 stanowisk;
- `UnityProject/Assets/FantasyTD/V2/Editor/Forest01TerrainGenerator.cs` — generator sceny i kafelków;
- `UnityProject/Assets/FantasyTD/V2/Editor/Forest01TerrainSmoke.cs` — test Unity sceny;
- `UnityProject/Assets/FantasyTD/V2/Scenes/Forest01.unity` — istniejąca gra; bez zmian w etapie terenu;
- `UnityProject/Assets/Resources/FT2/` — stare grafiki, których nie wolno nadpisywać.

**Docelowa hierarchia sceny (przykład):**
```text
Forest01_TerrainRoot
├── Grid_Isometric_28x20
│   ├── Ground_560_Tiles
│   ├── Road_Continuous
│   ├── Road_Stone_Edges
│   ├── Water_Pond
│   └── Water_Shore
├── Decorations_Editable
│   ├── Trees
│   ├── Rocks
│   ├── Bushes_And_Flowers
│   ├── Bridge_Decoration
│   ├── Start_Gate
│   └── Exit_Gate
├── BuildSites_10_Interactive
│   └── B01 ... B10
├── Terrain_Test_Orthographic_Camera
└── Terrain_Interaction
```
Do grafiki produkcyjnej dojdą EdgeCliffs, Culling/Sorting i niezależny HUD.

**Render i UI:** Unity 6, landscape, 16:9 i 20:9, kamera ortograficzna. Używać Y-sortingu/sorting groups, by wieże i wrogowie nie znikały za dekoracjami. Siatka może być widoczna w edytorze, ale niewidoczna na ekranie gry. Teren powinien pozostać rozpoznawalny w docelowym rozmiarze telefonu bez maksymalnego zoomu.

## 6. Interakcje i przejście do grywalnej mapy

### Stan etapu TEREN
- Dziesięć stanowisk musi odpowiadać na dotknięcie/mysz.
- Wyróżnienie stanowiska oraz możliwość odznaczenia/zmiany wyboru.
- Żadna interakcja nie może przechodzić „przez” HUD.
- Granica: prototypowy `Forest01TerrainInteraction` obecnie **nie** stawia wieży ani nie uruchamia fal.

### Kolejny etap ROZGRYWKA (osobny ticket, bez rozszerzania zakresu TEREN)
- Adapter współrzędnych siatki izometrycznej do istniejącego modelu trasy i stanowisk; zachować deterministyczny `BattleSimulator`.
- Jedna spójna trasa wrogów od Start_Gate do Exit_Gate; prawdziwy spawn/ruch/kolizje oraz końcowe HP bazy.
- Integracja 10 stanowisk z mechanizmem budowania; obecny `StarterLevel.cs` ma **12** pozycji z poprzedniego layoutu: nie zmieniać po cichu logiki i nie ignorować różnicy.
- Zachować obecny zestaw pilotażowy trzech obrońców i trzech fal w `StarterLevel.cs`, dopóki osobny ticket nie zatwierdzi zmian mechaniki.
- Zwycięstwo, porażka, restart, pauza, interakcje dotykowe, czytelna prezentacja kosztów i stanów.

**Ważne:** gameplayowa wizualizacja koncepcyjna ma przykładowe wskaźniki **20 HP / 230 waluty / WAVE 3/10** oraz inne kwiaty niż bieżący prototyp. Te liczby i jednostki są **dekoracyjną ilustracją** i nie zmieniają parametrów logiki. Aktualny `StarterLevel.cs` definiuje **15 HP**, **230 Essence**, **3 fale**, **3 rodzaje roślinnych obrońców**, a także 12 dawnych stanowisk. Zmiany wymagają oddzielnej decyzji produktowej.

## 7. Plan wdrożenia (jedno aktywne zadanie naraz)

| Faza | Ticket | Zakres | Dowód ukończenia |
|---|---|---|---|
| 0 | FT2-TERRAIN-001A | Geometria 28×20, walidacja, testy 560/40/10 | 5/5 statyczne; oddzielna scena w Unity i wygenerowany render |
| 1 | FT2-TERRAIN-001B | Realny import Unity 6 i naprawy wymagane do kompilacji/generacji; zebranie PNG z rendererem i raportu | Log Unity PASS na dokładnym SHA, zapis sceny i preview |
| 2 | FT2-ART-FOREST01 | Atlas trawy/ścieżki/brzegów, staw, most, drzewa/skały, bramy, podesty; wizualny QA wobec referencji | Rzeczywisty screenshot Unity 16:9 + 20:9, wizualna ocena właściciela |
| 3 | FT2-FOREST01-PLAYABLE | Adapter droga/stanowiska do `BattleSimulator`, ustawianie obrońców, pierwsza fala i pełna krótka sesja | Test PlayMode end-to-end; realna gra bez starych placeholderów |
| 4 | FT2-FOREST01-ANDROID | APK, instalacja, dotyk, pauza, restart, wydajność; poprawki na urządzeniu | Build SHA/APK hash, dowód uruchomienia i testów na Androidzie |
| 5 | FT2-FOREST01-RELEASE | Audyt czytelności i regresja jakości; usunięcie blockerów | Podpisana lista kontrolna, brak błędów blokujących |

**Zasada:** nie wykonywać równolegle konfliktujących zmian w `Forest01.unity`, generatorze i `manifest.json`. Przed integracją każda faza musi mieć osobny przetestowany commit i raport. PR #33 pozostaje draft do czasu poprawnego smoke z prawdziwego Unity.

## 8. Twarde kryteria odbioru

### GATE A — geometria (warunek formalny)
- [ ] Unity 6000.0.26f1 otwiera projekt, importuje pakiety i kompiluje bez błędów.
- [ ] Generator tworzy odrębną `Forest01_TerrainTest.unity`, bez uszkodzenia starej Forest01.
- [ ] Zliczenie w Unity: 560/560 podłoża, 40/40 drogi, 10/10 stanowisk, 18 pól wody, dekoracje i dwie bramy.
- [ ] W edytorze każda komórka i dekoracja są oddzielnie edytowalne.
- [ ] Runtime selection działa dotykiem i myszą, bez fałszywych trafień.
- [ ] Unity eksportuje 1280×720 screenshot oraz log PASS na tym samym SHA.
- [x] Hostowane testy struktury danych: 5/5 PASS w Github Actions na SHA `4da14ec295c8108c5a51e89bf41bc34e519883a9` (2026-10-22; tylko statyczna geometria).

### GATE B — wizualizacja (weryfikacja jakości)
- [ ] Droga nie ma szczelin ani efektu „powtarzalnego surowego rombu”, zakręty łączą się wizualnie.
- [ ] Woda ma brzegi, most i czytelny nieregularny kontur.
- [ ] Trzy gatunki sylwetek roślinności/rozmiarów i naturalne warianty skał nie zasłaniają trasy.
- [ ] Bramy i 10 podestów łatwo identyfikować bez legendy i przy małej rozdzielczości.
- [ ] Porównanie 1:1 ujęcia z Unity do czystej referencji i ocena zgodności światła, koloru, kompozycji oraz czytelności.
- [ ] Screenshoty z rzeczywistego Unity dla landscape 16:9 i 20:9 — bez ucięcia interfejsu.
- [ ] Oryginalne źródła wizualne z zapisanym pochodzeniem i prawami do użycia.

### GATE C — rozgrywka / Android (osobne kryteria)
- [ ] Cała krótka sesja PlayMode: start → wieże → fala → koniec, obsługa pauzy, restartu i porażki.
- [ ] Ekran dotykowy: wybór 10 stanowisk, wybór/ulepszenia w planowanym zakresie, brak zakrywania przez HUD.
- [ ] APK z Unity dla konkretnego SHA, log kompilacji, hash pliku.
- [ ] Rzeczywista instalacja i uruchomienie APK na urządzeniu, sprawdzona płynność i brak crashy.
- [ ] Docelowo 60 FPS na wskazanym testowym Androidzie, min. 30 FPS w scenach obciążonych — nie deklarować bez profilu.

## 9. Stan techniczny na dzień spisania specyfikacji

- Repozytorium `SonGohan231/FantasyTD`, PR #33, draft, bez scalenia do `main`.
- Kod nowej sceny i generatora **zapisany**, ale gotowego renderu z Unity nie potwierdzono.
- Statyczna geometria: PASS 5/5 na `4da14ec`.
- GitHub Actions realnego Unity (m.in. job `39750512081`) ostatnio zgłaszał `queued`. Status należy ponownie sprawdzić przed rozpoczęciem fazy wizualnej.
- ComputerMCP jest preferowanym wykonawcą; w bieżącej sesji zgłaszał `McpServerError: Session terminated`. Nie przypisywać mu wykonania testów bez potwierdzenia.
- Potwierdzone wcześniej Unity import/APK dotyczą **poprzedniej** grywalnej Forest01 i innego SHA, a nie nowej siatki 28×20.
- Wysokiej jakości ilustracje koncepcyjne są **zatwierdzonym benchmarkiem artystycznym**; proste wygenerowane kafelki z `Forest01TerrainGenerator` tego benchmarku jeszcze nie spełniają.

## 10. Definicja „ukończone Forest01”

Nie wystarcza ani pięć statycznych testów, ani brak błędów kompilacji, ani sama piękna grafika. Forest01 jest gotowym pierwszym poziomem dopiero wtedy, kiedy **zatwierdzony wygląd został odtworzony w edytowalnej scenie Unity i faktyczna pełna sesja działa na telefonie**. Wówczas dopiero wolno przechodzić do kolejnych map.

**Aktualna decyzja właściciela:** wizualizacje zatwierdzone; kierunek Forest01 zamrożony. **Nie zatwierdzono jeszcze** jakości assetów Unity ani działającego APK nowej mapy.
