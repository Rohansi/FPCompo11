using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame
{
    public abstract class State
    {
        public World World { get; protected set; }
        public LinkedList<Entity> Entities { get; protected set; }

        private Input _input;
        public Input Input
        {
            get { return _input ?? (_input = new Input()); }
        }

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

        public virtual bool ProcessEvent(InputArgs args)
        {
            return Input.ProcessInput(args) || Entities.Iterate().Any(e => e.Input.ProcessInput(args));
        }

        public IEnumerable<Entity> EntitiesInRegion(FloatRect rect)
        {
            var min = new Vector2(rect.Left, rect.Top) / Program.PixelsPerMeter;
            var aabb = new AABB(min, min + (new Vector2(rect.Width, rect.Height) / Program.PixelsPerMeter));
            var result = new List<Entity>(256);

            World.QueryAABB(f =>
            {
                result.Add((Entity)f.Body.UserData);
                return true;
            }, ref aabb);

            return result.Distinct().OrderBy(e => e.Depth + e.DepthBias);
        }

        public Vector2? FindOpenSpace(Vector2 center, float radius, Vector2 size)
        {
            const int maxRetry = 25;

            int i = 0;
            Vector2 position;
            bool empty;

            do
            {
                position = center + Util.LengthDir((float)Program.Random.NextDouble() * Util.Pi2, (float)Math.Sqrt(Program.Random.NextDouble()) * radius).ToFarseer();
                empty = true;

                var aabb = new AABB(position, size.X, size.Y);
                World.QueryAABB(f =>
                {
                    empty = false;
                    return false;
                }, ref aabb);

                i++;
            } while (i <= maxRetry && !empty);

            if (!empty)
                return null;

            return position;
        }
    }
}
