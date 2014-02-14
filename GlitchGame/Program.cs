using System;
using System.Diagnostics;
using GlitchGame.States;
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
        public const int InstructionsPerSecond = 25000;

        private const float Zoom = 2;
        private const uint MinWidth = 800;
        private const uint MinHeight = 600;

        public static readonly Random Random = new Random();
        public static RenderWindow Window { get; private set; }
        public static bool HasFocus { get; private set; }

        public static Camera HudCamera { get; private set; }
        public static Camera Camera { get; private set; }
        public static Font Font { get; private set; }
        public static State State { get; private set; }

        public static float TimeScale = 1;

        public static void Main()
        {
            Window = new RenderWindow(new VideoMode(1280, 720), "", Styles.Default, new ContextSettings(0, 0, 4));
            Window.SetFramerateLimit(FrameRate);

            #region Event Handlers
            Window.Closed += (sender, eventArgs) => Window.Close();

            Window.GainedFocus += (sender, args) => HasFocus = true;
            Window.LostFocus += (sender, args) => HasFocus = false;

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

            Window.MouseButtonPressed += (sender, args) => DispatchEvent(new MouseButtonInputArgs(args.Button, true, args.X, args.Y));
            Window.MouseButtonReleased += (sender, args) => DispatchEvent(new MouseButtonInputArgs(args.Button, false, args.X, args.Y));
            Window.MouseWheelMoved += (sender, args) => DispatchEvent(new MouseWheelInputArgs(args.Delta, args.X, args.Y));
            Window.MouseMoved += (sender, args) => DispatchEvent(new MouseMoveInputArgs(args.X, args.Y));
            Window.TextEntered += (sender, args) => DispatchEvent(new TextInputArgs(args.Unicode));
            Window.KeyPressed += (sender, args) => DispatchEvent(new KeyInputArgs(args.Code, true, args.Control, args.Shift));
            Window.KeyReleased += (sender, args) => DispatchEvent(new KeyInputArgs(args.Code, false, args.Control, args.Shift));
            #endregion

            HasFocus = true;
            HudCamera = new Camera(Window.DefaultView);
            Camera = new Camera(Window.DefaultView);
            Camera.Zoom = Zoom;

            Font = new Font("Data/OpenSans-Regular.ttf");

            SetState(new Game());

            var watch = Stopwatch.StartNew();
            var accumulator = 0f;

            while (Window.IsOpen())
            {
                var dt = (float)watch.Elapsed.TotalSeconds;
                watch.Restart();
                accumulator += dt;

                dt *= TimeScale;

                Window.DispatchEvents();
                State.Update(dt);

                if (accumulator >= FrameTime)
                {
                    accumulator -= FrameTime;

                    Camera.Apply(Window);
                    State.Draw(Window);

                    HudCamera.Apply(Window);
                    State.DrawHud(Window);

                    Assets.ResetSoundCounters();
                }

                Window.Display();
            }
        }

        public static void SetState(State newState)
        {
            if (State != null)
                State.Leave();

            State = newState;
            State.Enter();
        }

        private static void DispatchEvent(InputArgs args)
        {
            if (State == null)
                return;

            State.ProcessEvent(args);
        }
    }
}
