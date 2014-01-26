using System;
using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Weapons
{
    public sealed class DualLaserGun : Weapon
    {
        private const float Speed = 10f;

        public DualLaserGun(Ship parent) : base(parent)
        {
            Icon = new Sprite(Assets.LoadTexture("wep_dual_laser.png")).Center();
            MaxCooldown = 0.30f;
        }

        public override void Shoot()
        {
            const float up = (float)Math.PI / 2f;
            const float diff = 0.1f;

            var dir = Direction(up);
            Program.Entities.AddLast(new Bullet(Parent, new Vector2(-0.0f, 0) + Left * Parent.Size, dir * Speed));

            dir = Direction(up + diff);
            Program.Entities.AddLast(new Bullet(Parent, new Vector2(-0.2f, 0) + Left * Parent.Size, dir * Speed));

            dir = Direction(up);
            Program.Entities.AddLast(new Bullet(Parent, new Vector2(0.0f, 0) + Right * Parent.Size, dir * Speed));

            dir = Direction(up - diff);
            Program.Entities.AddLast(new Bullet(Parent, new Vector2(0.2f, 0) + Right * Parent.Size, dir * Speed));
        }
    }
}
