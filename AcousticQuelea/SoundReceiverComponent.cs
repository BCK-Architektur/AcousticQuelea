using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;

namespace AcousticQuelea
{
    public class SoundReceiverComponent : GH_Component
    {
        public SoundReceiverComponent()
          : base("Sound Receiver", "Receiver",
              "Creates a receiver that detects particles within a cylindrical region",
              "acoustics_tej", "Receivers")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Receiver Position", "Pos", "Base center point of the receiver", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height of the receiver cylinder", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Radius", "R", "Radius of the receiver cylinder", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Receiver", "Rec", "A cylindrical receiver for detecting sound particles", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Inputs
            Point3d receiverBase = Point3d.Origin;
            double height = 2.0;
            double radius = 1.0;

            if (!DA.GetData(0, ref receiverBase)) return;
            if (!DA.GetData(1, ref height)) return;
            if (!DA.GetData(2, ref radius)) return;

            // Create Receiver (Cylinder Representation)
            SoundReceiver receiver = new SoundReceiver(receiverBase, radius, height);

            // Output the receiver
            DA.SetData(0, receiver);
        }

        public override Guid ComponentGuid => new Guid("D4567890-1234-5678-90AB-CDEF12345678");
        protected override System.Drawing.Bitmap Icon => null;
    }

    // Receiver class to store receiver properties
    public class SoundReceiver
    {
        public Point3d BaseCenter { get; private set; }
        public double Radius { get; private set; }
        public double Height { get; private set; }

        public SoundReceiver(Point3d baseCenter, double radius, double height)
        {
            BaseCenter = baseCenter;
            Radius = radius;
            Height = height;
        }

        public bool IsParticleInside(Point3d particlePosition)
        {
            // Check if the particle is inside the receiver cylinder
            double zDiff = particlePosition.Z - BaseCenter.Z;
            if (zDiff < 0 || zDiff > Height)
                return false;

            // Check if particle is within the radius
            Point3d projectedPoint = new Point3d(particlePosition.X, particlePosition.Y, BaseCenter.Z);
            return projectedPoint.DistanceTo(BaseCenter) <= Radius;
        }
    }
}
