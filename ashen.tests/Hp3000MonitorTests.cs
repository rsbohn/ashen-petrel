using System;
using System.Collections.Generic;
using System.Reflection;
using Ashen;

namespace ashen.tests;

public class Hp3000MonitorTests
{
    [Fact]
    public void TryResolveOperand_Stor_LabelStartingWithP_ShouldResolveSymbol()
    {
        var symbols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["PA"] = 0x0123
        };

        var ok = TryResolveOperand("STOR", "PA", 0x0200, symbols, out var resolved, out var error);

        Assert.True(ok, error);
        Assert.Equal(Convert.ToString(0x0123, 8), resolved);

        var isa = new Hp3000Isa();
        Assert.True(isa.TryAssemble("STOR", resolved, out _));
    }

    private static bool TryResolveOperand(
        string mnemonic,
        string operand,
        int address,
        Dictionary<string, int> symbols,
        out string resolved,
        out string error)
    {
        var method = typeof(Hp3000Monitor).GetMethod(
            "TryResolveOperand",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var args = new object?[] { mnemonic, operand, address, symbols, null, null };
        var result = (bool)method!.Invoke(null, args)!;
        resolved = (string)args[4]!;
        error = (string)args[5]!;
        return result;
    }
}
