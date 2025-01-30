using Rhino.Geometry;
using System;
using System.Linq;
using System.Drawing; // For color handling

namespace AcousticQuelea
{
    public class SoundParticle
    {
        public Point3d Position { get; private set; }
        public Vector3d Velocity { get; private set; }
        public double Frequency { get; private set; }
        public double Intensity { get; private set; }
        public int Lifespan { get; private set; }
        public int Bounces { get; private set; }
        public int MaxBounces { get; private set; }
        public Color ParticleColor { get; private set; } // Particle color based on bounces

        private static readonly Random random = new Random();
        private const double IntensityFalloff = 0.005;
        private const double EdgeThreshold = 0.3;  // Edge detection threshold
        private const double ContainmentFactor = 0.5;

        public SoundParticle(Point3d position, Vector3d velocity, double frequency, int maxBounces)
        {
            Position = position;
            Frequency = frequency;
            Intensity = 1.0;
            Lifespan = 200;
            Velocity = velocity;
            MaxBounces = maxBounces;
            Bounces = 0;
            ParticleColor = Color.Blue; // Default color (starts as blue)
        }

        public void Move(Brep environment)
        {
            Position += Velocity;
            Intensity -= IntensityFalloff;

            if (environment != null)
            {
                if (ReflectIfHit(environment))
                {
                    Bounces++;
                    UpdateColor(); // Change color after each bounce

                    if (Bounces >= MaxBounces)
                    {
                        Intensity = 0; // Kill particle after max bounces
                    }
                }

                AvoidEdges(environment);  // Avoid naked edges
                BounceContain(environment);  // Keep within bounds
            }

            Lifespan--;
        }

        private bool ReflectIfHit(Brep environment)
        {
            Vector3d normal;
            var closestPoint = environment.ClosestPoint(Position, out Point3d pointOnBrep, out ComponentIndex ci, out double u, out double v, 1.0, out normal);

            if (closestPoint && ci.ComponentIndexType == ComponentIndexType.BrepFace)
            {
                if (Position.DistanceTo(pointOnBrep) < 0.5)
                {
                    Velocity = Reflect(Velocity, normal);
                    Intensity *= 0.9;
                    return true;
                }
            }
            return false;
        }

        // Update color based on the number of bounces
        private void UpdateColor()
        {
            int red = Math.Min(255, (Bounces * 50));  // Increases red component with bounces
            int blue = Math.Max(0, 255 - (Bounces * 50)); // Decreases blue component
            ParticleColor = Color.FromArgb(red, 0, blue);
        }

        private Vector3d Reflect(Vector3d vector, Vector3d normal)
        {
            return vector - 2 * (vector * normal) * normal;
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

        public bool IsAlive()
        {
            return Intensity > 0 && Lifespan > 0;
        }
    }
}
