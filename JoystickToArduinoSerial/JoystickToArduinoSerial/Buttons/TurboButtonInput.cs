using SharpDX.DirectInput;

namespace JoystickToArduinoSerial
{
    internal class TurboButtonInput : ButtonInput
    {
        float time;
        float timer;
        bool pressed;
        public TurboButtonInput(JoystickOffset offset, JoystickIds id, float frecuency) : base(offset, id)
        {
            time = 1f / frecuency;
        }

        public override void Update(float deltaTime)
        {
            if (pressed)
            {
                if (timer == 0)
                {
                    state = !state;
                }

                timer += deltaTime;

                if (timer >= time)
                    timer = 0f;
            }
            else
            {
                state = false;
            }
        }

        public override void SetState(JoystickUpdate up)
        {
            if (up.Offset != Offset)
                return;
            if (!pressed && up.Value > valueOffset)
                timer = 0;

            pressed = up.Value > valueOffset;
        }
    }

}
