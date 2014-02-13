using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlitchGame.Debugger.Widgets;
using GlitchGame.Gui.Widgets;

namespace GlitchGame.Debugger.Windows
{
    public class Memory : DebugWindow
    {
        private Window _window;
        private HexEditor _editor;

        public Memory(DebugView view)
            : base(view)
        {
            #region Widget Creation
            _window = new Window(20, 20, 81, 40, "Memory");

            _editor = new HexEditor(1, 1, 77, 36, 16);
            _window.Add(_editor);

            view.Desktop.Add(_window);
            #endregion
        }

        public override void Reset()
        {
            _editor.Reset();
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            _editor.Update(Target.Vm.Memory);
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
