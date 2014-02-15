using SFML.Window;
using Texter;

namespace GlitchGame.Gui
{
    public abstract class Widget
    {
        public IContainer Parent { get; private set; }

        public bool Visible = true;
        public bool Focussed = false;

        public int Left, Top;
        public uint Width { get; protected set; }
        public uint Height { get; protected set; }

        public virtual void Initialize(IContainer parent)
        {
            Parent = parent;
        }

        public virtual void Draw(ITextRenderer renderer)
        {
            
        }

        public virtual bool KeyPressed(Keyboard.Key key, string text)
        {
            return false;
        }

        public virtual bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            return false;
        }

        public virtual void MouseMoved(int x, int y)
        {
            
        }

        public virtual bool MouseWheelMoved(int x, int y, int delta)
        {
            return false;
        }

        public void Focus()
        {
            if (Parent != null)
                Parent.Focus(this);
        }

        public void BringToFront()
        {
            if (Parent != null)
                Parent.BringToFront(this);
        }
    }
}
