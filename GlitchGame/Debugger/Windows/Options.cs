using System;
using GlitchGame.Debugger.Widgets;

namespace GlitchGame.Debugger.Windows
{
    public class Options : DebugWindow
    {
        private Window _window;
        private NumericTextBox _timeScaleText;

        public Options(DebugView view)
            : base(view)
        {
            #region Widget Creation
            _window = new Window(2, 2, 30, 5, "Options");
            _window.Visible = false;
            view.Desktop.Add(_window);

            var timeScaleLabel = new Label(1, 1, 13, 1, "TimeScale %:");
            _window.Add(timeScaleLabel);

            _timeScaleText = new NumericTextBox(14, 1, 13);
            _timeScaleText.Minimum = 0;
            _timeScaleText.Maximum = 200;
            _timeScaleText.Changed += () => Program.TimeScale = _timeScaleText.Value / 100f;

            _window.Add(_timeScaleText);
            #endregion
        }

        public override void Reset()
        {
            Program.TimeScale = 1;
            _timeScaleText.Value = (int)Math.Round(Program.TimeScale * 100);
        }

        public override void Update()
        {
            
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
