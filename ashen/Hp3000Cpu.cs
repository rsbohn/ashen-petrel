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

        public int Pc { get; private set; }
        public bool Halted { get; private set; }

        public void Reset(int address = 0)
        {
            Pc = address & 0x7fff;
            Halted = false;
        }

        public bool Step()
        {
            if (Halted)
            {
                return false;
            }

            var opcode = _memory.Read(Pc);
            Pc = (Pc + 1) & 0x7fff;

            if (_isa.TryExecute(opcode, this))
            {
                return !Halted;
            }

            Halted = true;
            return false;
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
    }
}
