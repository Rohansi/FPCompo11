using System;
using GlitchGame.Entities;
using Microsoft.Xna.Framework;

namespace GlitchGame.Weapons
{
    public abstract class Weapon
    {
        protected static readonly Vector2 Left = new Vector2(-0.485f, -0.20f);
        protected static readonly Vector2 Right = new Vector2(0.485f, -0.20f);

        public float MaxCooldown { get; protected set; }
        public float Cooldown { get; protected set; }

        public float Progress
        {
            get { return MaxCooldown > 0 ? (Cooldown / MaxCooldown) : 1; }
        }
        
        protected Ship Parent { get; private set; }

        protected Weapon(Ship parent)
        {
            Parent = parent;
            MaxCooldown = 0;
            Cooldown = 0;
        }

        public void TryShoot()
        {
            if (Cooldown > 0)
                return;

            Cooldown = MaxCooldown;
            Shoot();
        }

        public void Update()
        {
            Cooldown = Math.Max(Cooldown - Program.FrameTime, 0);
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
