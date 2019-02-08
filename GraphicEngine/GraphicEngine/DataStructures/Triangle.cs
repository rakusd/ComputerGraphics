using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Project_4.Controllers;

namespace Project_4
{
    class Triangle
    {

        public static Color defaultColor = Color.Black;
        public Vector P1 { get; set; }
        public Vector P2 { get; set; }
        public Vector P3 { get; set; }
        public Vector V1 { get; set; }
        public Vector V2 { get; set; }
        public Vector V3 { get; set; }
        public Vector N1 { get; set; }
        public Vector N2 { get; set; }
        public Vector N3 { get; set; }
        public Color C { get; set; }
        public Color C1 { get; set; }
        public Color C2 { get; set; }
        public Color C3 { get; set; }

        public Triangle(Vector p1, Vector p2, Vector p3):this(p1,p2,p3,defaultColor)
        {

        }
        public Triangle(Vector p1,Vector p2,Vector p3,Color c)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            C = c;
        }
        public Triangle(Vector p1, Vector p2, Vector p3, Color c,Vector n1,Vector n2,Vector n3):this(p1,p2,p3,c)
        {
            N1 = n1;
            N2 = n2;
            N3 = n3;
        }
        public Triangle(Vector p1, Vector p2, Vector p3, Color c, Vector n1, Vector n2, Vector n3,Vector v1,Vector v2,Vector v3): this(p1,p2,p3,c,n1,n2,n3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        public void CalculateColors(Vector L,Vector reflectorPos,Vector reflectorTarget)
        {
            C1 = ColorHelper.CalculateColor(C, L, N1, V1, reflectorPos, reflectorTarget);
            C2 = ColorHelper.CalculateColor(C, L, N2,V2, reflectorPos, reflectorTarget);
            C3 = ColorHelper.CalculateColor(C, L, N3, V3, reflectorPos, reflectorTarget);
        }
        
    }
}
