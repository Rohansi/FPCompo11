using Texter;

namespace GlitchGame.Debugger
{
    public static class GuiSettings
    {
        public const string SolidBox = "         ";

        public static uint CharacterWidth = 8;
        public static uint CharacterHeight = 12;

        public static readonly Character Button = new Character(0, 15, 8);
        public static readonly Character ButtonShadowR = new Character(220, 25);
        public static readonly Character ButtonShadowB = new Character(223, 25);

        public static readonly Character Label = new Character(0, 0);

        public static readonly Character ListBox = new Character(0, 0);
        public static readonly Character ListBoxItem = new Character(0, 0);
        public static readonly Character ListBoxItemSelected = new Character(0, 7, 8);
        
        public static readonly Character Menu = new Character(0, 15, 7);
        public static readonly Character MenuOutline = new Character(0, 8, 7);
        public static readonly Character MenuItem = new Character(0, 0, 7);
        public static readonly Character MenuItemHighlight = new Character(0, 0, 8);
        public static readonly Character MenuArrow = new Character(16);

        public static readonly Character Scrollbar = new Character(176, 16);
        public static readonly Character ScrollbarUp = new Character(30, 16);
        public static readonly Character ScrollbarDown = new Character(31, 16);
        public static readonly Character ScrollbarThumb = new Character(219, 16);

        public static readonly Character TextBox = new Character(0, 15, 8);
        public static readonly Character TextBoxCaret = new Character('_', 15);

        public static readonly Character Window = new Character(0, 15, 7);
        public static readonly Character WindowCaption = new Character(0, 0);
    }
}
