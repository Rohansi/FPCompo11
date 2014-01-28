using GlitchGame.Devices;
using SFML.Graphics;

namespace GlitchGame
{
    public abstract class Entity : Transformable
    {
        public float DepthBias { get; private set; }

        protected Entity()
        {
            DepthBias = (float)Program.Random.NextDouble();
        }

        public abstract int Depth { get; }
        public abstract RadarValue Radar { get; }
        public bool Dead { get; protected set; }

        public abstract void Destroyed();
        public abstract void Update();
        public abstract void Draw(RenderTarget target);
    }
}
