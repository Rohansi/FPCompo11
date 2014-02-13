using SFML.Window;

namespace GlitchGame.Gui
{
    public class GuiSystem : Container
    {
        public GuiSystem(uint w, uint h)
            : base(w, h)
        {
            
        }

        public bool ProcessEvent(InputArgs args)
        {
            var keyInputArgs = args as KeyInputArgs;
            if (keyInputArgs != null)
            {
                if (!keyInputArgs.Pressed)
                    return false;

                return KeyPressed(keyInputArgs.Key, null);
            }

            var textInputArgs = args as TextInputArgs;
            if (textInputArgs != null)
            {
                return KeyPressed(Keyboard.Key.Unknown, textInputArgs.Text);
            }

            var mouseButtonArgs = args as MouseButtonInputArgs;
            if (mouseButtonArgs != null)
            {
                if (mouseButtonArgs.Pressed)
                    RemoveFocus();

                var mousePos = ConvertCoords(mouseButtonArgs.Position);
                return MousePressed(mousePos.X, mousePos.Y, mouseButtonArgs.Button, mouseButtonArgs.Pressed);
            }

            var mouseMoveArgs = args as MouseMoveInputArgs;
            if (mouseMoveArgs != null)
            {
                var mousePos = ConvertCoords(mouseMoveArgs.Position);
                MouseMoved(mousePos.X, mousePos.Y);
            }

            return false;
        }

        private Vector2i ConvertCoords(Vector2i mouseCoords)
        {
            var pos = Program.Window.MapPixelToCoords(mouseCoords, Program.HudCamera.View);
            return new Vector2i((int)(pos.X / GuiSettings.CharacterWidth), (int)(pos.Y / GuiSettings.CharacterHeight));
        }
    }
}
