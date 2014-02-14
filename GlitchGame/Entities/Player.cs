using System.Collections.Generic;
using System.Collections.ObjectModel;
using GlitchGame.Weapons;
using Microsoft.Xna.Framework;
using SFML.Window;

namespace GlitchGame.Entities
{
    public sealed class Player : Ship
    {
        public override int Depth { get { return 10; } }

        private bool _keyW, _keyA, _keyS, _keyD, _keySpace;
        private List<Weapon> _weapons;

        public ReadOnlyCollection<Weapon> Weapons
        {
            get { return new ReadOnlyCollection<Weapon>(_weapons); }
        } 

        public Player(State state, Vector2 position)
            : base(state, position, 2, 0)
        {
            _weapons = new List<Weapon>
            {
                new LaserGun(this),
                new DualLaserGun(this),
                //new MissileLauncher(this),
                new Disruptor(this),
                new DroneLauncher(this)
            };

            SwitchWeapon(0);

            MaxHealth = 10000;
            Health = MaxHealth;
            MaxEnergy = 1000;
            Energy = MaxEnergy;

            HealthRegenRate = 10;
            EnergyRegenRate = 25;

            DamageTakenMultiplier = 1;
            DamageMultiplier = 1;
            SpeedMultiplier = 1;

            Input.Key[Keyboard.Key.W] = args => _keyW = args.Pressed;
            Input.Key[Keyboard.Key.A] = args => _keyA = args.Pressed;
            Input.Key[Keyboard.Key.S] = args => _keyS = args.Pressed;
            Input.Key[Keyboard.Key.D] = args => _keyD = args.Pressed;
            Input.Key[Keyboard.Key.Space] = args => _keySpace = args.Pressed;

            for (var i = 0; i < 9; i++)
            {
                var weaponIndex = i;
                Input.Key[Keyboard.Key.Num1 + i] = args =>
                {
                    if (args.Pressed)
                        SwitchWeapon(weaponIndex);
                    return true;
                };
            }

            Input.Key[Keyboard.Key.Q] = args =>
            {
                if (args.Pressed)
                {
                    var currentWeapon = _weapons.IndexOf(Weapon);
                    SwitchWeapon((currentWeapon + _weapons.Count - 1) % _weapons.Count);
                }

                return true;
            };

            Input.Key[Keyboard.Key.E] = args =>
            {
                if (args.Pressed)
                {
                    var currentWeapon = _weapons.IndexOf(Weapon);
                    SwitchWeapon((currentWeapon + _weapons.Count + 1) % _weapons.Count);
                }

                return true;
            };
        }

        public void SwitchWeapon(int index)
        {
            if (index < 0 || index > _weapons.Count - 1)
                return;

            Weapon = _weapons[index];
        }

        public override void Update(float dt)
        {
            Shooting = false;
            Thruster = 0;
            AngularThruster = 0;

            if (_keyW)
                Thruster -= 1;

            if (_keyS)
                Thruster += 1;

            if (_keyA)
                AngularThruster -= 1;

            if (_keyD)
                AngularThruster += 1;

            if (_keySpace)
                Shooting = true;

            foreach (var weapon in Weapons)
            {
                weapon.Update(dt);
            }

            base.Update(dt);
        }
    }
}
