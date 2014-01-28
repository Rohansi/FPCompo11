using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Weapons
{
    public class Disruptor : Weapon
    {
        private const float Speed = 30;

        public override float MaxCooldown { get { return 0.75f; } }
        public override float EnergyCost { get { return 30; } }

        public Disruptor(Ship parent) : base(parent)
        {
            Icon = new Sprite(Assets.LoadTexture("wep_disruptor.png")).Center();
        }

        public override void Shoot()
        {
            Program.Entities.AddLast(new DisruptoBall(Parent, Center * Parent.Size, new Vector2(0, -Speed)));
            Assets.PlaySound("shoot_disruptor.wav", Parent.Position);
        }
    }
}
