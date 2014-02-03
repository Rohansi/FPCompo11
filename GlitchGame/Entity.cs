using FarseerPhysics.Dynamics;
using GlitchGame.Devices;
using SFML.Graphics;

namespace GlitchGame
{
    public abstract class Entity : Transformable
    {
        public readonly float DepthBias;
        public readonly State State;
        public Body Body { get; protected set; }
        
        public abstract int Depth { get; }
        public abstract RadarValue RadarType { get; }
        public bool Dead { get; protected set; }

        protected Entity(State state)
        {
            DepthBias = (float)Program.Random.NextDouble();

            State = state;
        }

        public virtual void Destroyed()
        {
            if (Body != null)
                State.World.RemoveBody(Body);
        }

        public abstract void Update();
        public abstract void Draw(RenderTarget target);
    }
}
