# Ashen Petrel Guide

## Overview

Ashen Petrel is a classic HP 3000 emulator scaffold featuring a Forth-flavored monitor and built-in assembler. The project provides a minimal but functional environment for exploring HP 3000 architecture and programming.

## Getting Started

### Building the Project

Build the project using the .NET CLI:

```bash
dotnet build ashen/ashen.csproj -c Release
```

### Running the Emulator

Start the monitor:

```bash
dotnet run --project ashen/ashen.csproj
```

You will be greeted with the Ashen monitor prompt:

```
Ashen HP3000 monitor (octal default)
Type 'help' for commands.
ash>
```

## Number Systems

The monitor uses **octal** as the default number base. This reflects the HP 3000's native word size and architecture.

- **Octal numbers**: Enter directly (e.g., `777`, `100`, `0`)
- **Decimal numbers**: Prefix with `#` (e.g., `#100`, `#255`)
- **Hexadecimal numbers**: Prefix with `$` (e.g., `$FF`, `$100`)

Examples:
```
ash> 100       # Pushes octal 100 (decimal 64) onto stack
ash> #100      # Pushes decimal 100 onto stack
ash> $100      # Pushes hex 100 (decimal 256) onto stack
```

## Stack-Based Operation

The monitor includes a data stack inspired by Forth, allowing you to build complex operations interactively.

### Basic Stack Commands

- `.` - Print and pop the top value from stack
- `dup` - Duplicate the top stack value
- `drop` - Remove the top stack value
- `swap` - Exchange the top two stack values
- `over` - Copy the second value to the top

### Arithmetic and Logic Operations

