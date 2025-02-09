using SharpDX.DirectInput;

namespace JoystickToArduinoSerial
{
    internal class ButtonInput
    {
        private int buttonId;

        private JoystickOffset offset;
        protected bool state;
        protected int valueOffset = 64;

        public bool State => state;
        public JoystickOffset Offset => offset;
        public int Value => state ? buttonId : 0;

        public ButtonInput(JoystickOffset offset, JoystickIds id)
        {
            buttonId = 1 << (int)id;
            this.offset = offset;
        }

        public virtual void SetState(JoystickUpdate up)
        {
            if (up.Offset != Offset)
                return;

            if (up.Value > valueOffset)
            {
                state = true;
            }
            else
            {
                state = false;
            }

        }

        public virtual void Update(float deltaTime)
        {

        }

    }

}
