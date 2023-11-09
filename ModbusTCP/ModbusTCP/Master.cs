using System.Net;
using System.Net.Sockets;

namespace ModbusTCP;

/// <summary>
/// Modbus TCP common driver class. 
/// </summary>
/// 
/// This class implements a modbus TCP master driver. It supports the following commands:
/// 
/// Read coils
/// Read discrete inputs
/// Write single coil
/// Write multiple coils
/// Read holding register
/// Read input register
/// Write single register
/// Write multiple register
/// 
/// All commands can be sent in synchronous or asynchronous mode. If a value is accessed
/// in synchronous mode the program will stop and wait for slave to response. If the 
/// slave didn't answer within a specified time a timeout exception is called.
/// The class uses multi threading for both synchronous and asynchronous access. For
/// the communication two lines are created. This is necessary because the synchronous
/// thread has to wait for a previous command to finish.
/// The synchronous channel can be disabled during connection. This can be necessary when
/// the slave only supports one connection.
/// 
public class Master
{
    // ------------------------------------------------------------------------
    // Private declarations
    private Socket? _socket;
    private readonly byte[] _buffer = new byte[2048];
    private ushort _timeout = 500;

    // ------------------------------------------------------------------------
    /// <summary>Response timeout. If the slave didn't answers within in this time an exception is called.</summary>
    /// <value>The default value is 500ms.</value>
    public ushort Timeout
    {
        get => _timeout;
        set
        {
            _timeout = value;
            _socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
            _socket?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
        }
    }

    // ------------------------------------------------------------------------
    /// <summary>Shows if a connection is active.</summary>
    public bool Connected => _socket?.Connected == true;

    // ------------------------------------------------------------------------
    /// <summary>Create master instance without parameters.</summary>
    public Master()
    {
    }

    // ------------------------------------------------------------------------
    /// <summary>Create master instance with parameters.</summary>
    /// <param name="ipString">IP address of modbus slave.</param>
    /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
    public Master(string ipString, ushort port)
    {
        Connect(ipString, port);
    }

    // ------------------------------------------------------------------------
    /// <summary>Create master instance with parameters.</summary>
    /// <param name="ip">IP address of modbus slave.</param>
    /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
    public Master(IPAddress ip, ushort port)
    {
        Connect(ip, port);
    }

    // ------------------------------------------------------------------------
    /// <summary>Start connection to slave.</summary>
    /// <param name="ipString">IP address of modbus slave.</param>
    /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
    public void Connect(string ipString, ushort port)
    {
        if (!IPAddress.TryParse(ipString, out var address))
        {
            address = Dns.GetHostEntry(ipString).AddressList[0];
        }
        Connect(address, port);
    }

    // ------------------------------------------------------------------------
    /// <summary>Start connection to slave.</summary>
    /// <param name="ipString">IP address of modbus slave.</param>
    /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
    public async Task ConnectAsync(string ipString, ushort port)
    {
        if (!IPAddress.TryParse(ipString, out var address))
        {
            address = (await Dns.GetHostEntryAsync(ipString)).AddressList[0];
        }
        await ConnectAsync(address, port);
    }

