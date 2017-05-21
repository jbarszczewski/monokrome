using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monokrome.GameLogic
{
    internal class Obstacle : GameObject
    {
        public float Speed { get; set; }
        public bool IsActive { get; set; }

        public Obstacle(Color colour1, Color colour2, Texture2D texture1, Texture2D texture2, Rectangle boundaries)
            : base(colour1, colour2, texture1, texture2, boundaries)
        {
        }

        public override void Update(GameTime gameTime)
        {
            this.Position.X -= this.Speed;
            this.IsActive = this.Position.X > -this.Rectangle.Width;

            base.Update(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
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
