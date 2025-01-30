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
        private List<SoundReceiver> receivers = new List<SoundReceiver>();
        private List<int> receiverHitCounts = new List<int>();

        public ParticleVisualizerComponent()
          : base("Particle Visualizer", "PVis",
              "Visualizes sound particles and tracks hits on multiple receivers",
              "acoustics_tej", "Visualization")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Particles", "P", "Sound particles from emitter", GH_ParamAccess.list);
            pManager.AddBrepParameter("Environment", "B", "Acoustic Brep environment", GH_ParamAccess.item);
            pManager.AddGenericParameter("Receivers", "R", "List of sound receivers", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Particle Paths", "Path", "Visualized particle paths", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Total Hits", "Hits", "Total number of particles hitting receivers", GH_ParamAccess.item);
            pManager.AddGenericParameter("Receivers", "Receivers", "List of receivers", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Hit Counts", "HitCounts", "Number of particles hitting each receiver (same order)", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<SoundParticle> inputParticles = new List<SoundParticle>();
            Brep environment = null;
            List<SoundReceiver> inputReceivers = new List<SoundReceiver>();

            if (!DA.GetDataList(0, inputParticles)) return;
            if (!DA.GetData(1, ref environment)) return;
            if (!DA.GetDataList(2, inputReceivers)) return;

            particles = new List<SoundParticle>();
            receivers = new List<SoundReceiver>(inputReceivers);
            receiverHitCounts = new List<int>(new int[inputReceivers.Count]); // Initialize counts with 0

            List<Line> paths = new List<Line>();
            int totalHits = 0;

            foreach (var particle in inputParticles)
            {
                if (particle.IsAlive()) // Only process alive particles
                {
                    var start = particle.Position;
                    particle.Move(environment);
                    particles.Add(particle);
                    paths.Add(new Line(start, particle.Position));

                    // Check if the particle hits any receiver
                    for (int i = 0; i < receivers.Count; i++)
                    {
                        if (receivers[i].IsParticleInside(particle.Position))
                        {
                            receiverHitCounts[i]++; // Increment hit count for this receiver
                            totalHits++;
                        }
                    }
                }
            }

            // Output results
            DA.SetDataList(0, paths);
            DA.SetData(1, totalHits);
            DA.SetDataList(2, receivers);        // Output the list of receivers
            DA.SetDataList(3, receiverHitCounts); // Output corresponding hit counts
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

            // Draw Receivers as cylinders
            for (int i = 0; i < receivers.Count; i++)
            {
                Cylinder receiverCylinder = new Cylinder(new Circle(receivers[i].BaseCenter, receivers[i].Radius), receivers[i].Height);
                Brep receiverBrep = receiverCylinder.ToBrep(true, true);
                args.Display.DrawBrepWires(receiverBrep, Color.White, 2);

                // Optionally, display hit count near the receiver
                args.Display.Draw2dText($"{receiverHitCounts[i]}", Color.White, receivers[i].BaseCenter, false, 12);
            }
        }

        public override Guid ComponentGuid => new Guid("ABC12345-6789-DEF0-1234-56789ABCDEF1");
        protected override System.Drawing.Bitmap Icon => null;
    }
}
