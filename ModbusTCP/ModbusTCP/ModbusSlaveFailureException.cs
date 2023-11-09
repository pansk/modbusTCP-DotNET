namespace ModbusTCP;

public class ModbusSlaveFailureException : ModbusException
{
    internal ModbusSlaveFailureException() : base(Code.SLAVE_DEVICE_FAILURE) { }
}