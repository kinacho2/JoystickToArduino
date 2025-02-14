using System.Collections.Generic;

namespace JoystickToArduinoSerial.Utils
{
    public enum JoyPosition
    {
        Center,
        Left,
        Right,
    }

    public static class JoystickSymbols
    {
        public static int LineCount => GENERIC.Length;

        public static readonly string[] GENERIC = new[] {
            " .-==\\-/==-. ",
            "((+) GEN ::) ",
            "|.(o)--(o).| ",
            "\\/        \\/ ",
            "             "

        };

        public static readonly string[] XBOX = new[] {
            " .==-\\-/-==. ",
            "((o) XBOX::))",
            "(.(+) - (o).)",
            "*  *¯'─'¯*  *",
            " ─*       *─ "
        };

        public static readonly string[] EMPTY = new[] {
            "             ",
            "             ",
            "             ",
            "             ",
            "             "
        };

        public static readonly string HALF_EMPTY = "      ";
        public static readonly string READY = " READY";
        public static readonly string PLAYER_1 = "  PLAYER 1   ";
        public static readonly string PLAYER_2 = "   PLAYER 2  ";

        public static void PrintJoystick(JoyPosition position, string[] joy, bool ready)
        {
            string[] center = EMPTY;
            string[] left = EMPTY;
            string[] right = EMPTY;

            switch (position)
            {
                case JoyPosition.Left:
                    left = joy;
                    break;
                case JoyPosition.Right:
                    right = joy;
                    break;
                case JoyPosition.Center:
                    center = joy;
                    break;
            }


            for (int i = 0; i < LineCount; i++)
            {
                var HalfLeft = HALF_EMPTY;
                var HalfRight = HALF_EMPTY;

                if (i == 2 && ready)
                {
                    if (position == JoyPosition.Left)
                    {
                        HalfLeft = READY;
                    }
                    if (position == JoyPosition.Right)
                    {
                        HalfRight = READY;
                    }
                }

                var result = HALF_EMPTY + left[i] + HalfLeft + center[i] + HALF_EMPTY + right[i] + HalfRight;
                Console.WriteLine(result);
            }
            Console.WriteLine();
        }

        public static void PrintTitle()
        {
            var s = HALF_EMPTY + PLAYER_1 + HALF_EMPTY + EMPTY[0] + HALF_EMPTY + PLAYER_2;
            Console.WriteLine(s);
            Console.WriteLine();
        }

        public static void Print(List<IJoystickInput> list, bool[] joyReadyArray, JoyPosition[] joyPositions)
        {
            Console.Clear();
            JoystickSymbols.PrintTitle();
            for (int i = 0; i < list.Count; i++)
            {
                var joystick = list[i];
                var joyReady = joyReadyArray[i];
                var position = joyPositions[i];
                JoystickSymbols.PrintJoystick(position, joystick.Symbols, joyReady);
            }
        }
    }
}
