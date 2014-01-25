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
        public static Body CreateAsteroid(float radius)
        {
            var body = new Body(Program.World);
            body.BodyType = BodyType.Dynamic;
            body.LinearDamping = 0.5f;
            body.AngularDamping = 1.0f;

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
            var rect1 = new PolygonShape(PolygonTools.CreateRectangle(0.25f * scale, 0.5f * scale, new Vector2(0, -0.5f) * scale, 0), 1);

            // tail
            var rect2 = new PolygonShape(PolygonTools.CreateRectangle(0.75f * scale, 0.5f * scale, new Vector2(0, 0.5f) * scale, 0), 3);

            body.CreateFixture(rect1);
            body.CreateFixture(rect2);

            return body;
        }

        public static Body CreateBullet(Body relativeTo, Vector2 offset, float speed = 10f, OnCollisionEventHandler collisionHandler = null)
        {
            var body = new Body(Program.World);
            body.BodyType = BodyType.Dynamic;
            body.IsBullet = true;

            var shape = new CircleShape(0.15f / 4, 1);
            body.CreateFixture(shape);

            body.Position = relativeTo.Position + relativeTo.GetWorldVector(offset);
            body.Rotation = relativeTo.Rotation;
            body.ApplyForce(body.GetWorldVector(new Vector2(0.0f, -speed)));

            body.OnCollision += (a, b, contact) =>
            {
                if (collisionHandler != null)
                    collisionHandler(a, b, contact);

                Program.World.RemoveBody(body);
                return false;
            };

            return body;
            /*createBullet(new Vector2(-0.575f, -0.20f));
            createBullet(new Vector2(0.575f, -0.20f));*/
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

        public static float Clamp(float value, float min, float max)
        {
            return value > max ? max : (value < min ? min : value);
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

        public static IEnumerable<IEntity> Iterate(this LinkedList<IEntity> entities)
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
