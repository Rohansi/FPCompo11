using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Weapons
{
    public class NerfGun : Weapon
    {
        private const float Speed = 30;

        private bool _left;

        public NerfGun(Ship parent) : base(parent)
        {
            Icon = new Sprite(Assets.LoadTexture("wep_nerfgun.png")).Center();
            MaxCooldown = 0.30f;

            _left = true;
        }

        public override void Shoot()
        {
            if (_left)
                Program.Entities.AddLast(new NerfBall(Parent, Left * Parent.Size, new Vector2(0, -Speed)));
            else
                Program.Entities.AddLast(new NerfBall(Parent, Right * Parent.Size, new Vector2(0, -Speed)));

            _left = !_left;

            Assets.PlaySound("shoot_nerf.wav");
        }
    }
}
