using SharpDX.DirectInput;

namespace JoystickToArduinoSerial
{
    internal class StickInput : ButtonInput
    {
        int middlePoint = 32511;
        int mult;

        public StickInput(JoystickOffset offset, JoystickIds id, bool up) : base(offset, id)
        {
            valueOffset = middlePoint / 10;

            mult = up ? 1 : -1;

        }

        public override void SetState(JoystickUpdate up)
        {
            if (up.Offset != Offset)
                return;

            if (mult > 0)
            {
                if (up.Value > middlePoint + valueOffset)
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
                if (up.Value < middlePoint - valueOffset)
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
