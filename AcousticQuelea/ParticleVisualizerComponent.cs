using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using Rhino.Display;
using System;
using System.Drawing;

namespace AcousticQuelea
{
    public class ParticleVisualizerComponent : GH_Component
    {
        private List<SoundParticle> particles = new List<SoundParticle>();

        public ParticleVisualizerComponent()
          : base("Particle Visualizer", "PVis",
              "Visualizes sound particles in the Brep environment with bounce-based color changes",
              "acoustics_tej", "Visualization")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Particles", "P", "Sound particles from emitter", GH_ParamAccess.list);
            pManager.AddBrepParameter("Environment", "B", "Acoustic Brep environment", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Particle Paths", "Path", "Visualized particle paths", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<SoundParticle> inputParticles = new List<SoundParticle>();
            Brep environment = null;

            if (!DA.GetDataList(0, inputParticles)) return;
            if (!DA.GetData(1, ref environment)) return;

            particles = new List<SoundParticle>();

            List<Line> paths = new List<Line>();

            foreach (var particle in inputParticles)
            {
                if (particle.IsAlive()) // Only process particles that haven't died
                {
                    var start = particle.Position;
                    particle.Move(environment);
                    particles.Add(particle); // Keep the updated particle in the list
                    paths.Add(new Line(start, particle.Position));
                }
            }

            DA.SetDataList(0, paths);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            foreach (var particle in particles)
            {
                if (particle.IsAlive()) // Only draw alive particles
                {
                    args.Display.DrawPoint(particle.Position, PointStyle.RoundSimple, 3, particle.ParticleColor);
                }
            }
        }

        public override Guid ComponentGuid => new Guid("ABC12345-6789-DEF0-1234-56789ABCDEF1");
        protected override System.Drawing.Bitmap Icon => null;
    }
}
