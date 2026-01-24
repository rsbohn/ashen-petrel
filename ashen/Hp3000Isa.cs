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
        private const ushort LoadMask = 0xF000;
        private const ushort LoadBase = 0x4000;
        private const ushort LoadXFlag = 0x0800;
        private const ushort LoadIFlag = 0x0400;
        private const ushort LoadDispMask = 0x01FF;
        private const ushort LoadDispSign = 0x0100;
        private const ushort LoadDispValueMask = 0x00FF;
        private const ushort IabzMask = 0x7FC0;
        private const ushort IabzBase = 0x11C0; // 010700 octal
        private const ushort IabzIndirectFlag = 0x0080;
        private const ushort IabzBackFlag = 0x0020;
        private const ushort IabzDispMask = 0x001F;
        private const ushort IxbzMask = 0xF7C0;
        private const ushort IxbzBase = 0x1280;
        private const ushort IxbzIndirectFlag = 0x0800;
        private const ushort IxbzBackFlag = 0x0020;
        private const ushort IxbzDispMask = 0x001F;
        private const ushort DxbzMask = 0xF7C0;
        private const ushort DxbzBase = 0x12C0;
        private const ushort DxbzIndirectFlag = 0x0800;
        private const ushort DxbzBackFlag = 0x0020;
        private const ushort DxbzDispMask = 0x001F;
        private const ushort BovMask = 0xFFC0;
        private const ushort BovBase = 0x1600;
        private const ushort BovDispSign = 0x0020;
        private const ushort BovDispMask = 0x001F;
        private const ushort BnovMask = 0xFFC0;
        private const ushort BnovBase = 0x1680;
        private const ushort BnovDispSign = 0x0020;
        private const ushort BnovDispMask = 0x001F;
        private const ushort BcyMask = 0xFFC0;
        private const ushort BcyBase = 0x1300;
        private const ushort BcyDispSign = 0x0020;
        private const ushort BcyDispMask = 0x001F;
        private const ushort BncyMask = 0xFFC0;
        private const ushort BncyBase = 0x1340;
        private const ushort BncyDispSign = 0x0020;
        private const ushort BncyDispMask = 0x001F;
        private const ushort BroMask = 0xFFC0;
        private const ushort BroBase = 0x1780;
        private const ushort BroDispSign = 0x0020;
        private const ushort BroDispMask = 0x001F;
        private const ushort CondBranchMask = 0xFE00;
        private const ushort CondBranchBase = 0xC200;
        private const ushort CondBranchCcfMask = 0x01C0;
        private const ushort CondBranchDispSign = 0x0020;
        private const ushort CondBranchDispMask = 0x001F;
        private const ushort StorMask = 0xF200;
        private const ushort StorBase = 0x5200;
        private const ushort StorXFlag = 0x0800;
        private const ushort StorIFlag = 0x0400;
        private const ushort StorDispMask = 0x01FF;
        private const ushort IncmMask = 0xF200;
        private const ushort IncmBase = 0xA000; // 120000 octal
        private const ushort DecmBase = 0xA200; // 121000 octal
        private const ushort LddMask = 0xF200;
        private const ushort LddBase = 0xD200;
        private const ushort StdMask = 0xF200;
        private const ushort StdBase = 0xE200;
        private const ushort ImmediateMask = 0xFF00;
        private const ushort ImmediateLdiBase = 0x2200;
        private const ushort ImmediateLdXiBase = 0x2300;
        private const ushort ImmediateValueMask = 0x00FF;
        private const ushort ScalBase = 0x3100; // 030400 octal
        private const ushort ScalMask = 0xFF00;
        private const ushort ScalOperandMask = 0x00FF;
        private const ushort SxitBase = 0x3400; // 032000 octal
        private const ushort SxitMask = 0xFF00;
        private const ushort SxitOperandMask = 0x00FF;
        private const ushort ShiftMask = 0xFFC0;
        private const ushort AslBase = 0x1000; // 010000 octal
        private const ushort AsrBase = 0x1040; // 010100 octal
        private const ushort LslBase = 0x1080; // 010200 octal
        private const ushort LsrBase = 0x10C0; // 010300 octal
        private const ushort ShiftCountMask = 0x003F;
        private const ushort DaslMask = 0xFDC0;
        private const ushort DaslBase = 0x1400; // 012000 octal
        private const ushort DasrBase = 0x1440; // 012100 octal
        private const ushort DlslBase = 0x1480; // 012200 octal
        private const ushort DlsrBase = 0x14C0; // 012300 octal
        private const ushort DaslXFlag = 0x0200;
        private const ushort DdivWord = 0x2179; // 020571 octal
        private const ushort StatusCcl = 0x0100;
        private const ushort StatusCce = 0x0200;
        private const ushort StatusCci = 0x0300;
        private const ushort StatusCcMask = 0x0300;
        private const ushort StatusCcg = 0x0000;
        private const ushort StatusO = 0x0800;
        private const ushort StatusC = 0x0400;

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
            _opcodes["DDIV"] = DdivWord;
        }

        public bool TryExecute(ushort opcode, Hp3000Cpu cpu)
        {
            switch (opcode)
            {
                case 0x0000: // NOP
                    return true;
                case 0x0001: // DELB
                    {
                        cpu.DropSecond();
                        return true;
                    }
                case 0x0002: // DDEL
                    {
                        cpu.Pop();
                        cpu.Pop();
                        return true;
                    }
                case 0x0003: // ZROX
                    {
                        cpu.X = 0;
                        return true;
                    }
                case 0x0004: // INCX
                    {
                        cpu.X = (ushort)(cpu.X + 1);
                        return true;
                    }
                case 0x0005: // DECX
                    {
                        cpu.X = (ushort)(cpu.X - 1);
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
                case 0x0008: // DCMP
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        var c = cpu.Pop();
                        var d = cpu.Pop();
                        var right = (uint)(((uint)b << 16) | a);
                        var left = (uint)(((uint)d << 16) | c);
                        UpdateDoubleCompareFlags(cpu, left, right);
                        return true;
                    }
                case 0x0009: // DADD
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        var c = cpu.Pop();
                        var d = cpu.Pop();
                        var right = ((uint)b << 16) | a;
                        var left = ((uint)d << 16) | c;
                        var sum = (ulong)left + right;
                        var result = (uint)sum;
                        var high = (ushort)(result >> 16);
                        var low = (ushort)(result & 0xFFFF);
                        cpu.Push(high);
                        cpu.Push(low);
                        UpdateDoubleAddFlags(cpu, left, right, result, sum > 0xFFFFFFFF);
                        return true;
                    }
                case 0x000C: // DIVL
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        var c = cpu.Pop();
                        uint quotient = 0;
                        uint remainder = 0;
                        var overflow = false;
                        if (a == 0)
                        {
                            overflow = true;
                        }
                        else
                        {
                            var dividend = ((uint)c << 16) | b;
                            quotient = dividend / a;
                            remainder = dividend % a;
                            if (quotient > 0xFFFF)
                            {
                                overflow = true;
                            }
                        }

                        cpu.Push((ushort)quotient);
                        cpu.Push((ushort)remainder);
                        UpdateDivFlags(cpu, (ushort)quotient, overflow);
                        return true;
                    }
                case 0x0010: // ADD
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        var sum = (uint)(b + a);
                        var result = (ushort)sum;
                        cpu.Push(result);
                        UpdateAddSubFlags(cpu, result, sum > 0xFFFF, IsAddOverflow(b, a, result));
                        return true;
                    }
                case 0x0011: // SUB
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        var result = (ushort)(b - a);
                        cpu.Push(result);
                        UpdateAddSubFlags(cpu, result, b < a, IsSubOverflow(b, a, result));
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
                        if (a == 0)
                        {
                            cpu.Push(0);
                            cpu.Push(0);
                            UpdateDivFlags(cpu, 0, overflow: true);
                            return true;
                        }

                        var quotient = (ushort)(b / a);
                        var remainder = (ushort)(b % a);
                        cpu.Push(quotient);
                        cpu.Push(remainder);
                        UpdateDivFlags(cpu, quotient, overflow: false);
                        return true;
                    }
                case 0x0032: // LMPY
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        var product = (uint)(b * a);
                        var high = (ushort)(product >> 16);
                        var low = (ushort)(product & 0xFFFF);
                        cpu.Push(high);
                        cpu.Push(low);
                        UpdateLogicalMultiplyFlags(cpu, low, high != 0);
                        return true;
                    }
                case 0x0033: // LDIV
                    {
                        var a = cpu.Pop();
                        var b = cpu.Pop();
                        var c = cpu.Pop();
                        uint quotient = 0;
                        uint remainder = 0;
                        var overflow = false;
                        if (a == 0)
                        {
                            overflow = true;
                        }
                        else
                        {
                            var dividend = ((uint)c << 16) | b;
                            quotient = dividend / a;
                            remainder = dividend % a;
                            if (quotient > 0xFFFF)
                            {
                                overflow = true;
                            }
                        }

                        cpu.Push((ushort)quotient);
                        cpu.Push((ushort)remainder);
                        UpdateDivFlags(cpu, (ushort)quotient, overflow);
                        return true;
                    }
                case 0x0014: // NEG
                    {
                        var a = cpu.Pop();
                        cpu.Push((ushort)(0 - a));
                        return true;
                    }
                case 0x0015: // TEST
                    {
                        var value = cpu.Peek();
                        UpdateCcFlags(cpu, value);
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
                case 0x001D: // XAX
                    {
                        var temp = cpu.Peek();
                        cpu.ReplaceTop(cpu.X);
                        cpu.X = temp;
                        return true;
                    }
                case 0x001E: // ADAX
                    {
                        var a = cpu.Pop();
                        var x = cpu.X;
                        var sum = (uint)(x + a);
                        var result = (ushort)sum;
                        cpu.X = result;
                        UpdateAddSubFlags(cpu, result, sum > 0xFFFF, IsAddOverflow(x, a, result));
                        return true;
                    }
                case 0x001F: // ADXA
                    {
                        var a = cpu.Peek();
                        var sum = (uint)(a + cpu.X);
                        var result = (ushort)sum;
                        cpu.ReplaceTop(result);
                        UpdateAddSubFlags(cpu, result, sum > 0xFFFF, IsAddOverflow(a, cpu.X, result));
                        return true;
                    }
                case 0x0020: // DEL
                    {
                        cpu.Pop();
                        return true;
                    }
                case 0x0021: // ZROB
                    {
                        cpu.ReplaceSecond(0);
                        UpdateCcFlags(cpu, 0);
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
                        var result = (ushort)~a;
                        cpu.Push(result);
                        UpdateCcFlags(cpu, result);
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
                case 0x003B: // INCB
                    {
                        var value = cpu.PeekSecond();
                        cpu.ReplaceSecond((ushort)(value + 1));
                        return true;
                    }
                case 0x003C: // DECB
                    {
                        var value = cpu.PeekSecond();
                        cpu.ReplaceSecond((ushort)(value - 1));
                        return true;
                    }
                case 0x003D: // XBX
                    {
                        var temp = cpu.PeekSecond();
                        cpu.ReplaceSecond(cpu.X);
                        cpu.X = temp;
                        return true;
                    }
                case 0x003E: // ADBX
                    {
                        var b = cpu.PeekSecond();
                        var x = cpu.X;
                        var sum = (uint)(x + b);
                        var result = (ushort)sum;
                        cpu.X = result;
                        UpdateAddSubFlags(cpu, result, sum > 0xFFFF, IsAddOverflow(x, b, result));
                        return true;
                    }
                case 0x003F: // ADXB
                    {
                        var b = cpu.PeekSecond();
                        var sum = (uint)(b + cpu.X);
                        var result = (ushort)sum;
                        cpu.ReplaceSecond(result);
                        UpdateAddSubFlags(cpu, result, sum > 0xFFFF, IsAddOverflow(b, cpu.X, result));
                        return true;
                    }
                default:
                    return opcode < Format2Mnemonics.Length;
            }
        }

        public bool TryExecuteWord(ushort word, Hp3000Cpu cpu)
        {
            if (TryExecuteSpecial(word, cpu))
            {
                return true;
            }

            if ((word & ScalMask) == ScalBase)
            {
                ExecuteScal((ushort)(word & ScalOperandMask), cpu);
                return true;
            }

            if ((word & SxitMask) == SxitBase)
            {
                ExecuteSxit((ushort)(word & SxitOperandMask), cpu);
                return true;
            }

            if ((word & ShiftMask) == AslBase)
            {
                ExecuteAsl((ushort)(word & ShiftCountMask), cpu);
                return true;
            }

            if ((word & ShiftMask) == AsrBase)
            {
                ExecuteAsr((ushort)(word & ShiftCountMask), cpu);
                return true;
            }

            if ((word & ShiftMask) == LslBase)
            {
                ExecuteLsl((ushort)(word & ShiftCountMask), cpu);
                return true;
            }

            if ((word & ShiftMask) == LsrBase)
            {
                ExecuteLsr((ushort)(word & ShiftCountMask), cpu);
                return true;
            }

            if ((word & DaslMask) == DaslBase)
            {
                ExecuteDasl((ushort)(word & ShiftCountMask), (word & DaslXFlag) != 0, cpu);
                return true;
            }

            if ((word & DaslMask) == DasrBase)
            {
                ExecuteDasr((ushort)(word & ShiftCountMask), (word & DaslXFlag) != 0, cpu);
                return true;
            }

            if ((word & DaslMask) == DlslBase)
            {
                ExecuteDlsl((ushort)(word & ShiftCountMask), (word & DaslXFlag) != 0, cpu);
                return true;
            }

            if ((word & DaslMask) == DlsrBase)
            {
                ExecuteDlsr((ushort)(word & ShiftCountMask), (word & DaslXFlag) != 0, cpu);
                return true;
            }

            if ((word & IncmMask) == IncmBase)
            {
                ExecuteMemAdjust(word, increment: true, cpu);
                return true;
            }

            if ((word & IncmMask) == DecmBase)
            {
                ExecuteMemAdjust(word, increment: false, cpu);
                return true;
            }

            if (word == DdivWord)
            {
                var a = cpu.Pop();
                var b = cpu.Pop();
                var c = cpu.Pop();
                var d = cpu.Pop();
                var divisor = ((uint)b << 16) | a;
                var dividend = ((uint)d << 16) | c;
                uint quotient = 0;
                uint remainder = 0;
                var overflow = false;
                if (divisor == 0)
                {
                    overflow = true;
                }
                else
                {
                    quotient = dividend / divisor;
                    remainder = dividend % divisor;
                }

                cpu.Push((ushort)(quotient >> 16));
                cpu.Push((ushort)(quotient & 0xFFFF));
                cpu.Push((ushort)(remainder >> 16));
                cpu.Push((ushort)(remainder & 0xFFFF));
                UpdateDoubleDivFlags(cpu, quotient, overflow);
                return true;
            }

            var immediateKind = (ushort)(word & ImmediateMask);
            if (immediateKind == ImmediateLdiBase)
            {
                var value = (ushort)(word & ImmediateValueMask);
                cpu.Push(value);
                UpdateCcFlags(cpu, value);
                return true;
            }

            if (immediateKind == ImmediateLdXiBase)
            {
                cpu.X = (ushort)(word & ImmediateValueMask);
                return true;
            }

            if ((word & IabzMask) == IabzBase)
            {
                var a = cpu.Peek();
                a = (ushort)(a + 1);
                cpu.ReplaceTop(a);
                if (a != 0)
                {
                    return true;
                }

                var offset = word & IabzDispMask;
                var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                var target = (word & IabzBackFlag) != 0
                    ? instructionAddress - offset
                    : instructionAddress + offset;
                target &= 0x7fff;

                if ((word & IabzIndirectFlag) != 0)
                {
                    target = cpu.ReadWord(target) & 0x7fff;
                }

                cpu.Pc = target;
                return true;
            }

            if ((word & IxbzMask) == IxbzBase)
            {
                cpu.X = (ushort)(cpu.X + 1);
                if (cpu.X != 0)
                {
                    return true;
                }

                var offset = word & IxbzDispMask;
                var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                var target = (word & IxbzBackFlag) != 0
                    ? instructionAddress - offset
                    : instructionAddress + offset;
                target &= 0x7fff;

                if ((word & IxbzIndirectFlag) != 0)
                {
                    target = cpu.ReadWord(target) & 0x7fff;
                }

                cpu.Pc = target;
                return true;
            }

            if ((word & CondBranchMask) == CondBranchBase)
            {
                var ccf = (ushort)((word & CondBranchCcfMask) >> 6);
                if (ShouldBranchOnCcf(ccf, cpu.Sta))
                {
                    var offset = word & CondBranchDispMask;
                    var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                    var target = (word & CondBranchDispSign) != 0
                        ? instructionAddress - offset
                        : instructionAddress + offset;
                    cpu.Pc = target & 0x7fff;
                }

                return true;
            }

            if ((word & BovMask) == BovBase)
            {
                if ((cpu.Sta & StatusO) != 0)
                {
                    var offset = word & BovDispMask;
                    var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                    var target = (word & BovDispSign) != 0
                        ? instructionAddress - offset
                        : instructionAddress + offset;
                    cpu.Pc = target & 0x7fff;
                }

                cpu.Sta = (ushort)(cpu.Sta & ~StatusO);
                return true;
            }

            if ((word & BnovMask) == BnovBase)
            {
                if ((cpu.Sta & StatusO) == 0)
                {
                    var offset = word & BnovDispMask;
                    var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                    var target = (word & BnovDispSign) != 0
                        ? instructionAddress - offset
                        : instructionAddress + offset;
                    cpu.Pc = target & 0x7fff;
                }

                cpu.Sta = (ushort)(cpu.Sta & ~StatusO);
                return true;
            }

            if ((word & BcyMask) == BcyBase)
            {
                if ((cpu.Sta & StatusC) != 0)
                {
                    var offset = word & BcyDispMask;
                    var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                    var target = (word & BcyDispSign) != 0
                        ? instructionAddress - offset
                        : instructionAddress + offset;
                    cpu.Pc = target & 0x7fff;
                }

                cpu.Sta = (ushort)(cpu.Sta & ~StatusC);
                return true;
            }

            if ((word & BncyMask) == BncyBase)
            {
                if ((cpu.Sta & StatusC) == 0)
                {
                    var offset = word & BncyDispMask;
                    var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                    var target = (word & BncyDispSign) != 0
                        ? instructionAddress - offset
                        : instructionAddress + offset;
                    cpu.Pc = target & 0x7fff;
                }

                cpu.Sta = (ushort)(cpu.Sta & ~StatusC);
                return true;
            }

            if ((word & BroMask) == BroBase)
            {
                var value = cpu.Pop();
                if ((value & 0x0001) != 0)
                {
                    var offset = word & BroDispMask;
                    var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                    var target = (word & BroDispSign) != 0
                        ? instructionAddress - offset
                        : instructionAddress + offset;
                    cpu.Pc = target & 0x7fff;
                }

                return true;
            }

            if ((word & DxbzMask) == DxbzBase)
            {
                cpu.X = (ushort)(cpu.X - 1);
                if (cpu.X != 0)
                {
                    return true;
                }

                var offset = word & DxbzDispMask;
                var instructionAddress = (cpu.Pc - 1) & 0x7fff;
                var target = (word & DxbzBackFlag) != 0
                    ? instructionAddress - offset
                    : instructionAddress + offset;
                target &= 0x7fff;

                if ((word & DxbzIndirectFlag) != 0)
                {
                    target = cpu.ReadWord(target) & 0x7fff;
                }

                cpu.Pc = target;
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

            if ((word & LddMask) == LddBase)
            {
                var displacement = word & StorDispMask;
                var loadTarget = (cpu.Db + displacement) & 0x7fff;
                if ((word & StorXFlag) != 0)
                {
                    loadTarget = (loadTarget + cpu.X) & 0x7fff;
                }

                if ((word & StorIFlag) != 0)
                {
                    loadTarget = cpu.ReadWord(loadTarget) & 0x7fff;
                }

                cpu.Push(cpu.ReadWord(loadTarget));
                cpu.Push(cpu.ReadWord((loadTarget + 1) & 0x7fff));
                UpdateCcFlags(cpu, cpu.Ra);
                return true;
            }

            if ((word & StdMask) == StdBase)
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

                var low = cpu.Pop();
                var high = cpu.Pop();
                cpu.WriteWord(storTarget, high);
                cpu.WriteWord((storTarget + 1) & 0x7fff, low);
                return true;
            }

            if ((word & BranchMask) == BranchBase)
            {
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

                var loadedValue = cpu.ReadWord(loadTarget);
                cpu.Push(loadedValue);
                UpdateCcFlags(cpu, loadedValue);
                return true;
            }

            return false;
        }

        public string Disassemble(ushort opcode)
        {
            if (TryDisassembleSpecial(opcode, out var special))
            {
                return special;
            }

            if ((opcode & ScalMask) == ScalBase)
            {
                return $"SCAL {ToOctal3((ushort)(opcode & ScalOperandMask))}";
            }

            if ((opcode & SxitMask) == SxitBase)
            {
                return $"SXIT {ToOctal3((ushort)(opcode & SxitOperandMask))}";
            }

            if ((opcode & ShiftMask) == AslBase)
            {
                return $"ASL {Convert.ToString(opcode & ShiftCountMask, 8)}";
            }

            if ((opcode & ShiftMask) == AsrBase)
            {
                return $"ASR {Convert.ToString(opcode & ShiftCountMask, 8)}";
            }

            if ((opcode & ShiftMask) == LslBase)
            {
                return $"LSL {Convert.ToString(opcode & ShiftCountMask, 8)}";
            }

            if ((opcode & ShiftMask) == LsrBase)
            {
                return $"LSR {Convert.ToString(opcode & ShiftCountMask, 8)}";
            }

            if ((opcode & DaslMask) == DaslBase)
            {
                var count = Convert.ToString(opcode & ShiftCountMask, 8);
                var suffix = (opcode & DaslXFlag) != 0 ? ",X" : "";
                return $"DASL {count}{suffix}";
            }

            if ((opcode & DaslMask) == DasrBase)
            {
                var count = Convert.ToString(opcode & ShiftCountMask, 8);
                var suffix = (opcode & DaslXFlag) != 0 ? ",X" : "";
                return $"DASR {count}{suffix}";
            }

            if ((opcode & DaslMask) == DlslBase)
            {
                var count = Convert.ToString(opcode & ShiftCountMask, 8);
                var suffix = (opcode & DaslXFlag) != 0 ? ",X" : "";
                return $"DLSL {count}{suffix}";
            }

            if ((opcode & DaslMask) == DlsrBase)
            {
                var count = Convert.ToString(opcode & ShiftCountMask, 8);
                var suffix = (opcode & DaslXFlag) != 0 ? ",X" : "";
                return $"DLSR {count}{suffix}";
            }

            if ((opcode & IncmMask) == IncmBase)
            {
                return DisassembleMemRef(opcode, "INCM");
            }

            if ((opcode & IncmMask) == DecmBase)
            {
                return DisassembleMemRef(opcode, "DECM");
            }

            if (opcode == HaltWord)
            {
                return "HALT 0";
            }

            if (opcode == DdivWord)
            {
                return "DDIV";
            }

            var immediateKind = (ushort)(opcode & ImmediateMask);
            if (immediateKind == ImmediateLdiBase || immediateKind == ImmediateLdXiBase)
            {
                return DisassembleImmediate(opcode);
            }

            if ((opcode & IabzMask) == IabzBase)
            {
                return DisassembleIabz(opcode);
            }

            if ((opcode & IxbzMask) == IxbzBase)
            {
                return DisassembleIxbz(opcode);
            }

            if ((opcode & CondBranchMask) == CondBranchBase)
            {
                return DisassembleCondBranch(opcode);
            }

            if ((opcode & BovMask) == BovBase)
            {
                return DisassembleBov(opcode);
            }

            if ((opcode & BnovMask) == BnovBase)
            {
                return DisassembleBnov(opcode);
            }

            if ((opcode & BcyMask) == BcyBase)
            {
                return DisassembleBcy(opcode);
            }

            if ((opcode & BncyMask) == BncyBase)
            {
                return DisassembleBncy(opcode);
            }

            if ((opcode & BroMask) == BroBase)
            {
                return DisassembleBro(opcode);
            }

            if ((opcode & DxbzMask) == DxbzBase)
            {
                return DisassembleDxbz(opcode);
            }

            if ((opcode & StorMask) == StorBase)
            {
                return DisassembleStor(opcode);
            }

            if ((opcode & LddMask) == LddBase)
            {
                return DisassembleLdd(opcode);
            }

            if ((opcode & StdMask) == StdBase)
            {
                return DisassembleStd(opcode);
            }

            if ((opcode & BranchMask) == BranchBase)
            {
                return DisassembleBranch(opcode);
            }

            if ((opcode & LoadMask) == LoadBase)
            {
                return DisassembleLoad(opcode);
            }

            var firstOpcode = (ushort)((opcode >> 6) & 0x003f);
            var secondOpcode = (ushort)(opcode & 0x003f);
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

            if (mnemonic.Equals("HALT", StringComparison.OrdinalIgnoreCase))
            {
                if (operand.Trim() == "0")
                {
                    opcode = HaltWord;
                    return true;
                }

                opcode = 0;
                return false;
            }

            if (mnemonic.Equals("SCAL", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleScal(operand, out opcode);
            }

            if (mnemonic.Equals("SXIT", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleSxit(operand, out opcode);
            }

            if (mnemonic.Equals("ASL", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleShift(operand, AslBase, out opcode);
            }

            if (mnemonic.Equals("ASR", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleShift(operand, AsrBase, out opcode);
            }

            if (mnemonic.Equals("LSL", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleShift(operand, LslBase, out opcode);
            }

            if (mnemonic.Equals("LSR", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleShift(operand, LsrBase, out opcode);
            }

            if (mnemonic.Equals("DASL", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleDasl(operand, out opcode);
            }

            if (mnemonic.Equals("DASR", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleDasr(operand, out opcode);
            }

            if (mnemonic.Equals("DLSL", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleDlsl(operand, out opcode);
            }

            if (mnemonic.Equals("DLSR", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleDlsr(operand, out opcode);
            }

            if (mnemonic.Equals("INCM", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleMemAdjust(operand, IncmBase, out opcode);
            }

            if (mnemonic.Equals("DECM", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleMemAdjust(operand, DecmBase, out opcode);
            }

            if (mnemonic.Equals("LOAD", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleLoad(operand, out opcode);
            }

            if (mnemonic.Equals("IABZ", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleIabz(operand, out opcode);
            }

            if (mnemonic.Equals("IXBZ", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleIxbz(operand, out opcode);
            }

            if (mnemonic.Equals("BN", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 0, out opcode);
            }

            if (mnemonic.Equals("BL", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 1, out opcode);
            }

            if (mnemonic.Equals("BE", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 2, out opcode);
            }

            if (mnemonic.Equals("BLE", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 3, out opcode);
            }

            if (mnemonic.Equals("BG", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 4, out opcode);
            }

            if (mnemonic.Equals("BNE", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 5, out opcode);
            }

            if (mnemonic.Equals("BGE", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 6, out opcode);
            }

            if (mnemonic.Equals("BA", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleCondBranch(operand, 7, out opcode);
            }

            if (mnemonic.Equals("BOV", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleBov(operand, out opcode);
            }

            if (mnemonic.Equals("BNOV", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleBnov(operand, out opcode);
            }

            if (mnemonic.Equals("BCY", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleBcy(operand, out opcode);
            }

            if (mnemonic.Equals("BNCY", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleBncy(operand, out opcode);
            }

            if (mnemonic.Equals("BRO", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleBro(operand, out opcode);
            }

            if (mnemonic.Equals("DXBZ", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleDxbz(operand, out opcode);
            }

            if (mnemonic.Equals("STOR", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleStor(operand, out opcode);
            }

            if (mnemonic.Equals("LDD", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleLdd(operand, out opcode);
            }

            if (mnemonic.Equals("STD", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleStd(operand, out opcode);
            }

            if (mnemonic.Equals("LDI", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleImmediate(operand, ImmediateLdiBase, out opcode);
            }

            if (mnemonic.Equals("LDXI", StringComparison.OrdinalIgnoreCase))
            {
                return TryAssembleImmediate(operand, ImmediateLdXiBase, out opcode);
            }

            if (mnemonic.Equals("WIO", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseOctal(operand, out var device) || device > 0x0F)
                {
                    opcode = 0;
                    return false;
                }

                opcode = (ushort)(0x3000 | (0x09 << 4) | device);
                return true;
            }

            if (mnemonic.Equals("RIO", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseOctal(operand, out var device) || device > 0x0F)
                {
                    opcode = 0;
                    return false;
                }

                opcode = (ushort)(0x3000 | (0x08 << 4) | device);
                return true;
            }

            opcode = 0;
            return false;
        }

        private static void ExecuteScal(ushort operand, Hp3000Cpu cpu)
        {
            var returnAddress = (ushort)(cpu.Pc & 0x7fff);
            if (operand == 0)
            {
                var label = cpu.Peek();
                var target = (cpu.Pb + (label & 0x7fff)) & 0x7fff;
                cpu.ReplaceTop(returnAddress);
                cpu.Pc = target;
                return;
            }

            var entryAddress = (cpu.Pl - operand) & 0x7fff;
            var entry = cpu.ReadWord(entryAddress);
            var targetAddress = (cpu.Pb + (entry & 0x7fff)) & 0x7fff;
            cpu.Push(returnAddress);
            cpu.Pc = targetAddress;
        }

        private static void ExecuteSxit(ushort operand, Hp3000Cpu cpu)
        {
            var returnAddress = cpu.Pop();
            for (var i = 0; i < operand; i++)
            {
                cpu.Pop();
            }

            cpu.Pc = returnAddress & 0x7fff;
        }

        private static void ExecuteAsl(ushort count, Hp3000Cpu cpu)
        {
            var value = cpu.Peek();
            var sign = (ushort)(value & 0x8000);
            var magnitude = (ushort)(value & 0x7fff);
            var shifted = count >= 15
                ? (ushort)0
                : (ushort)((magnitude << count) & 0x7fff);
            var result = (ushort)(sign | shifted);
            cpu.ReplaceTop(result);
            UpdateCcFlags(cpu, result);
        }

        private static void ExecuteAsr(ushort count, Hp3000Cpu cpu)
        {
            var value = cpu.Peek();
            ushort result;
            if (count >= 16)
            {
                result = (value & 0x8000) != 0 ? (ushort)0xFFFF : (ushort)0;
            }
            else
            {
                result = (ushort)(((short)value) >> count);
            }

            cpu.ReplaceTop(result);
            UpdateCcFlags(cpu, result);
        }

        private static void ExecuteLsl(ushort count, Hp3000Cpu cpu)
        {
            var value = cpu.Peek();
            var result = count >= 16 ? (ushort)0 : (ushort)(value << count);
            cpu.ReplaceTop(result);
            UpdateCcFlags(cpu, result);
        }

        private static void ExecuteLsr(ushort count, Hp3000Cpu cpu)
        {
            var value = cpu.Peek();
            var result = count >= 16 ? (ushort)0 : (ushort)(value >> count);
            cpu.ReplaceTop(result);
            UpdateCcFlags(cpu, result);
        }

        private static void ExecuteDasl(ushort count, bool useX, Hp3000Cpu cpu)
        {
            var shift = (ushort)((count + (useX ? cpu.X : 0)) & ShiftCountMask);
            var low = cpu.Pop();
            var high = cpu.Pop();
            var signBit = (uint)(high & 0x8000) << 16;
            var magnitude = ((uint)(high & 0x7FFF) << 16) | low;
            var carry = false;
            uint shiftedMagnitude;
            if (shift == 0)
            {
                shiftedMagnitude = magnitude;
            }
            else if (shift >= 31)
            {
                carry = magnitude != 0;
                shiftedMagnitude = 0;
            }
            else
            {
                var shiftedOutMask = ((1u << shift) - 1) << (31 - shift);
                carry = (magnitude & shiftedOutMask) != 0;
                shiftedMagnitude = (magnitude << shift) & 0x7FFFFFFF;
            }

            var result = signBit | shiftedMagnitude;
            cpu.Push((ushort)(result >> 16));
            cpu.Push((ushort)(result & 0xFFFF));
            UpdateDoubleCcCarryFlags(cpu, result, carry);
        }

        private static void ExecuteDasr(ushort count, bool useX, Hp3000Cpu cpu)
        {
            var shift = (ushort)((count + (useX ? cpu.X : 0)) & ShiftCountMask);
            var low = cpu.Pop();
            var high = cpu.Pop();
            var combined = ((uint)high << 16) | low;
            var signFill = (high & 0x8000) != 0 ? 0xFFFFFFFFu : 0u;
            var carry = false;
            uint result;
            if (shift == 0)
            {
                result = combined;
            }
            else if (shift >= 32)
            {
                carry = (combined & 0x7FFFFFFF) != 0;
                result = signFill;
            }
            else
            {
                var shiftedOutMask = (1u << shift) - 1;
                carry = (combined & shiftedOutMask) != 0;
                result = (combined >> shift) | (signFill << (32 - shift));
            }

            cpu.Push((ushort)(result >> 16));
            cpu.Push((ushort)(result & 0xFFFF));
            UpdateDoubleCcCarryFlags(cpu, result, carry);
        }

        private static void ExecuteDlsl(ushort count, bool useX, Hp3000Cpu cpu)
        {
            var shift = (ushort)((count + (useX ? cpu.X : 0)) & ShiftCountMask);
            var low = cpu.Pop();
            var high = cpu.Pop();
            var combined = ((uint)high << 16) | low;
            var carry = false;
            uint result;
            if (shift == 0)
            {
                result = combined;
            }
            else if (shift >= 32)
            {
                carry = combined != 0;
                result = 0;
            }
            else
            {
                var shiftedOutMask = 0xFFFFFFFFu << (32 - shift);
                carry = (combined & shiftedOutMask) != 0;
                result = combined << shift;
            }

            cpu.Push((ushort)(result >> 16));
            cpu.Push((ushort)(result & 0xFFFF));
            UpdateDoubleCcCarryFlags(cpu, result, carry);
        }

        private static void ExecuteDlsr(ushort count, bool useX, Hp3000Cpu cpu)
        {
            var shift = (ushort)((count + (useX ? cpu.X : 0)) & ShiftCountMask);
            var low = cpu.Pop();
            var high = cpu.Pop();
            var combined = ((uint)high << 16) | low;
            var carry = false;
            uint result;
            if (shift == 0)
            {
                result = combined;
            }
            else if (shift >= 32)
            {
                carry = combined != 0;
                result = 0;
            }
            else
            {
                var shiftedOutMask = (1u << shift) - 1;
                carry = (combined & shiftedOutMask) != 0;
                result = combined >> shift;
            }

            cpu.Push((ushort)(result >> 16));
            cpu.Push((ushort)(result & 0xFFFF));
            UpdateDoubleCcCarryFlags(cpu, result, carry);
        }

        private static void ExecuteMemAdjust(ushort word, bool increment, Hp3000Cpu cpu)
        {
            var displacement = word & StorDispMask;
            var target = (cpu.Db + displacement) & 0x7fff;
            if ((word & StorXFlag) != 0)
            {
                target = (target + cpu.X) & 0x7fff;
            }

            if ((word & StorIFlag) != 0)
            {
                target = cpu.ReadWord(target) & 0x7fff;
            }

            var value = cpu.ReadWord(target);
            value = increment ? (ushort)(value + 1) : (ushort)(value - 1);
            cpu.WriteWord(target, value);
            UpdateCcFlags(cpu, value);
        }

        private static bool TryExecuteSpecial(ushort word, Hp3000Cpu cpu)
        {
            if ((word & 0xFC00) != 0x3000)
            {
                return false;
            }

            var opcode = (ushort)((word >> 4) & 0x3F);
            var deviceCode = (ushort)(word & 0x000F);
            switch (opcode)
            {
                case 0x0F: // HALT 0
                    cpu.Halt("HALT");
                    return true;
                case 0x08: // RIO
                    return ExecuteRio(cpu, deviceCode);
                case 0x09: // WIO
                    return ExecuteWio(cpu, deviceCode);
            }

            return false;
        }

        private static bool ExecuteWio(Hp3000Cpu cpu, ushort deviceCode)
        {
            if (!cpu.TryReadIoStatus(deviceCode, out var status))
            {
                cpu.Sta = (ushort)((cpu.Sta & ~StatusCcMask) | StatusCcl);
                return true;
            }

            if ((status & 0x0002) != 0)
            {
                var value = cpu.Pop();
                cpu.WriteIoWord(deviceCode, value);
                cpu.Sta = (ushort)((cpu.Sta & ~StatusCcMask) | StatusCce);
                return true;
            }

            cpu.Push(status);
            cpu.Sta = (ushort)((cpu.Sta & ~StatusCcMask) | StatusCcg);
            return true;
        }

        private static bool ExecuteRio(Hp3000Cpu cpu, ushort deviceCode)
        {
            if (!cpu.TryReadIoStatus(deviceCode, out var status))
            {
                cpu.Sta = (ushort)((cpu.Sta & ~StatusCcMask) | StatusCcl);
                return true;
            }

            if ((status & 0x0002) != 0)
            {
                var value = cpu.ReadIoByte(deviceCode);
                cpu.Push(value);
                cpu.Sta = (ushort)((cpu.Sta & ~StatusCcMask) | StatusCce);
                return true;
            }

            cpu.Push(status);
            cpu.Sta = (ushort)((cpu.Sta & ~StatusCcMask) | StatusCcg);
            return true;
        }

        private static bool TryDisassembleSpecial(ushort word, out string disassembly)
        {
            disassembly = string.Empty;
            if ((word & 0xFC00) != 0x3000)
            {
                return false;
            }

            var opcode = (ushort)((word >> 4) & 0x3F);
            var deviceCode = (ushort)(word & 0x000F);
            switch (opcode)
            {
                case 0x0F:
                    disassembly = "HALT 0";
                    return true;
                case 0x08:
                    disassembly = $"RIO {Convert.ToString(deviceCode, 8)}";
                    return true;
                case 0x09:
                    disassembly = $"WIO {Convert.ToString(deviceCode, 8)}";
                    return true;
            }

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

            return $"BR .{direction}{offsetText}{suffix}";
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
            return $"LOAD .{direction}{offsetText}{suffix}";
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

        private static string DisassembleMemRef(ushort word, string mnemonic)
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
            return $"{mnemonic} DB+{offsetText}{suffix}";
        }

        private static string DisassembleLdd(ushort word)
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
            return $"LDD DB+{offsetText}{suffix}";
        }

        private static string DisassembleStd(ushort word)
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
            return $"STD DB+{offsetText}{suffix}";
        }

        private static string DisassembleIabz(ushort word)
        {
            var displacement = (ushort)(word & IabzDispMask);
            var direction = (word & IabzBackFlag) != 0 ? '-' : '+';
            var suffix = (word & IabzIndirectFlag) != 0 ? ",I" : "";
            var offsetText = Convert.ToString(displacement, 8);
            return $"IABZ .{direction}{offsetText}{suffix}";
        }

        private static string DisassembleIxbz(ushort word)
        {
            var displacement = (ushort)(word & IxbzDispMask);
            var direction = (word & IxbzBackFlag) != 0 ? '-' : '+';
            var suffix = (word & IxbzIndirectFlag) != 0 ? ",I" : "";
            var offsetText = Convert.ToString(displacement, 8);
            return $"IXBZ .{direction}{offsetText}{suffix}";
        }

        private static string DisassembleCondBranch(ushort word)
        {
            var ccf = (ushort)((word & CondBranchCcfMask) >> 6);
            var displacement = (ushort)(word & CondBranchDispMask);
            var direction = (word & CondBranchDispSign) != 0 ? '-' : '+';
            var offsetText = Convert.ToString(displacement, 8);
            var mnemonic = ccf switch
            {
                0 => "BN",
                1 => "BL",
                2 => "BE",
                3 => "BLE",
                4 => "BG",
                5 => "BNE",
                6 => "BGE",
                7 => "BA",
                _ => $"BCC {ccf}"
            };

            return $"{mnemonic} .{direction}{offsetText}";
        }

        private static string DisassembleDxbz(ushort word)
        {
            var displacement = (ushort)(word & DxbzDispMask);
            var direction = (word & DxbzBackFlag) != 0 ? '-' : '+';
            var suffix = (word & DxbzIndirectFlag) != 0 ? ",I" : "";
            var offsetText = Convert.ToString(displacement, 8);
            return $"DXBZ .{direction}{offsetText}{suffix}";
        }

        private static string DisassembleBov(ushort word)
        {
            var displacement = (ushort)(word & BovDispMask);
            var direction = (word & BovDispSign) != 0 ? '-' : '+';
            var offsetText = Convert.ToString(displacement, 8);
            return $"BOV .{direction}{offsetText}";
        }

        private static string DisassembleBnov(ushort word)
        {
            var displacement = (ushort)(word & BnovDispMask);
            var direction = (word & BnovDispSign) != 0 ? '-' : '+';
            var offsetText = Convert.ToString(displacement, 8);
            return $"BNOV .{direction}{offsetText}";
        }

        private static string DisassembleBcy(ushort word)
        {
            var displacement = (ushort)(word & BcyDispMask);
            var direction = (word & BcyDispSign) != 0 ? '-' : '+';
            var offsetText = Convert.ToString(displacement, 8);
            return $"BCY .{direction}{offsetText}";
        }

        private static string DisassembleBncy(ushort word)
        {
            var displacement = (ushort)(word & BncyDispMask);
            var direction = (word & BncyDispSign) != 0 ? '-' : '+';
            var offsetText = Convert.ToString(displacement, 8);
            return $"BNCY .{direction}{offsetText}";
        }

        private static string DisassembleBro(ushort word)
        {
            var displacement = (ushort)(word & BroDispMask);
            var direction = (word & BroDispSign) != 0 ? '-' : '+';
            var offsetText = Convert.ToString(displacement, 8);
            return $"BRO .{direction}{offsetText}";
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
            if (!TryParsePcRelative(basePart, requirePrefix: true, out var direction, out var offsetText))
            {
                return false;
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > BranchOffsetMask)
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

            if (!TryParsePcRelative(basePart, requirePrefix: false, out var direction, out var offsetText))
            {
                return false;
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

        private static bool TryAssembleLdd(string operand, out ushort opcode)
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

            opcode = (ushort)(LddBase | offset);
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

        private static bool TryAssembleStd(string operand, out ushort opcode)
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

            opcode = (ushort)(StdBase | offset);
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

        private static bool TryAssembleIabz(string operand, out ushort opcode)
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

            if (!TryParsePcRelative(basePart, requirePrefix: false, out var direction, out var offsetText))
            {
                return false;
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > IxbzDispMask)
            {
                return false;
            }

            opcode = (ushort)(IabzBase | offset);
            if (direction == '-')
            {
                opcode |= IabzBackFlag;
            }

            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= IabzIndirectFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleIxbz(string operand, out ushort opcode)
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

            if (!TryParsePcRelative(basePart, requirePrefix: false, out var direction, out var offsetText))
            {
                return false;
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > IxbzDispMask)
            {
                return false;
            }

            opcode = (ushort)(IxbzBase | offset);
            if (direction == '-')
            {
                opcode |= IxbzBackFlag;
            }

            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= IxbzIndirectFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleCondBranch(string operand, ushort ccf, out ushort opcode)
        {
            opcode = 0;
            if (string.IsNullOrWhiteSpace(operand))
            {
                return false;
            }

            var basePart = operand.Trim();
            if (basePart.Length < 1)
            {
                return false;
            }

            if (!TryParsePcRelative(basePart, requirePrefix: false, out var direction, out var offsetText))
            {
                return false;
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > CondBranchDispMask)
            {
                return false;
            }

            opcode = (ushort)(CondBranchBase | (ccf << 6) | offset);
            if (direction == '-')
            {
                opcode |= CondBranchDispSign;
            }

            return true;
        }

        private static bool TryAssembleDxbz(string operand, out ushort opcode)
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

            if (!TryParsePcRelative(basePart, requirePrefix: false, out var direction, out var offsetText))
            {
                return false;
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > DxbzDispMask)
            {
                return false;
            }

            opcode = (ushort)(DxbzBase | offset);
            if (direction == '-')
            {
                opcode |= DxbzBackFlag;
            }

            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= DxbzIndirectFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleBov(string operand, out ushort opcode)
        {
            return TryAssembleShortBranch(operand, BovBase, BovDispMask, BovDispSign, out opcode);
        }

        private static bool TryAssembleBnov(string operand, out ushort opcode)
        {
            return TryAssembleShortBranch(operand, BnovBase, BnovDispMask, BnovDispSign, out opcode);
        }

        private static bool TryAssembleBcy(string operand, out ushort opcode)
        {
            return TryAssembleShortBranch(operand, BcyBase, BcyDispMask, BcyDispSign, out opcode);
        }

        private static bool TryAssembleBncy(string operand, out ushort opcode)
        {
            return TryAssembleShortBranch(operand, BncyBase, BncyDispMask, BncyDispSign, out opcode);
        }

        private static bool TryAssembleBro(string operand, out ushort opcode)
        {
            return TryAssembleShortBranch(operand, BroBase, BroDispMask, BroDispSign, out opcode);
        }

        private static bool TryAssembleShortBranch(
            string operand,
            ushort baseOpcode,
            ushort dispMask,
            ushort dispSign,
            out ushort opcode)
        {
            opcode = 0;
            if (string.IsNullOrWhiteSpace(operand))
            {
                return false;
            }

            var basePart = operand.Trim();
            if (basePart.Length < 1)
            {
                return false;
            }

            if (!TryParsePcRelative(basePart, requirePrefix: true, out var direction, out var offsetText))
            {
                return false;
            }

            if (!TryParseOctal(offsetText, out var offset) || offset > dispMask)
            {
                return false;
            }

            opcode = (ushort)(baseOpcode | offset);
            if (direction == '-')
            {
                opcode |= dispSign;
            }

            return true;
        }

        private static bool ShouldBranchOnCcf(ushort ccf, ushort status)
        {
            var cc = (ushort)(status & StatusCcMask);
            return ccf switch
            {
                0 => false,
                1 => cc == StatusCcl,
                2 => cc == StatusCce,
                3 => cc == StatusCcl || cc == StatusCce,
                4 => cc == StatusCcg,
                5 => cc == StatusCcg || cc == StatusCcl,
                6 => cc == StatusCcg || cc == StatusCce,
                7 => true,
                _ => false
            };
        }

        private static void UpdateAddSubFlags(Hp3000Cpu cpu, ushort result, bool carry, bool overflow)
        {
            var cc = result == 0
                ? StatusCce
                : (result & 0x8000) != 0
                    ? StatusCcl
                    : StatusCcg;
            var updated = (ushort)(cpu.Sta & ~(StatusCcMask | StatusO | StatusC));
            updated |= cc;
            if (overflow)
            {
                updated |= StatusO;
            }

            if (carry)
            {
                updated |= StatusC;
            }

            cpu.Sta = updated;
        }

        private static void UpdateLogicalMultiplyFlags(Hp3000Cpu cpu, ushort result, bool carry)
        {
            var cc = result == 0
                ? StatusCce
                : (result & 0x8000) != 0
                    ? StatusCcl
                    : StatusCcg;
            var updated = (ushort)(cpu.Sta & ~(StatusCcMask | StatusO | StatusC));
            updated |= cc;
            if (carry)
            {
                updated |= StatusC;
            }

            cpu.Sta = updated;
        }

        private static void UpdateDoubleAddFlags(Hp3000Cpu cpu, uint left, uint right, uint result, bool carry)
        {
            var cc = result == 0
                ? StatusCce
                : (result & 0x80000000) != 0
                    ? StatusCcl
                    : StatusCcg;
            var updated = (ushort)(cpu.Sta & ~(StatusCcMask | StatusO | StatusC));
            updated |= cc;
            if (carry)
            {
                updated |= StatusC;
            }

            var signLeft = (left & 0x80000000) != 0;
            var signRight = (right & 0x80000000) != 0;
            var signResult = (result & 0x80000000) != 0;
            if (signLeft == signRight && signResult != signLeft)
            {
                updated |= StatusO;
            }

            cpu.Sta = updated;
        }

        private static void UpdateCcFlags(Hp3000Cpu cpu, ushort result)
        {
            var cc = result == 0
                ? StatusCce
                : (result & 0x8000) != 0
                    ? StatusCcl
                    : StatusCcg;
            var updated = (ushort)(cpu.Sta & ~StatusCcMask);
            updated |= cc;
            cpu.Sta = updated;
        }

        private static void UpdateDoubleCompareFlags(Hp3000Cpu cpu, uint left, uint right)
        {
            var leftSigned = unchecked((int)left);
            var rightSigned = unchecked((int)right);
            var cc = leftSigned == rightSigned
                ? StatusCce
                : leftSigned < rightSigned
                    ? StatusCcl
                    : StatusCcg;
            cpu.Sta = (ushort)((cpu.Sta & ~StatusCcMask) | cc);
        }

        private static bool IsAddOverflow(ushort b, ushort a, ushort result)
        {
            var signA = (a & 0x8000) != 0;
            var signB = (b & 0x8000) != 0;
            var signR = (result & 0x8000) != 0;
            return signA == signB && signR != signA;
        }

        private static bool IsSubOverflow(ushort b, ushort a, ushort result)
        {
            var signA = (a & 0x8000) != 0;
            var signB = (b & 0x8000) != 0;
            var signR = (result & 0x8000) != 0;
            return signA != signB && signR != signB;
        }

        private static void UpdateDivFlags(Hp3000Cpu cpu, ushort quotient, bool overflow)
        {
            var cc = quotient == 0
                ? StatusCce
                : (quotient & 0x8000) != 0
                    ? StatusCcl
                    : StatusCcg;
            var updated = (ushort)(cpu.Sta & ~(StatusCcMask | StatusO));
            updated |= cc;
            if (overflow)
            {
                updated |= StatusO;
            }

            cpu.Sta = updated;
        }

        private static void UpdateDoubleDivFlags(Hp3000Cpu cpu, uint quotient, bool overflow)
        {
            var cc = quotient == 0
                ? StatusCce
                : (quotient & 0x80000000) != 0
                    ? StatusCcl
                    : StatusCcg;
            var updated = (ushort)(cpu.Sta & ~(StatusCcMask | StatusO));
            updated |= cc;
            if (overflow)
            {
                updated |= StatusO;
            }

            cpu.Sta = updated;
        }

        private static void UpdateDoubleCcCarryFlags(Hp3000Cpu cpu, uint result, bool carry)
        {
            var cc = result == 0
                ? StatusCce
                : (result & 0x80000000) != 0
                    ? StatusCcl
                    : StatusCcg;
            var updated = (ushort)(cpu.Sta & ~(StatusCcMask | StatusO | StatusC));
            updated |= cc;
            if (carry)
            {
                updated |= StatusC;
            }

            cpu.Sta = updated;
        }

        private static bool TryParsePcRelative(
            string basePart,
            bool requirePrefix,
            out char direction,
            out string offsetText)
        {
            direction = '+';
            offsetText = basePart;
            if (string.IsNullOrWhiteSpace(basePart))
            {
                return false;
            }

            if (basePart[0] == '.')
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
                return true;
            }

            if ((basePart[0] == 'P' || basePart[0] == 'p')
                && basePart.Length >= 3
                && (basePart[1] == '+' || basePart[1] == '-'))
            {
                direction = basePart[1];
                offsetText = basePart[2..];
                return true;
            }

            return !requirePrefix;
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

        private static bool TryAssembleScal(string operand, out ushort opcode)
        {
            opcode = 0;
            if (!TryParseOctal(operand, out var value))
            {
                return false;
            }

            if (value > ScalOperandMask)
            {
                return false;
            }

            opcode = (ushort)(ScalBase | value);
            return true;
        }

        private static bool TryAssembleSxit(string operand, out ushort opcode)
        {
            opcode = 0;
            if (!TryParseOctal(operand, out var value))
            {
                return false;
            }

            if (value > SxitOperandMask)
            {
                return false;
            }

            opcode = (ushort)(SxitBase | value);
            return true;
        }

        private static bool TryAssembleShift(string operand, ushort baseOpcode, out ushort opcode)
        {
            opcode = 0;
            if (!TryParseOctal(operand, out var value))
            {
                return false;
            }

            if (value > ShiftCountMask)
            {
                return false;
            }

            opcode = (ushort)(baseOpcode | value);
            return true;
        }

        private static bool TryAssembleDasl(string operand, out ushort opcode)
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

            if (!TryParseOctal(parts[0].Trim(), out var value))
            {
                return false;
            }

            if (value > ShiftCountMask)
            {
                return false;
            }

            opcode = (ushort)(DaslBase | value);
            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= DaslXFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleDasr(string operand, out ushort opcode)
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

            if (!TryParseOctal(parts[0].Trim(), out var value))
            {
                return false;
            }

            if (value > ShiftCountMask)
            {
                return false;
            }

            opcode = (ushort)(DasrBase | value);
            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= DaslXFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleDlsl(string operand, out ushort opcode)
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

            if (!TryParseOctal(parts[0].Trim(), out var value))
            {
                return false;
            }

            if (value > ShiftCountMask)
            {
                return false;
            }

            opcode = (ushort)(DlslBase | value);
            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= DaslXFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleDlsr(string operand, out ushort opcode)
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

            if (!TryParseOctal(parts[0].Trim(), out var value))
            {
                return false;
            }

            if (value > ShiftCountMask)
            {
                return false;
            }

            opcode = (ushort)(DlsrBase | value);
            for (var i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i].Trim();
                if (suffix.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    opcode |= DaslXFlag;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAssembleMemAdjust(string operand, ushort baseOpcode, out ushort opcode)
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

            opcode = (ushort)(baseOpcode | offset);
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
