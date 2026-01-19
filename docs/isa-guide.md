# Ashen Petrel ISA Guide

This document details the implemented instruction set for the Ashen Petrel HP 3000 emulator.

## Instruction Format

The HP 3000 uses 16-bit words for instructions. Format 2 instructions pack two 6-bit opcodes per word, executed sequentially.

## Fully Implemented Instructions

| Mnemonic | Opcode | Description |
|----------|--------|-------------|
| `NOP` | 000000 | No operation. Does nothing. |
| `DELB` | 000001 | Delete B. Drops the second stack word. |
| `DDEL` | 000002 | Double delete. Pops two values from stack. |
| `ZROX` | 000003 | Zero X register. |
| `DCMP` | 000010 | Double compare: (D,C) vs (B,A) sets CC and pops both. |
| `DADD` | 000011 | Double add: (D,C) + (B,A) → (B,A). |
| `INCX` | 000004 | Increment X register. |
| `DECX` | 000005 | Decrement X register. |
| `ZERO` | 000006 | Push a zero onto the stack. |
| `DZRO` | 000007 | Push two zeros onto the stack. |
| `ADD` | 000020 | Add: b + a → result. |
| `SUB` | 000021 | Subtract: b - a → result. |
| `MPY` | 000022 | Multiply: b * a → result. |
| `LMPY` | 000062 | Logical multiply: b * a → (high, low), low on TOS. |
| `DIV` | 000023 | Divide: b / a → quotient, remainder on TOS. |
| `LDIV` | 000063 | Long divide: (C,B) / A → quotient, remainder on TOS. |
| `DIVL` | 000014 | Long divide: (C,B) / A → quotient, remainder on TOS. |
| `NEG` | 000024 | Negate top of stack. |
| `TEST` | 000025 | Set CC based on top of stack. |
| `XCH` | 000032 | Exchange top two stack items. |
| `INCA` | 000033 | Increment A (top of stack). |
| `DECA` | 000034 | Decrement A (top of stack). |
| `XAX` | 000035 | Exchange A and X. |
| `ADAX` | 000036 | Add A + X → X, update CC, pop A. |
| `ADXA` | 000037 | Add A + X → A, update CC (no stack change). |
| `DEL` | 000040 | Delete top of stack. |
| `ZROB` | 000041 | Zero B (second stack word). |
| `STBX` | 000026 | Copy RB into X (no stack change). |
| `LDXB` | 000042 | Copy X into RB (no stack change). |
| `STAX` | 000043 | Pop into X. |
| `LDXA` | 000044 | Push X onto stack. |
| `DUP` | 000045 | Duplicate top of stack. |
| `DDUP` | 000046 | Duplicate top two stack items. |
| `NOT` | 000064 | Bitwise NOT of top of stack. |
| `OR` | 000065 | Bitwise OR: b \| a → result. |
| `XOR` | 000066 | Bitwise XOR: b ^ a → result. |
| `AND` | 000067 | Bitwise AND: b & a → result. |
| `INCB` | 000073 | Increment B (second stack word). |
| `DECB` | 000074 | Decrement B (second stack word). |
| `XBX` | 000075 | Exchange B and X. |
| `ADBX` | 000076 | Add B + X → X, update CC (no stack change). |
| `ADXB` | 000077 | Add B + X → B, update CC. |

## Branch Instructions

Branch instructions use a different format with opcode base `0xC000` (octal 140000).

**Syntax:** `BR .±offset[,I][,X]`

- **.±offset**: Program counter relative offset (octal, 0-377)
  - `.+offset`: Branch forward
  - `.-offset`: Branch backward
- **,I**: Indirect addressing (use memory value as target)
- **,X**: Indexed addressing (add X register to target)

**Examples:**
- `BR .+10` - Branch forward 8 (decimal) locations
- `BR .-5,X` - Branch backward 5 (octal), indexed by X
- `BR .+100,I` - Branch forward to address in memory

## Short Branch Instructions

Short branches use a 5-bit signed displacement (±31) with opcode base `141000`.

| Mnemonic | CCF | Example | Octal |
|----------|-----|---------|-------|
| `BN` | 0 | `BN .+2` | 141002 |
| `BL` | 1 | `BL .+2` | 141102 |
| `BE` | 2 | `BE .+2` | 141202 |
| `BLE` | 3 | `BLE .+2` | 141302 |
| `BG` | 4 | `BG .+2` | 141402 |
| `BNE` | 5 | `BNE .+2` | 141502 |
| `BGE` | 6 | `BGE .+2` | 141602 |
| `BA` | 7 | `BA .+2` | 141702 |

Overflow/carry branches use separate short formats:

- `BOV .+2` → `013002` (branch on overflow, clears O)
- `BNOV .+2` → `013102` (branch on no overflow, clears O)
- `BCY .+2` → `011402` (branch on carry, clears C)
- `BNCY .+2` → `011502` (branch on no carry, clears C)
- `BRO .+2` → `013602` (branch on odd value, pops TOS)

## Full-Word Instructions

