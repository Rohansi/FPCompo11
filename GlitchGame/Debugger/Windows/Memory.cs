using GlitchGame.Debugger.Widgets;
using LoonyVM;

namespace GlitchGame.Debugger.Windows
{
    public class Memory : DebugWindow
    {
        private Window _window;
        private HexEditor _editor;
        private Label _offset;
        private NumericTextBox _byteText;
        private NumericTextBox _wordText;
        private NumericTextBox _dwordText;

        public Memory(DebugView view)
            : base(view)
        {
            #region Widget Creation
            _window = new Window(15, 15, 81, 41, "Memory");
            _window.Visible = false;
            View.Desktop.Add(_window);

            _editor = new HexEditor(1, 1, 77, 36, 16);
            _window.Add(_editor);

            _offset = new Label(1, 38, 26, 1, "");
            _window.Add(_offset);

            var byteLabel = new Label(40, 38, 4, 1, "B");
            _window.Add(byteLabel);

            _byteText = new NumericTextBox(42, 38, 10);
            _byteText.Minimum = sbyte.MinValue;
            _byteText.Maximum = sbyte.MaxValue;
            _byteText.Changed += () =>
            {
                if (_editor.Buffer == null)
                    return;

                try
                {
                    var byteOffset = _editor.SelectedOffset / 2;
                    _editor.Buffer.WriteSByte(byteOffset, (sbyte)_byteText.Value);
                }
                catch { }
            };

            _window.Add(_byteText);

            var wordLabel = new Label(53, 38, 0, 1, "W");
            _window.Add(wordLabel);

            _wordText = new NumericTextBox(55, 38, 10);
            _wordText.Minimum = short.MinValue;
            _wordText.Maximum = short.MaxValue;
            _wordText.Changed += () =>
            {
                if (_editor.Buffer == null)
                    return;

                try
                {
                    var byteOffset = _editor.SelectedOffset / 2;
                    _editor.Buffer.WriteShort(byteOffset, (short)_wordText.Value);
                }
                catch { }
            };

            _window.Add(_wordText);

            var dwordLabel = new Label(66, 38, 0, 1, "D");
            _window.Add(dwordLabel);

            _dwordText = new NumericTextBox(68, 38, 10);
            _dwordText.Minimum = int.MinValue;
            _dwordText.Maximum = int.MaxValue;
            _dwordText.Changed += () =>
            {
                if (_editor.Buffer == null)
                    return;

                try
                {
                    var byteOffset = _editor.SelectedOffset / 2;
                    _editor.Buffer.WriteInt(byteOffset, _dwordText.Value);
                }
                catch { }
            };

            _window.Add(_dwordText);
            #endregion
        }

        public override void Reset()
        {
            _editor.Buffer = null;

            _byteText.Value = 0;
            _wordText.Value = 0;
            _dwordText.Value = 0;
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            _editor.Buffer = Target.Vm.Memory;

            var byteOffset = _editor.SelectedOffset / 2;
            _offset.Caption = string.Format("Offset {0}=0x{0:X}", byteOffset);

            if (!_byteText.Focussed)
            {
                try
                {
                    _byteText.Value = _editor.Buffer.ReadSByte(byteOffset);
                }
                catch
                {
                    _byteText.Value = 0;
                }
            }

            if (!_wordText.Focussed)
            {
                try
                {
                    _wordText.Value = _editor.Buffer.ReadShort(byteOffset);
                }
                catch
                {
                    _wordText.Value = 0;
                }
            }

            if (!_dwordText.Focussed)
            {
                try
                {
                    _dwordText.Value = _editor.Buffer.ReadInt(byteOffset);
                }
                catch
                {
                    _dwordText.Value = 0;
                }
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

        public void Goto(int address)
        {
            _editor.SelectedOffset = address * 2;
            Show();
        }
    }
}
