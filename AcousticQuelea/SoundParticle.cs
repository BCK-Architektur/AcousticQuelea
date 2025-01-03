using Rhino.Geometry;
using System;

namespace AcousticQuelea
{
    public class SoundParticle
    {
        public Point3d Position { get; private set; }
        public Vector3d Velocity { get; private set; }
        public double Frequency { get; private set; }
        public double Intensity { get; private set; }
        public int Lifespan { get; private set; }

        private static readonly Random random = new Random();

        public SoundParticle(Point3d position, double frequency)
        {
            Position = position;
            Frequency = frequency;
            Intensity = 1.0;
            Lifespan = 200;
            Velocity = GetRandomDirection() * 2.0;
        }

        public void Move(Brep environment)
        {
            Position += Velocity;
            Intensity -= 0.005;

            if (environment != null)
            {
                ReflectIfHit(environment);
            }

            Lifespan--;
        }

        private void ReflectIfHit(Brep environment)
        {
            Vector3d normal;
            var closestPoint = environment.ClosestPoint(Position, out Point3d pointOnBrep, out ComponentIndex ci, out double u, out double v, 1.0, out normal);

            // Increase the collision detection threshold
            if (closestPoint && ci.ComponentIndexType == ComponentIndexType.BrepFace)
            {
                double collisionThreshold = 0.5;
                if (Position.DistanceTo(pointOnBrep) < collisionThreshold)
                {
                    Velocity = Reflect(Velocity, normal);
                    Intensity *= 0.9;
                }
            }
        }



        private Vector3d Reflect(Vector3d vector, Vector3d normal)
        {
            return vector - 2 * (vector * normal) * normal;
        }

        private Vector3d GetRandomDirection()
        {
            double x = random.NextDouble() * 2 - 1;
            double y = random.NextDouble() * 2 - 1;
            double z = random.NextDouble() * 2 - 1;
            var direction = new Vector3d(x, y, z);
            direction.Unitize();
            return direction;
        }

        public bool IsAlive()
        {
            return Intensity > 0 && Lifespan > 0;
        }
    }
}
