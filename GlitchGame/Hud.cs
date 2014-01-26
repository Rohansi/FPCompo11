using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame
{
    public class Hud : Drawable
    {
        const float Padding = 8;
        const float IconSize = 64;
        const float IconSizeHalf = IconSize / 2f;

        private Sprite _selected;

        public Hud()
        {
            _selected = new Sprite(Assets.LoadTexture("wep_selected.png")).Center();
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            var bounds = Program.HudCamera.Bounds;

            var iconPos = new Vector2f(Padding + IconSizeHalf, bounds.Height - IconSizeHalf - Padding);
            foreach (var wep in Program.Player.Weapons)
            {
                wep.Icon.Position = iconPos;
                target.Draw(wep.Icon);

                if (wep == Program.Player.Weapon)
                {
                    _selected.Position = iconPos;
                    target.Draw(_selected);
                }

                iconPos += new Vector2f(IconSize + Padding, 0);
            }
        }
    }
}
