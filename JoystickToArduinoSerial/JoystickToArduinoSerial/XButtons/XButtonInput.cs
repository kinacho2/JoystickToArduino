using SharpDX.XInput;

namespace JoystickToArduinoSerial
{
    internal class XButtonInput
    {
        private int buttonId;
        protected GamepadButtonFlags flag;
        public GamepadButtonFlags Flag => flag;

        protected bool state;
        public bool State => state;
        public int Value => state ? buttonId : 0;

        public XButtonInput(GamepadButtonFlags flag, JoystickIds id)
        {
            buttonId = 1 << (int)id;
            this.flag = flag;
        }


        public virtual void SetState(GamepadButtonFlags buttons)
        {
            this.state = (buttons & flag) != 0;
        }

        public virtual void Update(float deltaTime)
        {

        }
    }
}
