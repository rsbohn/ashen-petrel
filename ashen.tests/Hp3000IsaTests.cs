using Ashen;

namespace ashen.tests;

public class Hp3000IsaTests
{
    private Hp3000Cpu CreateCpu()
    {
        var memory = new Hp3000Memory(0x8000);
        var devices = new DeviceRegistry();
        var ioBus = new Hp3000IoBus(devices);
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
    public void Dadd_ShouldAddTwoDoublewords()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0002); // A low
        cpu.Push(0x0000); // B high
        cpu.Push(0x0003); // C low
        cpu.Push(0x0000); // D high
        isa.TryExecute(0x0009, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0000, cpu.Ra);
        Assert.Equal(0x0005, cpu.Rb);
    }

    [Fact]
    public void Dcmp_ShouldSetCcAndPopDoublewords()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        cpu.Sta = 0x0C00;

        cpu.Push(0x0000); // D high (left)
        cpu.Push(0x0002); // C low (left)
        cpu.Push(0x0000); // B high (right)
        cpu.Push(0x0001); // A low (right)
        isa.TryExecute(0x0008, cpu);

        Assert.Equal(0, cpu.Sr);
        Assert.Equal(0x0000, cpu.Sta & 0x0300);
        Assert.Equal(0x0C00, cpu.Sta & 0x0C00);
    }

    [Fact]
    public void Dcmp_Equal_ShouldSetCce()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x1234);
        cpu.Push(0x0000);
        cpu.Push(0x1234);
        cpu.Push(0x0000);
        isa.TryExecute(0x0008, cpu);

        Assert.Equal(0x0200, cpu.Sta & 0x0300);
    }

    [Fact]
    public void Ldd_ShouldPushDoublewordFromDb()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Db = 0x0100;
        cpu.WriteWord(0x0110, 0x1234);
        cpu.WriteWord(0x0111, 0xABCD);

        isa.TryExecuteWord((ushort)(0xD200 | 0x0010), cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0xABCD, cpu.Ra);
        Assert.Equal(0x1234, cpu.Rb);
    }

    [Fact]
    public void Std_ShouldStoreDoublewordToDb()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Db = 0x0200;
        cpu.Push(0xBEEF);
        cpu.Push(0xCAFE);

        isa.TryExecuteWord((ushort)(0xE200 | 0x0008), cpu);

        Assert.Equal(0, cpu.Sr);
        Assert.Equal(0xBEEF, cpu.ReadWord(0x0208));
        Assert.Equal(0xCAFE, cpu.ReadWord(0x0209));
    }

    [Fact]
    public void Pop_WithSpilledStack_ShouldKeepSrAtFour()
    {
        var cpu = CreateCpu();

        cpu.Push(1);
        cpu.Push(2);
        cpu.Push(3);
        cpu.Push(4);
        cpu.Push(5);

        Assert.Equal(4, cpu.Sr);
        Assert.Equal(5, cpu.StackDepth);

        cpu.Pop();

        Assert.Equal(4, cpu.Sr);
        Assert.Equal(4, cpu.StackDepth);
        Assert.Equal(4, cpu.Ra);
        Assert.Equal(1, cpu.Rd);
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
    public void Divl_ShouldDivideLong()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0002); // C high
        cpu.Push(0x0000); // B low
        cpu.Push(0x0003); // A divisor => dividend 0x00020000
        isa.TryExecute(0x000C, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0002, cpu.Ra); // remainder
        Assert.Equal(0xAAAA, cpu.Rb); // quotient 0x0000_aaaa
        Assert.Equal(0x0000, cpu.Sta & 0x0800);
    }

    [Fact]
    public void Divl_ByZero_ShouldSetOverflow()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x5678); // C high
        cpu.Push(0x1234); // B low
        cpu.Push(0x0000); // A divisor
        isa.TryExecute(0x000C, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0000, cpu.Ra);
        Assert.Equal(0x0000, cpu.Rb);
        Assert.Equal(0x0800, cpu.Sta & 0x0800);
    }

    [Fact]
    public void Ddiv_ShouldDivideDoubleword()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0002); // D high dividend
        cpu.Push(0x0000); // C low dividend
        cpu.Push(0x0000); // B high divisor
        cpu.Push(0x0003); // A low divisor => divisor 3
        isa.TryExecuteWord(0x2179, cpu);

        Assert.Equal(4, cpu.Sr);
        Assert.Equal(0x0002, cpu.Ra); // remainder low
        Assert.Equal(0x0000, cpu.Rb); // remainder high
        Assert.Equal(0xAAAA, cpu.Rc); // quotient low
        Assert.Equal(0x0000, cpu.Rd); // quotient high
        Assert.Equal(0x0000, cpu.Sta & 0x0800);
    }

    [Fact]
    public void Ddiv_ByZero_ShouldSetOverflow()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0xDEAD); // D high dividend
        cpu.Push(0xBEEF); // C low dividend
        cpu.Push(0x0000); // B high divisor
        cpu.Push(0x0000); // A low divisor
        isa.TryExecuteWord(0x2179, cpu);

        Assert.Equal(4, cpu.Sr);
        Assert.Equal(0x0000, cpu.Ra);
        Assert.Equal(0x0000, cpu.Rb);
        Assert.Equal(0x0000, cpu.Rc);
        Assert.Equal(0x0000, cpu.Rd);
        Assert.Equal(0x0800, cpu.Sta & 0x0800);
    }

    [Fact]
    public void Ldiv_ShouldDivideUnsignedLong()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0001); // C high
        cpu.Push(0x0001); // B low
        cpu.Push(0x0002); // A divisor => dividend 0x00010001
        isa.TryExecute(0x0033, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0001, cpu.Ra); // remainder
        Assert.Equal(0x8000, cpu.Rb); // quotient 0x8000
        Assert.Equal(0x0000, cpu.Sta & 0x0800);
    }

    [Fact]
    public void Ldiv_WithQuotientOverflow_ShouldWrapAndSetOverflow()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0001); // C high
        cpu.Push(0x0000); // B low
        cpu.Push(0x0001); // A divisor => dividend 0x00010000
        isa.TryExecute(0x0033, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0000, cpu.Ra); // remainder
        Assert.Equal(0x0000, cpu.Rb); // quotient wraps
        Assert.Equal(0x0800, cpu.Sta & 0x0800);
    }

    [Fact]
    public void Lmpy_ShouldMultiplyToDoubleword()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0003);
        cpu.Push(0x0004);
        isa.TryExecute(0x0032, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x000C, cpu.Ra); // low
        Assert.Equal(0x0000, cpu.Rb); // high
        Assert.Equal(0x0000, cpu.Sta & 0x0400);
        Assert.Equal(0x0000, cpu.Sta & 0x0300);
    }

    [Fact]
    public void Lmpy_WithHighWord_ShouldSetCarryAndCc()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x1000);
        cpu.Push(0x1000);
        isa.TryExecute(0x0032, cpu);

        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0000, cpu.Ra); // low
        Assert.Equal(0x0100, cpu.Rb); // high
        Assert.Equal(0x0400, cpu.Sta & 0x0400);
        Assert.Equal(0x0200, cpu.Sta & 0x0300);
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
    public void Test_ShouldUpdateStaCc()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0);
        isa.TryExecute(0x0015, cpu);

        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0x0200, cpu.Sta);
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
    public void Xax_ShouldExchangeAWithX()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x1234);
        cpu.X = 0x5678;
        isa.TryExecute(0x001D, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0x5678, cpu.Ra);
        Assert.Equal(0x1234, cpu.X);
    }

    [Fact]
    public void Adax_ShouldAddAToX()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x0010);
        cpu.X = 0x0020;
        isa.TryExecute(0x001E, cpu);
        
        Assert.Equal(0, cpu.Sr);
        Assert.Equal(0, cpu.StackDepth);
        Assert.Equal(0x0030, cpu.X);
    }

    [Fact]
    public void Adax_WithOverflow_ShouldSetFlags()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x0001);
        cpu.X = 0x7FFF;
        isa.TryExecute(0x001E, cpu);
        
        Assert.Equal(0, cpu.Sr);
        Assert.Equal(0, cpu.StackDepth);
        Assert.Equal(0x8000, cpu.X);
        Assert.Equal(0x0900, cpu.Sta); // Overflow and CCL
    }

    [Fact]
    public void Adxa_ShouldAddXToA()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x0010);
        cpu.X = 0x0020;
        isa.TryExecute(0x001F, cpu);
        
        Assert.Equal(1, cpu.Sr);
        Assert.Equal(0x0030, cpu.Ra);
        Assert.Equal(0x0020, cpu.X);
    }

    [Fact]
    public void Adxa_WithCarry_ShouldSetFlags()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0xFFFF);
        cpu.X = 0x0001;
        isa.TryExecute(0x001F, cpu);
        
        Assert.Equal(0x0000, cpu.Ra);
        Assert.Equal(0x0600, cpu.Sta); // Carry and CCE
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
    public void Not_ShouldUpdateStaCc()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0000);
        isa.TryExecute(0x0034, cpu);

        Assert.Equal(0xFFFF, cpu.Ra);
        Assert.Equal(0x0100, cpu.Sta);
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
        
        var result = isa.TryAssemble("BR", ".+10", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC008, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_BackwardWithoutModifiers()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", ".-10", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC108, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_WithIndirect()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", ".+10,I", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC408, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_WithIndexed()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", ".+10,X", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xC808, opcode);
    }

    [Fact]
    public void TryAssemble_Branch_WithBothModifiers()
    {
        var isa = new Hp3000Isa();
        
        var result = isa.TryAssemble("BR", ".+10,I,X", out var opcode);
        
        Assert.True(result);
        Assert.Equal(0xCC08, opcode);
    }

    [Fact]
    public void TryAssemble_Bro_ShouldAssemble()
    {
        var isa = new Hp3000Isa();

        var result = isa.TryAssemble("BRO", ".+2", out var opcode);

        Assert.True(result);
        Assert.Equal(0x1782, opcode);
    }

    [Fact]
    public void TryAssemble_Wio_ShouldAssemble()
    {
        var isa = new Hp3000Isa();

        var result = isa.TryAssemble("WIO", "01", out var opcode);

        Assert.True(result);
        Assert.Equal(0x3091, opcode);
    }

    [Fact]
    public void TryAssemble_Rio_ShouldAssemble()
    {
        var isa = new Hp3000Isa();

        var result = isa.TryAssemble("RIO", "17", out var opcode);

        Assert.True(result);
        Assert.Equal(0x308F, opcode);
    }

    [Fact]
    public void TryAssemble_Bcc_Mnemonics_ShouldMatchOpcodes()
    {
        var isa = new Hp3000Isa();

        Assert.True(isa.TryAssemble("BN", ".+2", out var bn));
        Assert.Equal(0xC202, bn);

        Assert.True(isa.TryAssemble("BL", ".+2", out var bl));
        Assert.Equal(0xC242, bl);

        Assert.True(isa.TryAssemble("BE", ".+2", out var be));
        Assert.Equal(0xC282, be);

        Assert.True(isa.TryAssemble("BLE", ".+2", out var ble));
        Assert.Equal(0xC2C2, ble);

        Assert.True(isa.TryAssemble("BG", ".+2", out var bg));
        Assert.Equal(0xC302, bg);

        Assert.True(isa.TryAssemble("BNE", ".+2", out var bne));
        Assert.Equal(0xC342, bne);

        Assert.True(isa.TryAssemble("BGE", ".+2", out var bge));
        Assert.Equal(0xC382, bge);

        Assert.True(isa.TryAssemble("BA", ".+2", out var ba));
        Assert.Equal(0xC3C2, ba);
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
        
        Assert.Equal("BR .+10", disassembly);
    }

    [Fact]
    public void Disassemble_Branch_BackwardWithModifiers()
    {
        var isa = new Hp3000Isa();
        
        var disassembly = isa.Disassemble(0xCD08);
        
        Assert.Equal("BR .-10,I,X", disassembly);
    }

    [Fact]
    public void Disassemble_Bro_ShouldReturnMnemonic()
    {
        var isa = new Hp3000Isa();

        var disassembly = isa.Disassemble(0x1782);

        Assert.Equal("BRO .+2", disassembly);
    }

    [Fact]
    public void Disassemble_Wio_ShouldReturnMnemonic()
    {
        var isa = new Hp3000Isa();

        var disassembly = isa.Disassemble(0x3091);

        Assert.Equal("WIO 1", disassembly);
    }

    [Fact]
    public void Disassemble_Rio_ShouldReturnMnemonic()
    {
        var isa = new Hp3000Isa();

        var disassembly = isa.Disassemble(0x308F);

        Assert.Equal("RIO 17", disassembly);
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
    public void TryExecuteWord_Bro_WithOddTop_ShouldBranchAndPop()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        cpu.Pc = 100;
        cpu.Sta = 0x0400;
        cpu.Push(0x0001);

        var result = isa.TryExecuteWord(0x1782, cpu);

        Assert.True(result);
        Assert.Equal(101, cpu.Pc);
        Assert.Equal(0, cpu.Sr);
        Assert.Equal(0x0400, cpu.Sta);
    }

    [Fact]
    public void TryExecuteWord_Bro_WithEvenTop_ShouldNotBranchAndPop()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        cpu.Pc = 100;
        cpu.Sta = 0x0100;
        cpu.Push(0x0002);

        var result = isa.TryExecuteWord(0x1782, cpu);

        Assert.True(result);
        Assert.Equal(100, cpu.Pc);
        Assert.Equal(0, cpu.Sr);
        Assert.Equal(0x0100, cpu.Sta);
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

    [Fact]
    public void Zrox_ShouldSetXToZero()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.X = 0x1234;
        isa.TryExecute(0x0003, cpu);
        
        Assert.Equal(0, cpu.X);
    }

    [Fact]
    public void Zrob_ShouldSetBToZeroAndUpdateSr()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x5678);
        cpu.Push(0x1234);
        cpu.Sta = 0x0C00;
        isa.TryExecute(0x0021, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x1234, cpu.Ra);
        Assert.Equal(0x0000, cpu.Rb);
        Assert.Equal(0x0200, cpu.Sta & 0x0300);
    }

    [Fact]
    public void Zrob_WithInsufficientStack_ShouldHalt()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x1234);
        isa.TryExecute(0x0021, cpu);
        
        Assert.True(cpu.Halted);
    }

    [Fact]
    public void Incb_ShouldIncrementSecondOfStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        cpu.Push(20);
        isa.TryExecute(0x003B, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(20, cpu.Ra);
        Assert.Equal(11, cpu.Rb);
    }

    [Fact]
    public void Decb_ShouldDecrementSecondOfStack()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(10);
        cpu.Push(20);
        isa.TryExecute(0x003C, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(20, cpu.Ra);
        Assert.Equal(9, cpu.Rb);
    }

    [Fact]
    public void Xbx_ShouldExchangeBWithX()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x1234);
        cpu.Push(0xABCD);
        cpu.X = 0x5678;
        isa.TryExecute(0x003D, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0xABCD, cpu.Ra);
        Assert.Equal(0x5678, cpu.Rb);
        Assert.Equal(0x1234, cpu.X);
    }

    [Fact]
    public void Adbx_ShouldAddBToX()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x0010);
        cpu.Push(0x0100);
        cpu.X = 0x0020;
        isa.TryExecute(0x003E, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0100, cpu.Ra);
        Assert.Equal(0x0010, cpu.Rb);
        Assert.Equal(0x0030, cpu.X);
    }

    [Fact]
    public void Adbx_WithOverflow_ShouldSetFlags()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x0001);
        cpu.Push(0x0100);
        cpu.X = 0x7FFF;
        isa.TryExecute(0x003E, cpu);
        
        Assert.Equal(0x8000, cpu.X);
        Assert.Equal(0x0900, cpu.Sta); // Overflow and CCL
    }

    [Fact]
    public void Adxb_ShouldAddXToB()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0x0010);
        cpu.Push(0x0100);
        cpu.X = 0x0020;
        isa.TryExecute(0x003F, cpu);
        
        Assert.Equal(2, cpu.Sr);
        Assert.Equal(0x0100, cpu.Ra);
        Assert.Equal(0x0030, cpu.Rb);
        Assert.Equal(0x0020, cpu.X);
    }

    [Fact]
    public void Adxb_WithCarry_ShouldSetFlags()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();
        
        cpu.Push(0xFFFF);
        cpu.Push(0x0100);
        cpu.X = 0x0001;
        isa.TryExecute(0x003F, cpu);
        
        Assert.Equal(0x0000, cpu.Rb);
        Assert.Equal(0x0600, cpu.Sta); // Carry and CCE
    }

    [Fact]
    public void TryAssemble_Dlsl_WithX_ShouldAssemble()
    {
        var isa = new Hp3000Isa();

        Assert.True(isa.TryAssemble("DLSL", "1,X", out var opcode));

        Assert.Equal(0x1681, opcode);
    }

    [Fact]
    public void Disassemble_Dlsr_WithX_ShouldReturnMnemonic()
    {
        var isa = new Hp3000Isa();

        var disassembly = isa.Disassemble(0x16C1);

        Assert.Equal("DLSR 1,X", disassembly);
    }

    [Fact]
    public void ExecuteDlsl_ShouldShiftLeftLogical()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x4000);
        cpu.Push(0x0001);
        cpu.Sta = 0;

        isa.TryExecuteWord(0x1481, cpu);

        Assert.Equal(0x0002, cpu.Ra);
        Assert.Equal(0x8000, cpu.Rb);
        Assert.Equal(0x0100, cpu.Sta);
    }

    [Fact]
    public void ExecuteDlsr_ShouldShiftRightLogicalAndSetCarry()
    {
        var cpu = CreateCpu();
        var isa = new Hp3000Isa();

        cpu.Push(0x0000);
        cpu.Push(0x0001);
        cpu.Sta = 0;

        isa.TryExecuteWord(0x14C1, cpu);

        Assert.Equal(0x0000, cpu.Ra);
        Assert.Equal(0x0000, cpu.Rb);
        Assert.Equal(0x0600, cpu.Sta);
    }
}
