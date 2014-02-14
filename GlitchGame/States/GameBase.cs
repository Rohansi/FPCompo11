using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using GlitchGame.Entities;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.States
{
    public abstract class GameBase : State
    {
        public readonly float Radius;
        public Player Player;

        private Background _background;
        private Hud _hud;

        protected GameBase(float radius, float asteroidDensity)
        {
            World = new World(new Vector2(0, 0));
            Entities = new LinkedList<Entity>();

            Radius = radius;

            Player = new Player(this, new Vector2(0, 0));
            Entities.AddLast(Player);

            _background = new Background("background.png");
            _hud = new Hud(this);

            #region Border
            var step = Util.Pi2 / (Util.Pi2 * radius);

            for (var dir = 0f; dir <= Util.Pi2; dir += step)
            {
                var count = 1;

                while (true)
                {
                    var idx = Program.Random.Next(Asteroid.Radiuses.Count);
                    var size = new Vector2(Asteroid.Radiuses[idx]);
                    var pos = Util.LengthDir(dir, radius).ToFarseer();
                    var space = FindOpenSpace(pos, 1, size);

                    if (!space.HasValue || count >= 10)
                        break;

                    Entities.AddLast(new Asteroid(this, space.Value, idx, true));
                    count++;
                }
            }
            #endregion

            #region Asteroids
            var area = Math.PI * (radius * radius);
            var asteroids = (int)((area / 4) * asteroidDensity);
            var asteroidSize = new Vector2(2, 2);

            for (var i = 0; i < asteroids; i++)
            {
                var space = FindOpenSpace(new Vector2(0), radius, asteroidSize);

                if (!space.HasValue)
                    continue;

                Entities.AddLast(new Asteroid(this, space.Value));
            }
            #endregion
        }

        public override void Enter()
        {
            
        }

        public override void Leave()
        {
            
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            Program.Camera.Position = Player.Body.Position.ToSfml() * Program.PixelsPerMeter;
        }

        public override void Draw(RenderTarget target)
        {
            target.Draw(_background);

            foreach (var e in EntitiesInRegion(Program.Camera.Bounds))
            {
                e.Draw(target);
            }

#if DEBUG
            var debugView = new FarseerDebugView(World);
            debugView.AppendFlags(DebugViewFlags.Shape);
            debugView.Draw(target);
#endif
        }

        public override void DrawHud(RenderTarget target)
        {
            target.Draw(_hud);
        }
    }
}
