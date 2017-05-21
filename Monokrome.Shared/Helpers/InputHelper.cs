using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Monokrome.Helpers
{
    public static class InputHelper
    {
        public static KeyboardState PreviousKeyboardState;
        public static KeyboardState CurrentKeyboardState;
        public static MouseState PreviousMouseState;
        public static MouseState MouseState;
        public static Vector3 ScreenScale;

        static InputHelper()
        {
            ScreenScale = Vector3.One;
        }

        public static void RefreshInput()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            PreviousMouseState = MouseState;
            var tempMouseState = Mouse.GetState();
            var x = (int)(tempMouseState.X / ScreenScale.X);
            var y = (int)(tempMouseState.Y / ScreenScale.Y);
            MouseState = new MouseState(
                x,
                y,
                tempMouseState.ScrollWheelValue,
                tempMouseState.LeftButton,
                tempMouseState.MiddleButton,
                tempMouseState.RightButton,
                tempMouseState.XButton1,
                tempMouseState.XButton2);
        }

        public static GestureSample GetTouchGesture()
        {
            if (TouchPanel.IsGestureAvailable)
            {
                var tempGesture = TouchPanel.ReadGesture();
                return new GestureSample(
                    tempGesture.GestureType,
                    tempGesture.Timestamp,
                    new Vector2(tempGesture.Position.X / ScreenScale.X, tempGesture.Position.Y / ScreenScale.Y),
                    new Vector2(tempGesture.Position2.X / ScreenScale.X, tempGesture.Position2.Y / ScreenScale.Y),
                    new Vector2(tempGesture.Delta.X / ScreenScale.X, tempGesture.Delta.Y / ScreenScale.Y),
                    new Vector2(tempGesture.Delta2.X / ScreenScale.X, tempGesture.Delta2.Y / ScreenScale.Y));
            }

            return new GestureSample();
        }
    }
}
