# Status Register Guide

This guide summarizes the Ashen HP3000 status register layout and how the
current emulator updates it.

## Bit Layout

- `M` (0o100000): mode flag
- `I` (0o040000): interrupt flag
- `T` (0o020000): trap flag
- `R` (0o010000): right-hand stack op flag
- `O` (0o004000): overflow flag
- `C` (0o002000): carry flag

Condition codes (CC) are encoded in bits 0o001400:

- `CCG` (0o000000): greater than
- `CCL` (0o000400): less than
- `CCE` (0o001000): equal to
- `CCI` (0o001400): invalid

## Display Format

The `regs` command prints STA as:

```
STA: M i t r o c CCG 000
```

Each flag letter is uppercase when set and lowercase when clear. The CC label
is one of `CCG`, `CCL`, `CCE`, or `CCI`. The trailing value is the 3-digit
octal status value.

## ADD/SUB Test Cases

These test cases document expected STA behavior for ADD and SUB:

| A (oct) | B (oct) | Op | Result (oct) | STA (c/o/CC) |
|---------|---------|----|--------------|--------------|
| `077777` | `000001` | ADD | `100000` | c O CCL |
| `177777` | `000001` | ADD | `000000` | C o CCE |
| `000000` | `000001` | SUB | `177777` | C o CCL |

## Indicator Updates (Current)

These operations currently update STA:

- `ADD`/`SUB`: sets CC (CCA), Carry, and Overflow.
- `DIV`: sets CC based on quotient; sets Overflow on divide-by-zero.
- `NOT`: sets CC based on result.
- `TEST`: sets CC based on TOS.

Other instructions leave STA unchanged unless specified.
