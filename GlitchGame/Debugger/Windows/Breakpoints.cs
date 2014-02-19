using GlitchGame.Debugger.Widgets;
using SFML.Window;
using Window = GlitchGame.Debugger.Widgets.Window;

namespace GlitchGame.Debugger.Windows
{
    public class Breakpoints : DebugWindow
    {
        private Window _window;
        private ListView<int> _breakpointList;
         
        public Breakpoints(DebugView view)
            : base(view)
        {
            _window = new Window(15, 15, 60, 25, "Breakpoints");
            _window.Visible = false;
            View.Desktop.Add(_window);

            _breakpointList = new ListView<int>(1, 1, 56, 21);

            _breakpointList.Columns.Add(new ListView<int>.Column("Name", 43, null, false, addr =>
            {
                if (Target == null || Target.Code.DebugInfo == null)
                    return addr.ToString("X8");

                var debugInfo = Target.Code.DebugInfo;
                var symbol = debugInfo.FindSymbol(addr);
                if (!symbol.HasValue)
                    return addr.ToString("X8");

                return string.Format("{0}+0x{1}", symbol.Value.Name, addr - symbol.Value.Address);
            }));

            _breakpointList.Columns.Add(new ListView<int>.Column("Address", 11, null, false, addr => string.Format("{0,11:X8}", addr)));

            _breakpointList.Clicked += (addr, button) =>
            {
                if (Target == null)
                    return;

                if (button == Mouse.Button.Left)
                    View.Get<Cpu>().Goto(addr);
                else if (button == Mouse.Button.Right)
                    Target.RemoveBreakpoint(addr);
            };

            _window.Add(_breakpointList);
        }

        public override void Reset()
        {
            _breakpointList.Items.Clear();
        }

        public override void Update()
        {
            _breakpointList.Items.Clear();

            if (Target != null)
                _breakpointList.Items.AddRange(Target.Breakpoints);
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
