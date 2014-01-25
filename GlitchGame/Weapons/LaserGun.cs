using System;
using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;

namespace GlitchGame.Weapons
{
    public sealed class LaserGun : Weapon
    {
        private static readonly Vector2 Speed = new Vector2(0, -10f);

        public LaserGun(Ship parent) : base(parent)
        {
            MaxCooldown = 0.15f;
        }

        public override void Shoot()
        {
            Program.Entities.AddLast(new Bullet(Parent, Left * Parent.Size, Speed));
            Program.Entities.AddLast(new Bullet(Parent, Right * Parent.Size, Speed));
        }
    }
}
