using SharpDX.XInput;

namespace JoystickToArduinoSerial
{
    internal class XStickInput : XButtonInput
    {
        short maxPoint = 32760;
        int mult;
        short deadPoint;

        public XStickInput(GamepadButtonFlags flag, JoystickIds id, bool up) : base(flag, id)
        {
            deadPoint = (short)(maxPoint / 3);

            mult = up ? 1 : -1;

        }

        public void SetState(short value)
        {
            if (mult > 0)
            {
                if (value > deadPoint)
                {
                    state = true;
                }
                else
                {
                    state = false;
                }
            }
            else
            {
                if (value < -deadPoint)
                {
                    state = true;
                }
                else
                {
                    state = false;
                }
            }
        }
    }
}
