# Ashen Petrel ISA Guide

This document details the implemented instruction set for the Ashen Petrel HP 3000 emulator.

## Implemented Instructions

| Mnemonic | Opcode | Description |
|----------|--------|-------------|
| `NOP` | 000000 | No operation. Does nothing. |
| `DELB` | 000001 | Delete B. This is a placeholder and does not affect the stack. |
| `DUP` | 000002 | Duplicate the top of the stack. |
| `DEL` | 000003 | Delete the top of the stack. |
| `ZERO` | 000004 | Push a zero onto the stack. |

## Format 2 Opcodes

Format 2 opcodes are listed below as octal values from `000` to `077`. They are
packed two per word, and `step` executes them sequentially (first opcode, then
the second opcode).

000 = NOP     033 = INCA    066 = XOR
001 = DELB    034 = DECA    067 = AND
002 = DDEL    035 = XAX     070 = FIXR
003 = ZROX    036 = ADAX    071 = FIXT
004 = INCX    037 = ADXA    072 = ??? (unknown)
005 = DECX    040 = DEL     073 = INCB
006 = ZERO    041 = ZROB    074 = DECB
007 = DZRO    042 = LDXB    075 = XBX
010 = DCMP    043 = STAX    076 = ADBX
011 = DADD    044 = LDXA    077 = ADXB
012 = DSUB    045 = DUP     
013 = MPYL    046 = DDUP    
014 = DIVL    047 = FLT     
015 = DNEG    050 = FCMP    
016 = DXCH    051 = FADD    
017 = CMP     052 = FSUB    
020 = ADD     053 = FMPY    
021 = SUB     054 = FDIV    
022 = MPY     055 = FNEG    
023 = DIV     056 = CAB     
024 = NEG     057 = LCMP    
025 = TEST    060 = LADD (Logical add)
026 = STBX    061 = LSUB    
027 = DTST    062 = LMPY    
030 = DFLT    063 = LDIV    
031 = BTST    064 = NOT     
032 = XCH     065 = OR
