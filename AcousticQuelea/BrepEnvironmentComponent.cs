using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace AcousticQuelea
{
    public class BrepEnvironmentComponent : GH_Component
    {
        public BrepEnvironmentComponent()
          : base("Brep Environment", "EnvBrep",
              "Defines an acoustic environment using Brep",
              "acoustics_tej", "Environment")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Environment Brep", "B", "Brep representing the acoustic environment", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Environment", "Env", "Acoustic Brep environment", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            if (!DA.GetData(0, ref brep)) return;

            DA.SetData(0, brep);
        }

        public override Guid ComponentGuid => new Guid("B9876543-DCBA-4321-FEBA-98765FEDCBA0");
        protected override System.Drawing.Bitmap Icon => null;
    }
}
