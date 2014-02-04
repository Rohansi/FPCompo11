using Microsoft.Xna.Framework;
using SFML.Window;

namespace GlitchGame.Entities.Projectiles
{
    public class DisruptoBall : Bullet
    {
        public override bool DirectionalRotation { get { return false; } }

        public DisruptoBall(Ship parent, Vector2 offset, Vector2 speed)
            : base(parent, offset, speed, "disruptoball.png")
        {
            var texSize = Sprite.Texture.Size;
            Sprite.Origin = new Vector2f(texSize.X / 2f, texSize.Y / 3f);
        }

        public override void Hit(Ship ship)
        {
            base.Hit(ship);

            ship.Energy -= 10.0f * EnergyDamageMultiplier(ship);

            var r = Program.Random.NextDouble();

            if (r <= 0.25f) // 25% chance to do nothing
            {
                return;
            }

            if (r <= 1.00f) // 75% chance to corrupt
            {
                if (ship is Computer)
                {
                    (ship as Computer).Corrupt();
                }
            }
        }
    }
}
