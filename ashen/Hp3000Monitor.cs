using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Ashen
{
    internal sealed class Hp3000Monitor
    {
        private readonly Hp3000Cpu _cpu;
        private readonly Hp3000Memory _memory;
        private readonly DeviceRegistry _devices;
        private readonly Hp3000Isa _isa;
        private readonly HashSet<int> _breakpoints = new();
        private readonly HashSet<int> _watches = new();
        private readonly Dictionary<string, Action<TokenStream>> _words;
        private Dictionary<string, int>? _lastSymbols;
        private string? _lastAsmPath;
        private bool _quit;

        public Hp3000Monitor(Hp3000Cpu cpu, Hp3000Memory memory, DeviceRegistry devices)
        {
            _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
            _devices = devices ?? throw new ArgumentNullException(nameof(devices));
            _isa = new Hp3000Isa();
            _words = BuildWordTable();
        }

        public void Run()
        {
            Console.WriteLine("Ashen HP3000 monitor (octal default)");
            Console.WriteLine("Type 'help' for commands.");

            while (!_quit)
            {
                Console.Write("ash> ");
                var line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }

                ExecuteLine(line);
            }
        }

        private void ExecuteLine(string line)
        {
            var tokens = Tokenize(line);
            if (tokens.Count == 0)
            {
                return;
            }

            var stream = new TokenStream(tokens);
            while (stream.HasMore)
            {
                var token = stream.Next();
                if (TryParseNumber(token, out var value))
                {
                    _cpu.Push((ushort)value);
                    continue;
                }

                if (_words.TryGetValue(token, out var action))
                {
                    try
                    {
                        action(stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"? {ex.Message}");
                    }
                    continue;
                }

                Console.WriteLine($"? unknown word '{token}'");
            }
        }

        private Dictionary<string, Action<TokenStream>> BuildWordTable()
        {
            return new Dictionary<string, Action<TokenStream>>(StringComparer.OrdinalIgnoreCase)
            {
                ["help"] = _ => ShowHelp(),
                ["words"] = _ => ListWords(),
                ["quit"] = _ => _quit = true,
                ["q"] = _ => _quit = true,
                ["exit"] = _ => _quit = true,
                ["."] = _ => PrintTop(),
                ["dup"] = _ => Dup(),
                ["drop"] = _ => Drop(),
                ["swap"] = _ => Swap(),
                ["over"] = _ => Over(),
                ["+"] = _ => BinOp((a, b) => a + b),
                ["-"] = _ => BinOp((a, b) => a - b),
                ["and"] = _ => BinOp((a, b) => a & b),
                ["or"] = _ => BinOp((a, b) => a | b),
                ["xor"] = _ => BinOp((a, b) => a ^ b),
                ["invert"] = _ => UnaryOp(a => ~a),
                ["!"] = _ => StoreWord(),
                ["@"] = _ => FetchWord(),
                ["reset"] = Reset,
                ["go"] = Go,
                ["run"] = RunCpu,
                ["step"] = StepCpu,
                ["trace"] = TraceCpu,
                ["t"] = TraceCpu,
                ["scal"] = ScalCall,
                ["regs"] = _ => ShowRegs(),
                ["exam"] = ExamMemory,
                ["x"] = ExamMemory,
                ["deposit"] = DepositMemory,
                ["dep"] = DepositMemory,
                ["d"] = DepositMemory,
                ["txt"] = StoreText,
                ["dis"] = Disassemble,
                ["break"] = SetBreak,
                ["breaks"] = _ => ListBreaks(),
                ["asm"] = Assemble,
                ["&asm"] = _ => ReassembleLastFile(),
                ["syms"] = _ => ListSymbols(),
                ["devs"] = _ => ListDevices(),
                ["status"] = StatusDevice,
                ["lptcols"] = SetLinePrinterColumns,
                ["lptradix"] = SetLinePrinterRadix,
                ["watch"] = AddWatch,
                ["unwatch"] = RemoveWatch,
                ["watches"] = ListWatches,
                ["attach"] = AttachDevice,
                ["detach"] = DetachDevice,
                ["readblk"] = ReadBlock,
                ["writeblk"] = WriteBlock
            };
        }

        private void ShowHelp()
        {
            Console.WriteLine("Core: help words reset go run step scal exam deposit dis break breaks asm &asm syms txt");
            Console.WriteLine("Stack: . dup drop swap over + - and or xor invert ! @");
            Console.WriteLine("Devices: devs status attach detach readblk writeblk lptcols lptradix");
            Console.WriteLine("Watch: watch unwatch watches");
            Console.WriteLine("Numbers are octal; use # for decimal.");
            Console.WriteLine("Assembler: asm <addr> <opcode> [operand] | asm <file> | asm <addr> | &asm | syms");
            Console.WriteLine("Memory: txt <addr> /text/ (writes ASCII bytes + 0 terminator)");
        }

        private void ListWords()
        {
            var words = _words.Keys.OrderBy(word => word);
            Console.WriteLine(string.Join(" ", words));
        }

        private void ReassembleLastFile()
        {
            if (string.IsNullOrWhiteSpace(_lastAsmPath))
            {
                Console.WriteLine("asm: no previous file");
                return;
            }

            AssembleFile(_lastAsmPath);
        }

        private void ListSymbols()
        {
            if (_lastSymbols == null || _lastSymbols.Count == 0)
            {
                Console.WriteLine("syms: (none)");
                return;
            }

            Console.WriteLine($"syms: {_lastAsmPath ?? "(unknown)"}");
            foreach (var entry in _lastSymbols.OrderBy(entry => entry.Value).ThenBy(entry => entry.Key))
            {
                Console.WriteLine($"{entry.Key} {ToOctal(entry.Value)}");
            }
        }

        private void AddWatch(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("watch <addr>");
                return;
            }

            address &= 0x7fff;
            if (_watches.Add(address))
            {
                Console.WriteLine($"watch {ToOctal(address)}");
            }
        }

        private void RemoveWatch(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("unwatch <addr>");
                return;
            }

            address &= 0x7fff;
            if (_watches.Remove(address))
            {
                Console.WriteLine($"unwatch {ToOctal(address)}");
            }
        }

        private void ListWatches(TokenStream _)
        {
            if (_watches.Count == 0)
            {
                Console.WriteLine("watches: (none)");
                return;
            }

            var ordered = _watches.OrderBy(addr => addr).Select(ToOctal);
            Console.WriteLine($"watches: {string.Join(" ", ordered)}");
        }

        private void PrintTop()
        {
            if (_cpu.Sr == 0)
            {
                Console.WriteLine("stack empty");
                return;
            }

            Console.WriteLine(ToOctal(_cpu.Pop()));
        }

        private void Dup()
        {
            RequireStack(1);
            _cpu.Push(_cpu.Peek());
        }

        private void Drop()
        {
            RequireStack(1);
            _cpu.Pop();
        }

        private void Swap()
        {
            RequireStack(2);
            var a = _cpu.Pop();
            var b = _cpu.Pop();
            _cpu.Push(a);
            _cpu.Push(b);
        }

        private void Over()
        {
            RequireStack(2);
            var a = _cpu.Pop();
            var b = _cpu.Pop();
            _cpu.Push(b);
            _cpu.Push(a);
            _cpu.Push(b);
        }

        private void BinOp(Func<int, int, int> op)
        {
            RequireStack(2);
            var b = _cpu.Pop();
            var a = _cpu.Pop();
            _cpu.Push((ushort)op(a, b));
        }

        private void UnaryOp(Func<int, int> op)
        {
            RequireStack(1);
            var a = _cpu.Pop();
            _cpu.Push((ushort)op(a));
        }

        private void StoreWord()
        {
            RequireStack(2);
            var address = _cpu.Pop();
            var value = _cpu.Pop();
            _memory.Write(address, (ushort)value);
        }

        private void FetchWord()
        {
            RequireStack(1);
            var address = _cpu.Pop();
            _cpu.Push(_memory.Read(address));
        }

        private void Reset(TokenStream stream)
        {
            var address = 0;
            if (stream.TryPeek(out var token) && TryParseNumber(token, out var value))
            {
                stream.Next();
                address = value;
            }

            _cpu.Reset(address);
            Console.WriteLine($"reset PC={ToOctal(_cpu.Pc)}");
        }

        private void Go(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("go <addr> [steps]");
                return;
            }

            _cpu.Reset(address);
            var steps = stream.TryNextNumber(out var maxSteps) ? maxSteps : 1000;
            var ran = _cpu.Run(steps);
            Console.WriteLine($"\nran {ToOctalCount(ran)} steps");
            ReportHaltReason();
        }

        private void RunCpu(TokenStream stream)
        {
            var steps = stream.TryNextNumber(out var maxSteps) ? maxSteps : 1000;
            var ran = _cpu.Run(steps);
            Console.WriteLine($"\nran {ToOctalCount(ran)} steps");
            ReportHaltReason();
        }

        private void StepCpu(TokenStream stream)
        {
            var steps = stream.TryNextNumber(out var maxSteps) ? maxSteps : 1;
            var ran = 0;
            for (var i = 0; i < steps; i++)
            {
                var watchSnapshot = CaptureWatchSnapshot();
                if (!_cpu.Step())
                {
                    break;
                }

                ReportWatchChanges(watchSnapshot);
                ran++;
            }

            Console.WriteLine($"\nstepped {ToOctalCount(ran)} steps");
            ReportHaltReason();
        }

        private void TraceCpu(TokenStream stream)
        {
            var steps = stream.TryNextNumber(out var maxSteps) ? maxSteps : 1;
            var ran = 0;
            for (var i = 0; i < steps; i++)
            {
                var stepped = _cpu.Step();
                ShowRegs();
                ReportHaltReason();
                if (!stepped)
                {
                    break;
                }

                ran++;
            }

            Console.WriteLine($"\ntraced {ToOctalCount(ran)} steps");
        }

        private void ExamMemory(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("exam <addr> [count]");
                return;
            }

            var count = stream.TryNextNumber(out var n) ? n : 8;
            var lineSize = 8;
            for (var i = 0; i < count; i++)
            {
                if (i % lineSize == 0)
                {
                    if (i > 0)
                    {
                        Console.WriteLine();
                    }

                    Console.Write($"{ToOctal(address + i)}:");
                }

                var value = _memory.Read(address + i);
                Console.Write($" {ToOctal(value)}");
            }

            Console.WriteLine();
        }

        private void DepositMemory(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("deposit <addr> <value> [value2 ...]");
                return;
            }

            var wrote = 0;
            while (stream.TryNextNumber(out var value))
            {
                _memory.Write(address + wrote, (ushort)value);
                wrote++;
            }

            if (wrote == 0)
            {
                Console.WriteLine("deposit <addr> <value> [value2 ...]");
            }
        }

        private void ScalCall(TokenStream stream)
        {
            if (stream.HasMore)
            {
                Console.WriteLine("scal");
                return;
            }

            if (!_isa.TryAssemble("SCAL", "0", out var opcode))
            {
                Console.WriteLine("scal: assemble failed");
                return;
            }

            if (!_isa.TryExecuteWord(opcode, _cpu))
            {
                Console.WriteLine("scal: execute failed");
            }
        }

        private void StoreText(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("txt <addr> /text/");
                return;
            }

            if (!stream.TryNext(out var token))
            {
                Console.WriteLine("txt <addr> /text/");
                return;
            }

            if (!TryParseSlashDelimitedText(token, stream, out var text, out var error))
            {
                Console.WriteLine($"txt: {error}");
                return;
            }

            var start = address & 0x7fff;
            var current = start;
            foreach (var ch in text)
            {
                _memory.Write(current, (ushort)(ch & 0xFF));
                current = (current + 1) & 0x7fff;
            }

            _memory.Write(current, 0);
            var count = text.Length + 1;
            Console.WriteLine($"txt {ToOctal(start)} len={ToOctalCount(count)}");
        }

        private void ShowRegs()
        {
            Console.WriteLine($"PC={ToOctal(_cpu.Pc)} PB={ToOctal(_cpu.Pb)} PL={ToOctal(_cpu.Pl)} SM={ToOctal(_cpu.Sm)} SR={_cpu.Sr} DB={ToOctal(_cpu.Db)} X={ToOctal(_cpu.X)} HALT={(_cpu.Halted ? "1" : "0")}");
            Console.WriteLine($"STACK: {ToOctal(_cpu.Ra)} {ToOctal(_cpu.Rb)} {ToOctal(_cpu.Rc)} {ToOctal(_cpu.Rd)} ... ({ToOctalCount(_cpu.StackDepth)})");
            Console.WriteLine($"STA: {FormatStatusFlags(_cpu.Sta)} {FormatStatusCondition(_cpu.Sta)} {ToOctalStatus(_cpu.Sta)}");
        }

        private void ReportHaltReason()
        {
            if (_cpu.Halted && !string.IsNullOrWhiteSpace(_cpu.HaltReason))
            {
                Console.WriteLine($"\nhalt: {_cpu.HaltReason}");
            }
        }

        private void Disassemble(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("dis <addr> [count]");
                return;
            }

            var count = stream.TryNextNumber(out var n) ? n : 1;
            for (var i = 0; i < count; i++)
            {
                var opcode = _memory.Read(address + i);
                var mnemonic = _isa.Disassemble(opcode);
                var line = $"{ToOctal(address + i)}: {ToOctal(opcode)} {mnemonic}";

                Console.WriteLine(line);
            }
        }

        private void Assemble(TokenStream stream)
        {
            if (!stream.TryNext(out var token))
            {
                Console.WriteLine("asm <addr> <opcode> [operand]");
                return;
            }

            if (File.Exists(token))
            {
                AssembleFile(token);
                return;
            }

            if (!TryParseNumber(token, out var address))
            {
                Console.WriteLine("asm <addr> <opcode> [operand]");
                return;
            }

            if (!stream.TryNext(out var mnemonic))
            {
                AssembleInteractive(address);
                return;
            }

            var operand = stream.TryNext(out var operandToken) ? operandToken : null;
            if (stream.HasMore)
            {
                Console.WriteLine("asm <addr> <opcode> [operand]");
                return;
            }

            if (!string.IsNullOrWhiteSpace(operand))
            {
                if (!_isa.TryAssemble(mnemonic, operand, out var opcodeWithOperand))
                {
                    if (IsOperandMnemonic(mnemonic) || _isa.TryAssemble(mnemonic, out _))
                    {
                        Console.WriteLine($"asm {mnemonic} {operand}: invalid operand");
                        return;
                    }

                    Console.WriteLine($"unknown mnemonic {mnemonic}");
                    return;
                }

                _memory.Write(address, opcodeWithOperand);
                Console.WriteLine($"{ToOctal(address)}: {mnemonic} {operand}");
                return;
            }

            if (IsOperandMnemonic(mnemonic))
            {
                Console.WriteLine($"asm {mnemonic}: requires an operand");
                return;
            }

            if (!_isa.TryAssemble(mnemonic, out var opcode))
            {
                Console.WriteLine($"unknown mnemonic {mnemonic}");
                return;
            }

            _memory.Write(address, opcode);
            Console.WriteLine($"{ToOctal(address)}: {mnemonic}");
        }

        private void AssembleInteractive(int address)
        {
            Console.WriteLine("Interactive Assembler. Enter '$' to quit.");
            var symbols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                Console.Write($"{ToOctal(address)}? ");
                var line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }

                var trimmed = line.Trim();
                if (trimmed == "$")
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (!TryAssembleInteractiveLine(trimmed, address, symbols, out var opcode, out var error))
                {
                    Console.WriteLine($"asm: {error}");
                    continue;
                }

                _memory.Write(address, opcode);
                Console.WriteLine($"{ToOctal(address)}: {ToOctal(opcode)}");
                address = (address + 1) & 0x7fff;
            }
        }

        private bool TryAssembleInteractiveLine(
            string line,
            int address,
            Dictionary<string, int> symbols,
            out ushort opcode,
            out string error)
        {
            opcode = 0;
            error = string.Empty;

            var trimmed = StripComment(line).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                error = "empty line";
                return false;
            }

            var parts = trimmed.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                error = "empty line";
                return false;
            }

            var mnemonic = parts[0];
            var operand = parts.Length > 1 ? parts[1].Trim() : null;

            if (mnemonic.EndsWith(",", StringComparison.Ordinal))
            {
                var firstMnemonic = mnemonic.TrimEnd(',');
                if (string.IsNullOrWhiteSpace(operand))
                {
                    error = "missing second opcode";
                    return false;
                }

                var secondParts = operand.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (secondParts.Length != 1)
                {
                    error = $"invalid packed opcodes '{trimmed}'";
                    return false;
                }

                if (!_isa.TryAssemble(firstMnemonic, out var firstOpcode)
                    || !_isa.TryAssemble(secondParts[0], out var secondOpcode))
                {
                    error = $"unknown mnemonic in '{trimmed}'";
                    return false;
                }

                opcode = (ushort)((firstOpcode << 6) | secondOpcode);
                return true;
            }

            if (operand != null)
            {
                if (!TryResolveOperand(mnemonic, operand, address, symbols, out var resolvedOperand, out var resolveError))
                {
                    error = resolveError;
                    return false;
                }

                if (_isa.TryAssemble(mnemonic, resolvedOperand, out var opcodeWithOperand))
                {
                    opcode = opcodeWithOperand;
                    return true;
                }

                if (IsOperandMnemonic(mnemonic))
                {
                    error = $"invalid operand '{operand}' for {mnemonic}";
                    return false;
                }
            }
            else if (IsOperandMnemonic(mnemonic))
            {
                error = $"{mnemonic} requires an operand";
                return false;
            }

            if (_isa.TryAssemble(mnemonic, out var opcodeSimple))
            {
                opcode = opcodeSimple;
                return true;
            }

            error = $"unknown mnemonic '{mnemonic}'";
            return false;
        }

        private void AssembleFile(string path)
        {
            var symbols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var lines = new List<AsmLine>();
            var address = _cpu.Pc;
            var origin = _cpu.Pc;
            var originSet = false;
            var lineNumber = 0;

            foreach (var rawLine in File.ReadLines(path))
            {
                lineNumber++;
                var line = StripComment(rawLine).Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var label = ExtractLabel(ref line);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    if (symbols.ContainsKey(label))
                    {
                        Console.WriteLine($"asm {path}:{lineNumber}: duplicate label '{label}'");
                        return;
                    }

                    symbols[label] = address & 0x7fff;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    continue;
                }

                var mnemonic = parts[0];
                var operand = parts.Length > 1 ? parts[1].Trim() : null;

                if (mnemonic.Equals("ORG", StringComparison.OrdinalIgnoreCase))
                {
                    if (operand == null || !TryParseNumber(operand, out var org))
                    {
                        Console.WriteLine($"asm {path}:{lineNumber}: invalid ORG operand '{operand ?? ""}'");
                        return;
                    }

                    address = org & 0x7fff;
                    if (!originSet)
                    {
                        origin = address;
                        originSet = true;
                    }
                    continue;
                }

                if (mnemonic.Equals("DW", StringComparison.OrdinalIgnoreCase))
                {
                    if (operand == null)
                    {
                        Console.WriteLine($"asm {path}:{lineNumber}: DW requires at least one value");
                        return;
                    }

                    var values = SplitOperands(operand);
                    lines.Add(new AsmLine(lineNumber, address, "DW", values, line));
                    address = (address + values.Count) & 0x7fff;
                    continue;
                }

                if (mnemonic.Equals("TXT", StringComparison.OrdinalIgnoreCase))
                {
                    if (operand == null)
                    {
                        Console.WriteLine($"asm {path}:{lineNumber}: TXT requires /text/");
                        return;
                    }

                    if (!TryParseTextLiteral(operand, out var text, out var error))
                    {
                        Console.WriteLine($"asm {path}:{lineNumber}: {error}");
                        return;
                    }

                    lines.Add(new AsmLine(lineNumber, address, "TXT", operand, line));
                    address = (address + text.Length) & 0x7fff;
                    continue;
                }

                if (mnemonic.Equals("DD", StringComparison.OrdinalIgnoreCase))
                {
                    if (operand == null)
                    {
                        Console.WriteLine($"asm {path}:{lineNumber}: DD requires at least one value");
                        return;
                    }

                    var values = SplitOperands(operand);
                    lines.Add(new AsmLine(lineNumber, address, "DD", values, line));
                    address = (address + (values.Count * 2)) & 0x7fff;
                    continue;
                }

                if (mnemonic.Equals("DQ", StringComparison.OrdinalIgnoreCase))
                {
                    if (operand == null)
                    {
                        Console.WriteLine($"asm {path}:{lineNumber}: DQ requires at least one value");
                        return;
                    }

                    var values = SplitOperands(operand);
                    lines.Add(new AsmLine(lineNumber, address, "DQ", values, line));
                    address = (address + (values.Count * 4)) & 0x7fff;
                    continue;
                }

                lines.Add(new AsmLine(lineNumber, address, mnemonic, operand, line));
                address = (address + 1) & 0x7fff;
            }

            var assembled = 0;
            foreach (var asmLine in lines)
            {
                if (asmLine.Mnemonic.Equals("DW", StringComparison.OrdinalIgnoreCase))
                {
                    var writeAddress = asmLine.Address;
                    foreach (var valueToken in asmLine.Values)
                    {
                        if (!TryResolveValue(valueToken, symbols, writeAddress, out var value))
                        {
                            Console.WriteLine($"asm {path}:{asmLine.LineNumber}: invalid literal '{valueToken}'");
                            return;
                        }

                        _memory.Write(writeAddress, (ushort)value);
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        assembled++;
                    }

                    continue;
                }

                if (asmLine.Mnemonic.Equals("DD", StringComparison.OrdinalIgnoreCase))
                {
                    var writeAddress = asmLine.Address;
                    foreach (var valueToken in asmLine.Values)
                    {
                        if (!TryResolveValue32(valueToken, symbols, writeAddress, out var value))
                        {
                            Console.WriteLine($"asm {path}:{asmLine.LineNumber}: invalid literal '{valueToken}'");
                            return;
                        }

                        _memory.Write(writeAddress, (ushort)(value >> 16));
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        _memory.Write(writeAddress, (ushort)(value & 0xffff));
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        assembled += 2;
                    }

                    continue;
                }

                if (asmLine.Mnemonic.Equals("TXT", StringComparison.OrdinalIgnoreCase))
                {
                    if (asmLine.Operand == null)
                    {
                        Console.WriteLine($"asm {path}:{asmLine.LineNumber}: TXT requires /text/");
                        return;
                    }

                    if (!TryParseTextLiteral(asmLine.Operand, out var text, out var error))
                    {
                        Console.WriteLine($"asm {path}:{asmLine.LineNumber}: {error}");
                        return;
                    }

                    var writeAddress = asmLine.Address;
                    foreach (var ch in text)
                    {
                        _memory.Write(writeAddress, (ushort)(ch & 0xFF));
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        assembled++;
                    }

                    continue;
                }

                if (asmLine.Mnemonic.Equals("DQ", StringComparison.OrdinalIgnoreCase))
                {
                    var writeAddress = asmLine.Address;
                    foreach (var valueToken in asmLine.Values)
                    {
                        if (!TryResolveValue64(valueToken, symbols, writeAddress, out var value))
                        {
                            Console.WriteLine($"asm {path}:{asmLine.LineNumber}: invalid literal '{valueToken}'");
                            return;
                        }

                        _memory.Write(writeAddress, (ushort)(value >> 48));
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        _memory.Write(writeAddress, (ushort)((value >> 32) & 0xffff));
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        _memory.Write(writeAddress, (ushort)((value >> 16) & 0xffff));
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        _memory.Write(writeAddress, (ushort)(value & 0xffff));
                        writeAddress = (writeAddress + 1) & 0x7fff;
                        assembled += 4;
                    }

                    continue;
                }

                var mnemonic = asmLine.Mnemonic;
                var operand = asmLine.Operand;

                if (mnemonic.EndsWith(",", StringComparison.Ordinal))
                {
                    var firstMnemonic = mnemonic.TrimEnd(',');
                    if (string.IsNullOrWhiteSpace(operand))
                    {
                        Console.WriteLine($"asm {path}:{asmLine.LineNumber}: missing second opcode");
                        return;
                    }

                    var secondParts = operand.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (secondParts.Length != 1)
                    {
                        Console.WriteLine($"asm {path}:{asmLine.LineNumber}: invalid packed opcodes '{asmLine.RawLine}'");
                        return;
                    }

                    if (!_isa.TryAssemble(firstMnemonic, out var firstOpcode)
                        || !_isa.TryAssemble(secondParts[0], out var secondOpcode))
                    {
                        Console.WriteLine($"asm {path}:{asmLine.LineNumber}: unknown mnemonic in '{asmLine.RawLine}'");
                        return;
                    }

                    var packed = (ushort)((firstOpcode << 6) | secondOpcode);
                    _memory.Write(asmLine.Address, packed);
                    assembled++;
                    continue;
                }

                if (operand != null)
                {
                    if (!TryResolveOperand(mnemonic, operand, asmLine.Address, symbols, out var resolvedOperand, out var error))
                    {
                        Console.WriteLine($"asm {path}:{asmLine.LineNumber}: {error}");
                        return;
                    }

                    if (_isa.TryAssemble(mnemonic, resolvedOperand, out var opcodeWithOperand))
                    {
                        _memory.Write(asmLine.Address, opcodeWithOperand);
                        assembled++;
                        continue;
                    }

                    if (IsOperandMnemonic(mnemonic))
                    {
                        Console.WriteLine($"asm {path}:{asmLine.LineNumber}: invalid operand '{operand}' for {mnemonic}");
                        return;
                    }
                }
                else if (IsOperandMnemonic(mnemonic))
                {
                    Console.WriteLine($"asm {path}:{asmLine.LineNumber}: {mnemonic} requires an operand");
                    return;
                }

                if (_isa.TryAssemble(mnemonic, out var opcode))
                {
                    _memory.Write(asmLine.Address, opcode);
                    assembled++;
                    continue;
                }

                Console.WriteLine($"asm {path}:{asmLine.LineNumber}: unknown mnemonic '{mnemonic}'");
                return;
            }

            Console.WriteLine($"assembled {ToOctal(assembled)} words ORG={ToOctal(origin)}");
            _lastSymbols = new Dictionary<string, int>(symbols, StringComparer.OrdinalIgnoreCase);
            _lastAsmPath = path;
        }

        private static string StripComment(string line)
        {
            var index = line.IndexOf(';');
            return index >= 0 ? line[..index] : line;
        }

        private static string ToOctalCount(int value)
        {
            return "0" + Convert.ToString(value, 8);
        }

        private static string ToOctalStatus(ushort value)
        {
            return Convert.ToString(value, 8).PadLeft(3, '0');
        }

        private static string FormatStatusFlags(ushort value)
        {
            return $"{FormatFlag(value, 0x8000, 'M')} {FormatFlag(value, 0x4000, 'I')} {FormatFlag(value, 0x2000, 'T')} {FormatFlag(value, 0x1000, 'R')} {FormatFlag(value, 0x0800, 'O')} {FormatFlag(value, 0x0400, 'C')}";
        }

        private static char FormatFlag(ushort value, ushort mask, char flag)
        {
            return (value & mask) != 0 ? flag : char.ToLowerInvariant(flag);
        }

        private static string FormatStatusCondition(ushort value)
        {
            return (value & 0x0300) switch
            {
                0x0000 => "CCG",
                0x0100 => "CCL",
                0x0200 => "CCE",
                0x0300 => "CCI",
                _ => "CC?"
            };
        }

        private static string ExtractLabel(ref string line)
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex < 0)
            {
                return string.Empty;
            }

            var label = line[..colonIndex].Trim();
            line = line[(colonIndex + 1)..].Trim();
            return label;
        }

        private static List<string> SplitOperands(string operand)
        {
            var tokens = new List<string>();
            var parts = operand.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                var inner = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                tokens.AddRange(inner);
            }

            return tokens;
        }

        private static bool TryResolveOperand(
            string mnemonic,
            string operand,
            int address,
            Dictionary<string, int> symbols,
            out string resolved,
            out string error)
        {
            resolved = string.Empty;
            error = string.Empty;

            var parts = operand.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                error = $"invalid operand '{operand}'";
                return false;
            }

            var basePart = parts[0].Trim();
            var suffix = string.Empty;
            if (parts.Length > 1)
            {
                suffix = "," + string.Join(",", parts.Skip(1)).Trim();
            }

            if (mnemonic.Equals("LOAD", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveRelativeBase(basePart, address, symbols, out var loadBase, out error))
                {
                    return false;
                }

                resolved = loadBase + suffix;
                return true;
            }

            if (IsRelativeMnemonic(mnemonic))
            {
                if (!TryResolveRelativeBase(basePart, address, symbols, out var branchBase, out error))
                {
                    return false;
                }

                resolved = branchBase + suffix;
                return true;
            }

            if (!TryResolveBase(basePart, address, symbols, out var baseResolved, out error))
            {
                return false;
            }

            resolved = baseResolved + suffix;
            return true;
        }

        private static bool TryResolveRelativeBase(
            string basePart,
            int address,
            Dictionary<string, int> symbols,
            out string resolved,
            out string error)
        {
            resolved = string.Empty;
            error = string.Empty;

            if (TryParseNumber(basePart, out _))
            {
                resolved = basePart;
                return true;
            }

            if (TryResolvePcRelativeToken(basePart, out resolved))
            {
                return true;
            }

            if (TryResolveSymbolOrDot(basePart, address, symbols, out var target))
            {
                var displacement = target - address;
                var direction = displacement < 0 ? '-' : '+';
                var magnitude = Math.Abs(displacement);
                resolved = $".{direction}{Convert.ToString(magnitude, 8)}";
                return true;
            }

            error = $"unknown label '{basePart}'";
            return false;
        }

        private static bool TryResolveBase(
            string basePart,
            int address,
            Dictionary<string, int> symbols,
            out string resolved,
            out string error)
        {
            resolved = string.Empty;
            error = string.Empty;

            if (TryParseNumber(basePart, out _))
            {
                resolved = basePart;
                return true;
            }

            if (TryResolveSymbolOrDot(basePart, address, symbols, out var resolvedValue))
            {
                resolved = Convert.ToString(resolvedValue, 8);
                return true;
            }

            if (TryResolvePcRelativeToken(basePart, out resolved))
            {
                return true;
            }

            var plusIndex = basePart.IndexOf('+');
            if (plusIndex > 0 && plusIndex < basePart.Length - 1)
            {
                var prefix = basePart[..(plusIndex + 1)];
                var label = basePart[(plusIndex + 1)..].Trim();
                if (TryParseNumber(label, out var number))
                {
                    resolved = prefix + Convert.ToString(number, 8);
                    return true;
                }
                if (TryResolveSymbolOrDot(label, address, symbols, out var value))
                {
                    resolved = prefix + Convert.ToString(value, 8);
                    return true;
                }
            }

            var minusIndex = basePart.IndexOf('-');
            if (minusIndex > 0 && minusIndex < basePart.Length - 1)
            {
                var prefix = basePart[..(minusIndex + 1)];
                var label = basePart[(minusIndex + 1)..].Trim();
                if (TryParseNumber(label, out var number))
                {
                    resolved = prefix + Convert.ToString(number, 8);
                    return true;
                }
                if (TryResolveSymbolOrDot(label, address, symbols, out var value))
                {
                    resolved = prefix + Convert.ToString(value, 8);
                    return true;
                }
            }

            error = $"unknown label '{basePart}'";
            return false;
        }

        private static bool TryResolvePcRelativeToken(string token, out string resolved)
        {
            resolved = string.Empty;
            if (token.Length < 3)
            {
                return false;
            }

            if (token[0] != '.' && token[0] != 'P' && token[0] != 'p')
            {
                return false;
            }

            var sign = token[1];
            if (sign != '+' && sign != '-')
            {
                return false;
            }

            var magnitudeText = token[2..];
            if (!TryParseNumber(magnitudeText, out var magnitude))
            {
                return false;
            }

            resolved = $".{sign}{Convert.ToString(magnitude, 8)}";
            return true;
        }

        private static bool TryResolveSymbolOrDot(
            string token,
            int address,
            Dictionary<string, int> symbols,
            out int value)
        {
            if (token == ".")
            {
                value = address;
                return true;
            }

            return TryResolveQualifiedSymbol(token, symbols, out value);
        }

        private static bool TryResolveValue(string token, Dictionary<string, int> symbols, int address, out int value)
        {
            if (TryParseNumber(token, out value))
            {
                return true;
            }

            if (token == ".")
            {
                value = address;
                return true;
            }

            if (TryResolveQualifiedSymbol(token, symbols, out var symbolValue))
            {
                value = symbolValue;
                return true;
            }

            value = 0;
            return false;
        }

        private static bool TryResolveValue32(string token, Dictionary<string, int> symbols, int address, out uint value)
        {
            if (TryParseNumber32(token, out value))
            {
                return true;
            }

            if (token == ".")
            {
                value = (uint)address;
                return true;
            }

            if (TryResolveQualifiedSymbol(token, symbols, out var symbolValue))
            {
                value = (uint)symbolValue;
                return true;
            }

            value = 0;
            return false;
        }

        private static bool TryResolveValue64(string token, Dictionary<string, int> symbols, int address, out ulong value)
        {
            if (TryParseNumber64(token, out value))
            {
                return true;
            }

            if (token == ".")
            {
                value = (ulong)address;
                return true;
            }

            if (TryResolveQualifiedSymbol(token, symbols, out var symbolValue))
            {
                value = (ulong)symbolValue;
                return true;
            }

            value = 0;
            return false;
        }

        private static bool TryResolveQualifiedSymbol(string token, Dictionary<string, int> symbols, out int value)
        {
            if (symbols.TryGetValue(token, out value))
            {
                return true;
            }

            var dotIndex = token.LastIndexOf('.');
            if (dotIndex <= 0 || dotIndex == token.Length - 1)
            {
                return false;
            }

            var baseName = token[..dotIndex];
            var qualifier = token[(dotIndex + 1)..];
            if (!symbols.TryGetValue(baseName, out var baseValue))
            {
                return false;
            }

            if (qualifier.Equals("high", StringComparison.OrdinalIgnoreCase))
            {
                value = baseValue;
                return true;
            }

            if (qualifier.Equals("low", StringComparison.OrdinalIgnoreCase))
            {
                value = (baseValue + 1) & 0x7fff;
                return true;
            }

            return false;
        }

        private static bool IsOperandMnemonic(string mnemonic)
        {
            return mnemonic.Equals("BR", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BRO", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BN", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BL", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BLE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BG", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BNE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BGE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BA", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BOV", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BNOV", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BCY", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BNCY", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("WIO", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("RIO", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("LDI", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("LDXI", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("LOAD", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("STOR", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("LDD", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("STD", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("IABZ", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("IXBZ", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("DXBZ", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("SCAL", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("SXIT", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("ASL", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("ASR", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("LSL", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("LSR", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("DASL", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("DASR", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("DLSL", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("DLSR", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("INCM", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("DECM", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRelativeMnemonic(string mnemonic)
        {
            return mnemonic.Equals("BR", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BRO", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BN", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BL", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BLE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BG", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BNE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BGE", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BA", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BOV", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BNOV", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BCY", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("BNCY", StringComparison.OrdinalIgnoreCase);
        }

        private readonly struct AsmLine
        {
            public AsmLine(int lineNumber, int address, string mnemonic, string? operand, string rawLine)
            {
                LineNumber = lineNumber;
                Address = address;
                Mnemonic = mnemonic;
                Operand = operand;
                Values = new List<string>();
                RawLine = rawLine;
            }

            public AsmLine(int lineNumber, int address, string mnemonic, List<string> values, string rawLine)
            {
                LineNumber = lineNumber;
                Address = address;
                Mnemonic = mnemonic;
                Operand = null;
                Values = values;
                RawLine = rawLine;
            }

            public int LineNumber { get; }
            public int Address { get; }
            public string Mnemonic { get; }
            public string? Operand { get; }
            public List<string> Values { get; }
            public string RawLine { get; }
        }

        private void SetBreak(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var address))
            {
                Console.WriteLine("break <addr>");
                return;
            }

            _breakpoints.Add(address & 0x7fff);
            Console.WriteLine($"break set at {ToOctal(address)}");
        }

        private void ListBreaks()
        {
            if (_breakpoints.Count == 0)
            {
                Console.WriteLine("no breakpoints");
                return;
            }

            var list = _breakpoints.Select(ToOctal);
            Console.WriteLine(string.Join(" ", list));
        }

        private void ListDevices()
        {
            foreach (var pair in _devices.All())
            {
                Console.WriteLine($"{pair.Key}: {pair.Value.Name} ({pair.Value.Status()})");
            }
        }

        private void StatusDevice(TokenStream stream)
        {
            if (!stream.TryNext(out var name))
            {
                Console.WriteLine("status <dev>");
                return;
            }

            if (!_devices.TryGet(name, out var device))
            {
                Console.WriteLine($"unknown device {name}");
                return;
            }

            Console.WriteLine($"{name}: {device.Status()}");
        }

        private void SetLinePrinterColumns(TokenStream stream)
        {
            if (!stream.TryNextNumber(out var cols))
            {
                Console.WriteLine("lptcols <80|128>");
                return;
            }

            if (!_devices.TryGet("lpt", out var device) || device is not LinePrinterDevice printer)
            {
                Console.WriteLine("lptcols: device 'lpt' not available");
                return;
            }

            if (cols != 80 && cols != 128)
            {
                Console.WriteLine("lptcols: columns must be 80 or 128");
                return;
            }

            printer.SetColumns(cols);
            Console.WriteLine($"lptcols {cols}");
        }

        private void SetLinePrinterRadix(TokenStream stream)
        {
            if (!stream.TryNext(out var token))
            {
                Console.WriteLine("lptradix <0|2|8|A|D|F>");
                return;
            }

            if (!_devices.TryGet("lpt", out var device) || device is not LinePrinterDevice printer)
            {
                Console.WriteLine("lptradix: device 'lpt' not available");
                return;
            }

            var radix = NormalizeRadixToken(token);
            if (radix == '\0')
            {
                Console.WriteLine("lptradix: radix must be 0, 2, 8, A, D, or F");
                return;
            }

            printer.SetRadix(radix);
            Console.WriteLine($"lptradix {radix}");
        }

        private static char NormalizeRadixToken(string token)
        {
            token = token.Trim();
            if (token.Length == 1)
            {
                var ch = char.ToUpperInvariant(token[0]);
                if (ch == '0' || ch == '2' || ch == '8' || ch == 'A' || ch == 'D' || ch == 'F')
                {
                    return ch;
                }
            }

            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                if (value == 0 || value == 2 || value == 8)
                {
                    return (char)('0' + value);
                }
            }

            return '\0';
        }
        private void AttachDevice(TokenStream stream)
        {
            if (!stream.TryNext(out var name) || !stream.TryNext(out var path))
            {
                Console.WriteLine("attach <dev> <path> [new]");
                return;
            }

            if (!_devices.TryGet(name, out var device))
            {
                Console.WriteLine($"unknown device {name}");
                return;
            }

            if (device is not IAttachableDevice attachable)
            {
                Console.WriteLine($"device {name} cannot attach");
                return;
            }

            var createNew = stream.TryNext(out var flag) && flag.Equals("new", StringComparison.OrdinalIgnoreCase);
            attachable.Attach(path, createNew);
            Console.WriteLine($"{name} attached to {path}");
        }

        private void DetachDevice(TokenStream stream)
        {
            if (!stream.TryNext(out var name))
            {
                Console.WriteLine("detach <dev>");
                return;
            }

            if (!_devices.TryGet(name, out var device))
            {
                Console.WriteLine($"unknown device {name}");
                return;
            }

            if (device is not IAttachableDevice attachable)
            {
                Console.WriteLine($"device {name} cannot detach");
                return;
            }

            attachable.Detach();
            Console.WriteLine($"{name} detached");
        }

        private void ReadBlock(TokenStream stream)
        {
            if (!stream.TryNext(out var name) || !stream.TryNextNumber(out var block) || !stream.TryNextNumber(out var address))
            {
                Console.WriteLine("readblk <dev> <block> <addr>");
                return;
            }

            if (!_devices.TryGet(name, out var device) || device is not IBlockDevice blockDevice)
            {
                Console.WriteLine($"device {name} not block capable");
                return;
            }

            blockDevice.ReadBlock(block, _memory, address);
            Console.WriteLine($"read {name} block {ToOctal(block)} into {ToOctal(address)}");
        }

        private void WriteBlock(TokenStream stream)
        {
            if (!stream.TryNext(out var name) || !stream.TryNextNumber(out var block) || !stream.TryNextNumber(out var address))
            {
                Console.WriteLine("writeblk <dev> <block> <addr>");
                return;
            }

            if (!_devices.TryGet(name, out var device) || device is not IBlockDevice blockDevice)
            {
                Console.WriteLine($"device {name} not block capable");
                return;
            }

            blockDevice.WriteBlock(block, _memory, address);
            Console.WriteLine($"wrote {name} block {ToOctal(block)} from {ToOctal(address)}");
        }

        private Dictionary<int, ushort> CaptureWatchSnapshot()
        {
            if (_watches.Count == 0)
            {
                return new Dictionary<int, ushort>();
            }

            var snapshot = new Dictionary<int, ushort>(_watches.Count);
            foreach (var address in _watches)
            {
                snapshot[address] = _memory.Read(address);
            }

            return snapshot;
        }

        private void ReportWatchChanges(Dictionary<int, ushort> snapshot)
        {
            if (snapshot.Count == 0)
            {
                return;
            }

            foreach (var (address, before) in snapshot)
            {
                var after = _memory.Read(address);
                if (after != before)
                {
                    Console.WriteLine($"watch {ToOctal(address)}: {ToOctal(before)} -> {ToOctal(after)}");
                }
            }
        }

        private void RequireStack(int count)
        {
            if (_cpu.Sr < count)
            {
                throw new InvalidOperationException("stack underflow");
            }
        }

        private static bool TryParseNumber(string token, out int value)
        {
            if (token.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(token[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }

            if (token.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                return TryParseBase(token[1..], 16, out value);
            }

            return TryParseBase(token, 8, out value);
        }

        private static bool TryParseNumber32(string token, out uint value)
        {
            if (token.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                return uint.TryParse(token[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }

            if (token.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                return TryParseBase32(token[1..], 16, out value);
            }

            return TryParseBase32(token, 8, out value);
        }

        private static bool TryParseNumber64(string token, out ulong value)
        {
            if (token.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                return ulong.TryParse(token[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }

            if (token.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                return TryParseBase64(token[1..], 16, out value);
            }

            return TryParseBase64(token, 8, out value);
        }

        private static bool TryParseBase(string token, int numberBase, out int value)
        {
            try
            {
                value = Convert.ToInt32(token, numberBase);
                return true;
            }
            catch (Exception)
            {
                value = 0;
                return false;
            }
        }

        private static bool TryParseBase32(string token, int numberBase, out uint value)
        {
            try
            {
                value = Convert.ToUInt32(token, numberBase);
                return true;
            }
            catch (Exception)
            {
                value = 0;
                return false;
            }
        }

        private static bool TryParseBase64(string token, int numberBase, out ulong value)
        {
            try
            {
                value = Convert.ToUInt64(token, numberBase);
                return true;
            }
            catch (Exception)
            {
                value = 0;
                return false;
            }
        }

        private static string ToOctal(int value)
        {
            return Convert.ToString(value & 0xffff, 8).PadLeft(6, '0');
        }

        private static bool TryParseTextLiteral(string operand, out string text, out string error)
        {
            text = string.Empty;
            error = string.Empty;

            var trimmed = operand.Trim();
            if (!trimmed.StartsWith("/", StringComparison.Ordinal))
            {
                error = "TXT requires /text/";
                return false;
            }

            var lastSlash = trimmed.LastIndexOf('/');
            if (lastSlash == 0)
            {
                error = "TXT requires closing /";
                return false;
            }

            if (lastSlash < trimmed.Length - 1 && trimmed[(lastSlash + 1)..].Trim().Length != 0)
            {
                error = "TXT has trailing characters after closing /";
                return false;
            }

            text = trimmed[1..lastSlash];
            return true;
        }

        private static bool TryParseSlashDelimitedText(string firstToken, TokenStream stream, out string text, out string error)
        {
            text = string.Empty;
            error = string.Empty;

            if (!firstToken.StartsWith("/", StringComparison.Ordinal))
            {
                error = "text must start with /";
                return false;
            }

            var builder = new StringBuilder(firstToken);
            var hasClosing = firstToken.Length >= 2 && firstToken.EndsWith("/", StringComparison.Ordinal);
            while (!hasClosing)
            {
                if (!stream.TryNext(out var next))
                {
                    error = "unterminated /text/";
                    return false;
                }

                builder.Append(' ');
                builder.Append(next);
                hasClosing = next.EndsWith("/", StringComparison.Ordinal);
            }

            var combined = builder.ToString();
            if (combined.Length < 2 || combined[0] != '/' || combined[^1] != '/')
            {
                error = "text must be delimited as /text/";
                return false;
            }

            text = combined[1..^1];
            return true;
        }

        private static List<string> Tokenize(string line)
        {
            var tokens = new List<string>();
            var current = string.Empty;
            var inQuotes = false;

            foreach (var ch in line)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && char.IsWhiteSpace(ch))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current);
                        current = string.Empty;
                    }

                    continue;
                }

                current += ch;
            }

            if (current.Length > 0)
            {
                tokens.Add(current);
            }

            return tokens;
        }

        internal sealed class TokenStream
        {
            private readonly List<string> _tokens;
            private int _index;

            public TokenStream(List<string> tokens)
            {
                _tokens = tokens;
            }

            public bool HasMore => _index < _tokens.Count;

            public string Next()
            {
                return _tokens[_index++];
            }

            public bool TryNext(out string token)
            {
                if (!HasMore)
                {
                    token = string.Empty;
                    return false;
                }

                token = Next();
                return true;
            }

            public bool TryPeek(out string token)
            {
                if (!HasMore)
                {
                    token = string.Empty;
                    return false;
                }

                token = _tokens[_index];
                return true;
            }

            public bool TryNextNumber(out int value)
            {
                if (!TryNext(out var token))
                {
                    value = 0;
                    return false;
                }

                return TryParseNumber(token, out value);
            }
        }
    }
}
