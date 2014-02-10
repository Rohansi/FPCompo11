using System;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;
using FarseerCircleShape = FarseerPhysics.Collision.Shapes.CircleShape;
using Transform = FarseerPhysics.Common.Transform;

namespace GlitchGame
{
    public class FarseerDebugView : DebugViewBase
    {
        public Color DefaultShapeColor = new Color(229, 178, 178);
        public Color InactiveShapeColor = new Color(128, 128, 76);
        public Color KinematicShapeColor = new Color(128, 128, 229);
        public Color SleepingShapeColor = new Color(153, 153, 153);
        public Color StaticShapeColor = new Color(128, 229, 128);

        private RenderTarget _target;

        public FarseerDebugView(World world) : base(world)
        {
            
        }

        public void Draw(RenderTarget target)
        {
            _target = target;

            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                foreach (Body b in World.BodyList)
                {
                    Transform xf;
                    b.GetTransform(out xf);
                    foreach (Fixture f in b.FixtureList)
                    {
                        if (b.Enabled == false)
                        {
                            DrawShape(f, xf, InactiveShapeColor);
                        }
                        else if (b.BodyType == BodyType.Static)
                        {
                            DrawShape(f, xf, StaticShapeColor);
                        }
                        else if (b.BodyType == BodyType.Kinematic)
                        {
                            DrawShape(f, xf, KinematicShapeColor);
                        }
                        else if (b.Awake == false)
                        {
                            DrawShape(f, xf, SleepingShapeColor);
                        }
                        else
                        {
                            DrawShape(f, xf, DefaultShapeColor);
                        }
                    }
                }
            }
        }

        private void DrawShape(Fixture fixture, Transform xf, Color color)
        {
            switch (fixture.Shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        var circle = (FarseerCircleShape)fixture.Shape;

                        Vector2 center = MathUtils.Mul(ref xf, circle.Position);
                        float radius = circle.Radius;
                        Vector2 axis = xf.q.GetXAxis();

                        DrawSolidCircle(center, radius, axis, color.R, color.G, color.B);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        var poly = (PolygonShape)fixture.Shape;
                        int vertexCount = poly.Vertices.Count;
                        var vertices = new Vector2[vertexCount];

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            vertices[i] = MathUtils.Mul(ref xf, poly.Vertices[i]);
                        }

                        DrawSolidPolygon(vertices, vertexCount, color.R, color.G, color.B);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void DrawPolygon(Vector2[] vertices, int count, float red, float blue, float green, bool closed = true)
        {
            throw new NotImplementedException();
        }

        public override void DrawSolidPolygon(Vector2[] vertices, int count, float red, float blue, float green)
        {
            var col = new Color((byte)red, (byte)green, (byte)blue, 64);
            var va = new VertexArray(PrimitiveType.Quads, (uint)count);
            for (uint i = 0; i < count; i++)
            {
                va[i] = new Vertex(new Vector2f(vertices[i].X * Program.PixelsPerMeter, vertices[i].Y * Program.PixelsPerMeter), col);
            }
            _target.Draw(va);
        }

        public override void DrawCircle(Vector2 center, float radius, float red, float blue, float green)
        {
            throw new NotImplementedException();
        }

        public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float blue, float green)
        {
            var col = new Color((byte)red, (byte)green, (byte)blue, 64);
            var shape = new SFML.Graphics.CircleShape(radius * Program.PixelsPerMeter, 16);
            shape.Position = new Vector2f(center.X * Program.PixelsPerMeter, center.Y * Program.PixelsPerMeter);
            shape.Origin = new Vector2f(radius * Program.PixelsPerMeter, radius * Program.PixelsPerMeter);
            shape.FillColor = col;
            _target.Draw(shape);
        }

        public override void DrawSegment(Vector2 start, Vector2 end, float red, float blue, float green)
        {
            throw new NotImplementedException();
        }

        public override void DrawTransform(ref Transform transform)
        {
            throw new NotImplementedException();
        }
    }
}
