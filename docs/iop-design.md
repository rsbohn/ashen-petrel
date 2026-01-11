# HP3000 IOP Design

## Call Diagram

```
Monitor (ash)
  └─ ExecuteLine()
      └─ Cpu.Run()
          └─ Isa.TryExecuteWord()
              └─ SIO Instruction (Start I/O)
                  ├─ CPU: Sets "IOP Busy" flag
                  ├─ CPU: Passes Device # and SIO Program Address to IOP
                  └─ IOP: Begins background execution
```

---

## Implementation Steps

### A. Define the IOP State Machine

The IOP doesn't just "run" like the CPU; it executes a specific set of
**I/O Commands** (like `Control`, `Read`, `Write`, `Jump`).

| Component | Responsibility |
| --- | --- |
| **Command Fetcher** | Reads the next I/O program word from `Hp3000Memory`. |
| **Device Interface** | Communicates with the `DeviceRegistry` to send/receive bytes. |
| **Byte Counter** | Tracks the number of words/bytes remaining in a DMA transfer. |

### B. Modify the `Hp3000Isa`

Implement the I/O opcodes in the ISA:

- `SIO`: Initiates the IOP.
- `RIO`: Real-time input (direct CPU/device).
- `WIO`: Real-time output.
- `TIO`: Test I/O (checks if IOP/device is busy).

### D. LPT Output Modes

The line printer supports multiple radix modes for WIO-driven output:

- `0` (default): ASCII mode, writes low byte.
- `2`: Binary `hhhhhhhh llllllll` (high byte then low byte), one word per line.
- `8`: Octal `000000` with space-separated words, eight words per line.
- `A`: Decimal `0..65535` with space-separated words, eight words per line.
- `F`: Hex `0x0000..0xFFFF` with space-separated words, eight words per line.

### C. Update the Monitor

Add monitor commands to inspect and debug the IOP:

- `exam iop`: Show current I/O command pointer and status.
- `trace iop`: Log I/O program execution.

---

## Next Steps

Draft the `Hp3000Iop` class skeleton (C#) and define the I/O command formats.
