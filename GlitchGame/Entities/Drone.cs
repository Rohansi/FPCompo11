using GlitchGame.Weapons;
using Microsoft.Xna.Framework;

namespace GlitchGame.Entities
{
    public sealed class Drone : Computer
    {
        public Drone(Vector2 position, float size)
            : base(position, size, 0, "drone")
        {
            Weapon = new NerfGun(this);

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
