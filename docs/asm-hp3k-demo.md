# HP 3000 Assembler Script

*2026-02-15T21:05:32Z by Showboat 0.5.0*

This walkthrough shows how to run the HP 3000 assembler script and inspect its outputs. The assembler reads a .asm file and writes <basename>.srec (S-record bytes) and <basename>.list (assembly listing) in the working directory.

```bash
python3 scripts/asm_hp3k.py sd/pfind.asm
```

```output
```

```bash
python3 - <<'PY'
from pathlib import Path
print("pfind.srec (first 5 lines):")
print("\n".join(Path("pfind.srec").read_text().splitlines()[:5]))
print("\npfind.list (first 8 lines):")
print("\n".join(Path("pfind.list").read_text().splitlines()[:8]))
PY
```

```output
pfind.srec (first 5 lines):
S1130100220252FF000653002300407B487F4078C0
S1130110003353005B01000400242204001100207A
S1130120C26B406F00150020C342C0282300486AF8
S113013000150020C282C014000448640015002089
S1130140C282C00E0004485E00150020C282C008AE

pfind.list (first 8 lines):
;; Prime factor finder for 64-bit N.
;; Uses 16-bit trial divisors and prints factors in decimal via WIO 1.
;; Set the line printer radix to decimal before running: lptradix A

000200      ORG 200
START:
000200 021002      LDI 2
000201 051377      STOR D
```

Run the assembler from the repository root (ashen-petrel) so pfind.srec and pfind.list land alongside the script invocation.
