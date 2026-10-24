# Forest01 — Android SDK fallback, bez Unity
Ten osobny port natywny w Java/Canvas korzysta z zatwierdzonych grafik z UnityProject/Assets/Resources/FT2 oraz kontraktu mapy i balansu StarterLevel.cs / BattleSimulator.cs (commit 02f1507).

Jeden poziom Forest01: 10 stanowisk, 4 typy obrońców, 6 fal, 5 typów przeciwników (w tym boss), esencja, dochód, spowolnienie, ulepszanie wież do LV3, fala, pauza, restart, wygrana/przegrana. Android SDK 35, min SDK 23; orientacja landscape. Gra to **nowa implementacja Android Canvas**, nie kompilacja Unity ani gwarancja identycznego zachowania i grafiki sceny Unity.

Workflow: .github/workflows/forest01-native-android.yml. Artefakt: Forest01-Android-SDK. Skrypt CI kopiuje oryginalne grafiki do assets w czasie buildu; plików PNG nie duplikujemy w repo. Nie wymaga żadnych danych licencyjnych Unity.

Testy: CI kompiluje aplikację, sprawdza obecność wszystkich 12 bitmap w APK i wylicza SHA256. Wymagane dalsze testy interakcji dotyku na fizycznym Androidzie; nie są zastępowane buildem.
