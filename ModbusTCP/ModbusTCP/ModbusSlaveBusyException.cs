namespace ModbusTCP;

public class ModbusSlaveBusyException : ModbusException
{
    internal ModbusSlaveBusyException() : base(Code.SLAVE_IS_BUSY) { }
}