 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    public class Vertice
    {
        public Vertice CloneVertice()
        {
            Vertice temp = new Vertice(this.X, this.Y);
            temp.right = this.right;
            temp.left = this.left;
            temp.conditionSet = this.conditionSet;
            temp.angle = this.angle;
            return temp;
        }
        public const int radius = 10;
        public float X { get; set; }
        public float Y { get; set; }
        public Vertice(float x,float y)
        {
            X = x;Y = y;
        }
        public double angle;
        public bool conditionSet = false;
        public Condition left = Condition.none, right = Condition.none;
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
