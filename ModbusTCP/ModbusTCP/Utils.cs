using System.Net;

namespace ModbusTCP;

internal static class Utils
{
    // ------------------------------------------------------------------------
    // Create modbus header for read/write action
    internal static byte[] CreateReadWriteHeader(ushort id, byte unit, ushort startReadAddress, ushort numRead, ushort startWriteAddress, ushort numWrite)
    {
        var data = new byte[numWrite * 2 + 17];
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 0, 2),
            id);
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 4, 2),
            IPAddress.HostToNetworkOrder((short)(11 + numWrite * 2)));
        data[6] = unit;						// Slave address
        data[7] = (byte)ModbusFunction.READ_WRITE_MULTIPLE_REGISTER;	// Function code
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 8, 2),
            IPAddress.HostToNetworkOrder((short)startReadAddress));
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 10, 2),
            IPAddress.HostToNetworkOrder((short)numRead));
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 12, 2),
            IPAddress.HostToNetworkOrder((short)startWriteAddress));
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 14, 2),
            IPAddress.HostToNetworkOrder((short)numWrite));
        data[16] = (byte)(numWrite * 2);
        return data;
    }

    // ------------------------------------------------------------------------
    // Create modbus header for read action
    internal static byte[] CreateReadHeader(ushort id, byte unit, ushort startAddress, ushort length, ModbusFunction function)
    {
        var data = new byte[12];
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 0, 2), 
            id);
        data[5] = 6;
        data[6] = unit;
        data[7] = (byte)function;
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 8, 2),
            IPAddress.HostToNetworkOrder((short)startAddress));
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 10, 2),
            IPAddress.HostToNetworkOrder((short)length));
        return data;
    }

    internal static byte[] CreateReadDeviceIdentifiers(ushort id, byte unit, byte objectId)
    {
        var data = new byte[12];
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 0, 2),
            id);
        data[5] = 5;
        data[6] = unit;
        data[7] = (byte)ModbusFunction.READ_DEVICE_IDENTIFIERS;
        data[8] = 0xe;
        data[9] = 1;
        data[10] = objectId;
        return data;
    }

    // ------------------------------------------------------------------------
    // Create modbus header for write action
    internal static byte[] CreateWriteHeader(ushort id, byte unit, ushort startAddress, ushort numData, ushort numBytes, ModbusFunction function)
    {
        var adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
        var data = new byte[numBytes + 11];
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 0, 2),
            id);
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 4, 2),
            IPAddress.HostToNetworkOrder((short)(5 + numBytes)));
        data[6] = unit;				// Slave address
        data[7] = (byte)function;	// Function code
        BitConverter.TryWriteBytes(
            new Span<byte>(data, 8, 2),
            IPAddress.HostToNetworkOrder((short)startAddress));
        switch (function)
        {
            case ModbusFunction.WRITE_MULTIPLE_COILS:
            case ModbusFunction.WRITE_MULTIPLE_REGISTER:
                BitConverter.TryWriteBytes(
                    new Span<byte>(data, 10, 2),
                    IPAddress.HostToNetworkOrder((short)numData));
                data[12] = (byte)(numBytes - 2);
                break;
        }
        return data;
    }
}