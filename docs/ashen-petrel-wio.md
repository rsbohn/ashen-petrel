# Ashen Petrel WIO Notes

This document describes the current `WIO` behavior in the emulator.

## Instruction Behavior

- `WIO <device>` pops the top of stack (TOS) and attempts to write it to the
  specified device.
- Success/failure is reflected in the condition codes (see `STA`).
- If the device reports output-ready, the write happens immediately.

## Device Codes

- `0` = `tty` (not implemented for output yet; `WIO 0` reports not-ready)
- `1` = `lpt` processed output (radix formatting)
- `2` = `lpt` raw output (16-bit word to bytes, no radix formatting)

## LPT Processed Output (Device 1)

The line printer supports multiple radix modes for WIO-driven output:

- `0` (default): ASCII mode, writes low byte.
- `2`: Binary `hhhhhhhh llllllll` (high byte then low byte), one word per line.
- `8`: Octal `000000` with space-separated words, eight words per line.
- `A`: Decimal `0..65535` with space-separated words, eight words per line.
- `F`: Hexadecimal `0x0000..0xFFFF` with space-separated words, eight words per line.

Use the monitor command `lptradix <0|2|8|A|F>` to change modes.

## LPT Raw Output (Device 2)

Raw mode bypasses radix formatting and writes the 16-bit value as two bytes
(high byte then low byte). Newline (`0x0A`) and form feed (`0x0C`) are emitted as
control characters. Carriage return (`0x0D`) is ignored.

Tip: If you want repeatable formatting across program runs, emit a raw NL/FF
word (e.g. `0x0A0C`) at the start to reset the line printer state.
