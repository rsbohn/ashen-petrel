using System;

namespace Ashen
{
    internal sealed class Hp3000Cpu
    {
        private readonly Hp3000Isa _isa;
        private readonly Hp3000Memory _memory;
        private readonly Hp3000IoBus _ioBus;
        private readonly DeviceRegistry _devices;

        public Hp3000Cpu(Hp3000Memory memory, Hp3000IoBus ioBus, DeviceRegistry devices)
        {
            _isa = new Hp3000Isa();
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
            _ioBus = ioBus ?? throw new ArgumentNullException(nameof(ioBus));
            _devices = devices ?? throw new ArgumentNullException(nameof(devices));
            Reset();
        }

        public int Pc { get; internal set; }
        public int Sp { get; internal set; }
        public int Sm { get; internal set; }
        public int Sr { get; internal set; }
        public ushort Ra { get; internal set; }
        public ushort Rb { get; internal set; }
        public ushort Rc { get; internal set; }
        public ushort Rd { get; internal set; }
        public ushort X { get; internal set; }
        public ushort Db { get; internal set; }
        public ushort Sta { get; internal set; }
        public int StackDepth { get; internal set; }
        public bool Halted { get; private set; }
        public string? HaltReason { get; private set; }

        public void Reset(int address = 0)
        {
            Pc = address & 0x7fff;
            Sp = _memory.Size - 1;
            Sm = 0x1000;
            Sr = 0;
            Ra = 0;
            Rb = 0;
            Rc = 0;
            Rd = 0;
            X = 0;
            Db = 0;
            Sta = 0;
            StackDepth = 0;
            Halted = false;
            HaltReason = null;
        }

        public void Push(ushort value)
        {
            if (Halted)
            {
                return;
            }

            if (Sr == 4)
            {
                Sm = (Sm - 1) & 0x7fff;
                _memory.Write(Sm, Rd);
            }

            Rd = Rc;
            Rc = Rb;
            Rb = Ra;
            Ra = value;
            if (Sr < 4)
            {
                Sr++;
            }
            StackDepth++;
        }

        public ushort Pop()
        {
            if (Halted)
            {
                return 0;
            }

            if (Sr == 0)
            {
                HaltWithError("stack underflow");
                return 0;
            }

            var hadSpill = Sr == 4 && StackDepth > 4;
            var value = Ra;
            Ra = Rb;
            Rb = Rc;
            Rc = Rd;
            if (hadSpill)
            {
                Rd = _memory.Read(Sm);
                Sm = (Sm + 1) & 0x7fff;
            }
            else
            {
                Rd = 0;
            }
            if (!hadSpill)
            {
                Sr--;
            }
            if (StackDepth > 0)
            {
                StackDepth--;
            }
            return value;
        }


        public bool Step()
        {
            if (Halted)
            {
                return false;
            }

            var word = _memory.Read(Pc);
            Pc = (Pc + 1) & 0x7fff;

            if (_isa.TryExecuteWord(word, this))
            {
                return !Halted;
            }

            var firstOpcode = (ushort)(word & 0x003f);
            var secondOpcode = (ushort)((word >> 6) & 0x003f);

            if (!_isa.TryExecute(firstOpcode, this))
            {
                HaltWithError($"unknown opcode {ToOctal(firstOpcode)}");
                return false;
            }

            if (Halted)
            {
                return false;
            }

            if (!_isa.TryExecute(secondOpcode, this))
            {
                HaltWithError($"unknown opcode {ToOctal(secondOpcode)}");
                return false;
            }

            return !Halted;
        }

        public int Run(int maxSteps)
        {
            if (maxSteps <= 0)
            {
                return 0;
            }

            var steps = 0;
            while (!Halted && steps < maxSteps)
            {
                if (!Step())
                {
                    break;
                }

                steps++;
            }

            return steps;
        }

        public ushort ReadWord(int address)
        {
            return _memory.Read(address);
        }

        public void WriteWord(int address, ushort value)
        {
            _memory.Write(address, value);
        }

        public void Halt(string? message = null)
        {
            Halted = true;
            HaltReason = message;
        }

        public ushort Peek()
        {
            if (Halted)
            {
                return 0;
            }

            if (Sr == 0)
            {
                HaltWithError("stack underflow");
                return 0;
            }

            return Ra;
        }

        public ushort PeekSecond()
        {
            if (Halted)
            {
                return 0;
            }

            if (Sr < 2)
            {
                HaltWithError("stack underflow");
                return 0;
            }

            return Rb;
        }

        public void DropSecond()
        {
            if (Halted)
            {
                return;
            }

            if (Sr < 2)
            {
                HaltWithError("stack underflow");
                return;
            }

            var hadSpill = Sr == 4 && StackDepth > 4;
            Rb = Rc;
            Rc = Rd;
            if (hadSpill)
            {
                Rd = _memory.Read(Sm);
                Sm = (Sm + 1) & 0x7fff;
            }
            else
            {
                Rd = 0;
            }

            if (!hadSpill)
            {
                Sr--;
            }
            if (StackDepth > 0)
            {
                StackDepth--;
            }
        }

        public void ReplaceTop(ushort value)
        {
            if (Halted)
            {
                return;
            }

            if (Sr == 0)
            {
                HaltWithError("stack underflow");
                return;
            }

            Ra = value;
        }

        public void ReplaceSecond(ushort value)
        {
            if (Halted)
            {
                return;
            }

            if (Sr < 2)
            {
                HaltWithError("stack underflow");
                return;
            }

            Rb = value;
        }

        private void HaltWithError(string message)
        {
            Halt(message);
        }

        private static string ToOctal(ushort value)
        {
            return Convert.ToString(value, 8).PadLeft(3, '0');
        }
    }
}
