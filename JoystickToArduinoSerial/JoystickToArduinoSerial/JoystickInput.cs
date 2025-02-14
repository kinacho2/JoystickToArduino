using JoystickToArduinoSerial.Utils;
using SharpDX.DirectInput;

namespace JoystickToArduinoSerial
{
    public class JoystickInput: IJoystickInput
    {
        public bool DebugMode { get; set; }
        ButtonInput Up;
        ButtonInput Down;
        ButtonInput Right;
        ButtonInput Left;

        ButtonInput A;
        ButtonInput B;
        ButtonInput X;
        ButtonInput Y;

        ButtonInput Pause;
        ButtonInput Mode;
        ButtonInput ModeAlt;

        Dictionary<JoystickOffset, List<ButtonInput>> buttons;


        Joystick Joystick;


        private int value;
        public int Value => value;

        public JoystickInput(Joystick joystick) 
        {
            Joystick = joystick;

            Up = new StickInput(JoystickOffset.Y, JoystickIds.UP, true);
            Down = new StickInput(JoystickOffset.Y, JoystickIds.DOWN, false);
            Right = new StickInput(JoystickOffset.X, JoystickIds.RIGHT, true);
            Left = new StickInput(JoystickOffset.X, JoystickIds.LEFT, false);

            A = new ButtonInput(JoystickOffset.Buttons14, JoystickIds.A);
            B = new ButtonInput(JoystickOffset.Buttons15, JoystickIds.B);
            X = new TurboButtonInput(JoystickOffset.Buttons13, JoystickIds.A, 10);
            Y = new TurboButtonInput(JoystickOffset.Buttons12, JoystickIds.B, 10);

            Pause = new ButtonInput(JoystickOffset.Buttons3, JoystickIds.PAUSE);
            Mode = new ButtonInput(JoystickOffset.Buttons0, JoystickIds.MODE);
            ModeAlt = new ButtonInput(JoystickOffset.Buttons11, JoystickIds.MODE);

            this.buttons = new Dictionary<JoystickOffset, List<ButtonInput>>();

            var buttons = new ButtonInput[(int)JoystickIds.COUNT];
            buttons[(int)JoystickIds.UP] = Up;
            buttons[(int)JoystickIds.DOWN] = Down;
            buttons[(int)JoystickIds.RIGHT] = Right;
            buttons[(int)JoystickIds.LEFT] = Left;
            buttons[(int)JoystickIds.A] = A;
            buttons[(int)JoystickIds.B] = B;
            buttons[(int)JoystickIds.PAUSE] = Pause;
            buttons[(int)JoystickIds.MODE] = Mode;
            buttons[(int)JoystickIds.MODE_ALT] = ModeAlt;
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

        public string[] Symbols => JoystickSymbols.GENERIC;
    }
}
