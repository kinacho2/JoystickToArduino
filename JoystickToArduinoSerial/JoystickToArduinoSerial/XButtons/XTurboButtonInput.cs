using SharpDX.XInput;

namespace JoystickToArduinoSerial
{
    internal class XTurboButtonInput : XButtonInput
    {
        float time;
        float timer;
        bool pressed;
        public XTurboButtonInput(GamepadButtonFlags flag, JoystickIds id, float frecuency) : base(flag, id)
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

        public override void SetState(GamepadButtonFlags buttons)
        {
            var state = (buttons & flag) != 0;
            if (!pressed && state)//was pressed
                timer = 0;

            pressed = state;
        }
    }
}
