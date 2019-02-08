using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;

namespace Project_4.Controllers
{
    class FillController
    {
        public Vector ReflectorPos { get; set; }
        public Vector ReflectorTarget { get; set; }
        public Vector L { get; set; }
        double[,] zBuffer;

        public void InitBuffer(int width,int height)
        {
            zBuffer = new double[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    zBuffer[i, j] = double.PositiveInfinity;
                }
            }
        }
        
        public void FillTriangles(DirectBitmap b,IEnumerable<Triangle> fillableTriangles,Graphics g)
        {
            foreach (Triangle t in fillableTriangles)
            {
                FillTriangle(b, t);
            }
            g.DrawImage(b.Bitmap, 0, 0);
        }

        private void FillTriangle(DirectBitmap b, Triangle t)
        {
            Vector[] tab = new Vector[] { t.P1, t.P2, t.P3 };
            int[] ind = { 0, 1, 2 };
            AetController.SortIndexes(ind, tab);
            int ymin = (int)tab[ind[0]][1];
            int ymax = (int)tab[ind[ind.Length - 1]][1];
            int k = 0;
            Aet head = null;

            t.CalculateColors(L,ReflectorPos,ReflectorTarget);


            for (int y = ymin; y <= ymax; y++)
            {
                while (k < 3 && (int)tab[ind[k]][1] == y)
                {
                    int prev = (ind[k] + 3 - 1) % 3;
                    int next = (ind[k] + 1) % 3;
                    if ((int)tab[next][1] > (int)tab[ind[k]][1])
                    {
                        int ymaximum = (int)tab[next][1];
                        double x = tab[ind[k]][0];
                        double step = (int)tab[next][0] == (int)tab[ind[k]][0] ? 0 : AetController.CalculateM(tab[next], tab[ind[k]]);
                        AetController.AddAet(ref head, new Aet(ymaximum, x, step));
                    }
                    else if ((int)tab[next][1] != (int)tab[ind[k]][1])
                    {
                        int ymaximum = (int)tab[ind[k]][1];
                        double x = tab[next][0];
                        double step = (int)tab[next][0] == (int)tab[ind[k]][0] ? 0 : AetController.CalculateM(tab[ind[k]], tab[next]);
                        AetController.DeleteAet(ref head, ymaximum, x, step);
                    }
                    if ((int)tab[prev][1] > (int)tab[ind[k]][1])
                    {
                        int ymaximum = (int)tab[prev][1];
                        double x = tab[ind[k]][0];
                        double step = (int)tab[prev][0] == (int)tab[ind[k]][0] ? 0 : AetController.CalculateM(tab[prev], tab[ind[k]]);
                        AetController.AddAet(ref head, new Aet(ymaximum, x, step));
                    }
                    else if ((int)tab[prev][1] != (int)tab[ind[k]][1])
                    {
                        int ymaximum = (int)tab[ind[k]][1];
                        double x = tab[prev][0];
                        double step = (int)tab[prev][0] == (int)tab[ind[k]][0] ? 0 : AetController.CalculateM(tab[ind[k]], tab[prev]);
                        AetController.DeleteAet(ref head, ymaximum, x, step);
                    }
                    k++;
                }
                AetController.SortAet(ref head);
                AetController.PaintAet(head, b, y,zBuffer,t);
                AetController.ActualizeAet(head);
            }



        }
    }
}
