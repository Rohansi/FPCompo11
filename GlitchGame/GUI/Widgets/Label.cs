using Texter;

namespace GlitchGame.GUI.Widgets
{
    public class Label : Widget
    {
        public string Caption;

        public Label(int x, int y, uint w, uint h, string caption)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;

            Caption = caption;
        }

        public override void Draw(ITextRenderer renderer)
        {
            var x = 0;
            var y = 0;

            foreach (var c in Caption)
            {
                if (c == '\n')
                {
                    x = 0;
                    y++;
                    continue;
                }

                renderer.Set(x, y, new Character(c, GuiSettings.Label.Foreground, GuiSettings.Label.Background));

                x++;
                if (x >= Width)
                {
                    x = 0;
                    y++;
                }

                if (y >= Height)
                    break;
            }
        }
    }
}
