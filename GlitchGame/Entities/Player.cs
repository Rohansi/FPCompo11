using GlitchGame.Weapons;
using Microsoft.Xna.Framework;
using SFML.Window;

namespace GlitchGame.Entities
{
    public sealed class Player : Ship
    {
        public override int DrawOrder { get { return 10; } }
        public override byte RadarType { get { return 1; } }

        public Player(Vector2 position)
            : base(position, "ship.png", 1)
        {
            Weapon = new DualLaserGun(this);
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
