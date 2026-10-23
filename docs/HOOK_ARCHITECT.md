# Hook Architect operating contract

Updated: 2026-10-22. This document describes the eight existing controls; it
does not authorize production deployment, PR merging, branch-rule changes, paid
missions, `dyrygent-unity`, or AppDeploy preview.

| Control | Trigger | Maximum | Gate | PASS | FAIL / NOT VERIFIED |
| --- | --- | ---: | --- | --- | --- |
| Context Recovery | Codex `SessionStart`: startup, resume, clear, compact, fork | 10 s | context | Current repo root, branch, full HEAD and bounded redacted `STATE.md` are returned | Non-Git or unreadable state is omitted; it never overwrites files |
| Definition of Done Guard | Codex `Stop` | 3 s | block once | A fresh Regression Test Gate receipt matches session, root, HEAD and worktree | Missing/stale/failed receipt causes one continuation; explicit NOT VERIFIED is allowed |
| Dangerous Command Guard | Codex `PreToolUse` for Bash | 3 s | block | Ordinary read/build/test commands continue | Destructive Git, recursive deletion, device/database destruction, force/delete push and remote-code piping are denied |
| Secret Leak Detector | FantasyTD and AgentOS pull-request workflows | 12 min / 5 min job | CI fail | Heuristic added-line scan and pinned Gitleaks scan report no finding | Finding fails the job; scanner output contains path/category, never the value |
| Regression Test Gate | Codex `PostToolUse` for Bash plus FantasyTD PR quality workflow | 3 s locally / 12 min CI | evidence + CI fail | Actual exit code 0 is stored as non-secret metadata; project tests pass in CI | Nonzero is recorded as FAIL; missing exit status is NOT VERIFIED |
| Handoff Integrity Checker | AgentOS pull-request workflow; hardened in draft PR #356 | 5 min | CI fail | Structural anchors and existing behavioral assertions bind intake, brief digest, mission and run payload | Missing binding or inconsistent actor, goal, revision or selection fails deterministic tests |
| Unity Gameplay Test Runner | Manual `workflow_dispatch` | 40 min | manual CI fail | Licensed Unity 6000.0.26f1 executes `Forest01Smoke.RunSmoke` and preserves logs | Missing license fails preflight and records NOT VERIFIED; never runs on every edit |
| Visual Regression Reviewer | Successful real-render `workflow_run` or manual run ID | 8 min | advisory | A real Unity PNG and approved actual-render baseline are dimension-compatible and within threshold | Missing render is NOT VERIFIED; missing baseline or changed pixels is REVIEW REQUIRED |

## False positives and cost controls

- The dangerous-command policy is intentionally conservative. A denied command
  must be split into a reviewed, non-destructive alternative; the hook does not
  grant approval or bypass the sandbox.
- The lightweight secret scanner can miss unknown formats and can match realistic
  examples. Tests cover environment references and short placeholders. Gitleaks
  v3 is the defense-in-depth scanner; comments and SARIF upload are disabled to
  avoid copying detected values into additional channels.
- Definition of Done only evaluates structured completion reports beginning with
  `CHANGED`, `IMPLEMENTED`, `WYKONANO` or `ZMIENIONO`. It accepts explicit NOT
  VERIFIED reporting and uses `stop_hook_active` to prevent continuation loops.
- Unity and image jobs are not per-edit hooks. Unity is manual; visual review
  follows only a successful real-render workflow or an explicit run ID.
- Pixel distance is not artistic approval. Forest01 quality remains a human
  product decision based on real Unity renders and gameplay/device evidence.

## Activation and rollback

Global Codex definitions live in `~/.codex/hooks.json`; scripts and tests live in
`~/.codex/hook-architect/`. Review/trust or disable them with `/hooks`. Disable
all lifecycle hooks temporarily with `[features] hooks = false` in
`~/.codex/config.toml`; restore by removing that override and reviewing `/hooks`.
The pre-completion snapshot and exact restoration notes are under
`~/.codex/hook-architect/backups/20261022-before-completion/`.

FantasyTD controls live in:

- `.github/workflows/ft2-hook-quality-gate.yml`
- `.github/workflows/ft2-real-unity-gameplay-manual.yml`
- `.github/workflows/ft2-visual-regression-review.yml`
- `scripts/hook-guards/`

Revert only the Hook Architect commit on the PR branch to restore the preceding
workflow versions. A workflow is a merge gate only if its exact job name is
configured as a required status check; branch rules are intentionally unchanged.
Prepared stable required-check candidates are:

- FantasyTD: `Geometry, core gameplay and secret checks (no Unity Editor)` and
  `Validate Unity GUID and tracked metadata`.
- AgentOS: `Secret and handoff structural guards`,
  `Reject stale approvals, changed budget and unselected missions`, and the
  existing `build + integration + MCP`.
