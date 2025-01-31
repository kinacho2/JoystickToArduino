using JoystickToArduinoSerial;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Diagnostics;
using System.IO.Ports;

using DeviceType = SharpDX.DirectInput.DeviceType;
internal class Program
{
    private static void Main(string[] args)
    {
        //Create serial port
        var serialReady = SerialPortHandShake(out var serialPort);//CreateSerialPort("COM3", out var serialPort);
        var joystickList = GetJoystickInputs();
        byte[] buffer = new byte[2];

        Stopwatch stopwatch = Stopwatch.StartNew();
        while (true)
        {
            
            Thread.Sleep(30);

            stopwatch.Stop();

            int i = 1;
            foreach (var input in joystickList)
            {
                input.Update((float)stopwatch.Elapsed.TotalSeconds);    
                buffer[0] = (byte)(input.Value + 1);
                buffer[1] = (byte)i;
                i++;
                if(serialReady)
                    serialPort.Write(buffer, 0, 2);

                //PrintBinary(input.Value, 0, 7);
                //Console.WriteLine();
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
                    if(s == "cl")
                    {
                        Console.WriteLine("cleanning buffer");
                    }
                }
            }


            stopwatch = Stopwatch.StartNew();
        }
    }

    static void PrintBinary(int input, int bitStart, int bitEnd)
    {
        for (int i = bitEnd; i >= bitStart; i--)
        {
            int state = (input & (1 << i)) != 0? 1: 0;
            Console.Write(state);
        }

        

    }

    static bool SerialPortHandShake(out SerialPort serialPort)
    {
        for(int i=0; i< 10; i++)
        {
            var serialReady = CreateSerialPort("COM"+i, out serialPort);

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

    static bool Handshake(SerialPort serialPort)
    {
        Thread.Sleep(100);
        serialPort.Write("h");
        serialPort.Write("h");
        var stopWatch = Stopwatch.StartNew();
        while (serialPort.BytesToRead == 0 && stopWatch.ElapsedMilliseconds < 1000) ;

        if(serialPort.BytesToRead > 0)
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

    static bool CreateSerialPort(string portName, out SerialPort serialPort)
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
        catch (Exception e)
        {

        }
        return false;
    }

    static List<IJoystickInput> GetJoystickInputs()
    {
        var list = new List<IJoystickInput>();

        GetXBoxController(list);
        if(list.Count < 2)
            GetGenericJoysticks(list);
        
        return list;
    }

    static void GetXBoxController(List<IJoystickInput> list)
    {
        var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

        // Get 1st controller available
        Controller controller = null;
        foreach (var selectControler in controllers)
        {
            if (selectControler.IsConnected)
            {
                controller = selectControler;
                break;
            }
        }

        if (controller == null)
        {
            return;
        }

        Console.WriteLine("Found a XInput controller available");

        // Poll events from joystick
        list.Add(new XBoxJoystickInput(controller));
      
    }

    static void GetGenericJoysticks(List<IJoystickInput> list)
    {

        // Initialize DirectInput
        var directInput = new DirectInput();

        // Find a Joystick Guid
        List<Guid> joystickGuids = new List<Guid>();
        // If Gamepad not found, look for a Joystick
        if (joystickGuids.Count < 2)
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                joystickGuids.Add(deviceInstance.InstanceGuid);

        // If Joystick not found, throws an error
        if (joystickGuids.Count == 0)
        {
            Console.WriteLine("No joystick found.");
            return;
        }

        // Instantiate the joystick
        foreach (var joystickGuid in joystickGuids)
        {
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            JoystickInput input = new JoystickInput(joystick);

            list.Add(input);

        }
    }

}