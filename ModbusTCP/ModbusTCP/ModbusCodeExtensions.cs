namespace ModbusTCP;

public static class ModbusCodeExtensions {
    public static ModbusException ToException(this ModbusException.Code exception)
    {
        return exception switch
        {
            ModbusException.Code.ILLEGAL_DATA_ADR => new ModbusIllegalAddressException(),
            ModbusException.Code.ILLEGAL_DATA_VAL => new ModbusIllegalDataException(),
            ModbusException.Code.ILLEGAL_FUNCTION => new ModbusIllegalFunctionException(),
            ModbusException.Code.SLAVE_DEVICE_FAILURE => new ModbusSlaveFailureException(),
            ModbusException.Code.SLAVE_IS_BUSY => new ModbusSlaveBusyException(),
            _ => new ModbusException(exception)
        };
    }
}