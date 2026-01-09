using System;
using System.Collections.Generic;

namespace Ashen
{
    internal sealed class Hp3000Isa
    {
        private const ushort HaltWord = 0x30F0;
        private const ushort BranchMask = 0xF000;
        private const ushort BranchBase = 0xC000;
        private const ushort BranchBack = 0x0100;
        private const ushort BranchIndirect = 0x0400;
        private const ushort BranchIndexed = 0x0800;
        private const ushort BranchOffsetMask = 0x00FF;
        private const ushort LoadMask = 0x7000;
        private const ushort LoadBase = 0x4000;
        private const ushort LoadXFlag = 0x0400;
        private const ushort LoadIFlag = 0x0200;
        private const ushort LoadDispMask = 0x01FF;
        private const ushort LoadDispSign = 0x0100;
        private const ushort LoadDispValueMask = 0x00FF;
        private const ushort StorMask = 0xF200;
        private const ushort StorBase = 0x5200;
        private const ushort StorXFlag = 0x0800;
        private const ushort StorIFlag = 0x0400;
        private const ushort StorDispMask = 0x01FF;
        private const ushort ImmediateMask = 0xFF00;
        private const ushort ImmediateLdiBase = 0x2200;
        private const ushort ImmediateLdXiBase = 0x2300;
        private const ushort ImmediateValueMask = 0x00FF;

        private static readonly string[] Format2Mnemonics =
        {
            "NOP",  "DELB", "DDEL", "ZROX", "INCX", "DECX", "ZERO", "DZRO",
            "DCMP", "DADD", "DSUB", "MPYL", "DIVL", "DNEG", "DXCH", "CMP",
            "ADD",  "SUB",  "MPY",  "DIV",  "NEG",  "TEST", "STBX", "DTST",
            "DFLT", "BTST", "XCH",  "INCA", "DECA", "XAX",  "ADAX", "ADXA",
            "DEL",  "ZROB", "LDXB", "STAX", "LDXA", "DUP",  "DDUP", "FLT",
            "FCMP", "FADD", "FSUB", "FMPY", "FDIV", "FNEG", "CAB",  "LCMP",
            "LADD", "LSUB", "LMPY", "LDIV", "NOT",  "OR",   "XOR",  "AND",
            "FIXR", "FIXT", "UNK",  "INCB", "DECB", "XBX",  "ADBX", "ADXB"
        };

        private readonly Dictionary<ushort, string> _mnemonics = new();
        private readonly Dictionary<string, ushort> _opcodes =
            new(StringComparer.OrdinalIgnoreCase);

        public Hp3000Isa()
        {
            for (ushort opcode = 0; opcode < Format2Mnemonics.Length; opcode++)
            {
                var mnemonic = Format2Mnemonics[opcode];
                if (string.IsNullOrWhiteSpace(mnemonic))
                {
                    continue;
                }

                _mnemonics[opcode] = mnemonic;
                if (!_opcodes.ContainsKey(mnemonic))
                {
                    _opcodes[mnemonic] = opcode;
                }
            }

            _mnemonics[HaltWord] = "HALT 0";
            _opcodes["HALT"] = HaltWord;
        }

