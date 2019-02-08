using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using Project_4.Controllers;

namespace Project_4.Shaders
{
    class GouraudShader : IShader
    {

        public Color fragmentShader(Triangle t, int x, int y,double z, Vector parameters, Vector L, Vector reflectorPos,Vector reflectorTarget)
        {
            int red = 0, green = 0, blue = 0;

            red = (int)(t.C1.R * parameters[0] + t.C2.R * parameters[1] + t.C3.R * parameters[2]);
            green = (int)(t.C1.G * parameters[0] + t.C2.G * parameters[1] + t.C3.G * parameters[2]);
            blue = (int)(t.C1.B * parameters[0] + t.C2.B * parameters[1] + t.C3.B * parameters[2]);

            ColorHelper.RoundToColor(ref red);
            ColorHelper.RoundToColor(ref green);
            ColorHelper.RoundToColor(ref blue);

            return Color.FromArgb(red, green, blue);
        }
    }
}
