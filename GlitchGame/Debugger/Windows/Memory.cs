using GlitchGame.Debugger.Widgets;
using GlitchGame.Gui.Widgets;
using LoonyVM;

namespace GlitchGame.Debugger.Windows
{
    public class Memory : DebugWindow
    {
        private Window _window;
        private HexEditor _editor;
        private Label _offset;
        private TextBox _byteText;
        private TextBox _wordText;
        private TextBox _dwordText;
        private bool _moved;

        public Memory(DebugView view)
            : base(view)
        {
            #region Widget Creation
            _window = new Window(20, 20, 81, 41, "Memory");

            _editor = new HexEditor(1, 1, 77, 36, 16);
            _editor.SelectionChanged += () => _moved = true;
            _window.Add(_editor);

            _offset = new Label(1, 38, 26, 1, "");
            _window.Add(_offset);

            var byteLabel = new Label(40, 38, 4, 1, "B");
            _window.Add(byteLabel);

            _byteText = new TextBox(42, 38, 10);
            _byteText.Changed += () =>
            {
                sbyte b;
                if (_moved || _editor.Buffer == null || !sbyte.TryParse(_byteText.Value, out b))
                    return;

                try
                {
                    var byteOffset = _editor.SelectedOffset / 2;
                    _editor.Buffer.WriteSByte(byteOffset, b);
                }
                catch { }
            };

            _window.Add(_byteText);

            var wordLabel = new Label(53, 38, 0, 1, "W");
            _window.Add(wordLabel);

            _wordText = new TextBox(55, 38, 10);
            _wordText.Changed += () =>
            {
                short w;
                if (_moved || _editor.Buffer == null || !short.TryParse(_wordText.Value, out w))
                    return;

                try
                {
                    var byteOffset = _editor.SelectedOffset / 2;
                    _editor.Buffer.WriteShort(byteOffset, w);
                }
                catch { }
            };

            _window.Add(_wordText);

            var dwordLabel = new Label(66, 38, 0, 1, "D");
            _window.Add(dwordLabel);

            _dwordText = new TextBox(68, 38, 10);
            _dwordText.Changed += () =>
            {
                int d;
                if (_moved || _editor.Buffer == null || !int.TryParse(_dwordText.Value, out d))
                    return;

                try
                {
                    var byteOffset = _editor.SelectedOffset / 2;
                    _editor.Buffer.WriteInt(byteOffset, d);
                }
                catch { }
            };

            _window.Add(_dwordText);

            view.Desktop.Add(_window);
            #endregion

            _moved = true;
        }

        public override void Reset()
        {
            _editor.Buffer = null;
            _moved = true;

            _byteText.Value = "";
            _wordText.Value = "";
            _dwordText.Value = "";
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            _editor.Buffer = Target.Vm.Memory;

            var byteOffset = _editor.SelectedOffset / 2;
            _offset.Caption = string.Format("Offset {0}=0x{0:X}", byteOffset);

            if (_moved)
            {
                try
                {
                    var b = _editor.Buffer.ReadSByte(byteOffset);
                    _byteText.Value = b.ToString("D");
                }
                catch
                {
                    _byteText.Value = "";
                }

                try
                {
                    var w = _editor.Buffer.ReadShort(byteOffset);
                    _wordText.Value = w.ToString("D");
                }
                catch
                {
                    _wordText.Value = "";
                }

                try
                {
                    var d = _editor.Buffer.ReadInt(byteOffset);
                    _dwordText.Value = d.ToString("D");
                }
                catch
                {
                    _dwordText.Value = "";
                }

                _moved = false;
            }
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

        public void Goto(int address)
        {
            _editor.SelectedOffset = address * 2;
            _window.BringToFront();
        }
    }
}
