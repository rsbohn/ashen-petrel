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
