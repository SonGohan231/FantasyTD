# Fantasy TD — MAP-A Android v2

To jest **mobile-first vertical slice** gry Tower Defense.

Kierunek:
- **wizualnie:** kolorowe, organiczne fantasy, żywe rośliny i groteskowe stworki;
- **mechanicznie:** mapa i budowanie bliższe Kingdom Rush;
- **platforma:** Android, landscape;
- **bez sci-fi:** brak futurystycznych dział, reaktorów, laserów i maszyn.

## Widoczne nazwy 4 roślin

| Dawna rola projektowa | Nazwa w grze | Mechanika |
|---|---|---|
| Pulsator | Pędostrzał | 22 dmg / 1 s / 3.2 RU |
| Generator | Kwiat Esencji | +20 Esencji / 20 s |
| Bastion | Dębowy Strażnik | korzenie zatrzymujące wroga |
| Kriodziało | Mroźny Pąk | 18 dmg + 35% slow |

Wewnętrzne role zachowano wyłącznie dla ciągłości balansu; gracz widzi nazwy fantasy.

## Wrogowie

1. Chochlik — podstawowy.
2. Wichrowy Skrzat — szybki.
3. Ogr Zarośli — wolny tank.
4. Żelazny Goblin — 15% odporności na obrażenia fizyczne.
5. Boss: Król Cierni — 2200 HP, 20% odporności fizycznej.

## MAP-A

- 24 × 14 RU
- 46.211 RU trasy
- 12 stałych miejsc do sadzenia
- brak free-placementu
- dotknięcie grządki otwiera radialne menu 4 roślin
- 6 fal
- boss w 6. fali
- baza: 20 HP
- start: 300 Esencji

## Android / mobilny UX

- automatyczne landscape left/right
- safe-area dla ekranów z wycięciami
- UI 1920×1080 z CanvasScaler
- pełna mapa dopasowywana do proporcji ekranu
- sterowanie dotykiem
- duże przyciski
- radialne menu budowy
- osobny przycisk rozpoczęcia fali
- 60 FPS target

## Otwieranie

Zalecane: **Unity 6 LTS**.

1. Rozpakuj ZIP.
2. Unity Hub → Add project from disk.
3. Wskaż folder `FantasyTD_MAP_A_Android_v2`.
4. Poczekaj na import.
5. `Tools → Fantasy TD → Open MAP-A Android v2`.
6. `Tools → Fantasy TD → Configure Android`.
7. Play.

## Budowa APK

Musisz mieć zainstalowany w Unity Hub moduł:
- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

Potem:

`Tools → Fantasy TD → Build Android APK`

Plik powinien powstać jako:

`Builds/Android/FantasyTD_MAP_A_v2.apk`

## Co jest już grywalne

- 12 grządek
- dotykowe wybieranie grządki
- radialne menu
- 4 rośliny
- ekonomia Esencji
- pociski
- obrażenia
- slow
- root
- 4 typy przeciwników
- boss
- 6 fal
- startowanie fal przyciskiem
- wygrana / przegrana / restart
- dopasowanie kamery do telefonu
- placeholdery roślin z łodygami, liśćmi, płatkami i oczami
- fantastyczne stworki zamiast obiektów sci-fi

## Ważne

Projekt został wygenerowany i sprawdzony statycznie, ale w tym środowisku nie ma Unity Editor.
Dlatego pierwsze otwarcie w Unity jest właściwym testem kompilacji i Play Mode.
