using System;
using System.Linq;
using GlitchGame.Devices;
using GlitchGame.Entities;
using SFML.Graphics;
using SFML.Window;
using Texter;

namespace GlitchGame
{
    public class DebugView : Transformable, Drawable
    {
        private const int Width = 60;
        private const int Height = 17;

        private State _state;
        private TextDisplay _display;
        private VertexArray _radar;
        private Computer _target;

        public DebugView()
        {
            TextDisplay.DataFolder = "Data/Texter/";
            _display = new TextDisplay(Width, Height);
            _radar = new VertexArray(PrimitiveType.LinesStrip, Program.RadarRays + 1);

            Origin = new Vector2f(Width * 8, Height * 12);
        }

        public void Attach(State state)
        {
            _state = state;

            Program.Window.MouseButtonPressed += MousePressed;
        }

        public void Detatch()
        {
            Program.Window.MouseButtonPressed -= MousePressed;

            _state = null;
            _target = null;
        }

        // TODO: fix this hardcoded mess
        private void DrawDebugView()
        {
            var color = new Character(foreground: 15);
            var machine = _target.Vm;
            var debugInfo = _target.Code.DebugInfo;

            #region Registers
            for (var i = 0; i <= 12; i++)
            {
                _display.DrawText(2, 1 + i, string.Format("{0} = {1:X8} {1,11}", RegisterNames[i], machine.Registers[i]), color);
            }

            var symbol = debugInfo == null ? null : debugInfo.FindSymbol(machine.IP);
            var symbolLine = debugInfo == null ? null : debugInfo.FindLine(machine.IP);
            string prettyIp;

            if (symbol.HasValue)
                prettyIp = string.Format("{0}+{1:X}", symbol.Value.Name, machine.IP - symbol.Value.Address);
            else
                prettyIp = string.Format("{0:X8}", machine.IP);

            if (symbolLine.HasValue)
                prettyIp += string.Format(" <{0}:{1}>", symbolLine.Value.FileName, symbolLine.Value.LineNumber);

            _display.DrawText(2, 15, prettyIp, color);
            #endregion

            #region Stats
            _display.DrawText(30, 1, string.Format("HP = {0} / {1}", _target.Health, _target.MaxHealth), color);
            _display.DrawText(30, 2, string.Format("EP = {0} / {1}", _target.Energy, _target.MaxEnergy), color);
            #endregion

            #region Radar
            var radarData = _target.Radar.RadarData;
            var center = new Vector2f(350, 107);

            for (uint i = 0; i < Program.RadarRays; i++)
            {
                var dir = i * ((float)Math.PI / (Program.RadarRays / 2));
                var dist = (radarData[i] >> 8) / 126f;
                var type = (RadarValue)(radarData[i] & 0xFF);

                var point = center + Util.RadarLengthDir(dir, 64 * dist).ToSfml();
                _radar[i] = new Vertex(point, RadarColor(type));
            }

            _radar[Program.RadarRays] = _radar[0];

            #endregion
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (_state == null)
                return;

            if (_target == null || _target.Dead)
            {
                _target = null;
                return;
            }

            _display.Clear(new Character(background: 255));

            DrawDebugView();

            states.Transform *= Transform;
            target.Draw(_display, states);

            target.Draw(_radar, states);
        }

        private void MousePressed(object sender, MouseButtonEventArgs args)
        {
            var pos = Program.Window.MapPixelToCoords(new Vector2i(args.X, args.Y), Program.Camera.View);
            var ents = _state.EntitiesInRegion(new FloatRect(pos.X - 32, pos.Y - 32, 64, 64));
            _target = ents.OfType<Computer>().FirstOrDefault();
        }

        private static readonly string[] RegisterNames =
        {
            "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9",
            "BP", "SP", "IP"
        };

        private Color RadarColor(RadarValue value)
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
