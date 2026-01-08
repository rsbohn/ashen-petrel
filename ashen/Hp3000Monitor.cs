using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ashen
{
    internal sealed class Hp3000Monitor
    {
        private readonly Hp3000Cpu _cpu;
        private readonly Hp3000Memory _memory;
        private readonly DeviceRegistry _devices;
        private readonly Hp3000Isa _isa;
        private readonly HashSet<int> _breakpoints = new();
        private readonly Dictionary<string, Action<TokenStream>> _words;
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
                ["regs"] = _ => ShowRegs(),
                ["exam"] = ExamMemory,
                ["x"] = ExamMemory,
                ["deposit"] = DepositMemory,
                ["dep"] = DepositMemory,
                ["d"] = DepositMemory,
                ["dis"] = Disassemble,
                ["break"] = SetBreak,
                ["breaks"] = _ => ListBreaks(),
                ["asm"] = Assemble,
                ["devs"] = _ => ListDevices(),
                ["status"] = StatusDevice,
                ["attach"] = AttachDevice,
                ["detach"] = DetachDevice,
                ["readblk"] = ReadBlock,
                ["writeblk"] = WriteBlock
            };
        }

        private void ShowHelp()
        {
            Console.WriteLine("Core: help words reset go run step exam deposit dis break breaks asm");
            Console.WriteLine("Stack: . dup drop swap over + - and or xor invert ! @");
            Console.WriteLine("Devices: devs status attach detach readblk writeblk");
            Console.WriteLine("Numbers are octal; use # for decimal.");
            Console.WriteLine("Assembler: asm <mnemonic> [addr]");
        }

        private void ListWords()
        {
            var words = _words.Keys.OrderBy(word => word);
            Console.WriteLine(string.Join(" ", words));
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
            var ran = _cpu.Run(maxSteps);
            Console.WriteLine($"ran {ran} steps");
            ReportHaltReason();
        }

        private void RunCpu(TokenStream stream)
        {
            var steps = stream.TryNextNumber(out var maxSteps) ? maxSteps : 1000;
            var ran = _cpu.Run(maxSteps);
            Console.WriteLine($"ran {ran} steps");
            ReportHaltReason();
        }

        private void StepCpu(TokenStream stream)
        {
            var steps = stream.TryNextNumber(out var maxSteps) ? maxSteps : 1;
            var ran = 0;
            for (var i = 0; i < steps; i++)
            {
                if (!_cpu.Step())
                {
                    break;
                }

                ran++;
            }

            Console.WriteLine($"stepped {ran} steps");
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

            Console.WriteLine($"traced {ran} steps");
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

        private void ShowRegs()
        {
            Console.WriteLine($"PC={ToOctal(_cpu.Pc)} SP={ToOctal(_cpu.Sp)} SM={ToOctal(_cpu.Sm)} SR={_cpu.Sr} RA={ToOctal(_cpu.Ra)} RB={ToOctal(_cpu.Rb)} RC={ToOctal(_cpu.Rc)} RD={ToOctal(_cpu.Rd)} X={ToOctal(_cpu.X)} HALT={(_cpu.Halted ? "1" : "0")} STACK={_cpu.Sr}");
        }

        private void ReportHaltReason()
        {
            if (_cpu.Halted && !string.IsNullOrWhiteSpace(_cpu.HaltReason))
            {
                Console.WriteLine($"halt: {_cpu.HaltReason}");
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
                Console.WriteLine("asm <mnemonic> [addr]");
                return;
            }

            var address = stream.TryNextNumber(out var addr) ? addr : _cpu.Pc;
            if (token.Equals("BR", StringComparison.OrdinalIgnoreCase))
            {
                if (!stream.TryNext(out var operand))
                {
                    Console.WriteLine("asm BR <operand> [addr]");
                    return;
                }

                if (!_isa.TryAssemble(token, operand, out var branchOpcode))
                {
                    Console.WriteLine($"unknown mnemonic {token} {operand}");
                    return;
                }

                _memory.Write(address, branchOpcode);
                var branchLine = $"{token} {operand} -> {ToOctal(address)}";

                Console.WriteLine(branchLine);
                return;
            }

            if (!_isa.TryAssemble(token, out var opcode))
            {
                Console.WriteLine($"unknown mnemonic {token}");
                return;
            }

            _memory.Write(address, opcode);
            var assembledLine = $"{token} -> {ToOctal(address)}";

            Console.WriteLine(assembledLine);
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

            return TryParseBase(token, 8, out value);
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

        private static string ToOctal(int value)
        {
            return Convert.ToString(value & 0xffff, 8).PadLeft(6, '0');
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
