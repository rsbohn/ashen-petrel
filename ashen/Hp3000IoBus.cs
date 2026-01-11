using System;

namespace Ashen
{
    internal sealed class Hp3000IoBus
    {
        private readonly DeviceRegistry _devices;

        public Hp3000IoBus(DeviceRegistry devices)
        {
            _devices = devices ?? throw new ArgumentNullException(nameof(devices));
        }

        public void WriteWord(ushort deviceCode, ushort value)
        {
            if (!TryResolveDevice(deviceCode, out var device))
            {
                return;
            }

            if (device is IWordOutputDevice wordOutput)
            {
                wordOutput.WriteWord(value);
                return;
            }

            if (device is IByteOutputDevice byteOutput)
            {
                byteOutput.WriteByte((byte)(value & 0xFF));
            }
        }

        public byte ReadByte(ushort deviceCode)
        {
            if (!TryResolveDevice(deviceCode, out _))
            {
                return 0;
            }

            return 0;
        }

        public bool TryReadStatus(ushort deviceCode, out ushort status)
        {
            status = 0;
            if (!TryResolveDevice(deviceCode, out var device))
            {
                return false;
            }

            if (device is IDeviceStatusWord statusDevice)
            {
                status = statusDevice.ReadStatusWord();
                return true;
            }

            if (device is IWordOutputDevice || device is IByteOutputDevice)
            {
                status = 0x0002;
                return true;
            }

            return false;
        }

        private bool TryResolveDevice(ushort deviceCode, out IDevice device)
        {
            var name = deviceCode switch
            {
                0 => "tty",
                1 => "lpt",
                _ => null
            };

            if (name == null)
            {
                device = null!;
                return false;
            }

            return _devices.TryGet(name, out device);
        }
    }
}
