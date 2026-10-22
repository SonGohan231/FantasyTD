import importlib.util
import pathlib
import subprocess
import tempfile
import unittest

SCRIPT = pathlib.Path(__file__).with_name("diff_secrets.py")
spec = importlib.util.spec_from_file_location("diff_secrets", SCRIPT)
scanner = importlib.util.module_from_spec(spec)
spec.loader.exec_module(scanner)


class DiffSecretTests(unittest.TestCase):
    def test_detects_representative_secrets_without_returning_values(self):
        github = "ghp_" + "A" * 36
        aws = "AKIA" + "B" * 16
        diff = f"+++ b/config.txt\n+token={github}\n+aws={aws}\n"
        found = list(scanner.findings(diff))
        self.assertIn(("config.txt", "provider token"), found)
        self.assertIn(("config.txt", "AWS access key"), found)
        self.assertNotIn(github, repr(found))
        self.assertNotIn(aws, repr(found))

    def test_ignores_references_placeholders_and_removed_lines(self):
        diff = "\n".join([
            "+++ b/workflow.yml",
            "+token: ${{ secrets.API_TOKEN }}",
            "+password = os.environ['APP_PASSWORD']",
            "+example = 'ghp_short_placeholder'",
            "-ghp_" + "Z" * 36,
        ])
        self.assertEqual([], list(scanner.findings(diff)))

    def test_cli_fails_without_printing_secret(self):
        secret = "github_pat_" + "C" * 30
        with tempfile.TemporaryDirectory() as directory:
            repo = pathlib.Path(directory)
            subprocess.run(["git", "init", "-q", repo], check=True)
            subprocess.run(["git", "-C", repo, "config", "user.name", "Hook Test"], check=True)
            subprocess.run(["git", "-C", repo, "config", "user.email", "hook@example.invalid"], check=True)
            (repo / "README.md").write_text("baseline\n", encoding="utf-8")
            subprocess.run(["git", "-C", repo, "add", "README.md"], check=True)
            subprocess.run(["git", "-C", repo, "commit", "-qm", "baseline"], check=True)
            base = subprocess.check_output(["git", "-C", repo, "rev-parse", "HEAD"], text=True).strip()
            (repo / "config.txt").write_text("token=" + secret + "\n", encoding="utf-8")
            subprocess.run(["git", "-C", repo, "add", "config.txt"], check=True)
            subprocess.run(["git", "-C", repo, "commit", "-qm", "fixture"], check=True)
            result = subprocess.run(
                ["python3", str(SCRIPT), "--base", base], cwd=repo,
                text=True, capture_output=True,
            )
            self.assertEqual(1, result.returncode)
            self.assertIn("SECRET_SCAN_FAIL config.txt: provider token", result.stdout)
            self.assertNotIn(secret, result.stdout + result.stderr)


if __name__ == "__main__":
    unittest.main(verbosity=2)
