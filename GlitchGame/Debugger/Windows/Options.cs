using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlitchGame.Gui.Widgets;

namespace GlitchGame.Debugger.Windows
{
    public class Options : DebugWindow
    {
        private Window _window;
        private TextBox _timeScaleText;

        public Options(DebugView view)
            : base(view)
        {
            #region Widget Creation
            _window = new Window(25, 25, 30, 5, "Options");
            _window.Visible = false;
            view.Desktop.Add(_window);

            var timeScaleLabel = new Label(1, 1, 13, 1, "TimeScale %:");
            _window.Add(timeScaleLabel);

            _timeScaleText = new TextBox(14, 1, 13);
            _timeScaleText.Changed += () =>
            {
                int v;
                if (int.TryParse(_timeScaleText.Value, out v))
                {
                    var oldTimeScale = (int)Math.Round(Program.TimeScale * 100);
                    Program.TimeScale = Util.Clamp(v / 100f, 0, 5);

                    if (v != oldTimeScale)
                        _timeScaleText.Value = ((int)Math.Round(Program.TimeScale * 100)).ToString("D");
                }
                else if (_timeScaleText.Value.Length > 0)
                {
                    _timeScaleText.Value = _timeScaleText.Value.Where(char.IsDigit).Aggregate("", (s, c) => s + c);
                }
            };

            _window.Add(_timeScaleText);
            #endregion
        }

        public override void Reset()
        {
            Program.TimeScale = 1;
            _timeScaleText.Value = ((int)Math.Round(Program.TimeScale * 100)).ToString("D");
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;


        }

        public override void Show()
        {
            _window.Visible = true;
            _window.Focus();
        }

        public override void Hide()
        {
            _window.Visible = false;
        }
    }
}
