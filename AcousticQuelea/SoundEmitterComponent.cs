using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using Rhino;

namespace AcousticQuelea
{
    public class SoundEmitterComponent : GH_Component
    {
        private static readonly Random rand = new Random(); // Static random instance

        public SoundEmitterComponent()
          : base("Sound Emitter", "EmitSound",
              "Emits sound particles from a point, either omnidirectional or in a directional cone",
              "acoustics_tej", "Emitters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Emitter Point", "Pt", "Point to emit particles from", GH_ParamAccess.item);
            pManager.AddNumberParameter("Frequency", "Freq", "Frequency of sound particles", GH_ParamAccess.item, 440.0);
            pManager.AddIntegerParameter("Particle Count", "Count", "Number of particles to emit", GH_ParamAccess.item, 100);

            // New parameters for directional mode
            pManager.AddBooleanParameter("Omni Directional", "Omni", "True for omnidirectional, False for directional cone", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Cone Angle", "Angle", "Spread angle of the emission cone (degrees)", GH_ParamAccess.item, 30.0);
            pManager.AddNumberParameter("XZ Rotation", "XZ Rot", "Rotation in XZ plane (degrees)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("YZ Rotation", "YZ Rot", "Rotation in YZ plane (degrees)", GH_ParamAccess.item, 0.0);
            pManager.AddIntegerParameter("Max Bounces", "Bounces", "Maximum number of bounces before the particle dies", GH_ParamAccess.item, 5);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particles", "P", "Emitted sound particles", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Inputs
            Point3d emitterPoint = Point3d.Origin;
            double frequency = 440.0;
            int count = 100;
            bool isOmni = true;
            double coneAngle = 30.0;
            double xzRotation = 0.0;
            double yzRotation = 0.0;
            int maxBounces = 6;

            // Get Data
            if (!DA.GetData(0, ref emitterPoint)) return;
            if (!DA.GetData(1, ref frequency)) return;
            if (!DA.GetData(2, ref count)) return;
            if (!DA.GetData(3, ref isOmni)) return;
            if (!DA.GetData(4, ref coneAngle)) return;
            if (!DA.GetData(5, ref xzRotation)) return;
            if (!DA.GetData(6, ref yzRotation)) return;
            if (!DA.GetData(7, ref maxBounces)) return;

            var particles = new List<SoundParticle>();

            if (isOmni)
            {
                // Omni-directional emission
                for (int i = 0; i < count; i++)
                {
                    Vector3d velocity = GetRandomDirection() * 2.0;
                    particles.Add(new SoundParticle(emitterPoint, velocity, frequency, maxBounces));
                }
            }
            else
            {
                // Directional cone emission
                Vector3d direction = new Vector3d(0, 0, 1); // Default: Z-axis

                // Apply XZ and YZ rotations
                direction.Transform(Transform.Rotation(RhinoMath.ToRadians(xzRotation), Vector3d.YAxis, Point3d.Origin)); // Rotate around Y-axis
                direction.Transform(Transform.Rotation(RhinoMath.ToRadians(yzRotation), Vector3d.XAxis, Point3d.Origin)); // Rotate around X-axis

                for (int i = 0; i < count; i++)
                {
                    Vector3d velocity = GetRandomConeDirection(direction, coneAngle);
                    particles.Add(new SoundParticle(emitterPoint, velocity, frequency, maxBounces));
                }
            }

            // Output particles
            DA.SetDataList(0, particles);
        }

        private Vector3d GetRandomDirection()
        {
            Vector3d direction;
            do
            {
                double x = rand.NextDouble() * 2 - 1;
                double y = rand.NextDouble() * 2 - 1;
                double z = rand.NextDouble() * 2 - 1;
                direction = new Vector3d(x, y, z);
            } while (direction.IsZero); // Ensure it's not zero

            direction.Unitize();
            return direction;
        }

        private Vector3d GetRandomConeDirection(Vector3d baseDirection, double angle)
        {
            double maxDeviation = RhinoMath.ToRadians(angle); // Convert to radians

            double theta = rand.NextDouble() * maxDeviation; // Random angle within cone
            double phi = rand.NextDouble() * 2 * Math.PI; // Random azimuth

            // Convert spherical to Cartesian coordinates
            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);

            Vector3d coneVector = new Vector3d(x, y, z);

            // Ensure valid transformation
            if (baseDirection.IsZero) baseDirection = Vector3d.ZAxis;

            // Create base planes for transformation
            Plane sourcePlane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            Plane targetPlane = new Plane(Point3d.Origin, baseDirection);

            // Correct ChangeBasis transformation
            Transform alignTransform = Transform.ChangeBasis(sourcePlane, targetPlane);
            coneVector.Transform(alignTransform);

            return coneVector;
        }

        public override Guid ComponentGuid => new Guid("B9123456-ABCD-1234-EFAB-56789ABCDEF0");

        protected override System.Drawing.Bitmap Icon => null;
    }
}
