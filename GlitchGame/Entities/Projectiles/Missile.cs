using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using GlitchGame.Devices;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Entities.Projectiles
{
    public class Missile : Projectile
    {
        public override bool DirectionalRotation { get { return true; } }

        public override RadarValue RadarType { get { return RadarValue.Bullet; } }

        private Entity _target;

        public Missile(Ship parent, Vector2 offset, Vector2 speed, string texture)
            : base(parent, texture, parent.Size)
        {
            #region Body Initialize
            Body = new Body(State.World);
            Body.BodyType = BodyType.Dynamic;
            Body.IsBullet = true;
            Body.LinearDamping = 1;
            Body.AngularDamping = 1;

            var size = parent.Size;
            var shape = new PolygonShape(PolygonTools.CreateRectangle(0.05f * size, 0.2f * size, new Vector2(0) * size, 0), 1);
            Body.CreateFixture(shape);
            Body.Position = parent.Body.Position + parent.Body.GetWorldVector(offset);
            Body.Rotation = parent.Body.Rotation;

            Body.OnCollision += (a, b, contact) =>
            {
                if (b.Body.UserData == Parent)
                    return false;

                if (b.Body.UserData is Projectile) // TODO: damage
                    return false;

                if (b.Body.UserData is Entity)
                    Hit((Entity)b.Body.UserData);

                Dead = true;
                return false;
            };

            Body.LinearVelocity = Parent.Body.LinearVelocity + Parent.Body.GetWorldVector(speed);
            Body.UserData = this;
            #endregion

            var start = Body.Position;
            var point = start + Util.RadarLengthDir(Body.Rotation - (Util.Pi2 / 4), 50);
            var min = 100f;
            Entity entity = null;

            // TODO: use multiple rays
            parent.State.World.RayCast((f, p, n, fr) =>
            {
                if (f.Body == parent.Body || f.Body.UserData is Bullet)
                    return -1;

                if (fr > min)
                    return 1;

                min = fr;
                entity = (Entity)f.Body.UserData;
                return fr;
            }, start, point);

            if (entity is Ship)
                _target = entity;
        }

        public void Hit(Entity ship)
        {
            const float radius = 300;
            const float radiusM = radius / Program.PixelsPerMeter;
            const float power = 2.5f;

            var entities = Parent.State.EntitiesInRegion(new FloatRect(Position.X - radius, Position.Y - radius, radius * 2, radius * 2));

            foreach (var e in entities)
            {
                if (e is Projectile)
                    continue;
                
                var body = e.Body;
                var center = Body.Position;
                var apply = e.Body.WorldCenter;

                if ((apply - center).Length() >= radiusM)
                    continue;

                var dir = apply - center;
                var dist = dir.Length();
                if (dist <= 0)
                    continue;

                var invDistance = 1 / dist;
                var impulseMag = Size * power * invDistance * invDistance;
                body.ApplyLinearImpulse(impulseMag * dir, apply);
            }
        }

        public override void Update(float dt)
        {
            if (_target != null)
            {
                var targetDir = Util.NormalizeRotation(Util.Direction(Body.Position, _target.Body.Position));
                var heading = Util.NormalizeRotation(Body.Rotation - (Util.Pi2 / 4));

                var dir = 1;
                var diff = heading - targetDir;
                if (diff > 0 ^ Math.Abs(diff) > (float)Math.PI)
                    dir = -1;
                
                var angularSpeed = 10f * Body.Inertia;
                Body.ApplyTorque(dir * angularSpeed);
            }

            var linearSpeed = 15f * Body.Mass;
            Body.ApplyForce(Body.GetWorldVector(new Vector2(0.0f, -linearSpeed)));

            base.Update(dt);
        }
    }
}
