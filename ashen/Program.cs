using System;

namespace Ashen
{
    internal static class Program
    {
        private const int DefaultMemoryWords = 1 << 15;

        public static void Main(string[] args)
        {
            var memory = new Hp3000Memory(DefaultMemoryWords);
            var ioBus = new Hp3000IoBus();
            var devices = new DeviceRegistry();

            devices.Add("tty", new ConsoleTtyDevice());
            devices.Add("lpt", new LinePrinterDevice("./media/print.out"));
            devices.Add("mt0", new MagTapeDevice(128));
            devices.Add("d0", new DiskDevice(128));

            var cpu = new Hp3000Cpu(memory, ioBus, devices);
            var monitor = new Hp3000Monitor(cpu, memory, devices);
            monitor.Run();
        }
    }
}
