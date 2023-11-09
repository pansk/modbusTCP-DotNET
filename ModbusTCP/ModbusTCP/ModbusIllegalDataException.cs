namespace ModbusTCP;

public class ModbusIllegalDataException : ModbusException
{
    internal ModbusIllegalDataException() : base(Code.ILLEGAL_DATA_VAL) { }
}