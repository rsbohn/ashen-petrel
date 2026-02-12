using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ashen;

namespace ashen.tests;

public class Hp3000MonitorTests
{
    [Fact]
    public void AssembleFile_Dd_ShouldEmitDoublewords()
    {
        var memory = new Hp3000Memory(0x8000);
        var devices = new DeviceRegistry();
        var ioBus = new Hp3000IoBus(devices);
        var cpu = new Hp3000Cpu(memory, ioBus, devices);
        var monitor = new Hp3000Monitor(cpu, memory, devices);

        var path = Path.Combine(Path.GetTempPath(), $"ashen-dd-{Guid.NewGuid():N}.asm");
        File.WriteAllText(path, "ORG 0\nDD $12345678\nDD #3000000000\nDD 37777777777\n");

        try
        {
            AssembleFile(monitor, path);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        Assert.Equal(0x1234, memory.Read(0));
        Assert.Equal(0x5678, memory.Read(1));
        Assert.Equal(0xB2D0, memory.Read(2));
        Assert.Equal(0x5E00, memory.Read(3));
        Assert.Equal(0xFFFF, memory.Read(4));
        Assert.Equal(0xFFFF, memory.Read(5));
    }

    [Fact]
    public void AssembleFile_Dq_ShouldEmitQuadwords()
    {
        var memory = new Hp3000Memory(0x8000);
        var devices = new DeviceRegistry();
        var ioBus = new Hp3000IoBus(devices);
        var cpu = new Hp3000Cpu(memory, ioBus, devices);
        var monitor = new Hp3000Monitor(cpu, memory, devices);

        var path = Path.Combine(Path.GetTempPath(), $"ashen-dq-{Guid.NewGuid():N}.asm");
        File.WriteAllText(path, "ORG 0\nDQ $1122334455667788\nDQ #1234567890123456789\n");

        try
        {
            AssembleFile(monitor, path);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        Assert.Equal(0x1122, memory.Read(0));
        Assert.Equal(0x3344, memory.Read(1));
        Assert.Equal(0x5566, memory.Read(2));
        Assert.Equal(0x7788, memory.Read(3));
        Assert.Equal(0x1122, memory.Read(4));
        Assert.Equal(0x10F4, memory.Read(5));
        Assert.Equal(0x7DE9, memory.Read(6));
        Assert.Equal(0x8115, memory.Read(7));
    }

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

    [Fact]
    public void TryResolveOperand_Ldd_DbPlusLiteral_ShouldResolve()
    {
        var symbols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var ok = TryResolveOperand("LDD", "DB+10", 0x0200, symbols, out var resolved, out var error);

        Assert.True(ok, error);
        Assert.Equal("DB+10", resolved);

        var isa = new Hp3000Isa();
        Assert.True(isa.TryAssemble("LDD", resolved, out _));
    }

    [Fact]
    public void TryResolveOperand_Load_DbPlusLiteral_ShouldResolve()
    {
        var symbols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var ok = TryResolveOperand("LOAD", "DB+36", 0x0200, symbols, out var resolved, out var error);

        Assert.True(ok, error);
        Assert.Equal("DB+36", resolved);

        var isa = new Hp3000Isa();
        Assert.True(isa.TryAssemble("LOAD", resolved, out var opcode));
        Assert.Equal(Convert.ToUInt16("041036", 8), opcode);
    }

    [Fact]
    public void AssembleCommand_ShouldAssembleWithAddressFirst()
    {
        var memory = new Hp3000Memory(0x8000);
        var devices = new DeviceRegistry();
        var ioBus = new Hp3000IoBus(devices);
        var cpu = new Hp3000Cpu(memory, ioBus, devices);
        var monitor = new Hp3000Monitor(cpu, memory, devices);
        var isa = new Hp3000Isa();

        ExecuteLine(monitor, "asm 10 LDI 3");

        Assert.True(isa.TryAssemble("LDI", "3", out var ldi));
        Assert.Equal(ldi, memory.Read(Convert.ToInt32("10", 8)));

        ExecuteLine(monitor, "asm 12 NOP");

        Assert.Equal(0x0000, memory.Read(Convert.ToInt32("12", 8)));
    }

    [Fact]
    public void AssembleCommand_WithAddressOnly_ShouldEnterInteractiveAssembler()
    {
        var memory = new Hp3000Memory(0x8000);
        var devices = new DeviceRegistry();
        var ioBus = new Hp3000IoBus(devices);
        var cpu = new Hp3000Cpu(memory, ioBus, devices);
        var monitor = new Hp3000Monitor(cpu, memory, devices);

        var input = new StringReader("DUP, INCA\n$\n");
        var output = new StringWriter();
        var originalIn = Console.In;
        var originalOut = Console.Out;
        Console.SetIn(input);
        Console.SetOut(output);

        try
        {
            ExecuteLine(monitor, "asm 200");
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }

        Assert.Equal(0x095B, memory.Read(Convert.ToInt32("200", 8)));
        Assert.Contains("000200? ", output.ToString());
        Assert.Contains("000200: 004533", output.ToString());
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

    private static void ExecuteLine(Hp3000Monitor monitor, string line)
    {
        var method = typeof(Hp3000Monitor).GetMethod(
            "ExecuteLine",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        method!.Invoke(monitor, new object?[] { line });
    }

    private static void AssembleFile(Hp3000Monitor monitor, string path)
    {
        var method = typeof(Hp3000Monitor).GetMethod(
            "AssembleFile",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        method!.Invoke(monitor, new object?[] { path });
    }
}
