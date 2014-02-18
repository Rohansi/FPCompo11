using GlitchGame.Debugger.Widgets;
using SFML.Window;
using Window = GlitchGame.Debugger.Widgets.Window;

namespace GlitchGame.Debugger.Windows
{
    public class Symbols : DebugWindow
    {
        private Window _window;
        private ListView<ShipDebug.Symbol> _symbolList; 
        private bool _needUpdate;

        public Symbols(DebugView view)
            : base(view)
        {
            _window = new Window(20, 20, 60, 25, "Symbols");
            _window.Visible = false;
            View.Desktop.Add(_window);

            _symbolList = new ListView<ShipDebug.Symbol>(1, 1, 56, 21);
            _symbolList.Columns.Add(new ListView<ShipDebug.Symbol>.Column("Name", 43, s => s.Name));
            _symbolList.Columns.Add(new ListView<ShipDebug.Symbol>.Column("Address", 11, s => s.Address, true, s => string.Format("{0,11:X8}", s.Address)));

            _symbolList.Clicked += (symbol, button) =>
            {
                if (Target == null)
                    return;

                if (button == Mouse.Button.Left)
                    View.Get<Cpu>().Goto(symbol.Address);
                else if (button == Mouse.Button.Right)
                    View.Get<Memory>().Goto(symbol.Address);
            };

            _window.Add(_symbolList);
        }

        public override void Reset()
        {
            _symbolList.Items.Clear();
            _needUpdate = true;
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            if (_needUpdate)
            {
                _symbolList.Items.Clear();

                var debugInfo = Target.Code.DebugInfo;
                if (debugInfo == null)
                    return;

                _symbolList.Items.AddRange(debugInfo.Symbols);
                _needUpdate = false;
            }
        }

        public void Show()
        {
            _window.Visible = true;
            _window.Focus();
        }

        public void Hide()
        {
            _window.Visible = false;
        }
    }
}
