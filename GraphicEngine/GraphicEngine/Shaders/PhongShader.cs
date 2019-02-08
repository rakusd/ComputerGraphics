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
    class PhongShader : IShader
    {
        public Color fragmentShader(Triangle t, int x, int y,double z, Vector parameters,Vector L,Vector reflectorPos, Vector reflectorTarget)
        {
            Vector N = (Vector)(t.N1 * parameters[0] + t.N2 * parameters[1] + t.N3 * parameters[2]);
            Vector pos = (Vector)(t.V1 * parameters[0] + t.V2 * parameters[1] + t.V3 * parameters[2]);

            return ColorHelper.CalculateColor(t.C, L, N, pos, reflectorPos,reflectorTarget);
        }
    }
}
