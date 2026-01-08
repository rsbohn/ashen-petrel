using System;
using System.Collections.Generic;

namespace Ashen
{
    internal sealed class Hp3000Isa
    {
        private const ushort BranchMask = 0xF000;
        private const ushort BranchBase = 0xC000;
        private const ushort BranchBack = 0x0100;
        private const ushort BranchIndirect = 0x0400;
        private const ushort BranchIndexed = 0x0800;
        private const ushort BranchOffsetMask = 0x00FF;

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
            if (!mnemonic.Equals("BR", StringComparison.OrdinalIgnoreCase))
            {
                opcode = 0;
                return false;
            }

            return TryAssembleBranch(operand, out opcode);
        }

        private static string ToOctal(ushort value)
        {
            return Convert.ToString(value, 8).PadLeft(6, '0');
        }

        private static string ToOctal6(ushort value)
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
