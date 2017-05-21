using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class GameMenu
    {
        private IList<Tuple<int, ICustomVisual>> customElements;
        private int currentScreen;
        public int CurrentScreen
        {
            get
            {
                return this.currentScreen;
            }
            set
            {
                this.currentScreen = value;
            }
        }

        public GameMenu()
        {
            this.customElements = new List<Tuple<int, ICustomVisual>>();
            this.CurrentScreen = 0;
        }

        public void AddVisual(ICustomVisual newVisual, int screenNumber)
        {
            this.customElements.Add(new Tuple<int, ICustomVisual>(screenNumber, newVisual));
        }

        public ICustomVisual GetVisual(string visualName)
        {
            return this.customElements.FirstOrDefault(x => x.Item2.Name.Equals(visualName)).Item2;
        }

        public void ActivateVisual(string visualName)
        {
            ICustomVisual visual = this.customElements.FirstOrDefault(x => x.Item2.Name.Equals(visualName)).Item2;
            if (visual != null)
                visual.IsActive = true;
        }

        public void DeactivateVisual(string visualName)
        {
            ICustomVisual visual = this.customElements.FirstOrDefault(x => x.Item2.Name.Equals(visualName)).Item2;
            if (visual != null)
                visual.IsActive = false;
        }

        public void Update(GameTime gameTime)
        {
            var tempScreen = this.CurrentScreen;
            var touchGesture = InputHelper.GetTouchGesture();
            foreach (ICustomVisual visual in this.customElements.Where(x => x.Item1 == tempScreen && x.Item2.IsActive).Select(x => x.Item2))
                visual.Update(gameTime, touchGesture);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var tempScreen = this.CurrentScreen;
            foreach (ICustomVisual visual in this.customElements.Where(x => x.Item1 == tempScreen && x.Item2.IsActive).Select(x => x.Item2))
                visual.Draw(spriteBatch);
        }
    }
}
