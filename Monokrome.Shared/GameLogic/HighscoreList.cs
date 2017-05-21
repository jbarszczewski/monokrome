using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monokrome.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monokrome.GameLogic
{
    public class HighscoreList : ICustomVisual
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string HeaderText { get; set; }
        public IList<string> TextRows { get; set; }
        public Vector2 Position;
        public SpriteFont TextFont { get; set; }
        public Color TextColor { get; set; }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Input.Touch.GestureSample touchGesture)
        {            
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(this.TextFont, this.HeaderText, this.Position, this.TextColor);
            float offset = this.TextFont.MeasureString(this.HeaderText).Y;
            float textheigth = this.TextFont.MeasureString("A").Y;
            foreach(string row in TextRows)
            {
                var textPosition = new Vector2(this.Position.X, this.Position.Y + offset);
                spriteBatch.DrawString(this.TextFont, row, textPosition, this.TextColor);
                offset += textheigth;
            }
        }
    }
}
