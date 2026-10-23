# FT2-UNIFY-001 — jeden aktywny rejestr integracji FantasyTD

Data weryfikacji: 2026-10-23. Zadanie GitHub: https://github.com/SonGohan231/FantasyTD/issues/36.

## Wersja referencyjna i reguła zakresu

- Jedyny aktywny cel: dopracowana, grywalna mapa Forest01 (28 × 20, 10 stanowisk, 6 fal), build APK na Android ARM64 i fizyczny test.
- Wybrana baza: commit `c8cfb7a` z `codex/forest01-integrated-20261022`, oddzielny worktree: `~/Repos/FTD/FantasyTD-Unified-20261023` / `codex/forest01-unified-20261023`.
- Zatwierdzone źródła geometrii: `docs/FT2_Forest01_28x20_Layout.md` i `docs/FT2_Forest01_Approved_Production_Spec.md`; źródło mechaniki: `UnityProject/Assets/FantasyTD/V2`.
- Nie twórz kolejnych etapów, nie mieszaj 12-stanowiskowego legacy MAP1 z 10-stanowiskowym Forest01; nie scalać do `main` bez realnego APK, renderu i oceny.

## Inwentaryzacja dotychczasowych gałęzi i przejętych wyników

| Źródło | Co wzięto / sprawdzono | Status |
|---|---|---|
| `codex/forest01-integrated-20261022` (`c8cfb7a`) | Aktualne 10 stanowisk, cztery rośliny, 6 fal, UI, ulepszanie, grafiki, Unity workflow | wybrana baza |
| `origin/codex/forest01-integrated-20261022` (`774fb9c`) | Historyczne poprawki synchronizacji i GUID; osobna, rozbieżna historia; NIE nadpisywać `c8cfb7a` | zachowana jako referencja |
| `origin/codex/ft2-forest01-isometric-terrain-20261022` (`155b95c`) | Layout 28×20 i źródło geometrii; blob layoutu identyczny z bazą | objęte |
| `origin/codex/ft2-qa-003-art-import-repair-20261022` (`8110d57`) | Oryginalna grafika Forest01 i jednostki; zweryfikowane identyczne blob SHA wybranych PNG | objęte |
| `codex/pr34-latest` (`ad66fd9`) | Selektywne przeniesienie walidatora sekretów, metadanych i jakościowych GitHub workflows; bez starych zmian gameplay | przeniesione |
| `codex/terr-01-local-unity-render` (`604200f`) | Historia prób renderowania, mapa i osobny worktree | zachowane, bez automerge |
| `codex/t0013-map1-production-polish` (`91a625a`) | Historyczny MAP1 12 stanowisk, nie jest Forest01 | archiwalna referencja |
| `codex/pr34-audit` i `codex/pr34-latest` | Starsze gałęzie QA i workflow | zachowane, bez kasowania |
| `main` | Brak modyfikacji, brak merge | bez zmian |

Związane wątki GitHub: #26 (vertical slice), #29 (grafika), #32 (upgrade), PR #28/#31 (asset/import), #33 (teren), #34 (gates). Jeden aktualny ticket odbiorowy: #36.

## Naprawa poziomu (kontrakt)

- `ForestBattlefield.Awake()` zawsze zaczyna od `_currentStage = 1`, a `CampaignSaveKey` oznacza **najwyższy odblokowany**, nie bieżący ekran. Nie następuje `SetInt` ani reset zapisu przy starcie.
- `RecordVictoryIfNeeded()` zwiększa zapisany maksymalny etap tylko po wygranej i wywołuje `PlayerPrefs.Save()`.
- `ChangeStage()` nadal czyta zapisaną granicę odblokowania i pozwala wybrać istniejący etap.
- Test RED z 23.10 (błędny odczyt zapisu przy starcie) -> GREEN (14/14 testów statycznych w izolowanym worktree). Trzeba wykonać realny cold restart na Androidzie.

## Wyniki testów i braki

- Forest01: 14/14 źródłowych testów Python PASS (bez Unity).
- Hook/CI guards: 12/12 testów Python PASS w odizolowanym środowisku z Pillow==11.3.0.
- Meta integrity: PASS (94 istniejące tracked .meta, bez nowych zmian asset path w chwili sprawdzenia).
- Poprzedni Unity Play Mode: archiwalny PASS, ale ostatni poprzedni log `forest01-playmode-integration.txt` z 22.10 ma FAIL; nie przenosić dowodu z innego commita.
- Brak prawdziwego renderu PNG z bieżącej gałęzi, brak zatwierdzonego APK i testu urządzenia.
- Chromebook ARM64 uruchamia Unity/FEX, lecz lokalna instalacja nie ma Android Build Support; hosting x86_64 wymaga licencji i zatwierdzonego CI.

## Bezpieczny sposób kontynuacji

1. Buduj tylko w `FantasyTD-Unified-20261023`; inne worktree / `ssss.txt desktop` pozostaw bez zmian.
2. Mierz wyniki na SHA nowego brancha, osobno testy statyczne, kompilację Unity, Play Mode, realny render, APK i telefon.
3. Nie zmieniaj balansu, assetów i kolejnych poziomów bez bramki QA oraz zatwierdzenia kierunku artystycznego.
4. Nie używaj `git reset --hard`, force-push ani czyszczenia cudzych plików.
5. Cała dalsza realizacja i artefakty przez https://github.com/SonGohan231/FantasyTD/issues/36.
