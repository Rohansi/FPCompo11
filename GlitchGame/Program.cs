﻿using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using GlitchGame.Entities;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame
{
    public class Program
    {
        public const int FrameRate = 60;
        public const float FrameTime = 1f / FrameRate;
        public const float PixelsPerMeter = 64;
        public const float DegreesPerRadian = 180f / (float)Math.PI;
        public const int RadarRays = 126; // dont change
        public const int InstructionsPerSecond = 10000;
        public const int InstructionsPerFrame = (int)(FrameTime * InstructionsPerSecond);

        public static Random Random = new Random();
        public static RenderWindow Window;
        public static bool HasFocus { get; private set; }

        public static Camera HudCamera;
        public static Camera Camera;
        public static World World;
        public static LinkedList<IEntity> Entities;
        public static Player Player;
        public static Font Font;

        public static void Main()
        {
            Window = new RenderWindow(new VideoMode(1280, 720), "", Styles.Close, new ContextSettings(0, 0, 16));
            Window.SetFramerateLimit(FrameRate);
            Window.Closed += (sender, eventArgs) => Window.Close();
            Window.GainedFocus += (sender, args) => HasFocus = true;
            Window.LostFocus += (sender, args) => HasFocus = false;
            HasFocus = true;

            Window.KeyPressed += (sender, args) =>
            {
                if (args.Code >= Keyboard.Key.Num1 && args.Code <= Keyboard.Key.Num9)
                {
                    Player.SwitchWeapon((int)args.Code - (int)Keyboard.Key.Num1);
                }
            };

            HudCamera = new Camera(Window.DefaultView);

            Camera = new Camera(Window.DefaultView);
            Camera.Zoom = 2;

            World = new World(new Vector2(0, 0));
            Entities = new LinkedList<IEntity>();

            Font = new Font("Data/OpenSans-Regular.ttf");

#if DEBUG
            var debugView = new SFMLDebugView(World);
            debugView.AppendFlags(DebugViewFlags.Shape);
#endif

            #region Border
            const float radius = 30;
            const float step = (float)(2 * Math.PI) / 600;

            for (var dir = 0f; dir <= 2 * Math.PI; dir += step)
            {
                var count = 1;

                while (true)
                {
                    var idx = Random.Next(Asteroid.Radiuses.Count);
                    var size = new Vector2(Asteroid.Radiuses[idx]);
                    var pos = Util.LengthDir(dir, radius);
                    var area = new FloatRect(pos.X - 0.5f, pos.Y - 0.5f, 1f, 1f);
                    var space = FindOpenSpace(area, size);

                    if (!space.HasValue || count >= 10)
                        break;

                    Entities.AddLast(new Asteroid(space.Value, idx, true));
                    count++;
                }
            }
            #endregion

            Player = new Player(new Vector2(0, 0));
            Entities.AddLast(Player);

            #region Asteroids
            // TODO: fill in the circle border
            const int asteroids = (int)(radius * radius) / 20;

            for (var i = 0; i < asteroids; i++)
            {
                var size = new Vector2(2, 2);
                var area = new FloatRect(-radius + 7, -radius + 7, radius * 2 - 14, radius * 2 - 14);
                var space = FindOpenSpace(area, size);

                if (!space.HasValue)
                    continue;

                Entities.AddLast(new Asteroid(space.Value));
            }
            #endregion

            #region Enemies
            const int ships = 10;

            for (var i = 0; i < ships; i++)
            {
                var size = new Vector2(2, 2);
                var area = new FloatRect(-radius + 7, -radius + 7, radius * 2 - 14, radius * 2 - 14);
                var space = FindOpenSpace(area, size);

                if (!space.HasValue)
                    continue;

                Entities.AddLast(new Enemy(space.Value));
            }
            #endregion

            var background = new Background("background.png");
            var hud = new Hud();

            while (Window.IsOpen())
            {
                // INPUT
                Window.DispatchEvents();

                // UPDATE
                Assets.ResetSoundCounters();

                foreach (var e in Entities.Iterate())
                {
                    e.Update();
                }

                foreach (var e in Entities.Where(e => e.Dead).ToList())
                {
                    e.Destroyed();
                    Entities.Remove(e);
                }

                World.Step(FrameTime);

                // DRAW
                Camera.Position = Player.Position;
                Camera.Apply(Window);
                Window.Draw(background);

                foreach (var e in EntitiesInRegion(Camera.Bounds))
                {
                    e.Draw(Window);
                }

#if DEBUG
                debugView.Draw(Window);
#endif

                // DRAW HUD
                HudCamera.Apply(Window);
                Window.Draw(hud);

                Window.Display();
            }
        }

        private static IEnumerable<IEntity> EntitiesInRegion(FloatRect rect)
        {
            var min = new Vector2(rect.Left, rect.Top) / PixelsPerMeter;
            var aabb = new AABB(min, min + (new Vector2(rect.Width, rect.Height) / PixelsPerMeter));
            var result = new List<IEntity>(256);

            World.QueryAABB(f =>
            {
                result.Add((IEntity)f.Body.UserData);
                return true;
            }, ref aabb);

            return result.Distinct().OrderBy(e => e.Depth);
        }

        private static Vector2? FindOpenSpace(FloatRect area, Vector2 size)
        {
            const int maxRetry = 25;

            int i = 0;
            Vector2 position;
            bool empty;

            do
            {
                position = new Vector2(area.Left + (float)Random.NextDouble() * area.Width, area.Top + (float)Random.NextDouble() * area.Height);
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
