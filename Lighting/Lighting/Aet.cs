using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2
{
    class Aet
    {
        public Aet(int ymax,float x,float step,Aet next=null)
        {
            Ymax = ymax;
            X = x;
            Step = step;
            Next = next; 
        }
        public int Ymax  { get; set; }
        public float X { get; set; }
        public float Step { get; set; }
        public Aet Next { get; set; }
    }
}
