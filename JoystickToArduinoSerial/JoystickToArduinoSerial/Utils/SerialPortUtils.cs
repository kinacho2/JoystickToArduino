using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoystickToArduinoSerial.Utils
{
    public static class SerialPortUtils
    {
        public static bool SerialPortHandShake(out SerialPort serialPort)
        {
            for (int i = 0; i < 10; i++)
            {
                var serialReady = CreateSerialPort("COM" + i, out serialPort);

                if (serialReady)
                {
                    if (Handshake(serialPort))
                    {
                        Console.WriteLine($"Port COM{i} handshake SUCCESS");

                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Port COM{i} handshake failed");
                        serialPort.Close();
                    }
                }

            }

            serialPort = null;

            return false;
        }

        private static bool Handshake(SerialPort serialPort)
        {
            Thread.Sleep(100);
            serialPort.WriteTimeout = 1000;
            try
            {
                serialPort.Write("h");
                serialPort.Write("h");
            }
            catch (Exception e)
            {
                Console.WriteLine("Write Timeout reached");
                return false;
            }
            var stopWatch = Stopwatch.StartNew();
            while (serialPort.BytesToRead == 0 && stopWatch.ElapsedMilliseconds < 1000) ;

            if (serialPort.BytesToRead > 0)
            {
                byte[] buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, serialPort.BytesToRead);

                if (buffer[0] == 'o')
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"handshake error {(char)buffer[0]}");
                }
            }
            else
            {
                Console.WriteLine("Timeout reached");
            }
            return false;
        }

        private static bool CreateSerialPort(string portName, out SerialPort serialPort)
        {
            //SerialPort
            serialPort = new SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = 9600;
            try
            {
                serialPort.Open();
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("CreateSerialPort ERROR: " + ex.Message);
            }
            catch (Exception e)
            {

            }
            return false;
        }
    }
}
