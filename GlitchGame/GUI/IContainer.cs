
namespace GlitchGame.GUI
{
    public interface IContainer
    {
        uint SurfaceWidth { get; }
        uint SurfaceHeight { get; }

        void Add(Widget widget);
        void Remove(Widget widget);
        void Focus(Widget widget);
        void BringToFront(Widget widget);
        void RemoveFocus();
    }
}
