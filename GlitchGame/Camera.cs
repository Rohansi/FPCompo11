using SFML.Graphics;
using SFML.Window;

namespace GlitchGame
{
    public class Camera
    {
        /// <summary>
        /// Center point of the camera
        /// </summary>
        public Vector2f Position;

        /// <summary>
        /// Gets or sets the current zoom level of the camera
        /// </summary>
        public float Zoom
        {
            get { return View.Size.X / _originalSize.X; }
            set { View.Size = _originalSize; View.Zoom(value); }
        }

        /// <summary>
        /// Calculates the area the camera should display
        /// </summary>
        public FloatRect Bounds
        {
            get { return new FloatRect(View.Center.X - (View.Size.X / 2), View.Center.Y - (View.Size.Y / 2), View.Size.X, View.Size.Y); }
        }

        public View View { get; private set; }

        private Vector2f _originalSize;

        public Camera(FloatRect rect) : this(new View(rect)) { }

        public Camera(View view)
        {
            View = new View(view);
            Position = view.Size / 2;
            _originalSize = view.Size;
        }

        public void Apply(RenderTarget rt)
        {
            View.Center = Position;
            rt.SetView(View);
        }
    }
}
