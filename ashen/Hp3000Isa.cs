using System;
using System.Collections.Generic;

namespace Ashen
{
    internal sealed class Hp3000Isa
    {
        private readonly Dictionary<ushort, string> _mnemonics = new()
        {
            { 0x0000, "NOP" },
            { 0x0001, "DELB" }
        };

        private readonly Dictionary<string, ushort> _opcodes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "NOP", 0x0000 },
                { "DELB", 0x0001 }
            };

        public bool TryExecute(ushort opcode, Hp3000Cpu cpu)
        {
            switch (opcode)
            {
                case 0x0000:
                    return true;
                case 0x0001:
                    return true;
                default:
                    return false;
            }
        }

        public string Disassemble(ushort opcode)
        {
            return _mnemonics.TryGetValue(opcode, out var mnemonic)
                ? mnemonic
                : $"DATA {ToOctal(opcode)}";
        }

        public bool TryAssemble(string token, out ushort opcode)
        {
            return _opcodes.TryGetValue(token, out opcode);
        }

        private static string ToOctal(ushort value)
        {
            return Convert.ToString(value, 8).PadLeft(6, '0');
        }
    }
}
