# Ashen Petrel ISA Guide

This document details the implemented instruction set for the Ashen Petrel HP 3000 emulator.

## Instruction Format

The HP 3000 uses 16-bit words for instructions. Format 2 instructions pack two 6-bit opcodes per word, executed sequentially.

## Fully Implemented Instructions

| Mnemonic | Opcode | Description |
|----------|--------|-------------|
| `NOP` | 000000 | No operation. Does nothing. |
| `DELB` | 000001 | Delete B. Placeholder, no stack effect. |
| `DDEL` | 000002 | Double delete. Pops two values from stack. |
| `ZERO` | 000006 | Push a zero onto the stack. |
| `DZRO` | 000007 | Push two zeros onto the stack. |
| `ADD` | 000020 | Add: b + a → result. |
| `SUB` | 000021 | Subtract: b - a → result. |
| `MPY` | 000022 | Multiply: b * a → result. |
| `DIV` | 000023 | Divide: b / a → result (zero if a=0). |
| `NEG` | 000024 | Negate top of stack. |
| `XCH` | 000032 | Exchange top two stack items. |
| `INCA` | 000033 | Increment A (top of stack). |
| `DECA` | 000034 | Decrement A (top of stack). |
| `DEL` | 000040 | Delete top of stack. |
| `DUP` | 000045 | Duplicate top of stack. |
| `DDUP` | 000046 | Duplicate top two stack items. |
| `NOT` | 000064 | Bitwise NOT of top of stack. |
| `OR` | 000065 | Bitwise OR: b \| a → result. |
| `XOR` | 000066 | Bitwise XOR: b ^ a → result. |
| `AND` | 000067 | Bitwise AND: b & a → result. |

## Branch Instructions

Branch instructions use a different format with opcode base `0xC000` (octal 140000).

**Syntax:** `BR P±offset[,I][,X]`

- **P±offset**: Program counter relative offset (octal, 0-377)
  - `P+offset`: Branch forward
  - `P-offset`: Branch backward
- **,I**: Indirect addressing (use memory value as target)
- **,X**: Indexed addressing (add X register to target)

**Examples:**
- `BR P+10` - Branch forward 8 (decimal) locations
- `BR P-5,X` - Branch backward 5 (octal), indexed by X
- `BR P+100,I` - Branch forward to address in memory

## Complete Format 2 Opcode Table

Format 2 opcodes are listed below as octal values from `000` to `077`. They are
packed two per word, and `step` executes them sequentially (first opcode, then
the second opcode). **Bold** opcodes are fully implemented.

| Octal | Mnemonic | Status | Octal | Mnemonic | Status |
|-------|----------|--------|-------|----------|--------|
| 000 | **NOP** | ✓ | 040 | **DEL** | ✓ |
| 001 | **DELB** | ✓ | 041 | ZROB | - |
| 002 | **DDEL** | ✓ | 042 | LDXB | - |
| 003 | ZROX | - | 043 | STAX | - |
| 004 | INCX | - | 044 | LDXA | - |
| 005 | DECX | - | 045 | **DUP** | ✓ |
| 006 | **ZERO** | ✓ | 046 | **DDUP** | ✓ |
| 007 | **DZRO** | ✓ | 047 | FLT | - |
| 010 | DCMP | - | 050 | FCMP | - |
| 011 | DADD | - | 051 | FADD | - |
| 012 | DSUB | - | 052 | FSUB | - |
| 013 | MPYL | - | 053 | FMPY | - |
| 014 | DIVL | - | 054 | FDIV | - |
| 015 | DNEG | - | 055 | FNEG | - |
| 016 | DXCH | - | 056 | CAB | - |
| 017 | CMP | - | 057 | LCMP | - |
| 020 | **ADD** | ✓ | 060 | LADD | - |
| 021 | **SUB** | ✓ | 061 | LSUB | - |
| 022 | **MPY** | ✓ | 062 | LMPY | - |
| 023 | **DIV** | ✓ | 063 | LDIV | - |
| 024 | **NEG** | ✓ | 064 | **NOT** | ✓ |
| 025 | TEST | - | 065 | **OR** | ✓ |
| 026 | STBX | - | 066 | **XOR** | ✓ |
| 027 | DTST | - | 067 | **AND** | ✓ |
| 030 | DFLT | - | 070 | FIXR | - |
| 031 | BTST | - | 071 | FIXT | - |
| 032 | **XCH** | ✓ | 072 | UNK | - |
| 033 | **INCA** | ✓ | 073 | INCB | - |
| 034 | **DECA** | ✓ | 074 | DECB | - |
| 035 | XAX | - | 075 | XBX | - |
| 036 | ADAX | - | 076 | ADBX | - |
| 037 | ADXA | - | 077 | ADXB | - |
