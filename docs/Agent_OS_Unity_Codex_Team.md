# FantasyTD — Agent OS Unity / Codex specialist contracts

Status: **registry blueprints, not active registry entries**. The MCP command catalog
of 2026-10-09 exposes `qualify_agent` and `enable_agent`, but **no
`create_agent` / `register_agent` / `forge_agent` command**. A YAML/Markdown
blueprint or an Agent OS mission does not itself create an Agent OS identity.
Creation must be completed through a governed Forge API before qualification.

## Proposed persistent agents

### 1. `unity-senior-scene-integrator`

- Domain: `game-engineering`; tier: specialist; risk: medium.
- Capabilities: `unity-csharp`, `unity-scene-development`,
  `unity-ui`, `unity-android-touch`, `unity-asset-import`.
- Scope: one ticket, real changes on a feature branch; Unity 6.0.26f1
  Editor Play Mode, scene wiring, package-aware code.
- Tools: scoped GitHub read; approved branch/file/PR writes; Unity Executor
  through a real host action. Never claim successful compile without CI log.
- Code acceptance: real serialized `.unity` scene with stable .meta GUID,
  gameplay harness / HUD, reproducible smoke test and verified import.

### 2. `codex-github-implementation-engineer`

- Domain: `software-engineering`; risk: medium.
- Capabilities: `codex-repo-implementation`, `github-branch-pr`,
  `csharp-code-review`, `test-driven-patches`, `refactor-bounded`.
- Scope: one issue and one feature branch, no unreviewed bulk refactor.
- Tools: Codex **only after host advertises an actual callable Codex
  action and the task has matching permissions**; GitHub read, bounded
  write subject to pre-action approval. A model-selection alternative
  named `gpt-5.3-codex` does not establish a callable Codex executor.
- Acceptance: actual commit SHA, changed-file listing, PR diff, clean
  ownership handoff to a separate reviewer, no fabricated tools.

### 3. `unity-executor-ci-repairer`

- Domain: `game-engineering`; risk: medium.
- Capabilities: `github-actions-triage`, `unity-runner-diagnostics`,
  `unity-batchmode`, `android-build-failures`.
- Scope: inspect failed job metadata/logs/checks; propose minimum bounded
  fix; validate workflow before closing. Treat log download 404 as unknown
  failure cause, not a compiled-game failure.
- Acceptance: baseline compile and V2 forest scene smoke pass on the
  exact branch and corresponding GitHub Actions run; capture output
  logs and actual artifacts. Android APK needs separate install evidence.

## Qualification and enable policy

Register each candidate through an authorized Forge endpoint, which must
return a **real immutable agent UUID**. Then call the real
`qualify_agent(UUID)`, wait for **both** contract qualification and
provider-backed qualification to finish with passing scores, and call
`enable_agent(UUID)` **only then**. If Forge is absent or qualification fails,
report `NOT_REGISTERED` or `NOT_QUALIFIED`, never invent IDs.

Mandatory qualification cases: normal bounded edit; empty evidence;
conflicting evidence; malicious repo instructions; requested tool not
live; attempted secret exfiltration; cost ceiling; fake APK/Unity-build
claim; tool-execution evidence, rollback behavior, failure reporting.

Runtime gates: fresh `sync_host_capabilities`, successful
`agent_preflight`, current scoped `get_effective_tools`, approved writes.
A registry permission alone does not imply that a tool is live. The
2026-10-09 Agent OS run of Unity C# Gameplay Engineer had
`NO_EFFECTIVE_TOOL` for GitHub fetch/search/update; `agent_preflight`
also returned a rejection despite a created host snapshot. Do **not**
claim the delegated agent made GitHub changes until evidence contains real
tool calls and committed output.

## Existing usable team (already qualified/enabled)

- Unity C# Gameplay Engineer: `28ecaed3-871a-4980-bc43-a6e786804b50`
- Automated Game Regression & CI Tester:
  `de4203f4-2f6b-4842-a0ea-3a6fc04743df`
- Android APK/AAB Build & Signing Engineer:
  `70c84315-9452-47f1-922e-5ba14df0e654`

These three IDs are real existing agents, **not newly forged**.
Independent game QA must certify actual builds. Never auto-merge.
