using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Project_4.Shaders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_4.Controllers
{
    static class AetController
    {
        public static Vector ReflectorPos { get; set; }
        public static Vector ReflectorTarget { get; set; }
        public static Vector L { get; set; }
        public static IShader Shader { get; set; }
        public static Vector CrossProduct(this Vector v1,Vector v2)
        {
            if (v1.Count != 3 || v2.Count != 3)
                throw new NotImplementedException();
            double x= v1[1] * v2[2] - v1[2] * v2[1];
            double y= -v1[0] * v2[2] + v1[2] * v2[0];
            double z= v1[0] * v2[1] - v1[1] * v2[0];
            return DenseVector.OfArray(new double[] { x, y, z });
        }
        public static void DeleteAet(ref Aet head, int ymaximum, double x, double step)
        {
            if (head.Ymax == ymaximum && head.Step == step)
            {
                head = head.Next;
                return;
            }

            Aet temp = head;

            while (temp.Next != null)
            {
                if (temp.Next.Ymax == ymaximum && temp.Next.Step == step)
                {
                    temp.Next = temp.Next.Next;
                    return;
                }
                temp = temp.Next;
            }
        }
        public static void ActualizeAet(Aet head)
        {
            if (head == null)
                return;
            Aet help = head;
            while (help != null)
            {
                help.X += help.Step;
                help = help.Next;
            }
        }
        public static double CalculateM(Vector end, Vector start)
        {
            double x2 = end[0];
            double y2 = end[1];
            double x1 = start[0];
            double y1 = start[1];
            return 1 / ((y2 - y1) / (x2 - x1));
        }
        public static void AddAet(ref Aet head, Aet added)
        {
            if (head == null)
            {
                head = added;
                return;
            }
            Aet temp = head;
            while (temp.Next != null)
                temp = temp.Next;
            temp.Next = added;
        }

        public static void SortAet(ref Aet head)
        {
            if (head == null || head.Next == null)
                return;
            if (head.X > head.Next.X)
            {
                Aet temp = head.Next;
                temp.Next = head;
                head.Next = null;
                head = temp;
            }
        }
        public static void PaintAet(Aet head, DirectBitmap b, int y, double[,] zBuffer,Triangle t)
        {
            if (y < 0 || y >= b.Height)
                return;
            if (head == null || head.Next == null)
                return;
            int start = (int)Math.Floor(head.X);
            int end = (int)Math.Floor(head.Next.X);

            Vector result;
            Matrix values;
            Vector parameters;
            double z;
            Color c;

            for (int i = start; i < end; i++)
            {
                
                if (i < 0 || i >= b.Width)
                    continue;
                if (zBuffer[i, y] < t.P1[2] && zBuffer[i, y] < t.P2[2] && zBuffer[i, y] < t.P3[2])
                    continue;

                result = DenseVector.OfArray(new double[] { i, y, 1 });
                values = DenseMatrix.OfArray(new double[,] { { t.P1[0], t.P2[0], t.P3[0] }, { t.P1[1], t.P2[1], t.P3[1] }, { 1, 1, 1 } });
                parameters = (Vector)values.Solve(result);

                z = t.P1[2] * parameters[0] + t.P2[2] * parameters[1] + t.P2[2] * parameters[2];

                if (zBuffer[i, y] < z)
                    continue;
                zBuffer[i, y] = z;

                c = Shader.fragmentShader(t,i,y,z,parameters,L,ReflectorPos,ReflectorTarget);
                b.SetPixel(i, y, c);
            }
        }

        public static void SortIndexes(int[] ind, Vector[] tab)
        {
            for (int i = 0; i < ind.Length; i++)
            {
                for (int j = 0; j < ind.Length - 1; j++)
                {
                    if (tab[ind[j]][1] > tab[ind[j + 1]][1])
                    {
                        int swap = ind[j];
                        ind[j] = ind[j + 1];
                        ind[j + 1] = swap;
                    }
                }
            }
        }

    }
}
