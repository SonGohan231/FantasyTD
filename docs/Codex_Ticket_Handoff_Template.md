# Codex Ticket Handoff Template

Copy this block for a new ticket.

```md
# TXXXX — [Short Title]

Status: ACTIVE

## Goal

[One measurable outcome.]

## Dependencies

- [TXXXX or None]

## Allowed areas

- [files/folders/systems that may be changed]

## Do not touch

- [files/folders/systems explicitly outside scope]

## Requirements

1. [...]
2. [...]
3. [...]

## Non-goals

- [...]
- [...]

## Acceptance criteria

- [ ] [...]
- [ ] [...]
- [ ] [...]

## Manual verification

1. [...]
2. [...]
3. [...]

## Evidence required

- [Unity Console / Play Mode / Build Report / file existence / device run]

## Completion report

Return:
- Ticket ID
- Status: PASS / PASS WITH ISSUES / FAIL / BLOCKED
- Summary of changes
- Files changed
- Commands/actions run
- Compile result
- Tests actually run
- Manual verification result
- Evidence
- Risks
- New issues
- Documentation updated
- Recommended next ticket
```

## Ticket sizing guidance

A good ticket:
- changes one coherent system,
- has explicit boundaries,
- can be manually verified in one focused session,
- does not require guessing future architecture.

A ticket is probably too large if it simultaneously:
- changes gameplay,
- changes UI,
- changes build configuration,
- adds packages,
- changes balance,
- adds multiple unrelated features.

If a ticket grows during implementation, stop and report the newly discovered dependency rather than silently absorbing it.
