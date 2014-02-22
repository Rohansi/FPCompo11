using GlitchGame.Debugger.Widgets;

namespace GlitchGame.Debugger.Windows
{
    public class Target : DebugWindow
    {
        private Window _window;
        private TextBox _codeText;
        private bool _needUpdate;

        public Target(DebugView view)
            : base(view)
        {
            _window = new Window(10, 10, 30, 7, "Target");
            _window.Visible = false;
            View.Desktop.Add(_window);

            var codeError = new Label(1, 1, 26, 2, "");
            _window.Add(codeError);

            _codeText = new TextBox(1, 3, 18);
            _window.Add(_codeText);

            var codeButton = new Button(20, 3, 7, "Load");
            codeButton.Clicked += () =>
            {
                if (Target == null || _codeText.Value.Length == 0)
                    return;

                try
                {
                    Target.Load(Assets.ReloadCode(_codeText.Value));
                    codeError.Caption = "";
                    _needUpdate = true;
                }
                catch
                {
                    codeError.Caption = "Failed to load code";
                }
            };
            _window.Add(codeButton);

            // TODO: put target stats in this window

            _needUpdate = true;
        }

        public override void Reset()
        {
            _needUpdate = true;
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            if (_needUpdate)
            {
                _codeText.Value = Target.Code.FileName;
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
