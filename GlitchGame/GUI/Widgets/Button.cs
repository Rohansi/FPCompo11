using System;
using SFML.Window;
using Texter;

namespace GlitchGame.Gui.Widgets
{
    public class Button : Widget
    {
        private bool _holding;

        public string Caption;
        public event Action Clicked;

        public Button(int x, int y, int w, string caption)
        {
            Left = x;
            Top = y;
            Width = (uint)w;
            Height = 2;

            Caption = caption;
        }

        public override void Draw(ITextRenderer renderer)
        {
            var captionX = (int)(Width - 1) / 2 - Caption.Length / 2;

            if (!_holding)
            {
                renderer.DrawBox(0, 0, Width - 1, 1, GuiSettings.SolidBox, GuiSettings.Button);
                renderer.Set((int)Width - 1, 0, GuiSettings.ButtonShadowR);

                for (var i = 0; i < Width; i++)
                {
                    renderer.Set(1 + i, 1, GuiSettings.ButtonShadowB);
                }

                renderer.DrawText(captionX, 0, Caption, GuiSettings.Button);
            }
            else
            {
                renderer.DrawBox(1, 0, Width, 1, GuiSettings.SolidBox, GuiSettings.Button);
                renderer.DrawText(captionX + 1, 0, Caption, GuiSettings.Button);
            }
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (button != Mouse.Button.Left)
                return true;

            if (pressed)
            {
                _holding = true;
            }
            else
            {
                if (x >= 0 && y >= 0 && x < Width && y < Height)
                {
                    if (_holding && Clicked != null)
                        Clicked();
                }

                _holding = false;
            }

            return true;
        }
    }
}
