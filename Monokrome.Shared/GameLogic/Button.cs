using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Monokrome.Helpers;
using Monokrome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monokrome.GameLogic
{
    public class Button : ICustomVisual
    {
        public string Name { get; set; }
        public Vector2 Position;
        public Texture2D Texture { get; set; }
        public Texture2D TexturePressed { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public Vector2 TextPositionOffset { get; set; }
        public SpriteFont TextFont { get; set; }
        public Action ClickAction { get; set; }
        public bool IsActive { get; set; }
        public bool IsSelected { get; set; }

        public void Update(GameTime gameTime, GestureSample touchGesture)
        {
            var buttonRectangle = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Texture.Width, this.Texture.Height);
            if (InputHelper.PreviousMouseState.LeftButton == ButtonState.Pressed && InputHelper.MouseState.LeftButton == ButtonState.Released && buttonRectangle.Contains(InputHelper.MouseState.Position))
                this.ClickAction.Invoke();

            if (touchGesture.GestureType == GestureType.Tap && buttonRectangle.Contains(touchGesture.Position))
                this.ClickAction.Invoke();

            if (buttonRectangle.Contains(InputHelper.MouseState.Position))
                this.IsSelected = true;
            else
                this.IsSelected = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (this.IsSelected)
                spriteBatch.Draw(this.TexturePressed, this.Position, Color.White);
            else
                spriteBatch.Draw(this.Texture, this.Position, Color.White);

            if (!string.IsNullOrEmpty(this.Text))
            {
                Vector2 textSize = this.TextFont.MeasureString(this.Text);
                var textPosition = new Vector2(this.Position.X + ((this.Texture.Width - textSize.X) / 2) + this.TextPositionOffset.X, this.Position.Y + ((this.Texture.Height - textSize.Y) / 2) + this.TextPositionOffset.Y);
                spriteBatch.DrawString(this.TextFont, this.Text, textPosition, this.TextColor);
            }
        }
    }
}
