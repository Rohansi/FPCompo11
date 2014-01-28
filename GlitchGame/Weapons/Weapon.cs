using GlitchGame.Entities;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Weapons
{
    public abstract class Weapon
    {
        protected static readonly Vector2 Left = new Vector2(-0.485f, 0);
        protected static readonly Vector2 Right = new Vector2(0.485f, 0);
        protected static readonly Vector2 Center = new Vector2(0, -0.5f);

        public Sprite Icon { get; protected set; }

        public abstract float MaxCooldown { get; }
        public float Cooldown { get; protected set; }
        public abstract float EnergyCost { get; }
        
        protected Ship Parent { get; private set; }

        protected Weapon(Ship parent)
        {
            Parent = parent;
            Cooldown = 0;
        }

        public void TryShoot()
        {
            if (Cooldown > 0)
                return;

            if (Parent.Energy < EnergyCost)
                return;

            Cooldown += MaxCooldown;
            Parent.Energy -= EnergyCost;
            Shoot();
        }

        public void Update()
        {
            if (Cooldown > 0)
                Cooldown -= Program.FrameTime;
        }

        protected Vector2 Direction(float dir)
        {
            return Util.LengthDir(dir, 1).ToFarseer();
        }

        /// <summary>
        /// Called when projectiles need to be made
        /// </summary>
        public abstract void Shoot();
    }
}
