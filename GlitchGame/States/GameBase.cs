using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using GlitchGame.Entities;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

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
            Program.Window.KeyPressed += KeyPressed;
        }

        public override void Leave()
        {
            Program.Window.KeyPressed -= KeyPressed;
        }

        public override void Update()
        {
            foreach (var e in Entities.Iterate())
            {
                e.Update();
            }

            foreach (var e in Entities.Where(e => e.Dead).ToList())
            {
                e.Destroyed();
                Entities.Remove(e);
            }

            World.Step(Program.FrameTime);

            Program.Camera.Position = Player.Position;
        }

        public override void Draw(RenderTarget target)
        {
            target.Draw(_background);

            foreach (var e in EntitiesInRegion(Program.Camera.Bounds))
            {
                e.Draw(target);
            }
        }

        public override void DrawHud(RenderTarget target)
        {
            target.Draw(_hud);
        }

        private void KeyPressed(object sender, KeyEventArgs args)
        {
            var weapons = Player.Weapons;
            var currentWeapon = weapons.IndexOf(Player.Weapon);

            if (args.Code >= Keyboard.Key.Num1 && args.Code <= Keyboard.Key.Num9)
            {
                Player.SwitchWeapon((int)args.Code - (int)Keyboard.Key.Num1);
            }

            if (args.Code == Keyboard.Key.Q)
            {
                Player.SwitchWeapon((currentWeapon + weapons.Count - 1) % weapons.Count);
            }

            if (args.Code == Keyboard.Key.E)
            {
                Player.SwitchWeapon((currentWeapon + weapons.Count + 1) % weapons.Count);
            }
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
