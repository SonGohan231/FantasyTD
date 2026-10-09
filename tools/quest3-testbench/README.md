# Quest 3 VR Test Bench (Windows 10/11)

**Status: MVP; hardware verification pending.** Portable desktop launcher for testing your own Quest 3 standalone APKs and Windows PCVR games. The two modes are deliberately separate.

## What it can do
- **Quest Native (APK):** ADB USB device discovery, statuses \`device\`/\`unauthorized\`/\`offline\`, APK installation, launch/stop of an Android package, logcat capture, screenshot, 15-second screenrecord, JSON + ZIP diagnostic report.
- **PCVR:** detect the Windows OpenXR active runtime and launch a user-selected PCVR EXE after you manually establish Meta Horizon Link.
- **Offline-first:** no account, telemetry, uploads, or modifications to your game files.
- **Windows package:** GitHub Actions tests source with mocked ADB, packages a portable EXE using PyInstaller, and publishes SHA256SUMS. This is **not** evidence that a Quest 3 has been connected to the CI machine.

## Connect Quest 3 via USB
1. Enable Developer Mode for the headset in your Meta developer account and mobile companion app.
2. Install [Android SDK Platform Tools](https://developer.android.com/tools/releases/platform-tools). Add its folder to PATH (or set ANDROID_SDK_ROOT).
3. Connect Quest 3 to Windows PC using a **data-capable USB-C cable**.
4. Put on the headset and approve **Allow USB debugging**. Re-approve if the USB debugging dialog appears after reconnection.
5. Launch Test Bench; select **Doctor**. The selected serial must read \`device\`. If \`unauthorized\`, approve debugging in the headset; if \`offline\`, reconnect cable / restart ADB.

## Run Quest Native APK
1. Select **Quest Native APK / USB**.
2. Pick a built .apk that targets Android and supports Meta Quest 3.
3. Click **Instaluj APK**.
4. Enter the **real Android application ID** (for example \`com.company.game\`; it is not the filename) and click **Uruchom grę**.
5. Wear the headset to interact with the game and controllers.
6. Use **Pobierz logi**, **Zrzut PNG**, or **Nagraj 15 s** to collect evidence; export the ZIP report.

This program cannot transform a flat Android app into a stereoscopic VR app. It also does not bypass APK entitlements or platform restrictions.

## Run PCVR / Unity
1. Install [Meta Horizon Link](https://www.meta.com/help/quest/), enable Quest Link in the headset, start Quest Link, and check the PC connection.
2. In the Meta Horizon Link desktop app set the **active OpenXR runtime** (Settings > General).
3. To test Unity directly, enable **Windows OpenXR** and Meta/Touch interaction profile in XR Plug-in Management, then press Play in the Unity Editor.
4. Or select your own PCVR EXE in the **PCVR** tab and launch it. The launcher does not replace Meta Link, SteamVR, or the OpenXR runtime.

Meta documentation: [Quest Link for App Development](https://developers.meta.com/vr/documentation/unity/unity-link/) and [Quest Developer Hub Performance Analyzer](https://developers.meta.com/vr/documentation/unity/ts-mqdh-logs-metrics/).

## Get Windows EXE
Open the GitHub Actions workflow **Quest 3 Test Bench Windows CI** for this PR/branch; after a successful run, download the \`Quest3VRTestBench-Windows-portable\` artifact. The artifact must contain \`Quest3VRTestBench.exe\` and \`SHA256SUMS.txt\`. If no Actions run exists, no Windows binary can be claimed to exist.

## Build from source / run tests

\`\`\`powershell
cd tools/quest3-testbench
python app.py
python -m unittest discover -s tests -p "test_*.py" -v
python -m pip install pyinstaller==6.16.0
pyinstaller --onefile --windowed --name Quest3VRTestBench app.py
\`\`\`

Python 3.12 includes Tkinter; no GUI pip dependencies required. Android SDK Platform Tools is separate.

## Verification and limitations
- CI/mock tests check command formation, permission states, errors, and PNG capture without using a Quest.
- **HARDWARE_NOT_VERIFIED** until a human with a physical Quest 3 supplies USB ADB connection evidence, test launch, display/controls result, and Quest Developer Hub performance logs.
- The app intentionally does **not** infer actual FPS/thermal/GPU metrics from PC telemetry. Use Meta Quest Developer Hub and OVR Metrics Tool.
- Recorded video is constrained by Android screenrecord capabilities; casting and 2D capture are not stereoscopic reconstruction.
- Screenshot and log captures may contain personal or sensitive app information; stored only on the user's local computer under \`Documents/Quest3TestBench\`.
- The current MVP lacks continuous live mirror; use Meta Quest Developer Hub casting. Add scrcpy as a separate reviewed feature.
- All program launch and ADB writes require local user actions. This is a test helper, not an automated controller/pose injector.

## QA evidence checklist
- [ ] Windows Python regression tests pass
- [ ] PyInstaller Windows EXE artifact and SHA256
- [ ] Start app without ADB/headset and confirm friendly Doctor error (no crash)
- [ ] Connect authorized Quest 3 via USB and detect it
- [ ] Install and start a purpose-built Quest VR APK, test head and controller tracking
- [ ] Export screenshot, video, logcat and ZIP
- [ ] Confirm Quest Link Unity Play Mode with controllers
- [ ] Collect actual FPS/CPU/GPU frame-time data using MQDH (hardware-only)
