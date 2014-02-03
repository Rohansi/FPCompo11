using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame
{
    public static class Util
    {
        public const float Pi2 = (float)Math.PI * 2;

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

        public static float Direction(Vector2 vec1, Vector2 vec2)
        {
            return (float)Math.Atan2(vec2.Y - vec1.Y, vec2.X - vec1.X);
        }

        public static float Direction(Vector2 vec)
        {
            return Direction(new Vector2(0), vec);
        }

        public static Vector2f LengthDir(float dir, float len)
        {
            return new Vector2f((float)Math.Cos(dir) * len, (float)-Math.Sin(dir) * len);
        }

        public static Vector2 RadarLengthDir(float dir, float len)
        {
            return new Vector2((float)Math.Cos(dir) * len, (float)Math.Sin(dir) * len);
        }

        public static float NormalizeRotation(float radians)
        {
            radians %= Pi2;
            if (radians < 0)
                radians += Pi2;
            return radians;
        }

        public static short ToMachineRotation(float radians)
        {
            var value = (radians % Pi2) * ((Program.RadarRays / 2) / Math.PI);
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
