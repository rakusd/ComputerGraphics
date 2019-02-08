using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project1
{
    public class Edge
    {
        public Vertice vertice1 {get;set;}
        public Vertice vertice2 {get;set;}
        public Edge(Vertice v1,Vertice v2)
        {
            vertice1 = v1;
            vertice2 = v2;
        }
        public bool SetCondtion(Condition cond)
        {
            if((vertice1.left!=Condition.none && vertice1.conditionSet) || (vertice2.right!=Condition.none && vertice2.conditionSet))
            {
                MessageBox.Show("Cannot make this edge " + cond + "cause one angle and edge condition are specified");
                return false;
            }
            if (cond!=Condition.none && (vertice1.left == cond || vertice2.right == cond))
            {
                MessageBox.Show("Cannot make this edge " + cond + " because one of adjacent edges is also " + cond);
                return false;
            }
            vertice1.right = vertice2.left = cond;
            return true;
        }
    }
}
