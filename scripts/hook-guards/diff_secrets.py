#!/usr/bin/env python3
"""Review added lines only. Print filename/category, never credential values."""
import argparse,re,subprocess,sys
PATTERNS=[
("private key",re.compile(r"-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----")),
("provider token",re.compile(r"(?:sk-proj-|ghp_|gho_|ghu_|github_pat_|xox[baprs]-)[A-Za-z0-9_-]{16,}")),
("hardcoded credential",re.compile(r"(?i)\b(?:password|passwd|api_key|auth_token|secret_key)\b\s*[:=]\s*['\"][^'\"]{12,}['\"]"))]
def findings(diff):
    name=""
    for line in diff.splitlines():
        if line.startswith("+++ b/"):name=line[6:]
        elif line.startswith("+") and not line.startswith("+++"):
            for label,pattern in PATTERNS:
                if pattern.search(line[1:]):yield name,label
def main():
    p=argparse.ArgumentParser();p.add_argument("--base",required=True);base=p.parse_args().base
    try:diff=subprocess.check_output(["git","diff","--no-ext-diff","--unified=0","--diff-filter=ACMR",base,"HEAD","--"],text=True)
    except (OSError,subprocess.CalledProcessError):
        print("SECRET_SCAN_NOT_VERIFIED: unable to inspect diff");return 2
    found=set(findings(diff))
    for file,label in sorted(found):print(f"SECRET_SCAN_FAIL {file}: {label}")
    if found:return 1
    print("SECRET_SCAN_PASS: heuristic only, not an exhaustive secret audit");return 0
if __name__=="__main__":sys.exit(main())
