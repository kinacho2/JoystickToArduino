using SharpDX.XInput;

namespace JoystickToArduinoSerial
{
    internal class XStickInput : XButtonInput
    {
        float maxPoint = 1f;
        int mult;
        float deadPoint;

        public XStickInput(GamepadButtonFlags flag, JoystickIds id, bool up) : base(flag, id)
        {
            deadPoint = maxPoint / 3;

            mult = up ? 1 : -1;

        }

        public void SetState(float value)
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
