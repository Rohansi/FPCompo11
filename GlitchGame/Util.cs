using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;
using CircleShape = FarseerPhysics.Collision.Shapes.CircleShape;

namespace GlitchGame
{
    public static class Util
    {
        public const float Pi2 = (float)Math.PI * 2;

        public static Body CreateAsteroid(float radius)
        {
            var body = new Body(Program.World);
            body.BodyType = BodyType.Dynamic;
            body.LinearDamping = 0.5f;
            body.AngularDamping = 0.5f;

            var shape = new CircleShape(radius, 20 * radius);
            body.CreateFixture(shape);
            return body;
        }

        public static Body CreateShip(float scale = 1)
        {
            var body = new Body(Program.World);
            body.BodyType = BodyType.Dynamic;
            body.LinearDamping = 0.5f;
            body.AngularDamping = 1.0f;

            // tip
            var rect1 = new PolygonShape(PolygonTools.CreateRectangle(0.23f * scale, 0.55f * scale, new Vector2(0, -0.45f) * scale, 0), 1);

            // tail
            var rect2 = new PolygonShape(PolygonTools.CreateRectangle(0.725f * scale, 0.45f * scale, new Vector2(0, 0.55f) * scale, 0), 3);

            body.CreateFixture(rect1);
            body.CreateFixture(rect2);

            return body;
        }

        public static Body CreateBullet(Body parent, Vector2 offset, float scale = 1, OnCollisionEventHandler collisionHandler = null)
        {
            var body = new Body(Program.World);
            body.BodyType = BodyType.Dynamic;
            body.IsBullet = true;

            var shape = new CircleShape((0.15f / 4) * scale, 0);
            body.CreateFixture(shape);
            body.Position = parent.Position + parent.GetWorldVector(offset);
            body.Rotation = parent.Rotation;

            body.OnCollision += (a, b, contact) =>
            {
                if (collisionHandler != null)
                {
                    if (!collisionHandler(a, b, contact))
                        return false;
                }

                Program.World.RemoveBody(body);
                return false;
            };

            return body;
        }

        public static Vector2f ToSfml(this Vector2 vec2)
        {
            return new Vector2f(vec2.X, vec2.Y);
        }

        public static Vector2 ToFarseer(this Vector2f vec2)
        {
            return new Vector2(vec2.X, vec2.Y);
        }

        public static Vector2f Mul(this Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static Vector2f Div(this Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static Sprite Center(this Sprite sprite)
        {
            sprite.Origin = new Vector2f(sprite.Texture.Size.X / 2f, sprite.Texture.Size.Y / 2f);
            return sprite;
        }

        public static void Center(this Text text, bool horizontal = true, bool vertical = true)
        {
            text.Origin = new Vector2f();

            var bounds = text.GetGlobalBounds();
            bounds.Left -= text.Position.X;
            bounds.Top -= text.Position.Y;

            var x = 0f;
            var y = 0f;

            if (horizontal)
            {
                x = bounds.Left / text.Scale.X;
                x += (bounds.Width / text.Scale.X) / 2;
            }

            if (vertical)
            {
                y = bounds.Top / text.Scale.Y;
                y += (bounds.Height / text.Scale.Y) / 2;
            }

            text.Origin = new Vector2f(x, y);
        }

        public static float Clamp(float value, float min, float max)
        {
            return value > max ? max : (value < min ? min : value);
        }

        public static float Distance(Vector2f p1, Vector2f p2)
        {
            return (float)Math.Sqrt(((p2.X - p1.X) * (p2.X - p1.X)) + ((p2.Y - p1.Y) * (p2.Y - p1.Y)));
        }

        public static float Direction(Vector2 vec)
        {
            return (float)Math.Atan2(vec.Y, vec.X);
        }

        public static Vector2f LengthDir(float dir, float len)
        {
            return new Vector2f((float)Math.Cos(dir) * len, (float)-Math.Sin(dir) * len);
        }

        public static Vector2 RadarLengthDir(float dir, float len)
        {
            return new Vector2((float)Math.Cos(dir) * len, (float)Math.Sin(dir) * len);
        }

        public static short ToMachineRotation(float radians)
        {
            var value = (radians % (2 * Math.PI)) * ((Program.RadarRays / 2) / Math.PI);
            if (value < 0)
                value += Program.RadarRays;
            return (short)value;
        }

        public static IEnumerable<Entity> Iterate(this LinkedList<Entity> entities)
        {
            var next = entities.First;

            while (next != null)
            {
                yield return next.Value;
                next = next.Next;
            }
        }
    }
}
