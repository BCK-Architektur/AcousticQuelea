using Rhino.Geometry;
using System;
using System.Linq;

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
        private const double EdgeThreshold = 0.3;  // Edge detection threshold
        private const double ContainmentFactor = 0.5;

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
                AvoidEdges(environment);  // Proactively steer away from edges
                ReflectIfHit(environment);
                BounceContain(environment);  // Bounce back if near boundary
            }

            Lifespan--;
        }

        // Reflect particles if they collide with Brep faces
        private void ReflectIfHit(Brep environment)
        {
            Vector3d normal;
            var closestPoint = environment.ClosestPoint(Position, out Point3d pointOnBrep, out ComponentIndex ci, out double u, out double v, 1.0, out normal);

            // Detect face collisions
            if (closestPoint && ci.ComponentIndexType == ComponentIndexType.BrepFace)
            {
                if (Position.DistanceTo(pointOnBrep) < 0.5)
                {
                    Velocity = Reflect(Velocity, normal);
                    Intensity *= 0.9;
                }
            }
        }

        // Contain particles within Brep bounding box to avoid escape
        private void BounceContain(Brep environment)
        {
            BoundingBox bounds = environment.GetBoundingBox(true);
            Point3d pos = Position;
            Vector3d vel = Velocity;

            if (pos.X >= bounds.Max.X || pos.X <= bounds.Min.X)
                vel.X *= -1;
            if (pos.Y >= bounds.Max.Y || pos.Y <= bounds.Min.Y)
                vel.Y *= -1;
            if (pos.Z >= bounds.Max.Z || pos.Z <= bounds.Min.Z)
                vel.Z *= -1;

            Velocity = vel;
        }

        // Avoid naked edges by detecting proximity and steering particles away
        private void AvoidEdges(Brep environment)
        {
            BrepEdge[] edges = environment.Edges.ToArray();

            foreach (var edge in edges)
            {
                if (edge.Valence == EdgeAdjacency.Naked)  // Check only naked edges
                {
                    double t;
                    edge.ClosestPoint(Position, out t);  // Get the closest point parameter
                    Point3d closestEdgePoint = edge.PointAt(t);  // Evaluate the point at t

                    double distance = Position.DistanceTo(closestEdgePoint);

                    if (distance < EdgeThreshold)  // If particle is too close to the edge
                    {
                        Vector3d away = Position - closestEdgePoint;
                        away.Unitize();
                        Velocity += away * ContainmentFactor;  // Steer away from edge
                    }
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
