using Microsoft.Xna.Framework;
using SFML.Window;

namespace GlitchGame.Entities.Projectiles
{
    public class NerfBall : Bullet
    {
        public NerfBall(Ship parent, Vector2 offset, Vector2 speed)
            : base(parent, offset, speed, "nerfball.png")
        {
            var texSize = Sprite.Texture.Size;
            Sprite.Origin = new Vector2f(texSize.X / 2f, texSize.Y / 2f);
        }

        public override void Hit(Ship ship)
        {
            base.Hit(ship);

            ship.Health -= 2.5f * Size * ship.DamageTakenMultiplier;
            ship.Energy -= 5 * Size;

            var d = Program.Random.NextDouble() <= 0.75 ? 1f : -1f;
            var r = Program.Random.NextDouble();

            if (r <= 0.25f) // 25% chance to do nothing
            {
                return;
            }

            if (r <= 0.50f) // 25% chance to change damage
            {
                ship.DamageMultiplier -= 0.05f * Size * d;
                return;
            }

            if (r <= 0.75f) // 25% chance to change speed
            {
                ship.SpeedMultiplier -= 0.05f * Size * d;
                return;
            }

            if (r <= 1.00f) // 25% chance to change regen
            {
                ship.HealthRegenRate -= 0.05f * Size * d;
            }
        }
    }
}
