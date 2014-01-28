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
        private const float BarWidth = 300;
        private const float BarHeight = 32;

        private Sprite _selected;

        private RectangleShape _healthBack;
        private RectangleShape _health;
        private Text _healthText;

        private RectangleShape _energyBack;
        private RectangleShape _energy;
        private Text _energyText;

        public Hud()
        {
            _selected = new Sprite(Assets.LoadTexture("wep_selected.png")).Center();

            // SETUP HEALTH
            _healthBack = new RectangleShape(new Vector2f(BarWidth + Padding * 2, BarHeight + Padding * 2));
            _healthBack.Position = new Vector2f(Padding, Padding);
            _healthBack.FillColor = new Color(0, 0, 0);
            _healthBack.OutlineThickness = 2;
            _healthBack.OutlineColor = new Color(38, 38, 38);

            _health = new RectangleShape(new Vector2f(BarWidth, BarHeight));
            _health.Position = _healthBack.Position + new Vector2f(Padding, Padding);
            _health.FillColor = new Color(0, 120, 0);

            _healthText = new Text("", Program.Font, (int)(BarHeight - Padding));
            _healthText.Position = _health.Position + new Vector2f(BarWidth / 2, BarHeight / 2);
            _healthText.Color = new Color(225, 225, 225);

            // SETUP ENERGY
            _energyBack = new RectangleShape(new Vector2f(BarWidth + Padding * 2, BarHeight + Padding * 2));
            _energyBack.FillColor = new Color(0, 0, 0);
            _energyBack.OutlineThickness = 2;
            _energyBack.OutlineColor = new Color(38, 38, 38);

            _energy = new RectangleShape(new Vector2f(BarWidth, BarHeight));
            _energy.FillColor = new Color(30, 30, 180);

            _energyText = new Text("", Program.Font, (int)(BarHeight - Padding));
            _energyText.Color = new Color(225, 225, 225);
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

            // DRAW HEALTH
            var player = Program.Player;
            var healthPercentage = Util.Clamp(player.Health / player.MaxHealth, 0, 1);
            _health.Size = new Vector2f(healthPercentage * BarWidth, BarHeight);

            _healthText.DisplayedString = string.Format("{0} HP", (int)Math.Round(player.Health));
            _healthText.Center();

            target.Draw(_healthBack);
            target.Draw(_health);
            target.Draw(_healthText);

            // DRAW EMERGY
            var energyPercentage = Util.Clamp(player.Energy / player.MaxEnergy, 0, 1);
            _energy.Size = new Vector2f(energyPercentage * BarWidth, BarHeight);

            _energyBack.Position = new Vector2f(bounds.Width - BarWidth - Padding * 3, Padding);
            _energy.Position = _energyBack.Position + new Vector2f(Padding, Padding);
            _energyText.Position = _energy.Position + new Vector2f(BarWidth / 2, BarHeight / 2);

            _energyText.DisplayedString = string.Format("{0} EP", (int)Math.Round(player.Energy));
            _energyText.Center();

            target.Draw(_energyBack);
            target.Draw(_energy);
            target.Draw(_energyText);
        }
    }
}
