using GlitchGame.Entities;

namespace GlitchGame.Debugger
{
    public abstract class DebugWindow
    {
        public Computer Target { get; set; }

        /// <summary>
        /// Called before switching targets
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Called before rendering the UI
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Called when the window should be shown
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Called when the window should be hidden
        /// </summary>
        public abstract void Hide();
    }
}
