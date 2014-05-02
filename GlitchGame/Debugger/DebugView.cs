using System;
using System.Collections.Generic;
using System.Linq;
using GlitchGame.Debugger.Widgets;
using GlitchGame.Debugger.Windows;
using GlitchGame.Devices;
using GlitchGame.Drawables;
using GlitchGame.Entities;
using SFML.Graphics;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger
{
    public sealed class DebugView : Drawable
    {
        private TextDisplay _display;
        private GuiSystem _gui;

        private State _state;
        private Computer _target;
        private TargetMarker _targetMarker;
        private VertexArray _radar;

        public readonly Container Desktop;
        public readonly List<DebugWindow> Windows;

        public DebugView()
        {
            TextDisplay.DataFolder = "Data/Texter/";

            _gui = new GuiSystem(1000, 250);

            Desktop = new Container(1000, 250);
            Desktop.Top = 1;
            _gui.Add(Desktop);

            Windows = new List<DebugWindow>
            {
                new Target(this),
                new Cpu(this),
                new Memory(this),
                new Options(this),
                new Symbols(this),
                new Breakpoints(this),
                new Watch(this),
                new Profiler(this)
            };

            var menu = new MenuBar();
            _gui.Add(menu);

            #region Title
            var title = new MenuItem("[Debugger]");
            var exit = new MenuItem("Exit");
            exit.Clicked += () => SetTarget(null);
            title.Items.Add(exit);
            menu.Items.Add(title);
            #endregion

            #region View
            var target = new MenuItem("Target");
            target.Clicked += () => Get<Target>().Show();
            menu.Items.Add(target);

            var cpu = new MenuItem("CPU");
            cpu.Clicked += () => Get<Cpu>().Show();
            menu.Items.Add(cpu);

            var memory = new MenuItem("Memory");
            memory.Clicked += () => Get<Memory>().Show();
            menu.Items.Add(memory);

            var symbols = new MenuItem("Symbols");
            symbols.Clicked += () => Get<Symbols>().Show();
            menu.Items.Add(symbols);

            var breakpoints = new MenuItem("Breakpoints");
            breakpoints.Clicked += () => Get<Breakpoints>().Show();
            menu.Items.Add(breakpoints);

            var watch = new MenuItem("Watch");
            watch.Clicked += () => Get<Watch>().Show();
            menu.Items.Add(watch);

            var profiler = new MenuItem("Profiler");
            profiler.Clicked += () => Get<Profiler>().Show();
            menu.Items.Add(profiler);

            var options = new MenuItem("Options");
            options.Clicked += () => Get<Options>().Show();
            menu.Items.Add(options);
            #endregion

            _targetMarker = new TargetMarker(1);
            _targetMarker.Color = new Color(180, 0, 0);

            _radar = new VertexArray(PrimitiveType.LinesStrip, Program.RadarRays + 1);
        }

        public T Get<T>() where T : DebugWindow
        {
            return Windows.OfType<T>().First();
        }

        public void Attach(State state)
        {
            if (_state != null)
                throw new Exception("Must detach first");

            _state = state;
            SetTarget(null);
        }

        public void Detatch()
        {
            if (_state == null)
                throw new Exception("Must attach first");

            _state = null;
            SetTarget(null);
        }

        public bool ProcessEvent(InputArgs args)
        {
            if (_target != null && _gui.ProcessEvent(args))
                return true;

            var mousePressedArgs = args as MouseButtonInputArgs;
            if (mousePressedArgs != null && mousePressedArgs.Pressed)
            {
                var mousePos = Program.Window.MapPixelToCoords(mousePressedArgs.Position, Program.Camera.View);
                var entities = _state.EntitiesInRegion(new FloatRect(mousePos.X - 32, mousePos.Y - 32, 64, 64));
                var newTarget = entities.OfType<Computer>().FirstOrDefault();

                if (_target == null)
                    SetTarget(newTarget);

                return true;
            }

            return false;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (_state == null || _target == null)
                return;

            if (_target.Dead)
            {
                SetTarget(null);
                return;
            }

            var targetPosition = _target.Body.Position.ToSfml() * Program.PixelsPerMeter;
            targetPosition = Program.HudCamera.Position + (targetPosition - Program.Camera.Position) / Program.Camera.Zoom;

            var targetSize = 80 * _target.Size / Program.Camera.Zoom;
            _targetMarker.Radius = targetSize;
            _targetMarker.Origin = new Vector2f(targetSize, targetSize);
            _targetMarker.Thickness = 3 * _target.Size;
            _targetMarker.Position = targetPosition;
            target.Draw(_targetMarker);

            var radarData = _target.Radar.RadarData;

            for (uint i = 0; i < Program.RadarRays; i++)
            {
                var dir = i * ((float)Math.PI / (Program.RadarRays / 2));
                var dist = Util.Clamp(radarData[i].Distance, 0, Radar.MaxDistanceP);
                var type = radarData[i].Type;

                var point = targetPosition + Util.RadarLengthDir(dir, dist / Program.Camera.Zoom).ToSfml();
                _radar[i] = new Vertex(point, RadarColor(type));
            }

            _radar[Program.RadarRays] = _radar[0];
            target.Draw(_radar);

            foreach (var w in Windows)
            {
                w.Update();
            }

            CheckResize();
            _display.Clear(new Character(background: 255));
            _gui.Draw(_display);

            target.Draw(_display);
        }

        private void SetTarget(Computer target)
        {
            _target = target;

            foreach (var w in Windows)
            {
                w.Reset();
                w.Target = _target;
            }
        }

        private void CheckResize()
        {
            var bounds = Program.HudCamera.Bounds;
            var width = (uint)bounds.Width / GuiSettings.CharacterWidth + 1;
            var height = (uint)bounds.Height / GuiSettings.CharacterHeight + 1;

            if (_display == null || _display.Width != width || _display.Height != height)
            {
                if (_display != null)
                    _display.Dispose();

                _display = new TextDisplay(width, height);

                GuiSettings.CharacterWidth = _display.CharacterWidth;
                GuiSettings.CharacterHeight = _display.CharacterHeight;
            }
        }

        private static Color RadarColor(RadarValue value)
        {
            switch (value)
            {
                case RadarValue.Ally:
                    return Color.Green;
                case RadarValue.Enemy:
                    return Color.Red;
                default:
                    return Color.White;
            }
        }
    }
}
