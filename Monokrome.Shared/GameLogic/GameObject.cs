using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monokrome.GameLogic
{
    internal abstract class GameObject
    {
        protected Rectangle boundaries;
        protected Color colour1;
        protected Color colour2;
        protected Color currentColour;
        protected Texture2D texture1;
        protected Texture2D texture2;
        protected Texture2D currentTexture;
        public Color CurrentColour
        {
            get { return this.currentColour; }
        }

        public Vector2 Position;
        public Texture2D CurrentTexture
        {
            get { return this.currentTexture; }
        }

        public Rectangle Rectangle { get; private set; }
        public Vector2 Size;

        public GameObject(Color colour1, Color colour2, Texture2D texture1, Texture2D texture2, Rectangle boundaries)
        {
            this.currentColour = this.colour1 = colour1;
            this.colour2 = colour2;
            this.currentTexture = this.texture1 = texture1;
            this.texture2 = texture2;
            this.boundaries = boundaries;
        }

        public abstract void SwitchColour();

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            this.Rectangle = new Rectangle((int)Position.X, (int)Position.Y, this.CurrentTexture.Width, this.CurrentTexture.Height);
            spriteBatch.Draw(this.CurrentTexture, this.Position, this.CurrentColour);
        }
    }
}
