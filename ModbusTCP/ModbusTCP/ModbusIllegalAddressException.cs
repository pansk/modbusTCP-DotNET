namespace ModbusTCP;

public class ModbusIllegalAddressException : ModbusException
{
    internal ModbusIllegalAddressException() :base(Code.ILLEGAL_DATA_ADR) {}
}