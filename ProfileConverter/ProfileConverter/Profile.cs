using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public struct Profile
    {
        public Profile(double gamma,Vector white,Vector red, Vector green, Vector blue)
        {
            Gamma = gamma;
            White = white;
            Red = red;
            Green = green;
            Blue = blue;
        }
        public double Gamma { get; set; }
        public Vector White { get; set; }
        public Vector Red { get; set; }
        public Vector Green { get; set; }
        public Vector Blue { get; set; }
    }
}
