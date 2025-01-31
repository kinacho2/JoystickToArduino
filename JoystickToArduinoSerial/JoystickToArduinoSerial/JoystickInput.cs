using SharpDX.DirectInput;
using SharpDX.XInput;

namespace JoystickToArduinoSerial
{
    public enum JoystickIds
    {
        UP = 0, 
        DOWN = 1, 
        LEFT = 2, 
        RIGHT = 3,
        A = 4,
        B = 5,
        PAUSE = 6, 
        MODE = 7,
        X = 8, 
        Y = 9,
        COUNT
    }

    public interface IJoystickInput
    {
        bool DebugMode { get; set; }
        void Update(float deltaTime);
        public int Value { get; }
    }

    public class XBoxJoystickInput : IJoystickInput
    { 
        public bool DebugMode { get; set; }

        Controller controller;
        SharpDX.XInput.State prevState;
        private int value;
        public int Value => value;

        StickInput Up;
        StickInput Down;
        StickInput Left;
        StickInput Right;

        ButtonInput A;
        ButtonInput B;
        ButtonInput X;
        ButtonInput Y;

        ButtonInput Pause;
        ButtonInput Mode;

        List<ButtonInput> buttons = new();
        List<StickInput> xButtons = new();
        List<StickInput> yButtons = new();


        public XBoxJoystickInput(Controller controller)
        {
            A = new ButtonInput(GamepadButtonFlags.A, JoystickIds.A);
            B = new ButtonInput(GamepadButtonFlags.X, JoystickIds.B);
            X = new TurboButtonInput(GamepadButtonFlags.B, JoystickIds.A, 10);
            Y = new TurboButtonInput(GamepadButtonFlags.Y, JoystickIds.B, 10);
            Pause = new ButtonInput(GamepadButtonFlags.Start, JoystickIds.PAUSE);
            Mode = new ButtonInput(GamepadButtonFlags.Back, JoystickIds.MODE);

            Up = new StickInput(GamepadButtonFlags.LeftThumb, JoystickIds.UP, true);
            Down = new StickInput(GamepadButtonFlags.LeftThumb, JoystickIds.DOWN, false);

            Right = new StickInput(GamepadButtonFlags.LeftThumb, JoystickIds.RIGHT, true);
            Left = new StickInput(GamepadButtonFlags.LeftThumb, JoystickIds.LEFT, false);

            //LeftThumbY: 32767 -32768, 
            //LeftThumbX: 
            buttons.Add(A);
            buttons.Add(B);
            buttons.Add(X);
            buttons.Add(Y);
            buttons.Add(Mode);
            buttons.Add(Pause);

            yButtons.Add(Up);
            yButtons.Add(Down);
            xButtons.Add(Left);
            xButtons.Add(Right);

            this.controller = controller;
            prevState = controller.GetState();
        }

        public void Update(float deltaTime)
        {
            if (controller.IsConnected)
            {
                var state = controller.GetState();
                if (DebugMode)
                {
                    if (prevState.PacketNumber != state.PacketNumber)
                       Console.WriteLine(state.Gamepad);
                }
                prevState = state;
                value = 0;
                foreach (var button in buttons)
                {
                    button.SetState(state.Gamepad.Buttons);
                    button.Update(deltaTime);
                    value |= button.Value;
                }

                foreach (var button in xButtons)
                {
                    button.SetState(state.Gamepad.LeftThumbX);
                    button.Update(deltaTime);
                    value |= button.Value;
                }

                foreach (var button in yButtons)
                {
                    button.SetState(state.Gamepad.LeftThumbY);
                    button.Update(deltaTime);
                    value |= button.Value;
                }

            }

        }

        private class ButtonInput
        {
            private int buttonId;
            protected GamepadButtonFlags flag;
            public GamepadButtonFlags Flag => flag;

            protected bool state;
            public bool State => state;
            public int Value => state ? buttonId : 0;

            public ButtonInput(GamepadButtonFlags flag, JoystickIds id)
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

