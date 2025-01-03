using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;

namespace AcousticQuelea
{
    public class SoundEmitterComponent : GH_Component
    {
        public SoundEmitterComponent()
          : base("Sound Emitter", "EmitSound",
              "Emits sound particles from a point",
              "acoustics_tej", "Emitters")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Emitter Point", "Pt", "Point to emit particles from", GH_ParamAccess.item);
            pManager.AddNumberParameter("Frequency", "Freq", "Frequency of sound particles", GH_ParamAccess.item, 440.0);
            pManager.AddIntegerParameter("Particle Count", "Count", "Number of particles to emit", GH_ParamAccess.item, 100);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particles", "P", "Emitted sound particles", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d emitterPoint = Point3d.Origin;
            double frequency = 440.0;
            int count = 100;

            if (!DA.GetData(0, ref emitterPoint)) return;
            if (!DA.GetData(1, ref frequency)) return;
            if (!DA.GetData(2, ref count)) return;

            var particles = new List<SoundParticle>();
            for (int i = 0; i < count; i++)
            {
                particles.Add(new SoundParticle(emitterPoint, frequency));
            }

            DA.SetDataList(0, particles);
        }

        public override Guid ComponentGuid => new Guid("B9123456-ABCD-1234-EFAB-56789ABCDEF0");
        protected override System.Drawing.Bitmap Icon => null;
    }
}
