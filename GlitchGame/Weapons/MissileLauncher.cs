using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Weapons
{
    public class MissileLauncher : Weapon
    {
        private const float Speed = 15;

        public override float MaxCooldown { get { return 1.0f; } }
        public override float EnergyCost { get { return 0.020f; } }

        private Sprite _icon;

        public MissileLauncher(Ship parent)
            : base(parent)
        {
            _icon = new Sprite(Assets.LoadTexture("wep_nerfgun.png")).Center();
        }

        public override bool Shoot()
        {
            Parent.State.Entities.AddLast(new Missile(Parent, Center * Parent.Size, new Vector2(0, -Speed), "missile.png"));

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
