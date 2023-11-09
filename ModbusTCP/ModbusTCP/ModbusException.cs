namespace ModbusTCP;

public class ModbusException : InvalidOperationException
{
    public enum Code : byte
    {
        /// <summary>Constant for exception illegal function.</summary>
        ILLEGAL_FUNCTION = 1,

        /// <summary>Constant for exception illegal data address.</summary>
        ILLEGAL_DATA_ADR = 2,

        /// <summary>Constant for exception illegal data value.</summary>
        ILLEGAL_DATA_VAL = 3,

        /// <summary>Constant for exception slave device failure.</summary>
        SLAVE_DEVICE_FAILURE = 4,

        /// <summary>Constant for exception slave is busy/booting up.</summary>
        SLAVE_IS_BUSY = 6,
    }

    public Code Exception { get; }

    internal ModbusException(Code exception)
    {
        Exception = exception;
    }
}