- `+` - Add top two values
- `-` - Subtract (second - top)
- `and` - Bitwise AND
- `or` - Bitwise OR
- `xor` - Bitwise XOR
- `invert` - Bitwise NOT (one's complement)

### Memory Operations

- `@` - Fetch: read word from memory address on stack, push value
- `!` - Store: pop address, pop value, write value to address

Example session:
```
ash> 1000 2000 +     # Push 1000, push 2000, add them
ash> .               # Print result (3000 octal = 1536 decimal)
3000 ok
ash> 100 dup +       # Push 100, duplicate it, add (100 + 100)
ash> .               # Print result (200 octal = 128 decimal)
200 ok
```

## CPU Control

### Execution Commands

- `reset [addr]` - Reset CPU to address (default: 0)
- `go <addr> [steps]` - Start execution at address
- `run [steps]` - Continue execution from current PC
- `step [count]` - Execute one or more instructions
- `regs` - Display all CPU registers

### Breakpoints

- `break <addr>` - Set breakpoint at address
- `breaks` - List all breakpoints
- `clear <addr>` - Clear breakpoint at address

## Memory Inspection

### Examining Memory

- `exam <addr> [count]` - Display memory contents
- `x <addr> [count]` - Shorthand for exam

Example:
```
ash> exam 0 10       # Show 10 words starting at address 0
000000: 000000  000000  000000  000000
000004: 000000  000000  000000  000000
000010: 000000  000000
```

### Depositing Values

- `deposit <addr> <value> [value2 ...]` - Write values to memory
- `dep <addr> <value> [value2 ...]` - Shorthand for deposit
- `d <addr> <value> [value2 ...]` - Even shorter shorthand

Example:
```
ash> dep 100 1 2 3 4     # Write values to addresses 100-103
ash> x 100 4             # Verify the write
```

## Assembly and Disassembly

### Disassembly

- `dis` - Disassemble instruction at current PC
- `dis <addr>` - Disassemble instruction at address

### Assembly

- `asm <mnemonic> [addr]` - Assemble instruction

The ISA currently includes minimal opcodes:
- `NOP` (0x0000) - No operation
- `DELB` (0x0001) - Delete byte

Example:
```
ash> asm NOP 100      # Assemble NOP at address 100
ash> dis 100          # Disassemble to verify
100: NOP
```

## Device Management

The emulator supports peripheral devices with block-based I/O.

### Device Types

- `tty` - Terminal/teletype
- `lpt` - Line printer
- `mt0` - Magnetic tape drive 0
- `d0` - Disk drive 0

### Device Commands

- `devs` - List all devices and their status
- `attach <dev> <path> [new]` - Attach file to device
- `detach <dev>` - Detach device
- `status <dev>` - Show device status

### Block I/O

Block devices use **128-word blocks** with **big-endian** word encoding.

- `readblk <dev> <block> <addr>` - Read block into memory
- `writeblk <dev> <block> <addr>` - Write memory to block

Example workflow:
```
ash> attach d0 disk.img new      # Create new disk image
ash> #256 0 dep                  # Put value at address 0
ash> writeblk d0 0 0             # Write block 0 from memory address 0
ash> 0 #128 0 dep               # Zero out that memory region
ash> readblk d0 0 0             # Read block back
ash> x 0                         # Verify the data
```

## Command Reference

### System Commands

- `help` - Show help message
- `words` - List all available commands
- `quit` / `exit` - Exit the monitor

### Complete Command List

```
System:           help, words, quit, exit
CPU Control:      reset, go, run, step, regs
Memory:           exam (x), deposit (dep, d)
Assembly:         asm, dis
Breakpoints:      break, breaks, clear
Stack:            . dup drop swap over + - and or xor invert @ !
Devices:          devs, attach, detach, status
Block I/O:        readblk, writeblk
```

## Tips and Best Practices

### Working with Octal

The HP 3000 was designed around 16-bit words, and octal provides a natural representation:
- 16 bits = 6 octal digits (000000 to 177777)
- Each octal digit represents exactly 3 bits
- Octal 177777 = decimal 65535 = hex FFFF

### Building Complex Commands

Use the stack to prepare arguments for commands:
```
ash> 100           # Push address
ash> 777 dup       # Push value twice
ash> !             # Store one copy
ash> 101 !         # Store second copy at next address
```

### Memory Layout Convention

While the emulator doesn't enforce a specific memory layout, consider:
- Low memory (0-377): Interrupt vectors and system area
- Mid memory: User program code
- High memory: Data and stack

### Debugging Programs

1. Load your code into memory using `deposit`
2. Set breakpoints at key locations with `break`
3. Use `go` to start execution
4. When hitting a breakpoint, use `regs` and `exam` to inspect state
5. Use `step` to single-step through code
6. Use `run` to continue execution

## Architecture Notes

### Memory

- 16-bit word-addressable memory
- Big-endian word encoding for I/O
- No memory protection (this is a simple emulator)

### Instruction Set

The ISA is currently minimal (scaffold implementation):
- Most operations are NOP or placeholder
- Use the monitor's stack operations for computation
- Full ISA implementation is pending

### I/O System

- Device-based I/O model
- Block-oriented storage (128 words per block)
- File-backed device images
- All I/O goes through the DeviceRegistry

## Examples

### Example 1: Simple Memory Test

```
ash> # Fill memory with pattern
ash> 1000 1 2 3 4 5 6 7 10 dep
ash> # Read it back
ash> x 1000 8
ash> # Should show: 1 2 3 4 5 6 7 10
```

### Example 2: Using the Stack for Calculation

```
ash> # Calculate (100 + 200) * 2
ash> 100 200 +      # Push 100, push 200, add -> 300
ash> dup +          # Duplicate and add -> 600
ash> .              # Print result
600 ok
```

### Example 3: Creating a Disk Image

```
ash> # Attach new disk
ash> attach d0 mydisk.img new
ash> # Prepare some data in memory
ash> 0 #100 #200 #300 #400 dep
ash> # Write to block 0
ash> writeblk d0 0 0
ash> # Verify
ash> status d0
d0: attached to 'mydisk.img'
```

### Example 4: Simple Program Loop

```
ash> # Store a simple program
ash> 100 0 0 0 dep               # NOP, NOP, NOP
ash> # Set breakpoint after loop
ash> break 103
ash> # Run from start
ash> go 100
ash> # Step through
ash> step 3
ash> regs
```

## Troubleshooting

### Stack Underflow

If you get a stack underflow error, you've tried to pop from an empty stack. Check your command sequence.

### Unknown Word

If the monitor reports an unknown word, verify:
- Spelling and case (commands are case-insensitive)
- Use `words` to see all available commands

### Device Errors

For device I/O errors:
- Ensure device is attached with `status <dev>`
- Check file permissions on the backing file
- Verify block numbers are within range

### Unexpected Values

Remember the default is octal! If you see unexpected numbers:
- Use `#` prefix for decimal input
- Octal 100 = decimal 64
- Check your number base

## Future Enhancements

The current implementation is a scaffold. Planned enhancements include:

- Full HP 3000 instruction set
- Interrupt handling
- Memory protection and segmentation
- Additional device types
- Symbolic debugging
- Program loading from files

## Additional Resources

- HP 3000 Architecture documentation
- Forth programming language references
- Project source code in `ashen/` directory
- Development log in `devlog/` directory

---

**Version**: 1.0  
**Last Updated**: 2026-01-08  
**Project**: Ashen Petrel HP 3000 Emulator
