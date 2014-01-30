using System;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame
{
    public class Hud : Drawable
    {
        public const float Padding = 4;
        public const float IconSize = 64;
        public const float IconSizeHalf = IconSize / 2f;
        public const float IconBorder = 2;
        public const float IconBorderTwice = IconBorder * 2;
        public const float BarWidth = 300;
        public const float BarHeight = 20;

        private Sprite _selected;

        private RectangleShape _statusBack;

        private RectangleShape _health;
        private Text _healthText;

        private RectangleShape _energy;
        private Text _energyText;

        public Hud()
        {
            _selected = new Sprite(Assets.LoadTexture("wep_selected.png")).Center();

            _statusBack = new RectangleShape(new Vector2f(BarWidth + Padding * 2, BarHeight * 2 + Padding * 3));
            _statusBack.Position = new Vector2f(Padding, Padding);
            _statusBack.FillColor = new Color(0, 0, 0);
            _statusBack.OutlineThickness = 2;
            _statusBack.OutlineColor = new Color(38, 38, 38);

            _health = new RectangleShape(new Vector2f(BarWidth, BarHeight));
            _health.Position = _statusBack.Position + new Vector2f(Padding, Padding);
            _health.FillColor = new Color(0, 120, 0);

            _healthText = new Text("", Program.Font, (int)(BarHeight - Padding));
            _healthText.Position = _health.Position + new Vector2f(BarWidth / 2, BarHeight / 2);
            _healthText.Color = new Color(225, 225, 225);

            _energy = new RectangleShape(new Vector2f(BarWidth, BarHeight));
            _energy.Position = _health.Position + new Vector2f(0, BarHeight + Padding);
            _energy.FillColor = new Color(30, 30, 180);

            _energyText = new Text("", Program.Font, (int)(BarHeight - Padding));
            _energyText.Position = _energy.Position + new Vector2f(BarWidth / 2, BarHeight / 2);
            _energyText.Color = new Color(225, 225, 225);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            var bounds = Program.HudCamera.Bounds;

            var iconPos = new Vector2f(Padding + IconSizeHalf, bounds.Height - IconSizeHalf - Padding);
            foreach (var wep in Program.Player.Weapons)
            {
                wep.Draw(target, iconPos);

                if (wep == Program.Player.Weapon)
                {
                    _selected.Position = iconPos;
                    target.Draw(_selected);
                }

                iconPos += new Vector2f(IconSize + Padding, 0);
            }

            var player = Program.Player;
            var healthPercentage = Util.Clamp(player.Health / player.MaxHealth, 0, 1);
            _health.Size = new Vector2f(healthPercentage * BarWidth, BarHeight);

            _healthText.DisplayedString = string.Format("{0} HP", (int)Math.Round(player.Health));
            _healthText.Center();

            var energyPercentage = Util.Clamp(player.Energy / player.MaxEnergy, 0, 1);
            _energy.Size = new Vector2f(energyPercentage * BarWidth, BarHeight);

            _energyText.DisplayedString = string.Format("{0} EP", (int)Math.Round(player.Energy));
            _energyText.Center();

            target.Draw(_statusBack);
            target.Draw(_health);
            target.Draw(_healthText);
            target.Draw(_energy);
            target.Draw(_energyText);
        }
    }
}
