using System;
using SFML.Window;
using Texter;

namespace GlitchGame.Gui.Widgets
{
    public class Checkbox : Widget
    {
        public string Caption;
        public bool Checked;

        public event Action Changed;

        public Checkbox(int x, int y, uint w, string caption)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = 1;

            Caption = caption;
        }

        public override void Draw(ITextRenderer renderer)
        {
            renderer.Set(0, 0, new Character('[', 0));
            renderer.Set(2, 0, new Character(']', 0));

            if (Checked)
                renderer.Set(1, 0, new Character('X', 0));
            
            renderer.DrawText(4, 0, Caption, new Character(0, 0));
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (!pressed)
                return true;

            Checked = !Checked;

            if (Changed != null)
                Changed();

            return true;
        }
    }
}
