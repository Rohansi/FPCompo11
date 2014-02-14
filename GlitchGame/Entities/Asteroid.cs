using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using GlitchGame.Devices;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Entities
{
    public sealed class Asteroid : Entity
    {
        public static readonly List<float> Radiuses = new List<float> { 0.215f, 0.35f, 0.5f, 0.95f };

        public override int Depth { get { return 1; } }
        public override RadarValue RadarType { get { return RadarValue.Asteroid; } }

        private Sprite _sprite;

        public Asteroid(State state, Vector2 position, int? typeOverride = null, bool isStatic = false)
            : base(state)
        {
            var type = typeOverride.HasValue ? typeOverride.Value : Program.Random.Next(Radiuses.Count);
            var radius = Radiuses[type];

            _sprite = new Sprite(Assets.LoadTexture(string.Format("asteroid{0}.png", type))).Center();

            var c = (byte)Program.Random.Next(180, 255);
            _sprite.Color = new Color(c, c, c);

            #region Body Initialize
            Body = new Body(State.World);
            Body.BodyType = BodyType.Dynamic;
            Body.LinearDamping = 0.5f;
            Body.AngularDamping = 0.5f;
            var shape = new FarseerPhysics.Collision.Shapes.CircleShape(radius, 20 * radius);
            Body.CreateFixture(shape);

            Body.UserData = this;

            if (isStatic)
                Body.BodyType = BodyType.Static;

            Body.Position = position;
            Body.Rotation = (float)(Program.Random.NextDouble() * (Math.PI * 2));
            #endregion
        }

        public override void Update(float dt)
        {

        }

        public override void Draw(RenderTarget target)
        {
            Position = Body.Position.ToSfml() * Program.PixelsPerMeter;
            Rotation = Body.Rotation * Program.DegreesPerRadian;

            target.Draw(_sprite, new RenderStates(Transform));
        }
    }
}