    // ------------------------------------------------------------------------
    /// <summary>Start connection to slave.</summary>
    /// <param name="ip">IP address of modbus slave.</param>
    /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
    public void Connect(IPAddress ip, ushort port)
    {
        _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(new IPEndPoint(ip, port));
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);
    }

    // ------------------------------------------------------------------------
    /// <summary>Start connection to slave.</summary>
    /// <param name="ip">IP address of modbus slave.</param>
    /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
    public async Task ConnectAsync(IPAddress ip, ushort port)
    {
        _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await _socket.ConnectAsync(new IPEndPoint(ip, port));
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);
    }

    // ------------------------------------------------------------------------
    /// <summary>Stop connection to slave.</summary>
    public void Disconnect()
    {
        if (Connected)
        {
            try
            {
                _socket?.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // ignored
            }
            _socket?.Close();
        }
        _socket = null;
    }

    // ------------------------------------------------------------------------
    /// <summary>Destroy master instance.</summary>
    ~Master()
    {
        Disconnect();
    }

    // ------------------------------------------------------------------------
    /// <summary>Destroy master instance</summary>
    public void Dispose()
    {
        Disconnect();
    }

    // ------------------------------------------------------------------------
    /// <summary>Read coils from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public byte[] ReadCoils(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 2000)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return WriteSyncData(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_COIL));
    }

    // ------------------------------------------------------------------------
    /// <summary>Read coils from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public async Task<byte[]> ReadCoilsAsync(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 2000)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return await WriteSyncDataAsync(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_COIL));
    }

    // ------------------------------------------------------------------------
    /// <summary>Read discrete inputs from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public byte[] ReadDiscreteInputs(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 2000)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return WriteSyncData(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_DISCRETE_INPUTS));
    }

    // ------------------------------------------------------------------------
    /// <summary>Read discrete inputs from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public async Task<byte[]> ReadDiscreteInputsAsync(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 2000)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return await WriteSyncDataAsync(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_DISCRETE_INPUTS));
    }

    // ------------------------------------------------------------------------
    /// <summary>Read holding registers from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public byte[] ReadHoldingRegister(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 125)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return WriteSyncData(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_HOLDING_REGISTER));
    }

    // ------------------------------------------------------------------------
    /// <summary>Read holding registers from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public async Task<byte[]> ReadHoldingRegisterAsync(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 125)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return await WriteSyncDataAsync(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_HOLDING_REGISTER));
    }

    // ------------------------------------------------------------------------
    /// <summary>Read input registers from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public byte[] ReadInputRegister(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 125)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return WriteSyncData(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_INPUT_REGISTER));
    }

    // ------------------------------------------------------------------------
    /// <summary>Read input registers from slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <returns>Contains the result of function.</returns>
    public async Task<byte[]> ReadInputRegisterAsync(ushort id, byte unit, ushort startAddress, ushort numInputs)
    {
        if (numInputs > 125)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        return await WriteSyncDataAsync(Utils.CreateReadHeader(id, unit, startAddress, numInputs, ModbusFunction.READ_INPUT_REGISTER));
    }

    // ------------------------------------------------------------------------
    /// <summary>Write single coil in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="onOff">Specifies if the coil should be switched on or off.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public byte[] WriteSingleCoils(ushort id, byte unit, ushort startAddress, bool onOff)
    {
        var data = Utils.CreateWriteHeader(id, unit, startAddress, 1, 1, ModbusFunction.WRITE_SINGLE_COIL);
        data[10] = onOff ? (byte)255 : (byte)0;
        return WriteSyncData(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Write single coil in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="onOff">Specifies if the coil should be switched on or off.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public async Task<byte[]> WriteSingleCoilsAsync(ushort id, byte unit, ushort startAddress, bool onOff)
    {
        var data = Utils.CreateWriteHeader(id, unit, startAddress, 1, 1, ModbusFunction.WRITE_SINGLE_COIL);
        data[10] = onOff ? (byte)255 : (byte)0;
        return await WriteSyncDataAsync(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Write multiple coils in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numBits">Specifies number of bits.</param>
    /// <param name="values">Contains the bit information in byte format.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public byte[] WriteMultipleCoils(ushort id, byte unit, ushort startAddress, ushort numBits, byte[] values)
    {
        var numBytes = Convert.ToUInt16(values.Length);
        if (numBytes > 250 || numBits > 2000)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        var data = Utils.CreateWriteHeader(id, unit, startAddress, numBits, (byte)(numBytes + 2), ModbusFunction.WRITE_MULTIPLE_COILS);
        Array.Copy(values, 0, data, 13, numBytes);
        return WriteSyncData(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Write multiple coils in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address from where the data read begins.</param>
    /// <param name="numBits">Specifies number of bits.</param>
    /// <param name="values">Contains the bit information in byte format.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public async Task<byte[]> WriteMultipleCoilsAsync(ushort id, byte unit, ushort startAddress, ushort numBits, byte[] values)
    {
        var numBytes = Convert.ToUInt16(values.Length);
        if (numBytes > 250 || numBits > 2000)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        var data = Utils.CreateWriteHeader(id, unit, startAddress, numBits, (byte)(numBytes + 2), ModbusFunction.WRITE_MULTIPLE_COILS);
        Array.Copy(values, 0, data, 13, numBytes);
        return await WriteSyncDataAsync(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Write single register in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address to where the data is written.</param>
    /// <param name="values">Contains the register information.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public byte[] WriteSingleRegister(ushort id, byte unit, ushort startAddress, byte[] values)
    {
        if (values.GetUpperBound(0) != 1)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        var data = Utils.CreateWriteHeader(id, unit, startAddress, 1, 1, ModbusFunction.WRITE_SINGLE_REGISTER);
        data[10] = values[0];
        data[11] = values[1];
        return WriteSyncData(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Write single register in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address to where the data is written.</param>
    /// <param name="values">Contains the register information.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public async Task<byte[]> WriteSingleRegisterAsync(ushort id, byte unit, ushort startAddress, byte[] values)
    {
        if (values.GetUpperBound(0) != 1)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        var data = Utils.CreateWriteHeader(id, unit, startAddress, 1, 1, ModbusFunction.WRITE_SINGLE_REGISTER);
        data[10] = values[0];
        data[11] = values[1];
        return await WriteSyncDataAsync(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Write multiple registers in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address to where the data is written.</param>
    /// <param name="values">Contains the register information.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public byte[] WriteMultipleRegister(ushort id, byte unit, ushort startAddress, byte[] values)
    {
        var numBytes = Convert.ToUInt16(values.Length);
        if (numBytes > 250)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        if (numBytes % 2 > 0)
        {
            numBytes++;
        }
        var data = Utils.CreateWriteHeader(id, unit, startAddress, Convert.ToUInt16(numBytes / 2), Convert.ToUInt16(numBytes + 2), ModbusFunction.WRITE_MULTIPLE_REGISTER);
        Array.Copy(values, 0, data, 13, values.Length);
        return WriteSyncData(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Write multiple registers in slave synchronous.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startAddress">Address to where the data is written.</param>
    /// <param name="values">Contains the register information.</param>
    /// <returns>Contains the result of the synchronous write.</returns>
    public async Task<byte[]> WriteMultipleRegisterAsync(ushort id, byte unit, ushort startAddress, byte[] values)
    {
        var numBytes = Convert.ToUInt16(values.Length);
        if (numBytes > 250)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        if (numBytes % 2 > 0)
        {
            numBytes++;
        }
        var data = Utils.CreateWriteHeader(id, unit, startAddress, Convert.ToUInt16(numBytes / 2), Convert.ToUInt16(numBytes + 2), ModbusFunction.WRITE_MULTIPLE_REGISTER);
        Array.Copy(values, 0, data, 13, values.Length);
        return await WriteSyncDataAsync(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Read/Write multiple registers in slave synchronous. The result is given in the response function.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startReadAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <param name="startWriteAddress">Address to where the data is written.</param>
    /// <param name="values">Contains the register information.</param>
    /// <returns>Contains the result of the synchronous command.</returns>
    public byte[] ReadWriteMultipleRegister(ushort id, byte unit, ushort startReadAddress, ushort numInputs, ushort startWriteAddress, byte[] values)
    {
        var numBytes = Convert.ToUInt16(values.Length);
        if (numBytes > 250)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        if (numBytes % 2 > 0)
        {
            numBytes++;
        }
        var data = Utils.CreateReadWriteHeader(id, unit, startReadAddress, numInputs, startWriteAddress, Convert.ToUInt16(numBytes / 2));
        Array.Copy(values, 0, data, 17, values.Length);
        return WriteSyncData(data);
    }

    // ------------------------------------------------------------------------
    /// <summary>Read/Write multiple registers in slave synchronous. The result is given in the response function.</summary>
    /// <param name="id">Unique id that marks the transaction. In asynchronous mode this id is given to the callback function.</param>
    /// <param name="unit">Unit identifier (previously slave address). In asynchronous mode this unit is given to the callback function.</param>
    /// <param name="startReadAddress">Address from where the data read begins.</param>
    /// <param name="numInputs">Length of data.</param>
    /// <param name="startWriteAddress">Address to where the data is written.</param>
    /// <param name="values">Contains the register information.</param>
    /// <returns>Contains the result of the synchronous command.</returns>
    public async Task<byte[]> ReadWriteMultipleRegisterAsync(ushort id, byte unit, ushort startReadAddress, ushort numInputs, ushort startWriteAddress, byte[] values)
    {
        var numBytes = Convert.ToUInt16(values.Length);
        if (numBytes > 250)
        {
            throw new InvalidOperationException("Too many inputs");
        }
        if (numBytes % 2 > 0)
        {
            numBytes++;
        }
        var data = Utils.CreateReadWriteHeader(id, unit, startReadAddress, numInputs, startWriteAddress, Convert.ToUInt16(numBytes / 2));
        Array.Copy(values, 0, data, 17, values.Length);
        return await WriteSyncDataAsync(data);
    }

    public byte[] ReadDeviceIdentifiers(ushort id, byte unit, byte objectId)
    {
        var data = Utils.CreateReadDeviceIdentifiers(id, unit, objectId);
        return WriteSyncData(data);
    }

    // ------------------------------------------------------------------------
    // Write data and and wait for response
    private byte[] WriteSyncData(byte[] writeData)
    {
        if (!Connected)
        {
            Disconnect();
            throw new InvalidOperationException("Not connected");
        }

        try
        {
            _socket?.Send(writeData, 0, writeData.Length, SocketFlags.None);
            var received = _socket?.Receive(_buffer, SocketFlags.None) ?? 0;
            if (received == 0)
            {
                throw new TimeoutException();
            }
            switch ((ModbusFunction)_buffer[7])
            {
                case > ModbusFunction.EXCEPTION:
                    throw ((ModbusException.Code)_buffer[8]).ToException();
                case ModbusFunction.WRITE_SINGLE_COIL:
                case ModbusFunction.WRITE_SINGLE_REGISTER:
                case ModbusFunction.WRITE_MULTIPLE_COILS:
                case ModbusFunction.WRITE_MULTIPLE_REGISTER:
                    // ------------------------------------------------------------
                    // Write response data
                    return _buffer.Skip(10).Take(2).ToArray();
                default:
                    // ------------------------------------------------------------
                    // Read response data
                    return _buffer.Skip(9).Take(_buffer[8]).ToArray();
            }
        }
        catch (SystemException)
        {
            Disconnect();
            throw;
        }
    }

    // ------------------------------------------------------------------------
    // Write data and and wait for response
    private async Task<byte[]> WriteSyncDataAsync(byte[] writeData)
    {
        if (!Connected)
        {
            Disconnect();
            throw new InvalidOperationException("Not connected");
        }

        try
        {
            await _socket?.SendAsync(writeData, SocketFlags.None)!;
            var received = _socket != null ? (await _socket.ReceiveAsync(_buffer, SocketFlags.None)) : 0;
            if (received == 0)
            {
                throw new TimeoutException();
            }
            switch ((ModbusFunction)_buffer[7])
            {
                case > ModbusFunction.EXCEPTION:
                    throw ((ModbusException.Code)_buffer[8]).ToException();
                case ModbusFunction.WRITE_SINGLE_COIL:
                case ModbusFunction.WRITE_SINGLE_REGISTER:
                case ModbusFunction.WRITE_MULTIPLE_COILS:
                case ModbusFunction.WRITE_MULTIPLE_REGISTER:
                    // ------------------------------------------------------------
                    // Write response data
                    return _buffer.Skip(10).Take(2).ToArray();
                default:
                    // ------------------------------------------------------------
                    // Read response data
                    return _buffer.Skip(9).Take(_buffer[8]).ToArray();
            }
        }
        catch (SystemException)
        {
            Disconnect();
            throw;
        }
    }
}
