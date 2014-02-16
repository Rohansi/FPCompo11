using GlitchGame.Debugger.Widgets;
using SFML.Window;
using Window = GlitchGame.Debugger.Widgets.Window;

namespace GlitchGame.Debugger.Windows
{
    public class Symbols : DebugWindow
    {
        private Window _window;
        private SymbolList _symbolList;
        private bool _needUpdate;

        public Symbols(DebugView view)
            : base(view)
        {
            _window = new Window(30, 30, 60, 20, "Symbols");
            _window.Visible = false;
            view.Desktop.Add(_window);

            _symbolList = new SymbolList(1, 1, 56, 16);
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
            _symbolList.Symbols.Clear();
            _needUpdate = true;
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            if (_needUpdate)
            {
                _symbolList.Symbols.Clear();

                var debugInfo = Target.Code.DebugInfo;
                if (debugInfo == null)
                    return;

                _symbolList.Symbols.AddRange(debugInfo.Symbols);
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
