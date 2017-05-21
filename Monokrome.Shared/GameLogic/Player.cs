using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Monokrome.Helpers;

namespace Monokrome.GameLogic
{
    internal class Player : GameObject
    {
        private float horizontalCurrentOffset;
        public float HorizontalMinOffset;
        public float HorizontalMaxOffset;
        public float Speed { get; set; }
        public float HorizontalNeutralPosition;

        public Player(Color colour1, Color colour2, Texture2D texture1, Texture2D texture2, Rectangle boundaries)
            : base(colour1, colour2, texture1, texture2, boundaries)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (InputHelper.CurrentKeyboardState.IsKeyDown(Keys.Up))
                this.Position.Y -= 20;
            if (InputHelper.CurrentKeyboardState.IsKeyDown(Keys.Down))
                this.Position.Y += 20;
            //if (InputHelper.CurrentKeyboardState.IsKeyDown(Keys.Left))
            //    this.horizontalCurrentOffset -= 4;
            //if (InputHelper.CurrentKeyboardState.IsKeyDown(Keys.Right))
            //    this.horizontalCurrentOffset += 4;

            while (TouchPanel.IsGestureAvailable)
            {
                //var tempGesture = TouchPanel.ReadGesture();
                //tempGesture = new GestureSample(
                //    tempGesture.GestureType,
                //    tempGesture.Timestamp,
                //    new Vector2(tempGesture.Position.X / InputHelper.ScreenScale.X, tempGesture.Position.Y / InputHelper.ScreenScale.Y),
                //    new Vector2(tempGesture.Position2.X / InputHelper.ScreenScale.X, tempGesture.Position2.Y / InputHelper.ScreenScale.Y),
                //    new Vector2(tempGesture.Delta.X / InputHelper.ScreenScale.X, tempGesture.Delta.Y / InputHelper.ScreenScale.Y),
                //    new Vector2(tempGesture.Delta2.X / InputHelper.ScreenScale.X, tempGesture.Delta2.Y / InputHelper.ScreenScale.Y));

                var touchGesture = InputHelper.GetTouchGesture();

                if (touchGesture.GestureType == GestureType.FreeDrag)
                {
                    this.Position.Y += touchGesture.Delta.Y * this.Speed;
                    // this.horizontalCurrentOffset += InputHelper.TouchGestureSample.Delta.X * this.Speed;
                }
            }

            this.horizontalCurrentOffset = MathHelper.Clamp(this.horizontalCurrentOffset, this.HorizontalMinOffset, this.HorizontalMaxOffset);
            this.Position.X = this.HorizontalNeutralPosition + this.horizontalCurrentOffset;
            this.Position.Y = MathHelper.Clamp(this.Position.Y, 0, this.boundaries.Height - this.Rectangle.Height);

            base.Update(gameTime);
        }

        public override void SwitchColour()
        {
            if (this.currentColour == this.colour1)
            {
                this.currentColour = this.colour2;
                this.currentTexture = this.texture2;
            }
            else
            {
                this.currentColour = this.colour1;
                this.currentTexture = this.texture1;
            }
        }
    }
}
