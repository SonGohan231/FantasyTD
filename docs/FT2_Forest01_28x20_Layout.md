# FT2 — Forest01_TerrainTest: układ mapy izometrycznej (28×20)

Status: **projekt układu / osobna scena testowa**. Nie zastępuje grywalnej sceny `V2/Scenes/Forest01.unity` ani 12 stanowisk i trzech fal z wcześniejszego prototypu.

**Zatwierdzony kierunek wizualny i plan wdrożenia:** [`FT2_Forest01_Approved_Production_Spec.md`](FT2_Forest01_Approved_Production_Spec.md). Ta strona definiuje współrzędne oraz geometryczne kryteria, a zaakceptowane wizualizacje definiują docelowy styl. Kafelki proceduralne nie są ostateczną grafiką.

## Kontrakt
- Grid: `GridLayout.CellLayout.Isometric`, szerokość 28, wysokość 20, 560 komórek (x=0…27, y=0…19).
- W pełni pokryta powierzchnia trawy: osobna Tilemap, każda komórka ma edytowalny `Tile`.
- Jedna ciągła droga z kafli piaskowych, osobna Tilemap; bez ukośnych skoków czy pustych połączeń.
- Oczko wodne, most i drzewa są **oddzielnymi** obiektami/warstwami. Most w wersji testowej jest elementem ozdobnym przy wodzie, nie dowodzi działania nawigacji przeciwników.
- 10 stanowisk to prototyp interaktywnego wyboru dotykiem/myszą, **nie** integracja z obecną symulacją walki.
- 2 bramy krańcowe: start lewy, koniec prawy; podłączenie ich do spawnu przeciwników jest kolejnym etapem.
- Wykorzystane wcześniej źródła PNG pozostają w `Assets/Resources/FT2/` bez modyfikacji; stworzone kafle testowe są osobnymi, edytowalnymi zasobami.
- Kamera ortograficzna wycentrowana na całej mapie. Orientacja pozioma.

## Trasa (kolejne punkty zwrotne)
```
(0,13) -> (4,13) -> (4,11) -> (9,11) -> (9,13)
       -> (13,13) -> (13,9) -> (18,9) -> (18,7)
       -> (22,7) -> (22,9) -> (27,9)
```
Każdy odcinek rozwijany do komórek sąsiadujących bokiem; 40 komórek drogi, bez rozwidleń w zapisanej kolejności.

## Stanowiska (10)
```
B01=(2,16)  B02=(5,15) B03=(7,9)  B04=(10,16) B05=(11,10)
B06=(15,15) B07=(15,7) B08=(20,11) B09=(23,5)  B10=(25,12)
```
Wszystkie stanowiska różne; poza drogą i wodą; odległość Manhattan od drogi co najmniej 2 komórki.

## Woda
Jezioro przy południowej części mapy (komórki od x=15 do 20 i y=2 do 5), oddzielone od głównej drogi. Most na (18,4) to dekoracja do dalszej edycji.

## Dekoracje
- Drzewa: (1,18), (4,18), (6,17), (11,18), (14,17), (20,16), (26,17), (0,4), (4,3), (9,4), (25,3), (27,1)
- Skały: (2,2), (7,5), (12,3), (14,16), (23,16), (26,7)
- Bramy: (0,13), (27,9)
- Warstwy: `Ground`, `Water`, `Road`, `Decorations`, `BuildSites` z niezależnymi hierarchiami do ręcznej edycji.

## Kryteria odbioru
1. Generator zapisuje osobną scenę `Assets/FantasyTD/V2/Scenes/Forest01_TerrainTest.unity`.
2. Edytor Unity importuje scenę bez błędów kompilacji.
3. `Ground` zawiera dokładnie 560 kafelków, w każdym miejscu siatki 28×20.
4. `Road` ma 40 kolejnych kafelków tworzących nieprzerwaną trasę.
5. 10 obiektów stanowisk ma niezależny komponent i kolider wyboru; test wybierania działa.
6. Scena zawiera niezależne dekoracje i dwie bramy.
7. Test wykonuje render kamery do PNG; raportuje PASS dopiero po zakończeniu.
8. Obecna grywalna `Forest01.unity` i zasoby FT2 nie są modyfikowane.

## Granice tego etapu
Nie twierdzimy, że podgląd jest już profesjonalną, finalną grafiką, że trasa zasila `BattleSimulator` ani że APK uruchomiono na Androidzie. To etap projektowania i testowania kompletnej geometrii terenu.
