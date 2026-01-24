# Ashen Petrel

Classic HP 3000 emulator scaffold with a forth-flavored monitor and built-in assembler (pending ISA).

## Build & Run

Build:
```
dotnet build ashen/ashen.csproj -c Release
```

Run:
```
dotnet run --project ashen/ashen.csproj
```

## Monitor Notes

- Numbers are octal; use `#` for decimal.
- Devices: `tty`, `lpt`, `mt0`, `d0`.
- Block devices use 128-word blocks and big-endian words.

## Commands

```
help
words
reset [addr]
go <addr> [steps]
run [steps]
step [count]
regs
exam <addr> [count]
x <addr> [count]
deposit <addr> <value> [value2 ...]
dep <addr> <value> [value2 ...]
d <addr> <value> [value2 ...]
dis
break <addr>
breaks
asm <addr> <opcode> [operand]
asm <addr>  (interactive mode)

stack: . dup drop swap over + - and or xor invert

attach <dev> <path> [new]
detach <dev>
status <dev>
devs
readblk <dev> <block> <addr>
writeblk <dev> <block> <addr>
```