| Mnemonic | Example | Description |
|----------|---------|-------------|
| `LDI` | `021377` | Load immediate (8-bit) onto stack. |
| `LDXI` | `021777` | Load immediate (8-bit) into X. |
| `LOAD` | `040007` | Load word at P±disp (optional ,I/,X) onto stack. |
| `STOR` | `051000` | Store TOS at DB+disp (optional ,I/,X). |
| `LDD` | `151076` | Load double at DB+disp, push *DB+disp then *DB+disp+1. |
| `STD` | `161076` | Store double at DB+disp, store TOS+1 then TOS. |
| `DDIV` | `020571` | Double divide: (D,C)/(B,A) → quotient (D,C), remainder (B,A). |
| `HALT` | `030360` | Halt execution. |
| `WIO` | `0302KK` | Write I/O; K (4-bit device code), uses TOS word. |
| `RIO` | `0301KK` | Read I/O; K (4-bit device code), pushes low byte. |
| `IABZ` | `0107KK` | Increment A; if zero then branch PC±disp (optional ,I). |
| `IXBZ` | `0112KK` | Increment X; if zero then branch PC±disp (optional ,I). |
| `DXBZ` | `0113KK` | Decrement X; if zero then branch PC±disp (optional ,I). |
| `SCAL` | `0304XX` | System call. Operand selects entry/label. |
| `SXIT` | `0320XX` | Exit system call. Pops return address, discards locals. |
| `ASL` | `0100NN` | Arithmetic shift left TOS by NN, preserve sign bit. |
| `ASR` | `0101NN` | Arithmetic shift right TOS by NN, preserve sign bit. |
| `LSL` | `0102NN` | Logical shift left TOS by NN. |
| `LSR` | `0103NN` | Logical shift right TOS by NN. |
| `DASL` | `0120NN` | Double arithmetic shift left by NN (optional ,X count add). |
| `DASR` | `0121NN` | Double arithmetic shift right by NN (optional ,X count add). |
| `DLSL` | `0122NN` | Double logical shift left by NN (optional ,X count add). |
| `DLSR` | `0123NN` | Double logical shift right by NN (optional ,X count add). |

Device codes: `0` = `tty`, `1` = `lpt` (processed), `2` = `lpt` (raw word).

## Complete Format 2 Opcode Table

Format 2 opcodes are listed below as octal values from `000` to `077`. They are
packed two per word, and `step` executes them sequentially (high 6 bits first,
then low 6 bits). **Bold** opcodes are fully implemented.

## Register Notes

The HP 3000 CPU register set includes:

- `P` (Program Counter) - current instruction address.
- `DL/DB` (Data Base) - start of data/global area.
- `S` (Stack Pointer) - top of the current stack.
- `Q` (Stack Marker) - current procedure local variables.
- `X` (Index Register) - array indexing and offsets.
- `STATUS` - CPU state and condition codes.

Ashen currently models `P`, `DB`, `X`, and `STA`, plus the register stack
(RA/RB/RC/RD) and `SM`. `Q` is noted here for future implementation.

| Octal | Mnemonic | Status | Octal | Mnemonic | Status |
|-------|----------|--------|-------|----------|--------|
| 000 | **NOP** | ✓ | 040 | **DEL** | ✓ |
| 001 | **DELB** | ✓ | 041 | **ZROB** | ✓ |
| 002 | **DDEL** | ✓ | 042 | **LDXB** | ✓ |
| 003 | **ZROX** | ✓ | 043 | **STAX** | ✓ |
| 004 | **INCX** | ✓ | 044 | **LDXA** | ✓ |
| 005 | **DECX** | ✓ | 045 | **DUP** | ✓ |
| 006 | **ZERO** | ✓ | 046 | **DDUP** | ✓ |
| 007 | **DZRO** | ✓ | 047 | FLT | - |
| 010 | **DCMP** | ✓ | 050 | FCMP | - |
| 011 | **DADD** | ✓ | 051 | FADD | - |
| 012 | DSUB | - | 052 | FSUB | - |
| 013 | MPYL | - | 053 | FMPY | - |
| 014 | **DIVL** | ✓ | 054 | FDIV | - |
| 015 | DNEG | - | 055 | FNEG | - |
| 016 | DXCH | - | 056 | CAB | - |
| 017 | CMP | - | 057 | LCMP | - |
| 020 | **ADD** | ✓ | 060 | LADD | - |
| 021 | **SUB** | ✓ | 061 | LSUB | - |
| 022 | **MPY** | ✓ | 062 | **LMPY** | ✓ |
| 023 | **DIV** | ✓ | 063 | **LDIV** | ✓ |
| 024 | **NEG** | ✓ | 064 | **NOT** | ✓ |
| 025 | **TEST** | ✓ | 065 | **OR** | ✓ |
| 026 | **STBX** | ✓ | 066 | **XOR** | ✓ |
| 027 | DTST | - | 067 | **AND** | ✓ |
| 030 | DFLT | - | 070 | FIXR | - |
| 031 | BTST | - | 071 | FIXT | - |
| 032 | **XCH** | ✓ | 072 | UNK | - |
| 033 | **INCA** | ✓ | 073 | **INCB** | ✓ |
| 034 | **DECA** | ✓ | 074 | **DECB** | ✓ |
| 035 | **XAX** | ✓ | 075 | **XBX** | ✓ |
| 036 | **ADAX** | ✓ | 076 | **ADBX** | ✓ |
| 037 | **ADXA** | ✓ | 077 | **ADXB** | ✓ |
