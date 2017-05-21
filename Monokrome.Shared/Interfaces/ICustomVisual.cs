using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monokrome.Interfaces
{
    public interface ICustomVisual
    {
        string Name { get; set; }
        bool IsActive { get; set; }
        void Update(GameTime gameTime, GestureSample touchGesture);
        void Draw(SpriteBatch spriteBatch);
    }
}
