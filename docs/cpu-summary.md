# HP3000 CPU Summary

## Call Diagram

```
Monitor (ash)
  ├─ Run()
  │   └─ ExecuteLine()
  │       ├─ asm <file> ──────────────┐
  │       │   └─ AssembleFile()       │
  │       │       ├─ Parse lines      │
  │       │       ├─ Resolve symbols  │
  │       │       └─ Write memory     │
  │       ├─ asm <mnemonic> ──────────┘
  │       │   └─ Assemble()
  │       ├─ run/step/trace
  │       │   └─ Cpu.Run() / Cpu.Step()
  │       │       ├─ Fetch word
  │       │       ├─ Isa.TryExecuteWord()
  │       │       │   ├─ full-word ops (BR/LOAD/STOR/LDI/LDXI/etc)
  │       │       │   └─ short branches (B*/BRO)
  │       │       └─ Isa.TryExecute() for packed ops
  │       │           ├─ stack ops (ZERO/DUP/DEL/etc)
  │       │           ├─ math ops (ADD/DADD/DCMP/etc)
  │       │           └─ flag updates
  │       └─ dis/exam/dep
  │           ├─ Disassemble() ── Isa.Disassemble()
  │           ├─ ExamMemory()
  │           └─ DepositMemory()
  └─ Devices
      └─ IoBus / DeviceRegistry interactions
```

## Core Components

- `Hp3000Cpu`: registers, stack model (RA/RB/RC/RD + SR/SM), fetch/execute loop.
- `Hp3000Isa`: opcode decode/execute, disassembler, assembler helpers.
- `Hp3000Memory`: word-addressable memory backing store.
- `Hp3000Monitor`: CLI front-end (assembly, trace, disassembly, deposit/exam).
- `Hp3000IoBus` + `DeviceRegistry`: device access plumbing.
