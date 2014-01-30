using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Weapons
{
    public class NerfGun : Weapon
    {
        private const float Speed = 30;

        public override float MaxCooldown { get { return 0.30f; } }
        public override float EnergyCost { get { return 15; } }

        private bool _left;
        private Sprite _icon;

        public NerfGun(Ship parent) : base(parent)
        {
            _icon = new Sprite(Assets.LoadTexture("wep_nerfgun.png")).Center();
            _left = true;
        }

        public override bool Shoot()
        {
            if (_left)
                Program.Entities.AddLast(new NerfBall(Parent, Left * Parent.Size, new Vector2(0, -Speed)));
            else
                Program.Entities.AddLast(new NerfBall(Parent, Right * Parent.Size, new Vector2(0, -Speed)));

            _left = !_left;

            Assets.PlaySound("shoot_nerf.wav", Parent.Position);

            return true;
        }

        public override void Draw(RenderTarget target, Vector2f position)
        {
            _icon.Position = position;
            target.Draw(_icon);
        }
    }
}
