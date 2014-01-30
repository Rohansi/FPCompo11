using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Weapons
{
    public sealed class LaserGun : Weapon
    {
        private const float Speed = 40f;

        public override float MaxCooldown { get { return 0.15f; } }
        public override float EnergyCost { get { return 5; } }

        private Sprite _icon;

        public LaserGun(Ship parent) : base(parent)
        {
            _icon = new Sprite(Assets.LoadTexture("wep_laser.png")).Center();
        }

        public override bool Shoot()
        {
            Parent.State.Entities.AddLast(new Laser(Parent, Left * Parent.Size, new Vector2(0, -Speed)));
            Parent.State.Entities.AddLast(new Laser(Parent, Right * Parent.Size, new Vector2(0, -Speed)));

            Assets.PlaySound("shoot_laser.wav", Parent.Position);

            return true;
        }

        public override void Draw(RenderTarget target, Vector2f position)
        {
            _icon.Position = position;
            target.Draw(_icon);
        }
    }
}
