﻿using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace AcousticQuelea
{
    public class AcousticQueleaInfo : GH_AssemblyInfo
    {
        public override string Name => "AcousticQuelea";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("4b4b41c4-0a34-4a8e-92cc-5bfe889ec4d9");

        //Return a string identifying you or your company.
        public override string AuthorName => "Tej";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "sai.jami@stud.th-owl.de";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}