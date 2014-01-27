using System;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame
{
    public class Hud : Drawable
    {
        private const float Padding = 8;
        private const float IconSize = 64;
        private const float IconSizeHalf = IconSize / 2f;
        private const float HealthWidth = 300;
        private const float HealthHeight = 32;

        private Sprite _selected;

        private RectangleShape _healthBack;
        private RectangleShape _health;
        private Text _healthText;

        public Hud()
        {
            _selected = new Sprite(Assets.LoadTexture("wep_selected.png")).Center();

            _healthBack = new RectangleShape(new Vector2f(HealthWidth + Padding * 2, HealthHeight + Padding * 2));
            _healthBack.Position = new Vector2f(Padding, Padding);
            _healthBack.FillColor = new Color(0, 0, 0);
            _healthBack.OutlineThickness = 2;
            _healthBack.OutlineColor = new Color(38, 38, 38);

            _health = new RectangleShape(new Vector2f(HealthWidth, HealthHeight));
            _health.Position = _healthBack.Position + new Vector2f(Padding, Padding);
            _health.FillColor = new Color(0, 120, 0);

            _healthText = new Text("", Program.Font, (int)(HealthHeight - Padding));
            _healthText.Position = _health.Position + new Vector2f(HealthWidth / 2, HealthHeight / 2);
            _healthText.Color = new Color(225, 225, 225);
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

            var player = Program.Player;
            var healthPercentage = Util.Clamp(player.Health / player.MaxHealth, 0, 1);
            _health.Size = new Vector2f(healthPercentage * HealthWidth, HealthHeight);

            _healthText.DisplayedString = string.Format("{0} HP", (int)Math.Round(player.Health));
            _healthText.Center();

            target.Draw(_healthBack);
            target.Draw(_health);
            target.Draw(_healthText);
        }
    }
}
