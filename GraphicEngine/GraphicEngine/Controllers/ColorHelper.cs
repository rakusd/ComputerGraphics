using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_4.Controllers
{
    static class ColorHelper
    {
        public const int reflectorPower = 10;
        public const int shinyPower = 5;
        public const double ka = 0.2;
        public const double kd = 0.4;
        public const double ks = 0.4;
        public const int norm = 2;
        public static Color lightColor=Color.White;
        public static Color reflectorColor=Color.White;
        public static bool foggy = false;
        public static double fogstart = 4;
        public static double fogend = 10;
        public static int RoundToColor(double val)
        {
            if (val > 255)
                val = 255;
            if (val < 0)
                val = 0;
            return (int)val;
        }
        public static void RoundToColor(ref int val)
        {
            if (val > 255)
                val = 255;
            if (val < 0)
                val = 0;
        }

        public static (double r,double g,double b) DiffuseAndSpecular(Color c,Vector L,Vector N,Vector pos,Color lightColor)
        {
            double r=0, g=0, b=0;
            if (lightColor.R == lightColor.G && lightColor.R == lightColor.B && lightColor.R == 0)
                return (r, g, b);

            double cos = L.DotProduct(N);
            if(cos > 0)
            {
                r += kd * cos * c.R * lightColor.R / 255;
                g += kd * cos * c.G * lightColor.G / 255;
                b += kd * cos * c.B * lightColor.B / 255;
            }

            Vector R = (Vector)(N.Multiply(2 * cos) - L);

            R = (Vector)R.Normalize(norm);
            Vector toView = (Vector)pos.Normalize(norm);

            double shinyCos = R.DotProduct(-toView);
            if (shinyCos > 0)
            {
                r += ks * Math.Pow(shinyCos, shinyPower) * c.R * lightColor.R / 255;
                g += ks * Math.Pow(shinyCos, shinyPower) * c.G * lightColor.G / 255;
                b += ks * Math.Pow(shinyCos, shinyPower) * c.B * lightColor.B / 255;
            }
            return (r, g, b);
        }
        public static Color CalculateColor(Color c, Vector _L, Vector _N, Vector _pos, Vector _reflectorPos, Vector _reflectorTarget)
        {

            Vector L = (Vector)_L.SubVector(0, 3);
            Vector N = (Vector)_N.SubVector(0, 3);
            Vector pos = (Vector)_pos.SubVector(0, 3);
            Vector reflectorPos = (Vector)_reflectorPos.SubVector(0, 3);
            Vector reflectorTarget= (Vector)_reflectorTarget.SubVector(0, 3);

            L = (Vector)L.Normalize(norm);
            N = (Vector)N.Normalize(norm);

            double red =ka*c.R;
            double green=ka*c.G;
            double blue=ka*c.B;

            double r, g, b;

            (r, g, b)= DiffuseAndSpecular(c, L, N, pos, lightColor);
            red += r;
            green += g;
            blue += b;

            Vector toView = (Vector)pos.Normalize(norm);
            Vector reflectorVector = (Vector)(reflectorPos - reflectorTarget);
            Vector curVector = (Vector)(reflectorPos - pos);

            reflectorVector = (Vector)reflectorVector.Normalize(norm);
            curVector = (Vector)curVector.Normalize(norm);

            double reflectorCos = reflectorVector.DotProduct(curVector);
            if (reflectorCos > 0)
            {
                double red2 = reflectorColor.R * Math.Pow(reflectorCos, reflectorPower);
                double green2 = reflectorColor.G * Math.Pow(reflectorCos, reflectorPower);
                double blue2 = reflectorColor.B * Math.Pow(reflectorCos, reflectorPower);

                (r, g, b) = DiffuseAndSpecular(c, curVector, N, pos, Color.FromArgb((int)red2, (int)green2, (int)blue2));
                red += r;
                green += g;
                blue += b;

            }

            Color ret=Color.FromArgb(RoundToColor(red), RoundToColor(green), RoundToColor(blue));
            if (foggy)
                ApplyFog(ref ret, pos);
            return ret;
        }
        public static void ApplyFog(ref Color color,Vector pos)
        {

            double f = ((fogend - pos.L2Norm()) / (fogend - fogstart));
            if (f > 1)
                f = 1;
            if (f < 0)
                f = 0;
            color = Color.FromArgb((int)(color.R*f), (int)(color.G * f ), (int)(color.B * f ));
        }
    }
}
