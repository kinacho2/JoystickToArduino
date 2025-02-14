using SharpDX.DirectInput;
using SharpDX.XInput;
using System.Collections.Generic;
using System.Diagnostics;
using DeviceType = SharpDX.DirectInput.DeviceType;

namespace JoystickToArduinoSerial.Utils
{
    public static class JoystickUtils
    {
        public static List<IJoystickInput> GetJoystickInputs(bool debug)
        {
            var list = new List<IJoystickInput>();
            

            GetXBoxControllers(list, debug);
            if (list.Count < 2)
                GetGenericJoysticks(list, debug);


            return list.Count > 1? SelectPositions(list) : list;
        }

        public static List<IJoystickInput> SelectPositions(List<IJoystickInput> list)
        {
            var resultArray = new IJoystickInput[list.Count];

            JoyPosition[] joyPositions = new JoyPosition[list.Count];
            bool[] joyReadyArray = new bool[list.Count];

            bool ready = false;
            bool leftSelected = false;
            bool rightSelected = false;

            JoystickSymbols.Print(list, joyReadyArray, joyPositions);

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!ready)
            {
                Thread.Sleep(100);
                bool changes = false;
                ready = true;

                for (int i=0; i < list.Count; i++)
                { 
                    var joystick = list[i];
                    joystick.Update((float)stopwatch.Elapsed.TotalSeconds);
                    var joyReady = joyReadyArray[i];
                    var position = joyPositions[i];

                    if (!joyReady)
                    {
                        if((joystick.Value & (1<<(int)JoystickIds.LEFT)) != 0)
                        {
                            if (position == JoyPosition.Center && !leftSelected)
                            {
                                leftSelected = true;
                                joyPositions[i] = JoyPosition.Left;
                                changes = true;
                            }
                            if(position == JoyPosition.Right)
                            {
                                rightSelected = false;
                                joyPositions[i] = JoyPosition.Center;
                                changes = true;
                            }
                        }
                        if ((joystick.Value & (1 << (int)JoystickIds.RIGHT)) != 0)
                        {
                            if (position == JoyPosition.Center && !rightSelected)
                            {
                                rightSelected = true;
                                joyPositions[i] = JoyPosition.Right;
                                changes = true;
                            }
                            if (position == JoyPosition.Left)
                            {
                                leftSelected = false;
                                joyPositions[i] = JoyPosition.Center;
                                changes = true;
                            }
                        }

                        if ((joystick.Value & (1 << (int)JoystickIds.PAUSE)) != 0)
                        {
                            if(position != JoyPosition.Center)
                            {
                                joyReady = joyReadyArray[i] = true;
                                changes = true;
                            }
                            else if(leftSelected)
                            {
                                rightSelected = true;
                                joyPositions[i] = JoyPosition.Right;
                                joyReady = joyReadyArray[i] = true;
                                changes = true;
                            }
                            else if(rightSelected)
                            {
                                leftSelected = true;
                                joyPositions[i] = JoyPosition.Left;
                                joyReady = joyReadyArray[i] = true;
                                changes = true;
                            }
                        }

                        

                    }
                    else
                    {
                        if ((joystick.Value & (1 << (int)JoystickIds.MODE)) != 0)
                        {
                            joyReady = joyReadyArray[i] = false;
                            changes = true;
                        }
                    }

                    ready &= joyReady;
                }

                if (changes)
                {
                    JoystickSymbols.Print(list, joyReadyArray, joyPositions);
                }
            }

            
            for (int i = 0; i < list.Count; i++)
            {
                var joystick = list[i];
                var position = joyPositions[i];
                if (position == JoyPosition.Left)
                    resultArray[0] = joystick;
                if(position == JoyPosition.Right)
                    resultArray[1] = joystick;
            }
            

            return resultArray.ToList();
        }

        public static void GetXBoxControllers(List<IJoystickInput> list, bool debug)
        {
            var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            // Get controllers availables
            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    Console.WriteLine("Found a XInput controller available");

                    var input = new XBoxJoystickInput(selectControler);
                    input.DebugMode = debug;
                    list.Add(input);

                    if (list.Count == 2)
                        return;
                }
            }
        }

        public static void GetGenericJoysticks(List<IJoystickInput> list, bool debug)
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            List<Guid> joystickGuids = new List<Guid>();
            // If Gamepad not found, look for a Joystick
            if (joystickGuids.Count < 2)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    joystickGuids.Add(deviceInstance.InstanceGuid);

            // If Joystick not found, throws an error -

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
                input.DebugMode = debug;

                list.Add(input);

                if (list.Count == 2)
                    return;
            }
        }
    }
}
