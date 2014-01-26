using System.Collections.Generic;
using System.Collections.ObjectModel;
using GlitchGame.Weapons;
using Microsoft.Xna.Framework;
using SFML.Window;

namespace GlitchGame.Entities
{
    public sealed class Player : Ship
    {
        public override int DrawOrder { get { return 10; } }
        public override byte RadarType { get { return 1; } }

        private List<Weapon> _weapons; 

        public ReadOnlyCollection<Weapon> Weapons
        {
            get { return new ReadOnlyCollection<Weapon>(_weapons); }
        } 

        public Player(Vector2 position)
            : base(position, "ship.png", 1.5f)
        {
            _weapons = new List<Weapon>()
            {
                new LaserGun(this),
                new DualLaserGun(this)
            };

            SwitchWeapon(0);

            MaxHealth = 10000;
            Health = MaxHealth;
        }

        public void SwitchWeapon(int index)
        {
            if (index < 0 || index > _weapons.Count - 1)
                return;

            Weapon = _weapons[index];
        }

        public override void Update()
        {
            Shooting = false;
            Thruster = 0;
            AngularThruster = 0;

            if (Program.HasFocus)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.W))
                    Thruster -= 1;

                if (Keyboard.IsKeyPressed(Keyboard.Key.S))
                    Thruster += 1;

                if (Keyboard.IsKeyPressed(Keyboard.Key.A))
                    AngularThruster -= 1;

                if (Keyboard.IsKeyPressed(Keyboard.Key.D))
                    AngularThruster += 1;

                if (Keyboard.IsKeyPressed(Keyboard.Key.Space))
                    Shooting = true;
            }

            Weapon.Update();

            base.Update();
        }
    }
}
