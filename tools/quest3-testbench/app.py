"""Quest 3 VR Test Bench — lokalne GUI Windows (bez chmury i telemetrii)."""
from __future__ import annotations

import json
import subprocess
import threading
import tkinter as tk
import zipfile
from datetime import datetime
from pathlib import Path
from tkinter import filedialog, messagebox, scrolledtext, ttk

from adb_tools import QuestADB, QuestError, pcvr_diagnostics


class TestBench(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Quest 3 VR Test Bench")
        self.geometry("870x650")
        self.minsize(720, 560)
        self.adb: QuestADB | None = None
        self.serial = tk.StringVar()
        self.apk = tk.StringVar()
        self.package = tk.StringVar()
        self.exe = tk.StringVar()
        self.events: list[dict] = []
        self.session = Path.home() / "Documents" / "Quest3TestBench" / datetime.now().strftime("%Y%m%d_%H%M%S")
        self.session.mkdir(parents=True, exist_ok=True)
        self._build()
        self.after(100, self.doctor)

    def _build(self):
        header = ttk.Frame(self, padding=12)
        header.pack(fill="x")
        ttk.Label(header, text="Quest 3 VR Test Bench", font=("Segoe UI", 17, "bold")).pack(side="left")
        ttk.Button(header, text="Sprawdź połączenie / Doctor", command=self.doctor).pack(side="right")

        tab = ttk.Notebook(self)
        tab.pack(fill="both", expand=True, padx=12, pady=3)
        standalone = ttk.Frame(tab, padding=14)
        pcvr = ttk.Frame(tab, padding=14)
        tab.add(standalone, text="Quest Native APK / USB")
        tab.add(pcvr, text="PCVR / Meta Link / OpenXR")

        row = ttk.Frame(standalone)
        row.pack(fill="x", pady=5)
        ttk.Label(row, text="Urządzenie ADB:").pack(side="left")
        self.device_combo = ttk.Combobox(row, textvariable=self.serial, width=35, state="readonly")
        self.device_combo.pack(side="left", padx=10)
        ttk.Button(row, text="Odśwież", command=self.doctor).pack(side="left")

        row = ttk.Frame(standalone)
        row.pack(fill="x", pady=5)
        ttk.Label(row, text="Plik APK:").pack(side="left")
        ttk.Entry(row, textvariable=self.apk).pack(side="left", fill="x", expand=True, padx=10)
        ttk.Button(row, text="Wybierz APK", command=self.choose_apk).pack(side="left")
        row = ttk.Frame(standalone)
        row.pack(fill="x", pady=5)
        ttk.Label(row, text="Identyfikator pakietu:").pack(side="left")
        ttk.Entry(row, textvariable=self.package).pack(side="left", fill="x", expand=True, padx=10)

        row = ttk.Frame(standalone)
        row.pack(fill="x", pady=12)
        for label, action in [
            ("Instaluj APK", self.install),
            ("Uruchom grę", self.launch),
            ("Zatrzymaj", self.stop),
            ("Pobierz logi", self.logs),
            ("Zrzut PNG", self.screenshot),
            ("Nagraj 15 s", self.record),
        ]:
            ttk.Button(row, text=label, command=action).pack(side="left", padx=3)

        ttk.Label(standalone, wraplength=750, text=(
            "Wymagane: tryb deweloperski Quest 3, kabel USB-C do transmisji danych "
            "i potwierdzenie 'Allow USB debugging' wewnątrz gogli. "
            "Ten tryb instaluje prawdziwe Android APK, nie uruchamia gry przez Quest Link."
        )).pack(anchor="w", pady=8)

        ttk.Label(pcvr, text="Meta Horizon Link uruchamia wersję PCVR, nie natywne APK.", 
                  font=("Segoe UI", 11, "bold")).pack(anchor="w", pady=7)
        ttk.Label(pcvr, text=(
            "Na Windows otwórz Meta Horizon Link, włącz Quest Link w goglach "
            "i ustaw Meta jako aktywny runtime OpenXR. W Unity włącz OpenXR "
            "dla Windows i uruchom Play Mode."
        ), wraplength=760).pack(anchor="w", pady=7)
        row = ttk.Frame(pcvr)
        row.pack(fill="x", pady=10)
        ttk.Label(row, text="Gra PCVR EXE:").pack(side="left")
        ttk.Entry(row, textvariable=self.exe).pack(side="left", fill="x", expand=True, padx=10)
        ttk.Button(row, text="Wybierz EXE", command=self.choose_exe).pack(side="left")
        ttk.Button(pcvr, text="Uruchom wskazaną grę PCVR", command=self.launch_pcvr).pack(anchor="w")
        self.runtime_label = ttk.Label(pcvr, text="OpenXR: nie sprawdzono")
        self.runtime_label.pack(anchor="w", pady=13)

        bottom = ttk.Frame(self, padding=12)
        bottom.pack(fill="both", expand=False)
        ttk.Label(bottom, text="Dziennik sesji:").pack(anchor="w")
        self.console = scrolledtext.ScrolledText(bottom, height=11, state="disabled")
        self.console.pack(fill="both", expand=True, pady=4)
        ttk.Button(bottom, text="Eksportuj raport sesji ZIP", command=self.export_report).pack(anchor="e")

    def say(self, message: str):
        self.console.configure(state="normal")
        self.console.insert("end", f"[{datetime.now().strftime('%H:%M:%S')}] {message}\n")
        self.console.see("end")
        self.console.configure(state="disabled")

    def work(self, label: str, fn):
        def worker():
            try:
                result = fn()
                self.after(0, lambda: self._result(label, str(result)))
            except Exception as exc:
                self.after(0, lambda error=str(exc): self._result(label, "BŁĄD: " + error))
        threading.Thread(target=worker, daemon=True).start()

    def _result(self, label: str, result: str):
        self.events.append({"time": datetime.now().isoformat(), "action": label, "result": result})
        self.say(f"{label}: {result[:2000]}")

    def doctor(self):
        def check():
            status = pcvr_diagnostics()
            try:
                self.adb = QuestADB()
                devices = self.adb.devices()
                serials = [d.serial for d in devices]
                def update():
                    self.device_combo["values"] = serials
                    if serials and self.serial.get() not in serials:
                        self.serial.set(serials[0])
                self.after(0, update)
                device_text = ", ".join(f"{d.serial} ({d.state})" for d in devices) or "brak"
                adb_status = "ADB: " + str(self.adb.executable) + " | urządzenia: " + device_text
            except QuestError as exc:
                self.adb = None
                adb_status = "ADB: " + str(exc)
                self.after(0, lambda: self.device_combo.configure(values=[]))
            self.after(0, lambda: self.runtime_label.configure(
                text="OpenXR runtime: " + status["openxr_runtime"]))
            return adb_status + " | " + json.dumps(status, ensure_ascii=False)
        self.work("Doctor", check)

    def _tool(self) -> tuple[QuestADB, str]:
        if not self.adb:
            raise QuestError("ADB nie jest gotowe. Kliknij Doctor.")
        if not self.serial.get():
            raise QuestError("Nie wybrano urządzenia.")
        return self.adb, self.serial.get()

    def choose_apk(self):
        path = filedialog.askopenfilename(filetypes=[("Android APK", "*.apk")])
        if path:
            self.apk.set(path)

    def choose_exe(self):
        path = filedialog.askopenfilename(filetypes=[("Program Windows", "*.exe")])
        if path:
            self.exe.set(path)

    def install(self):
        self.work("Instalacja APK", lambda: self._tool()[0].install(self.serial.get(), self.apk.get()))

    def launch(self):
        self.work("Uruchomienie APK", lambda: self._tool()[0].launch(self.serial.get(), self.package.get()))

    def stop(self):
        self.work("Zatrzymanie APK", lambda: self._tool()[0].stop(self.serial.get(), self.package.get()))

    def logs(self):
        def capture():
            text = self._tool()[0].logcat(self.serial.get())
            destination = self.session / "logcat.txt"
            destination.write_text(text, encoding="utf-8", errors="replace")
            return destination
        self.work("Logcat", capture)

    def screenshot(self):
        self.work("Screenshot", lambda: self._tool()[0].screenshot(
            self.serial.get(), self.session / f"screen_{len(self.events)}.png"))

    def record(self):
        self.work("Screenrecord", lambda: self._tool()[0].record(
            self.serial.get(), self.session / f"video_{len(self.events)}.mp4"))

    def launch_pcvr(self):
        def start():
            exe = Path(self.exe.get())
            if not exe.is_file() or exe.suffix.lower() != ".exe":
                raise QuestError("Wskaż istniejącą grę PCVR .exe")
            process = subprocess.Popen([str(exe)], cwd=str(exe.parent), shell=False)
            return f"Uruchomiono proces PID {process.pid}. Sprawdź obraz w Quest Link."
        self.work("Start PCVR", start)

    def export_report(self):
        report = {
            "name": "Quest 3 VR Test Bench",
            "hardware_verification": "HARDWARE_NOT_VERIFIED",
            "note": "Dziennik nie stanowi automatycznej certyfikacji działania na Quest 3.",
            "device_serial": self.serial.get(),
            "events": self.events,
            "pcvr": pcvr_diagnostics(),
        }
        path = self.session / "report.json"
        path.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
        archive = self.session.parent / f"{self.session.name}_report.zip"
        with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED) as zf:
            for file in self.session.iterdir():
                if file.is_file():
                    zf.write(file, arcname=file.name)
        self.say(f"Wyeksportowano: {archive}")
        messagebox.showinfo("Raport", str(archive))


if __name__ == "__main__":
    TestBench().mainloop()