        private class TurboButtonInput : ButtonInput
        {
            float time;
            float timer;
            bool pressed;
            public TurboButtonInput(GamepadButtonFlags flag, JoystickIds id, float frecuency) : base(flag, id)
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

        private class StickInput : ButtonInput
        {
            short maxPoint = 32760;
            int mult;
            short deadPoint;

            public StickInput(GamepadButtonFlags flag, JoystickIds id, bool up) : base(flag, id)
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


    public class JoystickInput: IJoystickInput
    {
        public bool DebugMode { get; set; }
        ButtonInput Up;
        ButtonInput Down;
        ButtonInput Left;
        ButtonInput Right;

        ButtonInput A;
        ButtonInput B;
        ButtonInput X;
        ButtonInput Y;

        ButtonInput Pause;
        ButtonInput Mode;

        Dictionary<JoystickOffset, List<ButtonInput>> buttons;


        Joystick Joystick;


        private int value;
        public int Value => value;

        public JoystickInput(Joystick joystick) 
        {
            Joystick = joystick;

            Up = new StickInput(JoystickOffset.Y, JoystickIds.UP, true);
            Down = new StickInput(JoystickOffset.Y, JoystickIds.DOWN, false);
            Left = new StickInput(JoystickOffset.X, JoystickIds.LEFT, true);
            Right = new StickInput(JoystickOffset.X, JoystickIds.RIGHT, false);

            A = new ButtonInput(JoystickOffset.Buttons14, JoystickIds.A);
            B = new ButtonInput(JoystickOffset.Buttons15, JoystickIds.B);
            X = new TurboButtonInput(JoystickOffset.Buttons13, JoystickIds.A, 5);
            Y = new TurboButtonInput(JoystickOffset.Buttons12, JoystickIds.B, 5);

            Pause = new ButtonInput(JoystickOffset.Buttons3, JoystickIds.PAUSE);
            Mode = new ButtonInput(JoystickOffset.Buttons0, JoystickIds.MODE);

            this.buttons = new Dictionary<JoystickOffset, List<ButtonInput>>();

            var buttons = new ButtonInput[(int)JoystickIds.COUNT];
            buttons[(int)JoystickIds.UP] = Up;
            buttons[(int)JoystickIds.DOWN] = Down;
            buttons[(int)JoystickIds.LEFT] = Left;
            buttons[(int)JoystickIds.RIGHT] = Right;
            buttons[(int)JoystickIds.A] = A;
            buttons[(int)JoystickIds.B] = B;
            buttons[(int)JoystickIds.PAUSE] = Pause;
            buttons[(int)JoystickIds.MODE] = Mode;
            buttons[(int)JoystickIds.X] = X;
            buttons[(int)JoystickIds.Y] = Y;

            foreach (var button in buttons)
            {
                this.buttons[button.Offset] = new List<ButtonInput>();
            }

            foreach (var button in buttons)
            {
                var list = this.buttons[button.Offset];
                list.Add(button);
            }


        }

        public void Update(float deltaTime)
        {
            Joystick.Poll();
            var datas = Joystick.GetBufferedData();
            var states = Joystick.GetCurrentState();

            if (DebugMode)
            {
                foreach (var data in datas)
                {
                    Console.WriteLine(data);
                }
            }

            foreach (var state in datas)
            {
                if (buttons.TryGetValue(state.Offset, out var buttonList))
                {
                    foreach(var button in buttonList)
                        button.SetState(state);
                }

            }
            value = 0;
            foreach (var keyValue in buttons)
            {
                foreach (var button in keyValue.Value)
                {
                    button.Update(deltaTime);
                    value |= button.Value;
                }
            }
        }

        private class ButtonInput
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

        private class StickInput : ButtonInput
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
                    if (up.Value > (middlePoint + valueOffset))
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
                    if (up.Value < (middlePoint - valueOffset))
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

        private class TurboButtonInput : ButtonInput
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

   
}
