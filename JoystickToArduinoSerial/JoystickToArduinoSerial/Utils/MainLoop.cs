using System.Diagnostics;

namespace JoystickToArduinoSerial.Utils
{
    public static class MainLoop
    {
        public static void Run(bool debug = false)
        {
            //Create serial port
            var serialReady = SerialPortUtils.SerialPortHandShake(out var serialPort);//CreateSerialPort("COM3", out var serialPort);
            var joystickList = JoystickUtils.GetJoystickInputs(debug);
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


                    stopwatch = Stopwatch.StartNew();
                }
                catch
                {
                    return;
                }

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
