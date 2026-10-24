# Unity Personal + GameCI v4 — Forest01 Android builds

Applies to: `.github/workflows/ft2-android-apk-hosted.yml` (Unity 6000.0.26f1, GameCI v4).

## Current status (24 October 2026)
- Source-contract tests do not require a Unity license.
- The previously attempted hosted Android run [39938160490](https://github.com/SonGohan231/FantasyTD/actions/runs/39938160490) failed in the *license preflight*, before Unity import, Play Mode, render, or Android build.
- The Chromebook's signed-in Unity CLI reports Unity Personal, **but that OAuth entitlement is not a GameCI `UNITY_LICENSE` file**.
- At the time of inspection, this GitHub repository's Actions secrets did not include `UNITY_LICENSE`, `UNITY_EMAIL`, or `UNITY_PASSWORD`.
- No production APK or device test is proven by the source tests.

## Required configuration — Unity Personal only

Follow [GameCI v4: Activation](https://game.ci/docs/github/activation/#personal-license).

1. On a computer where the **Unity Hub can sign in and activate Personal**, open **Unity Hub > Preferences > Licenses > Add > Get a free personal license**.
2. Locate the resulting `Unity_lic.ulf`. Typical locations:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
3. In this repository open [Settings > Secrets and variables > Actions](https://github.com/SonGohan231/FantasyTD/settings/secrets/actions). Create **repository secrets**, not ordinary variables:
   - `UNITY_LICENSE` = **entire content** of the real `.ulf` file.
   - `UNITY_EMAIL` = email for the same Unity account.
   - `UNITY_PASSWORD` = password for that Unity account.
4. **Do not set `UNITY_SERIAL` for Unity Personal.** It is intended for a different licensing route; the Forest01 workflow now explicitly checks the three Personal secrets instead of silently accepting email/password without a license file.
5. After the secrets exist, run the hosted workflow on a **trusted branch** containing the workflow (after an approved integration), and inspect **all** real Unity steps before claiming an APK exists. The job uploads logs/artifacts even on failure.

## Security and caveats
- **Never** commit a `.ulf` or a password into Git, paste them into chat, add them to a PR, or print them into logs.
- A local Unity CLI OAuth session / access token is not a substitute for GameCI license data. Do not export or reuse cached OAuth credentials as GitHub secrets.
- Do not use obsolete instructions to force the deprecated manual Personal `.alf` activation on Unity's website.
- GitHub Actions secret names may be listed for diagnostics, but their values are intentionally unreadable after creation.
- Unity Personal licenses may need renewal. A passing secret-presence check proves only the secrets **exist**; activation itself is validated later by GameCI/Unity.
- In this Chromebook environment, earlier inspection found no `.ulf`. Repeating the local CLI sign-in does not create one.
- Avoid running Unity builds on untrusted fork PR code with privileged Actions secrets.

## Verification criteria
- Source contracts PASS on the selected commit.
- GameCI activates a valid Unity license without printing secret contents.
- Unity 6000.0.26f1 imports and compiles the project.
- Play Mode report has `status=PASS` and `playModeEntered=True`, and a non-empty rendered PNG exists.
- A non-empty `FantasyTD_FT2_Forest01_Debug.apk` and SHA-256 are uploaded as artifacts.
- Physical Android installation and touch interaction require a separate device test.

This setup does not alter game code or merge any feature branch.
