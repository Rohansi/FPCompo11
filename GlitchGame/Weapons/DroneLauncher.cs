using System.Collections.Generic;
using GlitchGame.Entities;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Weapons
{
    public class DroneLauncher : Weapon
    {
        private const float LaunchSpeed = 10;
        private const int MaxConcurrent = 8;
        private const float BuildTime = 5;

        public override float MaxCooldown { get { return 1.0f; } }
        public override float EnergyCost { get { return 25; } }

        private Sprite _icon;
        private Text _amountText;

        private List<Drone> _drones;
        private int _amount;
        private float _time;
        private bool _left;

        public DroneLauncher(Ship parent)
            : base(parent)
        {
            _icon = new Sprite(Assets.LoadTexture("wep_drone_launcher.png")).Center();

            _amountText = new Text("", Program.Font, 14);
            _amountText.Color = new Color(225, 225, 225);

            _drones = new List<Drone>(MaxConcurrent);
            _amount = 0;
            _time = 0;
            _left = false;
        }

        public override void Update()
        {
            base.Update();

            _drones.RemoveAll(d => d.Dead);

            if (_drones.Count + _amount < MaxConcurrent)
            {
                _time += Program.FrameTime;
            }
            else
            {
                _time = 0;
            }

            if (_time >= BuildTime)
            {
                _amount++;
                _time -= BuildTime;
            }
        }

        public override void Draw(RenderTarget target, Vector2f position)
        {
            _icon.Position = position;
            target.Draw(_icon);
            
            if (_amount == 0)
            {
                var darken = new RectangleShape(new Vector2f(Hud.IconSize - Hud.IconBorderTwice, Hud.IconSize - Hud.IconBorderTwice));
                darken.Origin = new Vector2f(Hud.IconSizeHalf - Hud.IconBorder, Hud.IconSizeHalf - Hud.IconBorder);
                darken.FillColor = new Color(0, 0, 0, 200);
                darken.Position = position;
                target.Draw(darken);
            }

            if (_time > 0)
            {
                var per = _time / BuildTime;
                var box = new RectangleShape(new Vector2f(per * (Hud.IconSize - Hud.IconBorderTwice), 8));
                box.FillColor = new Color(0, 180, 0, 128);
                box.Position = position - new Vector2f(Hud.IconSizeHalf - Hud.IconBorder, Hud.IconSizeHalf - Hud.IconBorder);
                target.Draw(box);
            }

            _amountText.DisplayedString = _amount.ToString("G");
            _amountText.Position = position + new Vector2f(Hud.IconSizeHalf - Hud.Padding, Hud.IconSizeHalf - Hud.Padding);

            var bounds = _amountText.GetLocalBounds();
            _amountText.Origin = new Vector2f(bounds.Width + bounds.Left, bounds.Height + bounds.Top);

            target.Draw(_amountText);
        }

        public override bool Shoot()
        {
            if (_amount <= 0)
                return false;

            var side = _left ? Left : Right;
            _left = !_left;

            var pos = Parent.Body.Position + Parent.Body.GetWorldVector(side * Parent.Size);
            var drone = new Drone(Parent.State, pos, Parent.Size / 8);

            drone.Body.LinearVelocity = Parent.Body.LinearVelocity + Parent.Body.GetWorldVector(new Vector2(0, -LaunchSpeed));
            drone.Body.Rotation = Parent.Body.Rotation;

            Parent.State.Entities.AddLast(drone);
            _drones.Add(drone);
            _amount--;

            Assets.PlaySound("shoot_drone.wav", Parent.Position);

            return true;
        }
    }
}
