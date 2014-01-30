using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame
{
    public abstract class State
    {
        public World World;
        public LinkedList<Entity> Entities;

        protected State()
        {
            World = new World(new Vector2(0, 0));
            Entities = new LinkedList<Entity>();
        }

        public abstract void Enter();
        public abstract void Leave();
        public abstract void Update();
        public abstract void Draw(RenderTarget target);
        public abstract void DrawHud(RenderTarget target);
    }
}
