using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Weapons
{
    public class Disruptor : Weapon
    {
        private const float Speed = 30;

        public override float MaxCooldown { get { return 0.75f; } }
        public override float EnergyCost { get { return 30; } }

        private Sprite _icon;

        public Disruptor(Ship parent) : base(parent)
        {
            _icon = new Sprite(Assets.LoadTexture("wep_disruptor.png")).Center();
        }

        public override bool Shoot()
        {
            Parent.State.Entities.AddLast(new DisruptoBall(Parent, Center * Parent.Size, new Vector2(0, -Speed)));
            Assets.PlaySound("shoot_disruptor.wav", Parent.Position);

            return true;
        }

        public override void Draw(RenderTarget target, Vector2f position)
        {
            _icon.Position = position;
            target.Draw(_icon);
        }
    }
}
