using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Weapons
{
    public sealed class LaserGun : Weapon
    {
        private const float Speed = 40f;

        public LaserGun(Ship parent) : base(parent)
        {
            Icon = new Sprite(Assets.LoadTexture("wep_laser.png")).Center();
            MaxCooldown = 0.15f;
        }

        public override void Shoot()
        {
            Program.Entities.AddLast(new Bullet(Parent, Left * Parent.Size, new Vector2(0, -Speed)));
            Program.Entities.AddLast(new Bullet(Parent, Right * Parent.Size, new Vector2(0, -Speed)));
        }
    }
}
