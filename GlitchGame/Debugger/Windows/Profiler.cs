using System.Collections.Generic;
using GlitchGame.Debugger.Widgets;

namespace GlitchGame.Debugger.Windows
{
    public class Profiler : DebugWindow
    {
        private class ProfilerItem
        {
            public readonly string Function;
            public int Samples;

            public ProfilerItem(string function)
            {
                Function = function;
                Samples = 1;
            }
        }

        private Window _window;
        private ListView<ProfilerItem> _profilerView;
        private Button _start;
        private Label _message;

        private bool _profiling;
        private Dictionary<string, ProfilerItem> _items;
        private int _totalSamples;

        public Profiler(DebugView view)
            : base(view)
        {
            _window = new Window(30, 30, 60, 25, "Profiler");
            _window.Visible = false;
            View.Desktop.Add(_window);

            _profilerView = new ListView<ProfilerItem>(1, 1, 56, 19);
            _profilerView.Columns.Add(new ListView<ProfilerItem>.Column("Function", 43, p => p.Function));
            _profilerView.Columns.Add(new ListView<ProfilerItem>.Column("Samples", 11, p => p.Samples, true, p => string.Format("{0,11:P}", p.Samples / (float)_totalSamples)));
            _window.Add(_profilerView);

            _start = new Button(1, 21, 10, "Start");
            _start.Clicked += () =>
            {
                if (Target == null)
                    return;

                if (!_profiling)
                    Reset();

                _profiling = !_profiling;
            };

            _window.Add(_start);

            _message = new Label(13, 21, 40, 1, "Profiling requires debug information");
            _window.Add(_message);

            _profiling = false;
            _items = new Dictionary<string, ProfilerItem>();
            _totalSamples = 0;
        }

        public override void Reset()
        {
            _profilerView.Items.Clear();
            _items.Clear();

            _profiling = false;
            _totalSamples = 0;
        }

        public override void Update()
        {
            var debugInfo = Target.Code.DebugInfo;

            _start.Caption = _profiling ? "Stop" : "Start";
            _message.Visible = debugInfo == null;

            if (Target == null || !_window.Visible || debugInfo == null || Target.Paused || !_profiling)
                return;

            // TODO: this needs to be based on TimeScale or something...

            _totalSamples++;

            var symbol = debugInfo.FindSymbol(Target.Vm.IP);
            if (!symbol.HasValue)
                return;

            var function = symbol.Value.Name.Split('.')[0];

            ProfilerItem item;
            if (_items.TryGetValue(function, out item))
            {
                item.Samples++;
            }
            else
            {
                item = new ProfilerItem(function);
                _items.Add(function, item);
                _profilerView.Items.Add(item);
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
