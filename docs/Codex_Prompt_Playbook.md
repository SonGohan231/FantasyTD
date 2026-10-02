# Codex Prompt Playbook

Use short prompts that point to persistent repository documents.

Do not restate the whole project in every Codex request.

---

# 1. Start an active ticket

```text
We are working on Fantasy TD.

Before making changes, read:
- AGENTS.md
- docs/Repo_Current_State.md
- docs/Tickets.md
- docs/Known_Issues_And_Followups.md

Implement the ACTIVE ticket only.

Do not implement planned/future tickets.
Do not refactor unrelated systems.
Do not widen scope.

Follow the ticket's:
- Allowed areas
- Do not touch
- Acceptance criteria
- Manual verification

At the end:
1. return the required completion report,
2. update docs/Repo_Current_State.md,
3. update docs/Known_Issues_And_Followups.md if needed.

Do not claim Unity/Play Mode/build/device validation unless actually performed.
```

---

# 2. Planning-only pass

```text
Read AGENTS.md, docs/Repo_Current_State.md and the relevant design document.

Do not edit code.

Review the proposed next ticket for:
- scope size,
- dependencies,
- allowed areas,
- do-not-touch boundaries,
- acceptance criteria,
- manual verification.

Return a revised ticket only.
Keep it small enough to verify in one focused Unity session.
```

---

# 3. Debug a failed ticket

```text
Do not broaden scope.

Active ticket:
[TICKET ID]

Observed failure:
[PASTE ERROR / BUILD LOG / REPRO STEPS]

First:
- identify the most likely root cause from evidence,
- distinguish confirmed facts from hypotheses,
- propose the smallest diagnostic step.

Then make the smallest change required to fix the active ticket.
Do not refactor unrelated systems.

Re-run only the validation necessary to prove the fix.
Report exact evidence.
```

---

# 4. Astra review of a Codex completion report

Recommended Astra preset:
- normal review: `debug_qa`, balanced
- architecture-heavy review: `game_2d`, balanced
- difficult multi-system failure: `debug_qa`, max

Prompt:

```text
Review the following Codex completion report for Fantasy TD.

Repository rules:
- one ticket at a time,
- no unrelated refactors,
- MAP-A is the current milestone,
- Android landscape,
- organic fantasy, no sci-fi,
- unrun checks must not be counted as passed.

Current ticket:
[PASTE ACTIVE TICKET]

Repo current state:
[PASTE RELEVANT CURRENT STATE]

Codex completion report:
[PASTE REPORT]

Evaluate:
1. Did the ticket meet every acceptance criterion?
2. Which claims have actual evidence?
3. Which checks are missing?
4. Are there likely regressions or scope violations?
5. What belongs in Known_Issues_And_Followups?
6. Status: PASS / PASS WITH ISSUES / FAIL / BLOCKED.
7. What is the smallest safe next ticket?

Return:
- review status,
- evidence assessment,
- missing verification,
- risks,
- documentation updates,
- full proposed next ticket using the repository ticket template.

Do not write implementation code.
```

Astra's output is advisory. ChatGPT/user approves the next ticket.

---

# 5. Ask Codex to only inspect

```text
Read-only task.

Inspect:
[AREA]

Do not modify files, packages, scenes, or Project Settings.

Return:
- evidence found,
- confirmed facts,
- likely risks,
- unknowns,
- recommended next diagnostic action.
```

---

# 6. Android build ticket

```text
Implement/execute only the ACTIVE Android build ticket.

Do not change gameplay or balance.

Before build:
- record compile state,
- verify scenes included,
- verify Android toolchain availability.

Build using the project's defined build path.

After build:
- report Build Report result,
- confirm whether the APK physically exists,
- report exact path and size.

Do not claim device success unless the APK was actually installed and launched.
```

---

# 7. Completion-report quality check

```text
Before finalizing your report, verify that every PASS statement has evidence.

List anything not run as NOT RUN or BLOCKED.

Do not use phrases such as "should work" as test evidence.
```
