using GlitchGame.Entities;

namespace GlitchGame.Debugger
{
    public abstract class DebugWindow
    {
        public DebugView View { get; protected set; }
        public Computer Target { get; set; }

        protected DebugWindow(DebugView view)
        {
            View = view;
        }

        /// <summary>
        /// Called before switching targets
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Called before rendering the UI
        /// </summary>
        public abstract void Update();
    }
}
