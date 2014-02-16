using System;
using System.Collections.Generic;
using System.Linq;
using GlitchGame.Debugger.Windows;
using GlitchGame.Entities;
using GlitchGame.Gui;
using GlitchGame.Gui.Widgets;
using SFML.Graphics;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger
{
    public class DebugView : Drawable
    {
        private TextDisplay _display;
        private GuiSystem _gui;

        private State _state;
        private Computer _target;
        private TargetMarker _targetMarker;

        public readonly Container Desktop;
        public readonly List<DebugWindow> Windows;

        public DebugView()
        {
            TextDisplay.DataFolder = "Data/Texter/";

            _gui = new GuiSystem(1000, 250);

            Desktop = new Container(1000, 250);
            Desktop.Top = 1;
            _gui.Add(Desktop);

            Windows = new List<DebugWindow>();
            Windows.Add(new Cpu(this));
            Windows.Add(new Memory(this));
            Windows.Add(new Options(this));
            Windows.Add(new Symbols(this));

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
            var view = new MenuItem("View");
            menu.Items.Add(view);

            var cpu = new MenuItem("CPU");
            cpu.Clicked += () => Get<Cpu>().Show();
            view.Items.Add(cpu);

            var mem = new MenuItem("Memory");
            mem.Clicked += () => Get<Memory>().Show();
            view.Items.Add(mem);

            var sym = new MenuItem("Symbols");
            sym.Clicked += () => Get<Symbols>().Show();
            view.Items.Add(sym);
            #endregion

            #region Options
            var options = new MenuItem("Options");
            options.Clicked += () => Get<Options>().Show();
            menu.Items.Add(options);
            #endregion

            _targetMarker = new TargetMarker(1);
            _targetMarker.Color = new Color(180, 0, 0);
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
            var targetSize = 80 * _target.Size / Program.Camera.Zoom;
            _targetMarker.Radius = targetSize;
            _targetMarker.Origin = new Vector2f(targetSize, targetSize);
            _targetMarker.Thickness = 5 * _target.Size;
            _targetMarker.Position = Program.HudCamera.Position + (targetPosition - Program.Camera.Position) / Program.Camera.Zoom;
            target.Draw(_targetMarker);

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
    }

    public class TargetMarker : Transformable, Drawable
    {
        private bool _dirty;
        private VertexArray _vertices;

        private float _radius;
        private float _thickness;
        private Color _color;

        public TargetMarker(float radius)
        {
            _dirty = true;
            _vertices = new VertexArray(PrimitiveType.Quads, 32);
            _radius = radius;
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _dirty = true;
            }
        }

        public float Thickness
        {
            get { return _thickness; }
            set
            {
                _thickness = value;
                _dirty = true;
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                _dirty = true;
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (_dirty)
                Rebuild();

            states.Transform *= Transform;
            target.Draw(_vertices, states);
        }

        private void Rebuild()
        {
            var edgeWidth = (_radius * 2) / 3;

            _vertices[00] = new Vertex(new Vector2f(0, 0), _color);
            _vertices[01] = new Vertex(new Vector2f(edgeWidth, 0), _color);
            _vertices[02] = new Vertex(new Vector2f(edgeWidth, _thickness), _color);
            _vertices[03] = new Vertex(new Vector2f(0, _thickness), _color);

            _vertices[04] = new Vertex(new Vector2f(0, 0), _color);
            _vertices[05] = new Vertex(new Vector2f(_thickness, 0), _color);
            _vertices[06] = new Vertex(new Vector2f(_thickness, edgeWidth), _color);
            _vertices[07] = new Vertex(new Vector2f(0, edgeWidth), _color);

            _vertices[08] = new Vertex(new Vector2f(edgeWidth * 2, 0), _color);
            _vertices[09] = new Vertex(new Vector2f(edgeWidth * 3, 0), _color);
            _vertices[10] = new Vertex(new Vector2f(edgeWidth * 3, _thickness), _color);
            _vertices[11] = new Vertex(new Vector2f(edgeWidth * 2, _thickness), _color);

            _vertices[12] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, 0), _color);
            _vertices[13] = new Vertex(new Vector2f(edgeWidth * 3, 0), _color);
            _vertices[14] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth), _color);
            _vertices[15] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, edgeWidth), _color);

            _vertices[16] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, edgeWidth * 2), _color);
            _vertices[17] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 2), _color);
            _vertices[18] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 3), _color);
            _vertices[19] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, edgeWidth * 3), _color);

            _vertices[20] = new Vertex(new Vector2f(edgeWidth * 2, edgeWidth * 3 - _thickness), _color);
            _vertices[21] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 3 - _thickness), _color);
            _vertices[22] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 3), _color);
            _vertices[23] = new Vertex(new Vector2f(edgeWidth * 2, edgeWidth * 3), _color);

            _vertices[24] = new Vertex(new Vector2f(0, edgeWidth * 2), _color);
            _vertices[25] = new Vertex(new Vector2f(_thickness, edgeWidth * 2), _color);
            _vertices[26] = new Vertex(new Vector2f(_thickness, edgeWidth * 3), _color);
            _vertices[27] = new Vertex(new Vector2f(0, edgeWidth * 3), _color);

            _vertices[28] = new Vertex(new Vector2f(0, edgeWidth * 3 - _thickness), _color);
            _vertices[29] = new Vertex(new Vector2f(edgeWidth, edgeWidth * 3 - _thickness), _color);
            _vertices[30] = new Vertex(new Vector2f(edgeWidth, edgeWidth * 3), _color);
            _vertices[31] = new Vertex(new Vector2f(0, edgeWidth * 3), _color);

            _dirty = false;
        }
    }
}
