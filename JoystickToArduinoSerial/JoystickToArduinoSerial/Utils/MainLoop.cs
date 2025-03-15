using System.Diagnostics;
using System.IO.Ports;

namespace JoystickToArduinoSerial.Utils
{
    public static class MainLoop
    {
        public static void Run(bool debug = false, bool debugSerialPort = false)
        {
            var joystickList = JoystickUtils.GetJoystickInputs(debug);
            //Create serial port
            var serialReady = SerialPortUtils.SerialPortHandShake(out var serialPort);//CreateSerialPort("COM3", out var serialPort);
            if (!debug && !serialReady)
            {
                Console.WriteLine("Error getting serial port");
            }
            byte[] buffer = new byte[2];

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    Thread.Sleep(10);

                    stopwatch.Stop();

                    int i = 1;
                    foreach (var input in joystickList)
                    {
                        input.Update((float)stopwatch.Elapsed.TotalSeconds);
                        buffer[0] = (byte)(input.Value + 1);
                        buffer[1] = (byte)i;
                        i++;
                        if (serialReady)
                        {
                            if (debugSerialPort)
                            {
                                Console.Write($"buffer: [");
                                PrintBinary(buffer[0] - 1, 0, 7);
                                Console.WriteLine($"]");
                            }
                            serialPort.Write(buffer, 0, 2);
                        }
                    }
                    if (serialReady)
                    {
                        while (serialPort.BytesToRead > 0)
                        {
                            serialPort.Read(buffer, 0, 2);
                            var s = "";
                            s += (char)buffer[0];
                            s += (char)buffer[1];

                            if (s == "er")
                            {
                                Console.Write("error: ");
                                serialPort.Read(buffer, 0, 1);
                                PrintBinary(buffer[0], 0, 7);
                                Console.WriteLine();
                            }
                            if (s == "cl")
                            {
                                Console.WriteLine("cleanning buffer");
                            }
                        }
                    }
                    else if(!debug)
                    {
                        if(serialPort != null)
                            serialPort.Close();
                        ReconnectSerial(out serialReady, out serialPort);
                    }


                    stopwatch = Stopwatch.StartNew();
                }
                catch
                {
                    serialPort.Close();
                    ReconnectSerial(out serialReady, out serialPort);
                }

            }
        }

        static void ReconnectSerial(out bool serialReady, out SerialPort serialPort)
        {

            serialPort = default;
            serialReady = false;
            
            while (!serialReady)
            {
                Console.WriteLine("Serial port miss, trying to connect");
                serialReady = SerialPortUtils.SerialPortHandShake(out serialPort);
                Thread.Sleep(1000);
            }

        }

        private static void PrintBinary(int input, int bitStart, int bitEnd)
        {
            for (int i = bitEnd; i >= bitStart; i--)
            {
                int state = (input & (1 << i)) != 0 ? 1 : 0;
                Console.Write(state);
            }
        }
    }
}
