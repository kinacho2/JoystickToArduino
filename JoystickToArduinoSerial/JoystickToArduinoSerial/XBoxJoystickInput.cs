﻿using JoystickToArduinoSerial.Utils;
using SharpDX.XInput;
using System.Numerics;

namespace JoystickToArduinoSerial
{
    public class XBoxJoystickInput : IJoystickInput
    {
        public bool DebugMode { get; set; }

        Controller controller;
        SharpDX.XInput.State prevState;
        private int value;
        public int Value => value;

        XStickInput Up;
        XStickInput Down;
        XStickInput Right;
        XStickInput Left;

        XButtonInput A;
        XButtonInput B;
        XButtonInput X;
        XButtonInput Y;

        XButtonInput Pause;
        XButtonInput Mode;
        XButtonInput ModeAlt;

        List<XButtonInput> buttons = new();
        List<XStickInput> xButtons = new();
        List<XStickInput> yButtons = new();

        XButtonInput StickCenter;

        public XBoxJoystickInput(Controller controller)
        {
            A = new XButtonInput(GamepadButtonFlags.A, JoystickIds.A);
            B = new XButtonInput(GamepadButtonFlags.X, JoystickIds.B);
            X = new XTurboButtonInput(GamepadButtonFlags.B, JoystickIds.A, 16);
            Y = new XTurboButtonInput(GamepadButtonFlags.Y, JoystickIds.B, 16);
            Pause = new XButtonInput(GamepadButtonFlags.Start, JoystickIds.PAUSE);
            Mode = new XButtonInput(GamepadButtonFlags.Back, JoystickIds.MODE);
            ModeAlt = new XButtonInput(GamepadButtonFlags.RightShoulder, JoystickIds.MODE);
            
            Up = new XStickInput(GamepadButtonFlags.LeftThumb, JoystickIds.UP, true);
            Down = new XStickInput(GamepadButtonFlags.LeftThumb, JoystickIds.DOWN, false);

            Left = new XStickInput(GamepadButtonFlags.LeftThumb, JoystickIds.LEFT, false);
            Right = new XStickInput(GamepadButtonFlags.LeftThumb, JoystickIds.RIGHT, true);

            StickCenter = new XMultipleButtons(GamepadButtonFlags.LeftThumb, JoystickIds.STICK_CENTER, new JoystickIds[] 
            {
                JoystickIds.UP,
                JoystickIds.DOWN,
                JoystickIds.LEFT, 
                JoystickIds.RIGHT
            });

            buttons.Add(A);
            buttons.Add(B);
            buttons.Add(X);
            buttons.Add(Y);
            buttons.Add(Mode);
            buttons.Add(Pause);
            buttons.Add(ModeAlt);
            buttons.Add(StickCenter);

            yButtons.Add(Up);
            yButtons.Add(Down);
            xButtons.Add(Right);
            xButtons.Add(Left);

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

                Vector2 stick = new Vector2((float)state.Gamepad.LeftThumbX / (float)short.MaxValue, (float)state.Gamepad.LeftThumbY / (float)short.MaxValue);
                if(stick.LengthSquared() > 1)
                {
                    stick = stick / stick.Length();
                }

                foreach (var button in xButtons)
                {
                    button.SetState(stick.X);
                    button.Update(deltaTime);
                    value |= button.Value;
                }

                foreach (var button in yButtons)
                {
                    button.SetState(stick.Y);
                    button.Update(deltaTime);
                    value |= button.Value;
                }

            }

        }

        public string[] Symbols => JoystickSymbols.XBOX;

    }
}
