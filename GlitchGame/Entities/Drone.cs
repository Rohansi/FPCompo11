using GlitchGame.Weapons;
using Microsoft.Xna.Framework;

namespace GlitchGame.Entities
{
    public sealed class Drone : Computer
    {
        public Drone(State state, Vector2 position, float size)
            : base(state, position, size, 0, "drone")
        {
            var r = Program.Random.NextDouble();

            if (r <= 0.50f)
                Weapon = new NerfGun(this);
            else
                Weapon = new LaserGun(this);

            MaxHealth = 150;
            Health = MaxHealth;
            MaxEnergy = 500;
            Energy = MaxEnergy;

            HealthRegenRate = 0;
            EnergyRegenRate = -5;

            DamageTakenMultiplier = 1;
            DamageMultiplier = 1;
            SpeedMultiplier = 1;
        }

        public override void Update(float dt)
        {
            if (Energy <= 0)
            {
                Dead = true;
                return;
            }

            base.Update(dt);
        }
    }
}