        public bool TryExecute(ushort opcode, Hp3000Cpu cpu)
        {
            switch (opcode)
            {
                case 0x0000: // NOP
                    return true;
                case 0x0001: // DELB
                    return true;
                case 0x0002: // DDEL
                    {
                        cpu.Pop();
                        cpu.Pop();
                        return true;
                    }
                case 0x0006: // ZERO
                    {
                        cpu.Push(0);
                        return true;
                    }
                case 0x0007: // DZRO
                    {
                        cpu.Push(0);
                        cpu.Push(0);
                        return true;
                    }
                case 0x0010: // ADD
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push((ushort)(b + a));
                        return true;
                    }
                case 0x0011: // SUB
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push((ushort)(b - a));
                        return true;
                    }
                case 0x0012: // MPY
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push((ushort)(b * a));
                        return true;
                    }
                case 0x0013: // DIV
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push(a == 0 ? (ushort)0 : (ushort)(b / a));
                        return true;
                    }
                case 0x0014: // NEG
                    {
                        var a = cpu.Pop();
                        cpu.Push((ushort)(0 - a));
                        return true;
                    }
                case 0x0016: // STBX
                    {
                        cpu.X = cpu.PeekSecond();
                        return true;
                    }
                case 0x001A: // XCH
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push(a);
                        cpu.Push(b);
                        return true;
                    }
                case 0x001B: // INCA
                    {
                        var value = cpu.Peek();
                        cpu.ReplaceTop((ushort)(value + 1));
                        return true;
                    }
                case 0x001C: // DECA
                    {
                        var value = cpu.Peek();
                        cpu.ReplaceTop((ushort)(value - 1));
                        return true;
                    }
                case 0x0020: // DEL
                    {
                        cpu.Pop();
                        return true;
                    }
                case 0x0022: // LDXB
                    {
                        cpu.ReplaceSecond(cpu.X);
                        return true;
                    }
                case 0x0023: // STAX
                    {
                        var value = cpu.Pop();
                        cpu.X = value;
                        return true;
                    }
                case 0x0024: // LDXA
                    {
                        cpu.Push(cpu.X);
                        return true;
                    }
                case 0x0025: // DUP
                    {
                        var value = cpu.Pop();
                        cpu.Push(value);
                        cpu.Push(value);
                        return true;
                    }
                case 0x0026: // DDUP
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push(b);
                        cpu.Push(a);
                        cpu.Push(b);
                        cpu.Push(a);
                        return true;
                    }
                case 0x0034: // NOT
                    {
                        var a = cpu.Pop();
                        cpu.Push((ushort)~a);
                        return true;
                    }
                case 0x0035: // OR
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push((ushort)(b | a));
                        return true;
                    }
                case 0x0036: // XOR
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push((ushort)(b ^ a));
                        return true;
                    }
                case 0x0037: // AND
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        cpu.Push((ushort)(b & a));
                        return true;
                    }
                default:
                    return opcode < Format2Mnemonics.Length;
            }
        }

        public bool TryExecuteWord(ushort word, Hp3000Cpu cpu)
        {
            if (word == HaltWord)
            {
                cpu.Halt("HALT");
                return true;
            }

            var immediateKind = (ushort)(word & ImmediateMask);
            if (immediateKind == ImmediateLdiBase)
            {
                cpu.Push((ushort)(word & ImmediateValueMask));
                return true;
            }

            if (immediateKind == ImmediateLdXiBase)
            {
                cpu.X = (ushort)(word & ImmediateValueMask);
                return true;
            }

            if ((word & LoadMask) == LoadBase)
            {
                var displacement = word & LoadDispValueMask;
                if ((word & LoadDispSign) != 0)
                {
                    displacement = (ushort)(-displacement);
                }

                var loadInstructionAddress = (cpu.Pc - 1) & 0x7fff;
                var loadTarget = (loadInstructionAddress + displacement) & 0x7fff;
                if ((word & LoadXFlag) != 0)
                {
                    loadTarget = (loadTarget + cpu.X) & 0x7fff;
                }

                if ((word & LoadIFlag) != 0)
                {
                    loadTarget = cpu.ReadWord(loadTarget) & 0x7fff;
                }

                cpu.Push(cpu.ReadWord(loadTarget));
                return true;
            }

            if ((word & StorMask) == StorBase)
            {
                var displacement = word & StorDispMask;
                var storTarget = (cpu.Db + displacement) & 0x7fff;
                if ((word & StorXFlag) != 0)
                {
                    storTarget = (storTarget + cpu.X) & 0x7fff;
                }

                if ((word & StorIFlag) != 0)
                {
                    storTarget = cpu.ReadWord(storTarget) & 0x7fff;
                }

                var value = cpu.Pop();
                cpu.WriteWord(storTarget, value);
                return true;
            }

            if ((word & BranchMask) != BranchBase)
            {
                return false;
            }

            var offset = word & BranchOffsetMask;
            var instructionAddress = (cpu.Pc - 1) & 0x7fff;
            var target = (word & BranchBack) != 0
                ? instructionAddress - offset
                : instructionAddress + offset;
            target &= 0x7fff;

            if ((word & BranchIndexed) != 0)
            {
                target = (target + cpu.X) & 0x7fff;
            }

            if ((word & BranchIndirect) != 0)
            {
                target = cpu.ReadWord(target) & 0x7fff;
            }

            cpu.Pc = target;
            return true;
        }

        public string Disassemble(ushort opcode)
        {
            if (opcode == HaltWord)
            {
                return "HALT 0";
            }

            var immediateKind = (ushort)(opcode & ImmediateMask);
            if (immediateKind == ImmediateLdiBase || immediateKind == ImmediateLdXiBase)
            {
                return DisassembleImmediate(opcode);
            }

            if ((opcode & LoadMask) == LoadBase)
            {
                return DisassembleLoad(opcode);
            }

            if ((opcode & StorMask) == StorBase)
            {
                return DisassembleStor(opcode);
            }

            if ((opcode & BranchMask) == BranchBase)
            {
                return DisassembleBranch(opcode);
            }

            var firstOpcode = (ushort)(opcode & 0x003f);
            var secondOpcode = (ushort)((opcode >> 6) & 0x003f);
            var firstMnemonic = _mnemonics.TryGetValue(firstOpcode, out var first)
                ? first
                : $"DATA {ToOctal6(firstOpcode)}";
            var secondMnemonic = _mnemonics.TryGetValue(secondOpcode, out var second)
                ? second
                : $"DATA {ToOctal6(secondOpcode)}";

            return $"{firstMnemonic}, {secondMnemonic}";
        }

        public bool TryAssemble(string token, out ushort opcode)
        {
            return _opcodes.TryGetValue(token, out opcode);
        }

        public bool TryAssemble(string mnemonic, string operand, out ushort opcode)
        {
            if (mnemonic.Equals("BR", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleBranch(operand, out opcode);
            }

            if (mnemonic.Equals("LOAD", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleLoad(operand, out opcode);
            }

            if (mnemonic.Equals("STOR", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleStor(operand, out opcode);
            }

            if (mnemonic.Equals("LDI", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleImmediate(operand, ImmediateLdiBase, out opcode);
            }

            if (mnemonic.Equals("LDXI", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleImmediate(operand, ImmediateLdXiBase, out opcode);
            }

            opcode = 0;
            return false;
        }

        private static string ToOctal(ushort value)
        {
            return Convert.ToString(value, 8).PadLeft(6, '0');
        }

        private static string ToOctal6(ushort value)
        {
            return Convert.ToString(value, 8).PadLeft(3, '0');
        }

        private static string ToOctal3(ushort value)
        {
            return Convert.ToString(value, 8).PadLeft(3, '0');
        }

        private static string DisassembleBranch(ushort word)
        {
            var offset = word & BranchOffsetMask;
            var offsetText = Convert.ToString(offset, 8);
            var direction = (word & BranchBack) != 0 ? '-' : '+';
            var suffix = "";

            if ((word & BranchIndirect) != 0)
            {
                suffix += ",I";
            }

            if ((word & BranchIndexed) != 0)
            {
                suffix += ",X";
            }

            return $"BR P{direction}{offsetText}{suffix}";
        }

        private static string DisassembleImmediate(ushort word)
        {
            var value = (ushort)(word & ImmediateValueMask);
            var kind = (ushort)(word & ImmediateMask);
            var mnemonic = kind == ImmediateLdXiBase ? "LDXI" : "LDI";
            return $"{mnemonic} {ToOctal3(value)}";
        }

        private static string DisassembleLoad(ushort word)
        {
            var displacement = (ushort)(word & LoadDispValueMask);
            var direction = '+';
            if ((word & LoadDispSign) != 0)
            {
                direction = '-';
            }

            var suffix = "";
            if ((word & LoadIFlag) != 0)
            {
                suffix += ",I";
            }

            if ((word & LoadXFlag) != 0)
            {
                suffix += ",X";
            }

            var offsetText = Convert.ToString(displacement, 8);
            return $"LOAD P{direction}{offsetText}{suffix}";
        }

        private static string DisassembleStor(ushort word)
        {
            var displacement = (ushort)(word & StorDispMask);
            var suffix = "";
            if ((word & StorIFlag) != 0)
            {
                suffix += ",I";
            }

            if ((word & StorXFlag) != 0)
            {
                suffix += ",X";
            }

            var offsetText = Convert.ToString(displacement, 8);
            return $"STOR DB+{offsetText}{suffix}";
        }

        private static bool TryAssembleBranch(string operand, out ushort opcode)
        {
            opcode = 0;
            if (string.IsNullOrWhiteSpace(operand))
            {
                return false;
            }

            var parts = operand.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return false;
            }

            var basePart = parts[0].Trim();
            if (basePart.Length < 3 || (basePart[0] != 'P' && basePart[0] != 'p'))
            {
                return false;
            }

            var direction = basePart[1];
            if (direction != '+' && direction != '-')
            {
                return false;
            }

            if (!TryParseOctal(basePart[2..], out var offset) || offset > BranchOffsetMask)
            {
                return false;
            }

            opcode = (ushort)(BranchBase | offset);
            if (direction == '-')
            {
                opcode |= BranchBack;
            }

            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= BranchIndirect;
                }
                else if (suffix.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= BranchIndexed;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleLoad(string operand, out ushort opcode)
        {
            opcode = 0;
            if (string.IsNullOrWhiteSpace(operand))
            {
                return false;
            }

            var parts = operand.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return false;
            }

            var basePart = parts[0].Trim();
            if (basePart.Length < 1)
            {
                return false;
            }

            var direction = '+';
            var offsetText = basePart;
            if (basePart[0] == 'P' || basePart[0] == 'p')
            {
                if (basePart.Length < 3)
                {
                    return false;
                }

                direction = basePart[1];
                if (direction != '+' && direction != '-')
                {
                    return false;
                }

                offsetText = basePart[2..];
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > LoadDispValueMask)
            {
                return false;
            }

            opcode = (ushort)(LoadBase | offset);
            if (direction == '-')
            {
                opcode |= LoadDispSign;
            }

            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= LoadIFlag;
                }
                else if (suffix.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= LoadXFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleStor(string operand, out ushort opcode)
        {
            opcode = 0;
            if (string.IsNullOrWhiteSpace(operand))
            {
                return false;
            }

            var parts = operand.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return false;
            }

            var basePart = parts[0].Trim();
            if (basePart.Length < 1)
            {
                return false;
            }

            var offsetText = basePart;
            if (basePart.StartsWith("DB", StringComparison.OrdinalIgnoreCase))
            {
                if (basePart.Length < 4 || basePart[2] != '+')
                {
                    return false;
                }

                offsetText = basePart[3..];
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > StorDispMask)
            {
                return false;
            }

            opcode = (ushort)(StorBase | offset);
            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= StorIFlag;
                }
                else if (suffix.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= StorXFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleImmediate(string operand, ushort baseOpcode, out ushort opcode)
        {
            opcode = 0;
            if (!TryParseOctal(operand, out var value))
            {
                return false;
            }

            if (value > ImmediateValueMask)
            {
                return false;
            }

            opcode = (ushort)(baseOpcode | value);
            return true;
        }

        private static bool TryParseOctal(string token, out ushort value)
        {
            try
            {
                value = Convert.ToUInt16(token, 8);
                return true;
            }
            catch (Exception)
            {
                value = 0;
                return false;
            }
        }
    }
}
