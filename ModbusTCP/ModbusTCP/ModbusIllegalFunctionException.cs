namespace ModbusTCP;

public class ModbusIllegalFunctionException : ModbusException
{
    internal ModbusIllegalFunctionException() : base(Code.ILLEGAL_FUNCTION) {}
}