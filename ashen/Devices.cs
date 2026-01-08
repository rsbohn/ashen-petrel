using System;
using System.Collections.Generic;
using System.IO;

namespace Ashen
{
    internal interface IDevice
    {
        string Name { get; }
        string Status();
    }

    internal interface IAttachableDevice : IDevice
    {
        bool IsAttached { get; }
        string? Path { get; }
        void Attach(string path, bool createNew);
        void Detach();
    }

    internal interface IBlockDevice : IAttachableDevice
    {
        int BlockWords { get; }
        void ReadBlock(int block, Hp3000Memory memory, int address);
        void WriteBlock(int block, Hp3000Memory memory, int address);
    }

    internal sealed class DeviceRegistry
    {
        private readonly Dictionary<string, IDevice> _devices = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, IDevice device)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Device name required.", nameof(name));
            }

            _devices[name] = device ?? throw new ArgumentNullException(nameof(device));
        }

        public bool TryGet(string name, out IDevice device)
        {
            return _devices.TryGetValue(name, out device!);
        }

        public IEnumerable<KeyValuePair<string, IDevice>> All()
        {
            return _devices;
        }
    }

    internal sealed class ConsoleTtyDevice : IDevice
    {
        public string Name => "Console TTY";

        public string Status()
        {
            return "ready";
        }
    }

    internal sealed class LinePrinterDevice : IAttachableDevice
    {
        private string _path;

        public LinePrinterDevice(string defaultPath)
        {
            _path = defaultPath;
        }

        public string Name => "Line Printer";
        public bool IsAttached => true;
        public string? Path => _path;

        public void Attach(string path, bool createNew)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path required.", nameof(path));
            }

            if (createNew && File.Exists(path))
            {
                File.Delete(path);
            }

            DeviceHelpers.EnsureDirectory(path);
            _path = path;
        }

        public void Detach()
        {
            // Keep attached so output remains available.
        }

        public void PrintLine(string text)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_path) ?? ".");
            File.AppendAllText(_path, text + Environment.NewLine);
        }

        public string Status()
        {
            return $"output={_path}";
        }
    }

    internal sealed class MagTapeDevice : IBlockDevice
    {
        private readonly int _blockWords;
        private string? _path;

        public MagTapeDevice(int blockWords)
        {
            _blockWords = blockWords;
        }

        public string Name => "9-Track Tape";
        public int BlockWords => _blockWords;
        public bool IsAttached => _path != null;
        public string? Path => _path;

        public void Attach(string path, bool createNew)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path required.", nameof(path));
            }

            if (createNew)
            {
                DeviceHelpers.EnsureDirectory(path);
                using var _ = File.Create(path);
            }

            _path = path;
        }

        public void Detach()
        {
            _path = null;
        }

        public void ReadBlock(int block, Hp3000Memory memory, int address)
        {
            EnsureAttached();
            using var stream = OpenStream(FileAccess.ReadWrite);
            ReadBlockInternal(stream, block, memory, address);
        }

        public void WriteBlock(int block, Hp3000Memory memory, int address)
        {
            EnsureAttached();
            using var stream = OpenStream(FileAccess.ReadWrite);
            WriteBlockInternal(stream, block, memory, address);
        }

        public string Status()
        {
            return _path == null ? "detached" : $"attached={_path}";
        }

        private FileStream OpenStream(FileAccess access)
        {
            return new FileStream(_path!, FileMode.OpenOrCreate, access, FileShare.Read);
        }

        private void EnsureAttached()
        {
            if (_path == null)
            {
                throw new InvalidOperationException("Tape not attached.");
            }
        }

        private void ReadBlockInternal(FileStream stream, int block, Hp3000Memory memory, int address)
        {
            var buffer = new byte[_blockWords * 2];
            stream.Seek((long)block * buffer.Length, SeekOrigin.Begin);
            var bytes = stream.Read(buffer, 0, buffer.Length);
            if (bytes < buffer.Length)
            {
                Array.Clear(buffer, bytes, buffer.Length - bytes);
            }

            for (var i = 0; i < _blockWords; i++)
            {
                var hi = buffer[i * 2];
                var lo = buffer[i * 2 + 1];
                memory.Write(address + i, (ushort)((hi << 8) | lo));
            }
        }

        private void WriteBlockInternal(FileStream stream, int block, Hp3000Memory memory, int address)
        {
            var buffer = new byte[_blockWords * 2];
            for (var i = 0; i < _blockWords; i++)
            {
                var value = memory.Read(address + i);
                buffer[i * 2] = (byte)(value >> 8);
                buffer[i * 2 + 1] = (byte)(value & 0xff);
            }

            stream.Seek((long)block * buffer.Length, SeekOrigin.Begin);
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    internal sealed class DiskDevice : IBlockDevice
    {
        private readonly int _blockWords;
        private string? _path;

        public DiskDevice(int blockWords)
        {
            _blockWords = blockWords;
        }

        public string Name => "Disk";
        public int BlockWords => _blockWords;
        public bool IsAttached => _path != null;
        public string? Path => _path;

        public void Attach(string path, bool createNew)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path required.", nameof(path));
            }

            if (createNew)
            {
                DeviceHelpers.EnsureDirectory(path);
                using var _ = File.Create(path);
            }

            _path = path;
        }

        public void Detach()
        {
            _path = null;
        }

        public void ReadBlock(int block, Hp3000Memory memory, int address)
        {
            EnsureAttached();
            using var stream = OpenStream(FileAccess.ReadWrite);
            ReadBlockInternal(stream, block, memory, address);
        }

        public void WriteBlock(int block, Hp3000Memory memory, int address)
        {
            EnsureAttached();
            using var stream = OpenStream(FileAccess.ReadWrite);
            WriteBlockInternal(stream, block, memory, address);
        }

        public string Status()
        {
            return _path == null ? "detached" : $"attached={_path}";
        }

        private FileStream OpenStream(FileAccess access)
        {
            return new FileStream(_path!, FileMode.OpenOrCreate, access, FileShare.Read);
        }

        private void EnsureAttached()
        {
            if (_path == null)
            {
                throw new InvalidOperationException("Disk not attached.");
            }
        }

        private void ReadBlockInternal(FileStream stream, int block, Hp3000Memory memory, int address)
        {
            var buffer = new byte[_blockWords * 2];
            stream.Seek((long)block * buffer.Length, SeekOrigin.Begin);
            var bytes = stream.Read(buffer, 0, buffer.Length);
            if (bytes < buffer.Length)
            {
                Array.Clear(buffer, bytes, buffer.Length - bytes);
            }

            for (var i = 0; i < _blockWords; i++)
            {
                var hi = buffer[i * 2];
                var lo = buffer[i * 2 + 1];
                memory.Write(address + i, (ushort)((hi << 8) | lo));
            }
        }

        private void WriteBlockInternal(FileStream stream, int block, Hp3000Memory memory, int address)
        {
            var buffer = new byte[_blockWords * 2];
            for (var i = 0; i < _blockWords; i++)
            {
                var value = memory.Read(address + i);
                buffer[i * 2] = (byte)(value >> 8);
                buffer[i * 2 + 1] = (byte)(value & 0xff);
            }

            stream.Seek((long)block * buffer.Length, SeekOrigin.Begin);
            stream.Write(buffer, 0, buffer.Length);
        }

    }

    internal static class DeviceHelpers
    {
        public static void EnsureDirectory(string path)
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
