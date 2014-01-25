using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Entities
{
    public class Asteroid : Transformable, IEntity
    {
        public static readonly List<float> Radiuses = new List<float> { 0.25f, 0.35f, 0.5f, 1.0f };

        public int DrawOrder { get { return 1; } }
        public byte RadarType { get { return 0; } }
        public bool Dead { get; private set; }

        private Sprite _sprite;
        private Body _body;

        public Asteroid(Vector2 position, int? typeOverride = null, bool isStatic = false)
        {
            var type = typeOverride.HasValue ? typeOverride.Value : Program.Random.Next(Radiuses.Count);
            var radius = Radiuses[type];

            _sprite = new Sprite(Assets.LoadTexture(string.Format("asteroid{0}.png", type))).Center();

            _body = Util.CreateAsteroid(radius);
            _body.UserData = this;

            if (isStatic)
                _body.BodyType = BodyType.Static;

            _body.Position = position;
            _body.Rotation = (float)(Program.Random.NextDouble() * (Math.PI * 2));
        }

        public void Destroyed()
        {
            Program.World.RemoveBody(_body);
        }

        public void Update()
        {

        }

        public void Draw(RenderTarget target)
        {
            Position = _body.Position.ToSfml() * Program.PixelsPerMeter;
            Rotation = _body.Rotation * Program.DegreesPerRadian;

            target.Draw(_sprite, new RenderStates(Transform));
        }
    }
}
