using System;
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

        private const float Zoom = 2;
        private const uint MinWidth = 800;
        private const uint MinHeight = 600;

        public static Random Random = new Random();
        public static RenderWindow Window;
        public static bool HasFocus { get; private set; }

        public static Camera HudCamera;
        public static Camera Camera;
        public static World World;
        public static LinkedList<Entity> Entities;
        public static Player Player;
        public static Font Font;

        public static void Main()
        {
            Window = new RenderWindow(new VideoMode(1280, 720), "", Styles.Default, new ContextSettings(0, 0, 16));
            Window.SetFramerateLimit(FrameRate);
            Window.Closed += (sender, eventArgs) => Window.Close();

            Window.GainedFocus += (sender, args) => HasFocus = true;
            Window.LostFocus += (sender, args) => HasFocus = false;
            HasFocus = true;

            Window.Resized += (sender, args) =>
            {
                if (args.Width < MinWidth || args.Height < MinHeight)
                {
                    Window.Size = new Vector2u(Math.Max(args.Width, MinWidth), Math.Max(args.Height, MinHeight));
                    return;
                }

                HudCamera = new Camera(new FloatRect(0, 0, args.Width, args.Height));
                Camera = new Camera(new FloatRect(0, 0, args.Width, args.Height));
                Camera.Zoom = Zoom;
            };

            HudCamera = new Camera(Window.DefaultView);
            Camera = new Camera(Window.DefaultView);
            Camera.Zoom = Zoom;

            Window.KeyPressed += (sender, args) =>
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
            };

            World = new World(new Vector2(0, 0));
            Entities = new LinkedList<Entity>();

            Font = new Font("Data/OpenSans-Regular.ttf");

#if DEBUG
            var debugView = new SFMLDebugView(World);
            debugView.AppendFlags(DebugViewFlags.Shape);
#endif

            #region Border
            const float radius = 30;
            const float step = Util.Pi2 / (Util.Pi2 * radius);

            for (var dir = 0f; dir <= Util.Pi2; dir += step)
            {
                var count = 1;

                while (true)
                {
                    var idx = Random.Next(Asteroid.Radiuses.Count);
                    var size = new Vector2(Asteroid.Radiuses[idx]);
                    var pos = Util.LengthDir(dir, radius).ToFarseer();
                    var space = FindOpenSpace(pos, 1, size);

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
            const int asteroids = (int)(Math.PI * (radius * radius)) / 50;

            for (var i = 0; i < asteroids; i++)
            {
                var size = new Vector2(2, 2);
                var space = FindOpenSpace(new Vector2(0), radius, size);

                if (!space.HasValue)
                    continue;

                Entities.AddLast(new Asteroid(space.Value));
            }
            #endregion

            #region Enemies
            const int enemies = 10;

            for (var i = 0; i < enemies; i++)
            {
                var size = new Vector2(2, 2);
                var space = FindOpenSpace(new Vector2(), radius, size);

                if (!space.HasValue)
                    continue;

                Entities.AddLast(new Computer(space.Value));
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

        private static IEnumerable<Entity> EntitiesInRegion(FloatRect rect)
        {
            var min = new Vector2(rect.Left, rect.Top) / PixelsPerMeter;
            var aabb = new AABB(min, min + (new Vector2(rect.Width, rect.Height) / PixelsPerMeter));
            var result = new List<Entity>(256);

            World.QueryAABB(f =>
            {
                result.Add((Entity)f.Body.UserData);
                return true;
            }, ref aabb);

            return result.Distinct().OrderBy(e => e.Depth + e.DepthBias);
        }

        private static Vector2? FindOpenSpace(Vector2 center, float radius, Vector2 size)
        {
            const int maxRetry = 25;

            int i = 0;
            Vector2 position;
            bool empty;

            do
            {
                position = center + Util.LengthDir((float)Random.NextDouble() * Util.Pi2, (float)Math.Sqrt(Random.NextDouble()) * radius).ToFarseer();
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
