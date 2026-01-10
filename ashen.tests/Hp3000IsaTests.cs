using Ashen;

namespace ashen.tests;

public class Hp3000IsaTests
{
    private Hp3000Cpu CreateCpu()
    {
        var memory = new Hp3000Memory(0x8000);
        var ioBus = new Hp3000IoBus();
        var devices = new DeviceRegistry();
        return new Hp3000Cpu(memory, ioBus, devices);
    }

    [Fact]
    public void Nop_ShouldDoNothing()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        isa.TryExecute(0x0000, cpu);
        
        Assert.Equal(0, cpu.Sr);
    }

    [Fact]
    public void Delb_ShouldRemoveSecondStackValue()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(1);
        cpu.Push(2);
        cpu.Push(3);
        isa.TryExecute(0x0001, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(3, cpu.Ra);
        Assert.Equal(1, cpu.Rb);
    }

    [Fact]
    public void Zero_ShouldPushZero()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        isa.TryExecute(0x0006, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0, cpu.Ra);
    }

    [Fact]
    public void Dzro_ShouldPushTwoZeros()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        isa.TryExecute(0x0007, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0, cpu.Ra);
        Assert.Equal(0, cpu.Rb);
    }

    [Fact]
    public void Add_ShouldAddTwoNumbers()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        cpu.Push(20);
        isa.TryExecute(0x0010, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(30, cpu.Ra);
    }

    [Fact]
    public void Add_ShouldUpdateStaFlags()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x7FFF);
        cpu.Push(1);
        isa.TryExecute(0x0010, cpu);

        Assert.Equal(0x8000, cpu.Ra);
        Assert.Equal(0x0900, cpu.Sta);
    }

    [Fact]
    public void Add_WithCarry_ShouldSetCarryAndZero()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0xFFFF);
        cpu.Push(1);
        isa.TryExecute(0x0010, cpu);

        Assert.Equal(0, cpu.Ra);
        Assert.Equal(0x0600, cpu.Sta);
    }

    [Fact]
    public void Sub_ShouldSubtractTwoNumbers()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(30);
        cpu.Push(10);
        isa.TryExecute(0x0011, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(20, cpu.Ra);
    }

    [Fact]
    public void Sub_ShouldUpdateStaFlags()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0);
        cpu.Push(1);
        isa.TryExecute(0x0011, cpu);

        Assert.Equal(0xFFFF, cpu.Ra);
        Assert.Equal(0x0500, cpu.Sta);
    }

    [Fact]
    public void Mpy_ShouldMultiplyTwoNumbers()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(5);
        cpu.Push(6);
        isa.TryExecute(0x0012, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(30, cpu.Ra);
    }

    [Fact]
    public void Div_ShouldDivideTwoNumbers()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(30);
        cpu.Push(5);
        isa.TryExecute(0x0013, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0, cpu.Ra);
        Assert.Equal(6, cpu.Rb);
    }

    [Fact]
    public void Div_ByZero_ShouldReturnZero()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(30);
        cpu.Push(0);
        isa.TryExecute(0x0013, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0, cpu.Ra);
        Assert.Equal(0, cpu.Rb);
    }

    [Fact]
    public void Neg_ShouldNegateNumber()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        isa.TryExecute(0x0014, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(unchecked((ushort)(-10)), cpu.Ra);
    }

    [Fact]
    public void Xch_ShouldExchangeTopTwoStackValues()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        cpu.Push(20);
        isa.TryExecute(0x001A, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(10, cpu.Ra);
        Assert.Equal(20, cpu.Rb);
    }

    [Fact]
    public void Inca_ShouldIncrementTopOfStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        isa.TryExecute(0x001B, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(11, cpu.Ra);
    }

    [Fact]
    public void Deca_ShouldDecrementTopOfStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        isa.TryExecute(0x001C, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(9, cpu.Ra);
    }

    [Fact]
    public void Del_ShouldRemoveTopOfStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        cpu.Push(20);
        isa.TryExecute(0x0020, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(10, cpu.Ra);
    }

    [Fact]
    public void Ddel_ShouldRemoveTopTwoFromStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        cpu.Push(20);
        cpu.Push(30);
        isa.TryExecute(0x0002, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(10, cpu.Ra);
    }

    [Fact]
    public void Dup_ShouldDuplicateTopOfStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        isa.TryExecute(0x0025, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(10, cpu.Ra);
        Assert.Equal(10, cpu.Rb);
    }

    [Fact]
    public void Ddup_ShouldDuplicateTopTwoOnStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        cpu.Push(20);
        isa.TryExecute(0x0026, cpu);
        
        Assert.Equal(4, cpu.Sr);
        Assert.Equal(20, cpu.Ra);
        Assert.Equal(10, cpu.Rb);
        Assert.Equal(20, cpu.Rc);
        Assert.Equal(10, cpu.Rd);
    }

    [Fact]
    public void Not_ShouldInvertBits()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x00FF);
        isa.TryExecute(0x0034, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0xFF00, cpu.Ra);
    }

    [Fact]
    public void Or_ShouldPerformBitwiseOr()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x00FF);
        cpu.Push(0xFF00);
        isa.TryExecute(0x0035, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0xFFFF, cpu.Ra);
    }

    [Fact]
    public void Xor_ShouldPerformBitwiseXor()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0xFFFF);
        cpu.Push(0x00FF);
        isa.TryExecute(0x0036, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0xFF00, cpu.Ra);
    }

    [Fact]
    public void And_ShouldPerformBitwiseAnd()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0xFFFF);
        cpu.Push(0x00FF);
        isa.TryExecute(0x0037, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0x00FF, cpu.Ra);
    }

    [Fact]
    public void TryAssemble_ShouldAssembleMnemonic()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("ADD", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0x0010, opcode);
    }

    [Fact]
    public void TryAssemble_WithInvalidMnemonic_ShouldReturnFalse()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("INVALID", out var opcode);
        
        Assert.False(result);
        Assert.Equal(0, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_ForwardWithoutModifiers()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", "P+10", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC008, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_BackwardWithoutModifiers()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", "P-10", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC108, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_WithIndirect()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", "P+10,I", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC408, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_WithIndexed()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", "P+10,X", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC808, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_WithBothModifiers()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", "P+10,I,X", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xCC08, opcode);
    }

    [Fact]
    public void Disassemble_ShouldReturnMnemonics()
    {
        var isa = new Hp3000Isa();
        
        var disassembly = isa.Disassemble(0x0410);
        
        Assert.Equal("ADD, ADD", disassembly);
    }

    [Fact]
    public void Disassemble_Branch_ForwardSimple()
    {
        var isa = new Hp3000Isa();
        
        var disassembly = isa.Disassemble(0xC008);
        
        Assert.Equal("BR P+10", disassembly);
    }

    [Fact]
    public void Disassemble_Branch_BackwardWithModifiers()
    {
        var isa = new Hp3000Isa();
        
        var disassembly = isa.Disassemble(0xCD08);
        
        Assert.Equal("BR P-10,I,X", disassembly);
    }

    [Fact]
    public void TryExecuteWord_Branch_Forward()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        cpu.Pc = 100;
        
        var result = isa.TryExecuteWord(0xC00A, cpu);
        
        Assert.True(result);
        Assert.Equal(109, cpu.Pc);
    }

    [Fact]
    public void TryExecuteWord_Branch_Backward()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        cpu.Pc = 100;
        
        var result = isa.TryExecuteWord(0xC10A, cpu);
        
        Assert.True(result);
        Assert.Equal(89, cpu.Pc);
    }

    [Fact]
    public void TryExecuteWord_Branch_WithIndex()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        cpu.Pc = 100;
        cpu.X = 5;
        
        var result = isa.TryExecuteWord(0xC80A, cpu);
        
        Assert.True(result);
        Assert.Equal(114, cpu.Pc);
    }

    [Fact]
    public void TryExecuteWord_Branch_WithIndirect()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        cpu.Pc = 100;
        cpu.WriteWord(109, 200);
        
        var result = isa.TryExecuteWord(0xC40A, cpu);
        
        Assert.True(result);
        Assert.Equal(200, cpu.Pc);
    }

    [Fact]
    public void TryExecuteWord_NonBranch_ShouldReturnFalse()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        var result = isa.TryExecuteWord(0x0010, cpu);
        
        Assert.False(result);
    }

    [Fact]
    public void Stax_ShouldStoreValueToX()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x1234);
        isa.TryExecute(0x0023, cpu);
        
        Assert.Equal(0, cpu.Sr);
        Assert.Equal(0x1234, cpu.X);
    }

    [Fact]
    public void Ldxa_ShouldPushXToStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.X = 0x5678;
        isa.TryExecute(0x0024, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0x5678, cpu.Ra);
    }

    [Fact]
    public void Stax_Ldxa_Combination_ShouldMoveStackToX()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0xAAAA);
        isa.TryExecute(0x0023, cpu); // STAX - pop into X
        isa.TryExecute(0x0024, cpu); // LDXA - push X

        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0xAAAA, cpu.Ra);
    }
}
