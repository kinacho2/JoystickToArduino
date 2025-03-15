
using SharpDX.XInput;

namespace JoystickToArduinoSerial
{
    internal class XMultipleButtons : XButtonInput
    {
        public XMultipleButtons(GamepadButtonFlags flag, JoystickIds id, JoystickIds[] buttons) : base(flag, id)
        {
            buttonId = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttonId |= 1 << (int)buttons[i];
            }
        }
    }
}
