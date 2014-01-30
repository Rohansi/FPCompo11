using GlitchGame.Weapons;
using Microsoft.Xna.Framework;

namespace GlitchGame.Entities
{
    public sealed class Drone : Computer
    {
        public Drone(Vector2 position, float size)
            : base(position, "ship.png", size, 0)
        {
            Weapon = new LaserGun(this);

            MaxHealth = 250;
            Health = MaxHealth;
            MaxEnergy = 500;
            Energy = MaxEnergy;

            HealthRegenRate = 0;
            EnergyRegenRate = -5;

            DamageTakenMultiplier = 1;
            DamageMultiplier = 1;
            SpeedMultiplier = 1;
        }

        public override void Update()
        {
            if (Energy <= 0)
            {
                Dead = true;
                return;
            }

            base.Update();
        }
    }
}
