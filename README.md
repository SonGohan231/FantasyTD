# Fantasy TD

Repozytorium projektu mobilnej gry Tower Defense w Unity.

## Aktualny cel

Nie budujemy jeszcze całej gry. Aktualnym milestone jest:

**MAP-A Android v2 — działający, zweryfikowany vertical slice.**

Najpierw ma działać:
- Unity compile,
- MAP-A,
- 12 stanowisk sadzenia,
- radialne menu dotykowe,
- 4 rośliny,
- 4 typy przeciwników,
- 1 boss,
- 6 fal,
- zwycięstwo/porażka/restart,
- Android landscape,
- build APK.

Dopiero po stabilizacji MAP-A przechodzimy do kolejnych map i większego rosteru.

## Struktura

```text
FantasyTD/
├── AGENTS.md
├── README.md
├── docs/
│   ├── Full_Design_Document.md
│   ├── MVP_Technical_Design.md
│   ├── Tickets.md
│   ├── Repo_Current_State.md
│   ├── Manual_Verification_Guide.md
│   ├── Known_Issues_And_Followups.md
│   ├── Codex_Prompt_Playbook.md
│   ├── Codex_Ticket_Handoff_Template.md
│   └── Prompt_Context_Pack.md
├── ProjectData/
│   └── Tower_Defense_Design_Balance_v1_0.xlsx
└── UnityProject/
    ├── Assets/
    ├── Packages/
    └── ProjectSettings/
```

## Jak zaczynać pracę

Agent powinien przeczytać w tej kolejności:

1. `AGENTS.md`
2. `docs/Repo_Current_State.md`
3. `docs/Tickets.md`
4. pliki projektowe związane z aktywnym ticketem
5. pozostałą dokumentację tylko wtedy, gdy ticket jej potrzebuje

Nie należy za każdym razem wczytywać całego Full Design Document do prostego zadania.

## Role

- **ChatGPT** — project manager, game design, kolejność ticketów.
- **Dyrygent** — reviewer/architect/debug/QA advisor.
- **Codex** — implementacja w repo i uruchamianie narzędzi.
- **Unity/Android tooling** — źródło prawdy dla compile, Play Mode i buildów.
- **Użytkownik** — właściciel produktu i ostateczna akceptacja.

## Unity

Projekt Unity znajduje się w:

`UnityProject/`

Aktualnie deklarowana wersja:

`6000.0.26f1`

Menu edytora przewidziane w projekcie:
- `Tools → Fantasy TD → Open MAP-A Android v2`
- `Tools → Fantasy TD → Validate MAP-A`
- `Tools → Fantasy TD → Configure Android`
- `Tools → Fantasy TD → Build Android APK`

## Ważne

Aktualny projekt został przygotowany poza aktywnym Unity Editor, dlatego część jego zachowania nadal wymaga realnej walidacji w Unity.

Nie traktuj deklaracji w dokumentacji jako dowodu, że Play Mode lub APK już działa.


## Dyrygent Unity Executor

Automatyczna walidacja Unity działa przez GitHub Actions w `.github/workflows/unity-executor.yml`. Szczegóły: `docs/Dyrygent_Unity_Executor.md`.
