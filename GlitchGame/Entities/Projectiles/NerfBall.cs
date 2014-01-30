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

            ship.Health -= 2.5f * HealthDamageMultiplier(ship);
            ship.Energy -= 5.0f * EnergyDamageMultiplier(ship);

            var d = Program.Random.NextDouble() <= 0.75 ? 1f : -1f;
            ship.NerfMultiplier -= 0.05f * Size * d;
        }
    }
}
