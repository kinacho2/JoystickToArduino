﻿using JoystickToArduinoSerial.Utils;

internal class Program
{
    private static void Main(string[] args)
    {
        MainLoop.Run(debug: false, debugSerialPort: false);
    }
}