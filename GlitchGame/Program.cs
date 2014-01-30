using System;
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
        public const int InstructionsPerSecond = 10000;
        public const int InstructionsPerFrame = (int)(FrameTime * InstructionsPerSecond);

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

            Font = new Font("Data/OpenSans-Regular.ttf");

            SetState(new Game());

            while (Window.IsOpen())
            {
                // INPUT
                Window.DispatchEvents();

                // UPDATE
                Assets.ResetSoundCounters();
                State.Update();

                // DRAW
                Camera.Apply(Window);
                State.Draw(Window);

                // DRAW HUD
                HudCamera.Apply(Window);
                State.DrawHud(Window);

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
    }
}
