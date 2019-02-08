using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_4
{
    class Aet
    {
        public Aet(int ymax, double x, double step, Aet next = null)
        {
            Ymax = ymax;
            X = x;
            Step = step;
            Next = next;
        }
        public int Ymax { get; set; }
        public double X { get; set; }
        public double Step { get; set; }
        public Aet Next { get; set; }
    }
}
