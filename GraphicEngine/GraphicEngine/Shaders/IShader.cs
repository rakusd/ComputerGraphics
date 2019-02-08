using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Project_4.Controllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_4.Shaders
{
    interface IShader
    {
        Color fragmentShader(Triangle t, int x, int y, double z, Vector parameters, Vector L, Vector reflectorPos, Vector reflectorTarget);
    } 
}
