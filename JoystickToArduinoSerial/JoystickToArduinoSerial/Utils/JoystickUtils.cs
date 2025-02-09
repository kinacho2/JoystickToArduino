using SharpDX.DirectInput;
using SharpDX.XInput;
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

            return list;
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
