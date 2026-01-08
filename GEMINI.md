# Project Overview

This project is a C#/.NET 9 application that emulates a classic HP 3000 computer. It features a Forth-flavored command-line monitor for interacting with the emulated machine. The project is a scaffold, with a minimal instruction set architecture (ISA) implemented.

The emulator includes the following components:
- A CPU (`Hp3000Cpu`)
- Memory (`Hp3000Memory`)
- An I/O bus (`Hp3000IoBus`)
- A set of emulated devices, including a TTY, line printer, magnetic tape, and disk.
- A monitor (`Hp3000Monitor`) that provides a command-line interface for controlling the emulator.

## Building and Running

To build the project, run:
```
dotnet build ashen/ashen.csproj -c Release
```

To run the project, execute:
```
dotnet run --project ashen/ashen.csproj
```

## Development Conventions

The project is written in C# 12 and targets .NET 9. It follows standard C# coding conventions. The code is organized into classes representing the different components of the emulated computer. The monitor uses a dictionary of "words" to implement its command set, which is a common pattern in Forth-like languages.

The project is in an early stage of development, as indicated by the "scaffold" nature of the ISA. Contributions would likely involve adding more instructions to the `Hp3000Isa` class and expanding the capabilities of the emulated devices.
