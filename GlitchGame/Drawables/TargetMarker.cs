using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Drawables
{
    public class TargetMarker : Transformable, Drawable
    {
        private bool _dirty;
        private VertexArray _vertices;

        private float _radius;
        private float _thickness;
        private Color _color;

        public TargetMarker(float radius)
        {
            _dirty = true;
            _vertices = new VertexArray(PrimitiveType.Quads, 32);
            _radius = radius;
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _dirty = true;
            }
        }

        public float Thickness
        {
            get { return _thickness; }
            set
            {
                _thickness = value;
                _dirty = true;
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                _dirty = true;
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (_dirty)
                Rebuild();

            states.Transform *= Transform;
            target.Draw(_vertices, states);
        }

        private void Rebuild()
        {
            var edgeWidth = (_radius * 2) / 3;

            _vertices[00] = new Vertex(new Vector2f(0, 0), _color);
            _vertices[01] = new Vertex(new Vector2f(edgeWidth, 0), _color);
            _vertices[02] = new Vertex(new Vector2f(edgeWidth, _thickness), _color);
            _vertices[03] = new Vertex(new Vector2f(0, _thickness), _color);

            _vertices[04] = new Vertex(new Vector2f(0, 0), _color);
            _vertices[05] = new Vertex(new Vector2f(_thickness, 0), _color);
            _vertices[06] = new Vertex(new Vector2f(_thickness, edgeWidth), _color);
            _vertices[07] = new Vertex(new Vector2f(0, edgeWidth), _color);

            _vertices[08] = new Vertex(new Vector2f(edgeWidth * 2, 0), _color);
            _vertices[09] = new Vertex(new Vector2f(edgeWidth * 3, 0), _color);
            _vertices[10] = new Vertex(new Vector2f(edgeWidth * 3, _thickness), _color);
            _vertices[11] = new Vertex(new Vector2f(edgeWidth * 2, _thickness), _color);

            _vertices[12] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, 0), _color);
            _vertices[13] = new Vertex(new Vector2f(edgeWidth * 3, 0), _color);
            _vertices[14] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth), _color);
            _vertices[15] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, edgeWidth), _color);

            _vertices[16] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, edgeWidth * 2), _color);
            _vertices[17] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 2), _color);
            _vertices[18] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 3), _color);
            _vertices[19] = new Vertex(new Vector2f(edgeWidth * 3 - _thickness, edgeWidth * 3), _color);

            _vertices[20] = new Vertex(new Vector2f(edgeWidth * 2, edgeWidth * 3 - _thickness), _color);
            _vertices[21] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 3 - _thickness), _color);
            _vertices[22] = new Vertex(new Vector2f(edgeWidth * 3, edgeWidth * 3), _color);
            _vertices[23] = new Vertex(new Vector2f(edgeWidth * 2, edgeWidth * 3), _color);

            _vertices[24] = new Vertex(new Vector2f(0, edgeWidth * 2), _color);
            _vertices[25] = new Vertex(new Vector2f(_thickness, edgeWidth * 2), _color);
            _vertices[26] = new Vertex(new Vector2f(_thickness, edgeWidth * 3), _color);
            _vertices[27] = new Vertex(new Vector2f(0, edgeWidth * 3), _color);

            _vertices[28] = new Vertex(new Vector2f(0, edgeWidth * 3 - _thickness), _color);
            _vertices[29] = new Vertex(new Vector2f(edgeWidth, edgeWidth * 3 - _thickness), _color);
            _vertices[30] = new Vertex(new Vector2f(edgeWidth, edgeWidth * 3), _color);
            _vertices[31] = new Vertex(new Vector2f(0, edgeWidth * 3), _color);

            _dirty = false;
        }
    }
}
