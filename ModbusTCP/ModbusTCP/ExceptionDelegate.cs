namespace ModbusTCP;

/// <summary>Exception data event. This event is called when the data is incorrect</summary>
public delegate void ExceptionDelegate(ushort id, byte unit, ModbusFunction function, ModbusException.Code exceptionCode);
