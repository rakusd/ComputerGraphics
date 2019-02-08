using System; 
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Project1
{
    public partial class Form1 : Form
    {
        //Comment/Uncomment below to display console
        /*private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();*/
        //Comment/Uncomment above to display console
        

        //-------------------------------------------------------------------------------------------------------
        //                              INITIALIZATION
        //-------------------------------------------------------------------------------------------------------


        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
           // Predefined();
        }


        //-------------------------------------------------------------------------------------------------------
        //                              FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------


        public List<Vertice> vertices = new List<Vertice>();
        public List<Vertice> copy = new List<Vertice>();

        public Vertice selectedVertice;
        public Edge selectedEdge;

        public bool finishedDrawing;
        private int lastX, lastY;

        //-------------------------------------------------------------------------------------------------------
        //                              POLYGON MOVING LOGIC
        //-------------------------------------------------------------------------------------------------------
        public bool TryToSetAngle(Vertice v,double angle)//Done
        {
            int step = 1;
            copy = new List<Vertice>();

            foreach (Vertice vertice in vertices)
            {
                Vertice temp = new Vertice(vertice.X, vertice.Y);
                temp.right = vertice.right;
                temp.left = vertice.left;
                temp.conditionSet = vertice.conditionSet;
                temp.angle = vertice.angle;
                copy.Add(temp);
            }

            if (TryToSetAngleRight(copy[vertices.IndexOf(v)],ref step,angle))
            {
                return true;
            }

            copy = new List<Vertice>();

            foreach (Vertice vertice in vertices)
            {
                Vertice temp = new Vertice(vertice.X, vertice.Y);
                temp.right = vertice.right;
                temp.left = vertice.left;
                temp.conditionSet = vertice.conditionSet;
                temp.angle = vertice.angle;
                copy.Add(temp);
            }
            step = 1;
            if (TryToSetAngleLeft(copy[vertices.IndexOf(v)],ref step,angle))
            {
                return true;
            }
            return false;
        }
        public bool TryToSetAngleRight(Vertice v,ref int step, double angle)//Done
        {
            if (step >= copy.Count)
                return false;
            step++;

            if (v.right != Condition.none)
                return false;
            int i = copy.IndexOf(v);
            Vertice next = copy[(i + 1) % copy.Count];
            Vertice prev = copy[(i - 1 + copy.Count) % copy.Count]; 

            double deg = (angle * Math.PI) / 180.0;

            float newX = (float)(Math.Cos(deg) * (prev.X - v.X) - Math.Sin(deg) * (prev.Y - v.Y) + v.X);
            float newY = (float)(Math.Sin(deg) * (prev.X - v.X) + Math.Cos(deg) * (prev.Y - v.Y) + v.Y);
            float oldX = next.X;
            float oldY = next.Y;
            next.X = newX;
            next.Y = newY;

            if (Math.Round(calculateCopiedAngle(v)) != Math.Round(angle))
            {
                next.X = oldX; next.Y = oldY;
                deg = -deg;
                newX = (float)(Math.Cos(deg) * (prev.X - v.X) - Math.Sin(deg) * (prev.Y - v.Y) + v.X);
                newY = (float)(Math.Sin(deg) * (prev.X - v.X) + Math.Cos(deg) * (prev.Y - v.Y) + v.Y);
                oldX = next.X;
                oldY = next.Y;
                next.X = newX;
                next.Y = newY;
            }


            if (next.conditionSet)
            {
                return TryToSetAngleRight(next,ref step, next.angle);
            }
            else
            {
                return ApplyConditionRight(next, ref step, newX-oldX, newY-oldY);
            }
        }
        public bool TryToSetAngleLeft(Vertice v,ref int step, double angle)//Done
        {
            if (step >= copy.Count)
                return false;
            step++;
            if (v.left != Condition.none)
                return false;
            int i = copy.IndexOf(v);
            Vertice next = copy[(i + 1) % copy.Count];
            Vertice prev = copy[(i - 1 + copy.Count) % copy.Count];

            double deg = (angle * Math.PI) / 180.0;

            if (!(TurnsLeft(prev, v, next) ^ calculateCopiedAngle(v) > 180))
            {
                deg = -deg;
            }

            float newX = (float)(Math.Cos(deg) * (next.X - v.X) - Math.Sin(deg) * (next.Y - v.Y) + v.X);
            float newY = (float)(Math.Sin(deg) * (next.X - v.X) + Math.Cos(deg) * (next.Y - v.Y) + v.Y);

            float oldX = prev.X;
            float oldY = prev.Y;
            prev.X = newX;
            prev.Y = newY;

            if (prev.conditionSet)
            {
                return TryToSetAngleLeft(prev,ref step, prev.angle);
            }
            else
            {
                return ApplyConditionLeft(prev, ref step, newX - oldX, newY - oldY);
            }
        }
        public bool TryToSetEdge(Edge e,Condition c)
        {
            int i = vertices.IndexOf(e.vertice1);
            int j = (i + 1) % vertices.Count;

            int step = 1;

            copy = new List<Vertice>();
            foreach (Vertice vertice in vertices)
            {
                Vertice temp = new Vertice(vertice.X, vertice.Y);
                temp.right = vertice.right;
                temp.left = vertice.left;
                temp.conditionSet = vertice.conditionSet;
                temp.angle = vertice.angle;
                copy.Add(temp);
            }
            float oldX = copy[j].X, oldY = copy[j].Y;
            Edge e1 = new Edge(copy[i], copy[j]);

            if (!e1.SetCondtion(c))
                return false;

            if (c==Condition.horizontal)
            {
                copy[j].Y = copy[i].Y;
            }
            else if(c==Condition.vertical)
            {
                copy[j].X = copy[i].X;
            }

            if(copy[j].conditionSet && copy[i].conditionSet)
            {
                if (TryToSetAngleRight(copy[j], ref step, copy[j].angle) && TryToSetAngleLeft(copy[i], ref step, copy[i].angle))
                    return true;
            }
            else if(copy[j].conditionSet)
            {
                step = 1;
                if (TryToSetAngleRight(copy[j], ref step, copy[j].angle))
                    return true;
            }
            else if(copy[i].conditionSet)
            {
                step = 1;
                if(c==Condition.horizontal)
                {
                    if (TryToSetAngleLeft(copy[i], ref step, copy[i].angle) && ApplyConditionRight(copy[j], ref step, 0, copy[j].Y - oldY))
                        return true;
                }
                else if(c==Condition.vertical)
                {
                    if (TryToSetAngleLeft(copy[i], ref step, copy[i].angle) && ApplyConditionRight(copy[j], ref step, copy[j].X - oldX, 0))
                        return true;
                }
            }
            else
            {
                step = 1;
                if (c == Condition.horizontal)
                {
                    if (ApplyConditionRight(copy[j], ref step, 0, copy[j].Y - oldY))
                        return true;
                }
                else if (c == Condition.vertical)
                {
                    if (ApplyConditionRight(copy[j], ref step, copy[j].X - oldX, 0))
                        return true;
                }
            }

            return false;
        }
        public void MoveWholePolygon(float xChange,float yChange)//Done
        {
            foreach(Vertice v in vertices)
            {
                v.X += xChange;
                v.Y += yChange;
            }
        }
        public bool CheckIfCondition(Vertice v, float xChange, float yChange)//Done
        {
            if (!finishedDrawing)
                return false;
            if (vertices.Count < 2)
                return true;

            int i = vertices.IndexOf(v);
            int step = 1;
            copy = new List<Vertice>();

            foreach (Vertice vertice in vertices)
            {
                Vertice temp = new Vertice(vertice.X, vertice.Y);
                temp.right = vertice.right;
                temp.left = vertice.left;
                temp.conditionSet = vertice.conditionSet;
                temp.angle = vertice.angle;
                copy.Add(temp);
            }

            copy[i].X += xChange;
            copy[i].Y += yChange;

            Vertice next = copy[(i + 1) % copy.Count];
            Vertice prev = copy[(i - 1 + copy.Count) % copy.Count];

            if (copy[i].right != Condition.none || copy[i].conditionSet || next.conditionSet)
            {
                if (!ApplyConditionRight(copy[i], ref step, xChange, yChange))
                    return false;
            }
            //step = 1;
            if (v.left != Condition.none || copy[i].conditionSet || prev.conditionSet)
            {
                if (!ApplyConditionLeft(copy[i], ref step, xChange, yChange))
                    return false;
            }
            return true;
        }
        public void ApplyCondition()//Done
        {
            if(selectedVertice!=null)
            {
                selectedVertice = copy[vertices.IndexOf(selectedVertice)];
            }
            int i=0, j=0;
            if(selectedEdge!=null)
            {
                i = vertices.IndexOf(selectedEdge.vertice1);
                j = vertices.IndexOf(selectedEdge.vertice2);
            }
            vertices = copy;
            if(selectedEdge!=null)
            {
                selectedEdge = new Edge(copy[i], copy[j]);
            }
        }
        public bool ApplyConditionRight(Vertice v, ref int step, float xChange,float yChange)//Done
        {
            if (step>=copy.Count)//Check if alright
            {
                Console.WriteLine("Right stopped");
                return false;
            }
                

            int i = copy.IndexOf(v);
            Vertice next = copy[(i + 1) % copy.Count];

            if (!v.conditionSet && !next.conditionSet && v.right == Condition.none)
                return true;

            step++;

            if(v.right!=Condition.none)
            {
                if (v.right == Condition.horizontal && next.Y == v.Y)
                {
                    return true;                   
                }
                else if (v.right == Condition.vertical && next.X == v.X)
                {
                    return true;
                }
                else if(v.right==Condition.horizontal && next.right==Condition.vertical)
                {
                    next.Y = v.Y;
                    return true;
                }
                else if(v.right==Condition.vertical && next.right==Condition.horizontal)
                {
                    next.X = v.X;
                    return true;
                }
                else if(v.right==Condition.horizontal && next.conditionSet)
                {
                    Vertice next2= copy[(i + 2) % copy.Count];
                    float A, B, C;
                    A = next.Y - next2.Y;
                    B = next2.X - next.X;
                    C = next.X * next2.Y - next2.X * next.Y;

                    if (A == 0)
                    {
                        next2.Y = next.Y = v.Y;
                        return true;
                    }

                    next.X = (B*v.Y+C)/-A;
                    next.Y = v.Y;
                    
                    if(next.Y==next2.Y)
                    {
                        v.Y++;next.Y++;
                        next.X = (B * v.Y + C) / -A;
                    }

                    return true;

                }
                else if(v.right==Condition.vertical && next.conditionSet)
                {
                    Vertice next2 = copy[(i + 2) % copy.Count];
                    float A, B, C;
                    A = next.Y - next2.Y;
                    B = next2.X - next.X;
                    C = next.X * next2.Y - next2.X * next.Y;

                    if (B == 0)
                    {
                        next2.X = next.X = v.X;
                        return true;
                    }

                    next.Y = (A * v.X + C) / -B;
                    next.X = v.X;

                    if (next.X == next2.X)
                    {
                        v.X++; next.X++;
                        next.Y = (A * v.X + C) / -B;
                    }

                    return true;
                }
                else if(v.right==Condition.horizontal)
                {
                    if (next.Y == v.Y)
                        return true;
                    else
                    {
                        float old = next.Y;
                        next.Y = v.Y;
                        return ApplyConditionRight(next, ref step, 0, v.Y - old);
                    }
                }
                else if(v.right==Condition.vertical)
                {
                    if (next.X == v.X)
                        return true;
                    else
                    {
                        float old = next.X;
                        next.X = v.X;
                        return ApplyConditionRight(next, ref step, v.X - old, 0);
                    }
                }
            }
            else if(!v.conditionSet)//v.right==Condition.none
            {
                if(!next.conditionSet)
                {
                    return true;
                }
                else
                {
                    Vertice next2 = copy[(i + 2) % copy.Count];

                    float A1, B1, C1,A2,B2,C2;
                    A1 = next.Y - next2.Y;
                    B1 = next2.X - next.X;
                    C1 = next.X * next2.Y - next2.X * next.Y;

                    A2 = next.Y - (v.Y - yChange);
                    B2 = (v.X - xChange)-next.X;
                    C2 = next.X * (v.Y - yChange) - (v.X - xChange) * next.Y;

                    if(next.Y==v.Y-yChange)//same as horizontal
                    {
                        if (A1 == 0)
                        {
                            next2.Y = next.Y = v.Y;
                            return true;
                        }

                        next.X = (B1 * v.Y + C1) / -A1;
                        next.Y = v.Y;

                        if (next.Y == next2.Y)
                        {
                            v.Y++; next.Y++;
                            next.X = (B1 * v.Y + C1) / -A1;
                        }

                        return true;
                    }
                    else if(next.X==v.X-xChange)//same as vertical
                    {
                        if (B1 == 0)
                        {
                            next2.X = next.X = v.X;
                            return true;
                        }

                        next.Y = (A1 * v.X + C1) / -B1;
                        next.X = v.X;

                        if (next.X == next2.X)
                        {
                            v.X++; next.X++;
                            next.Y = (A1 * v.X + C1) / -B1;
                        }
                    }
                    else
                    {
                        C2 = -A2 * v.X - B2 * v.Y;
                        (next.X, next.Y) = GetIntersection(A1, B1, C1, A2, B2, C2);
                        return true;
                    }
                    return true;
                }
            }
            else//v.conditionSet==true && v.next==Condition.none
            {
                next.X += xChange;
                next.Y += yChange;
                return ApplyConditionRight(next,ref step, xChange, yChange);
            }
            return true;
        }
        public bool ApplyConditionLeft(Vertice v,ref int step, float xChange,float yChange)//Done
        {

            if (step >= copy.Count)//Check if alright
            {
                Console.WriteLine("Left stopped");
                return false;
            }


            int i = copy.IndexOf(v);
            Vertice prev = copy[(i -1+copy.Count) % copy.Count];

            if (!v.conditionSet && !prev.conditionSet && v.left == Condition.none)
                return true;

            step++;

            if (v.left != Condition.none)
            {
                if (v.left == Condition.horizontal && prev.Y == v.Y)
                {
                    return true;
                }
                else if (v.left == Condition.vertical && prev.X == v.X)
                {
                    return true;
                }
                else if (v.left == Condition.horizontal && prev.left == Condition.vertical)
                {
                    prev.Y = v.Y;
                    return true;
                }
                else if (v.left == Condition.vertical && prev.left == Condition.horizontal)
                {
                    prev.X = v.X;
                    return true;
                }
                else if (v.left == Condition.horizontal && prev.conditionSet)
                {
                    Vertice prev2 = copy[(i - 2 + copy.Count) % copy.Count];
                    float A, B, C;
                    A = prev.Y - prev2.Y;
                    B = prev2.X - prev.X;
                    C = prev.X * prev2.Y - prev2.X * prev.Y;

                    if (A == 0)
                    {
                        prev2.Y = prev.Y = v.Y;
                        return true;
                    }

                    prev.X = (B * v.Y + C) / -A;
                    prev.Y = v.Y;

                    if (prev.Y == prev2.Y)
                    {
                        v.Y++; prev.Y++;
                        prev.X = (B * v.Y + C) / -A;
                    }

                    return true;

                }
                else if (v.left == Condition.vertical && prev.conditionSet)
                {
                    Vertice prev2 = copy[(i - 2 + copy.Count) % copy.Count];
                    float A, B, C;
                    A = prev.Y - prev2.Y;
                    B = prev2.X - prev.X;
                    C = prev.X * prev2.Y - prev2.X * prev.Y;

                    if (B == 0)
                    {
                        prev2.X = prev.X = v.X;
                        return true;
                    }

                    prev.Y = (A * v.X + C) / -B;
                    prev.X = v.X;

                    if (prev.X == prev2.X)
                    {
                        v.X++; prev.X++;
                        prev.Y = (A * v.X + C) / -B;
                    }

                    return true;
                }
                else if (v.left == Condition.horizontal)
                {
                    if (prev.Y == v.Y)
                        return true;
                    else
                    {
                        float old = prev.Y;
                        prev.Y = v.Y;
                        return ApplyConditionLeft(prev, ref step, 0, v.Y - old);
                    }
                }
                else if (v.left == Condition.vertical)
                {
                    if (prev.X == v.X)
                        return true;
                    else
                    {
                        float old = prev.X;
                        prev.X = v.X;
                        return ApplyConditionLeft(prev, ref step, v.X - old, 0);
                    }
                }
            }
            else if (!v.conditionSet)//v.left==Condition.none
            {
                if (!prev.conditionSet)
                {
                    return true;
                }
                else
                {
                    Vertice prev2 = copy[(i - 2+copy.Count) % copy.Count];

                    float A1, B1, C1, A2, B2, C2;
                    A1 = prev.Y - prev2.Y;
                    B1 = prev2.X - prev.X;
                    C1 = prev.X * prev2.Y - prev2.X * prev.Y;

                    A2 = prev.Y - (v.Y - yChange);
                    B2 = (v.X - xChange) - prev.X;
                    C2 = prev.X * (v.Y - yChange) - (v.X - xChange) * prev.Y;

                    if (prev.Y == v.Y - yChange)//same as horizontal
                    {
                        if (A1 == 0)
                        {
                            prev2.Y = prev.Y = v.Y;
                            return true;
                        }

                        prev.X = (B1 * v.Y + C1) / -A1;
                        prev.Y = v.Y;

                        if (prev.Y == prev2.Y)
                        {
                            v.Y++; prev.Y++;
                            prev.X = (B1 * v.Y + C1) / -A1;
                        }

                        return true;
                    }
                    else if (prev.X == v.X - xChange)//same as vertical
                    {
                        if (B1 == 0)
                        {
                            prev2.X = prev.X = v.X;
                            return true;
                        }

                        prev.Y = (A1 * v.X + C1) / -B1;
                        prev.X = v.X;

                        if (prev.X == prev2.X)
                        {
                            v.X++; prev.X++;
                            prev.Y = (A1 * v.X + C1) / -B1;
                        }
                        return true;
                    }
                    else
                    {
                        C2 = -A2 * v.X - B2 * v.Y;
                        (prev.X, prev.Y) = GetIntersection(A1, B1, C1, A2, B2, C2);
                        return true;
                    }
                    return true;
                }
            }
            else//v.conditionSet==true && v.prev==Condition.none
            {
                prev.X += xChange;
                prev.Y += yChange;
                return ApplyConditionLeft(prev, ref step, xChange, yChange);
            }
            return true;
        }

        //-------------------------------------------------------------------------------------------------------
        //                              SELECTING/ADDING EDGE/VERTICE
        //-------------------------------------------------------------------------------------------------------

        private bool SelectEdge(int x,int y)//Done
        {
            if (vertices.Count < 2)
                return false;

            double min =8;
            double cur;
            for(int i=0;i<vertices.Count-1;i++)
            {
                cur = calculateDist(vertices[i], vertices[i + 1], x, y);
                if(cur<min)
                {
                    min = cur;
                    selectedEdge = new Edge(vertices[i], vertices[i + 1]);
                }
            }

            cur = calculateDist(vertices[0], vertices[vertices.Count - 1], x, y);
            if(cur<min)
            {
                min = cur;
                selectedEdge = new Edge(vertices[vertices.Count - 1],vertices[0]);
            }

            if (selectedEdge != null)
            {
                button1.Enabled = button2.Enabled = button3.Enabled = true;
                if (selectedEdge.vertice1.right != Condition.none || selectedEdge.vertice2.left != Condition.none)
                    button6.Enabled = true;
                return true;
            }
                
            return false;
        }
        private bool SelectVertice(int x, int y)//Done
        {
            if (vertices.Count < 1)
                return false;
            selectedVertice = null;
            foreach (Vertice v in vertices)
            {
                if (Math.Sqrt(Math.Pow(v.X - x, 2) + Math.Pow(v.Y - y, 2)) < Vertice.radius)    
                {
                    selectedVertice = v;
                    button4.Enabled = button5.Enabled = true;
                    textBox1.Text = string.Format("{0:0.0}", calculateAngle(selectedVertice));
                    if (selectedVertice.conditionSet)
                        button7.Enabled = true;
                    return true;
                }
            }
            return false;
        }
        public bool CheckIfFinished(int x, int y)//Done
        {
            if (vertices.Count < 3)
                return false;
            if (Math.Sqrt(Math.Pow(vertices[0].X - x, 2) + Math.Pow(vertices[0].Y - y, 2)) < Vertice.radius)
                return true;
            return false;
        }
        public void TryToAddVertice(int x, int y)//Done
        {
            if (finishedDrawing)
                return;
            foreach(Vertice v in vertices)
            {
                if (Math.Sqrt(Math.Pow(v.X - x, 2) + Math.Pow(v.Y - y, 2)) < Vertice.radius)
                    return;
            }
            vertices.Add(new Vertice(x, y));
            int i = vertices.Count - 2;
            if(checkBox1.Checked && vertices.Count>1)
            {
                if (vertices[i].X == vertices[i + 1].X && vertices[i].left!=Condition.vertical)
                {
                    vertices[i].right = vertices[i + 1].left = Condition.vertical;
                }
                else if (vertices[i].Y == vertices[i + 1].Y && vertices[i].left != Condition.horizontal)
                {
                    vertices[i].right = vertices[i + 1].left = Condition.horizontal;
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------
        //                              MOUSE EVENTS
        //-------------------------------------------------------------------------------------------------------

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)//Done
        {
            switch(e.Button)
            {
                case MouseButtons.Left:
                    {
                        //textBox1.Text = $"({e.X},{e.Y})";
                        if(selectedVertice!=null)
                        {
                            lastX = e.X;
                            lastY = e.Y;
                            pictureBox1.Capture = true;
                        }
                        else if (!finishedDrawing)
                        {
                            if (CheckIfFinished(e.X, e.Y))
                            {
                                finishedDrawing = true;
                                break;
                            }
                            TryToAddVertice(e.X, e.Y);
                        }
                        else
                        {//Comment line below to not paint after completing painting
                            TryToAddVertice(e.X, e.Y);
                        }
                    }
                    break;
                case MouseButtons.Right:
                    {
                        textBox1.Text = 0.ToString();
                        selectedEdge = null;
                        selectedVertice = null;
                        button1.Enabled = button2.Enabled = button3.Enabled = button4.Enabled = button5.Enabled =button6.Enabled=button7.Enabled= false;
                        if(!SelectVertice(e.X, e.Y))
                        {
                            SelectEdge(e.X, e.Y);
                        }
                    }
                    break;
            }
            pictureBox1.Refresh();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)//Done?
        {
            if (selectedVertice != null && e.Button == MouseButtons.Left)
            {
                int xChange=e.X-lastX, yChange=e.Y-lastY;
                if(CheckIfCondition(selectedVertice,xChange,yChange))
                {
                    selectedVertice.X += xChange;
                    selectedVertice.Y += yChange;
                    ApplyCondition();

                }
                else
                {
                    MoveWholePolygon(xChange, yChange);
                }

                textBox1.Text = string.Format("{0:0.0}",calculateAngle(selectedVertice));
            }
            lastX = e.X;
            lastY = e.Y;
            pictureBox1.Refresh();
            return;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)//Done
        {
            if (e.Button == MouseButtons.Left)
            {
                pictureBox1.Capture = false;
                if(selectedVertice!=null && checkBox1.Checked)
                {
                    int i = vertices.IndexOf(selectedVertice);
                    Vertice next = vertices[(i + 1) % vertices.Count];
                    Vertice prev = vertices[(i - 1 + vertices.Count) % vertices.Count];
                    if (selectedVertice.X == next.X && selectedVertice.right == Condition.none && next.right != Condition.vertical && selectedVertice.left != Condition.vertical)
                    {
                        selectedVertice.right = next.left = Condition.vertical;
                    }
                    if (selectedVertice.X == prev.X && selectedVertice.left == Condition.none && prev.left != Condition.vertical && selectedVertice.right != Condition.vertical)
                    {
                        selectedVertice.left = prev.right = Condition.vertical;
                    }
                    if (selectedVertice.Y == next.Y && selectedVertice.right == Condition.none && next.right != Condition.horizontal && selectedVertice.left != Condition.horizontal)
                    {
                        selectedVertice.right = next.left = Condition.horizontal;
                    }
                    if (selectedVertice.Y == prev.Y && selectedVertice.left == Condition.none && prev.left != Condition.horizontal && selectedVertice.right != Condition.horizontal)
                    {
                        selectedVertice.left = prev.right = Condition.horizontal;
                    }
                }
            }
                
            if (e.Button != MouseButtons.Left || selectedVertice == null)
                return;
            
            pictureBox1.Refresh();
        }


        //-------------------------------------------------------------------------------------------------------
        //                              PREDEFINED POLYGON
        //-------------------------------------------------------------------------------------------------------

        public void PredefinedApplyCondition(Vertice v, int xChange, int yChange)//Done
        {


            int step = 1;
            if (vertices.Count < 2)
                return;
            int i = vertices.IndexOf(v);
            Vertice next = vertices[(i + 1) % vertices.Count];
            Vertice prev = vertices[(i - 1 + vertices.Count) % vertices.Count];
            if (v.right != Condition.none || v.conditionSet || next.conditionSet)
            {
                ApplyConditionRight(v, ref step, xChange, yChange);
            }
            if (v.left != Condition.none || v.conditionSet || prev.conditionSet)
            {
                ApplyConditionLeft(v, ref step, xChange, yChange);
            }
        }
        private void Predefined()//Done
        {
            selectedEdge = null;
            selectedVertice = null;
            button1.Enabled = button2.Enabled = button3.Enabled = button4.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = false;
            finishedDrawing = true;
            vertices = new List<Vertice>
            {
                new Vertice(300,70),new Vertice(420,70),new Vertice(500,150),new Vertice(460,350),new Vertice(250,350),new Vertice(250,200)
            };
            vertices[0].right = vertices[1].left = Condition.horizontal;
            PredefinedApplyCondition(vertices[0], 0, 0);
            vertices[4].right = vertices[5].left = Condition.vertical;
            PredefinedApplyCondition(vertices[4], 0, 0);
            vertices[2].conditionSet = true;
            vertices[2].angle = calculateAngle(vertices[2]);

            pictureBox1.Refresh();
        }

        //-------------------------------------------------------------------------------------------------------
        //                              PAINT
        //-------------------------------------------------------------------------------------------------------


        private void pictureBox1_Paint(object sender, PaintEventArgs e)//Done
        {
            Graphics g = e.Graphics;
            PaintVertices(g);
            PaintEdges(g);
            PaintSelectedVertice(g);
            PaintSelectedEdge(g);
            PaintCurrentEdge(g);
            if(checkBox1.Checked && selectedVertice!=null)
            {
                PaintSuggestion(g);
            }
        }
        private void PaintSuggestion(Graphics g)
        {
            using (Brush b = new SolidBrush(Color.White))
            {
                if(selectedVertice!=null)
                {
                    Font myFont = new Font(FontFamily.GenericSerif, 12);
                    int i = vertices.IndexOf(selectedVertice);
                    Vertice next = vertices[(i + 1) % vertices.Count];
                    Vertice prev = vertices[(i - 1 + vertices.Count) % vertices.Count];
                    if(selectedVertice.X==next.X && selectedVertice.right==Condition.none && next.right != Condition.vertical && selectedVertice.left != Condition.vertical)
                    {
                        g.DrawString("V?", myFont, b, selectedVertice.X, (selectedVertice.Y + next.Y) / 2);
                    }
                    if (selectedVertice.X == prev.X && selectedVertice.left == Condition.none && prev.left != Condition.vertical && selectedVertice.right != Condition.vertical)
                    {
                        g.DrawString("V?", myFont, b, selectedVertice.X, (selectedVertice.Y + prev.Y) / 2);
                    }
                    if (selectedVertice.Y == next.Y && selectedVertice.right == Condition.none && next.right != Condition.horizontal && selectedVertice.left!=Condition.horizontal)
                    {
                        g.DrawString("H?", myFont, b, (selectedVertice.X+next.X)/2, selectedVertice.Y);
                    }
                    if (selectedVertice.Y == prev.Y && selectedVertice.left == Condition.none && prev.left!=Condition.horizontal && selectedVertice.right!=Condition.horizontal)
                    {
                        g.DrawString("H?", myFont, b, (selectedVertice.X+prev.X)/2, selectedVertice.Y);
                    }
                }
            }
        }
        private void PaintVertices(Graphics g)//Done
        {
            using (Pen p = new Pen(Color.White, 3))
            {
                foreach (Vertice v in vertices)
                {
                    g.FillEllipse(p.Brush, v.X - Vertice.radius / 2, v.Y - Vertice.radius / 2, Vertice.radius, Vertice.radius);
                    if (v.conditionSet)
                    {
                        g.DrawString(string.Format("{0:0.0}", v.angle), new Font(FontFamily.GenericSerif, 12), new SolidBrush(Color.White), v.X, v.Y);
                    }
                }
            }
        }
        private void PaintEdges(Graphics g)//Done
        {
            using (Brush b = new SolidBrush(Color.White))
            {
                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    MyDrawLine(vertices[i].X, vertices[i].Y, vertices[i + 1].X, vertices[i + 1].Y, g, b);
                    if (vertices[i].right == Condition.horizontal)
                    {
                        g.DrawString("H", new Font(FontFamily.GenericSerif, 12), b, (vertices[i].X + vertices[i + 1].X) / 2, vertices[i].Y);
                    }
                    else if (vertices[i].right == Condition.vertical)
                    {
                        g.DrawString("V", new Font(FontFamily.GenericSerif, 12), b, vertices[i].X, (vertices[i].Y + vertices[i + 1].Y) / 2);
                    }
                }
                if (finishedDrawing)
                {
                    MyDrawLine(vertices[0].X, vertices[0].Y, vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y, g, b);
                    if (vertices[vertices.Count - 1].right == Condition.horizontal)
                    {
                        g.DrawString("H", new Font(FontFamily.GenericSerif, 12), b, (vertices[0].X + vertices[vertices.Count - 1].X) / 2, vertices[0].Y);
                    }
                    else if (vertices[vertices.Count - 1].right == Condition.vertical)
                    {
                        g.DrawString("V", new Font(FontFamily.GenericSerif, 12), b, vertices[0].X, (vertices[0].Y + vertices[vertices.Count - 1].Y) / 2);
                    }
                }

            }
        }
        private void PaintCurrentEdge(Graphics g)//Done
        {
            if (vertices.Count < 1 || finishedDrawing)
                return;
            using (Brush b = new SolidBrush(Color.White))
            {
                MyDrawLine(vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y, lastX, lastY, g, b);
                if (checkBox1.Checked)
                {
                    Font myFont = new Font(FontFamily.GenericSerif, 12);
                    if (lastX == vertices[vertices.Count - 1].X && vertices[vertices.Count - 1].left != Condition.vertical)
                    {
                        g.DrawString("V?", myFont, b, lastX, (lastY + vertices[vertices.Count - 1].Y) / 2);
                    }
                    if (lastY == vertices[vertices.Count - 1].Y && vertices[vertices.Count - 1].left != Condition.horizontal)
                    {
                        g.DrawString("H?", myFont, b, (lastX + vertices[vertices.Count - 1].X) / 2, lastY);
                    }
                }
                
            }
        }
        private void PaintSelectedVertice(Graphics g)//Done
        {
            if (selectedVertice != null)
            {
                using (Pen p = new Pen(Color.Red, 3))
                {
                    g.FillEllipse(p.Brush, selectedVertice.X - Vertice.radius / 2, selectedVertice.Y - Vertice.radius / 2, Vertice.radius, Vertice.radius);
                }
            }
        }
        private void PaintSelectedEdge(Graphics g)//Done
        {
            if (selectedEdge != null)
            {
                using (Brush b = new SolidBrush(Color.Red))
                {
                    MyDrawLine(selectedEdge.vertice1.X, selectedEdge.vertice1.Y,selectedEdge.vertice2.X, selectedEdge.vertice2.Y, g, b);
                }
            }

        }


        //-------------------------------------------------------------------------------------------------------
        //                              MATH FUNCTIONS
        //-------------------------------------------------------------------------------------------------------

        public (float x,float y) GetIntersection(float A1,float B1,float C1,float A2,float B2,float C2)
        {
            //A2=/=0,B2=/=0
            float x = (( B1 * C2 / B2 ) - C1) / (A1 - ( A2 * B1 / B2 ));
            float y = (A2*x+C2) / B2;y = -y;
            return (x, y);

        }
        public void MyDrawLine(float u1, float v1, float u2, float v2, Graphics g, Brush b)//Done
        {
            //using (Pen p = new Pen(b))
            //{
            //    g.DrawLine(p, u1, v1, u2, v2);
            //}

            {
                int x1 = (int)Math.Round(u1), y1 = (int)Math.Round(v1), x2 = (int)Math.Round(u2), y2 = (int)Math.Round(v2);
                int d, dx, dy, ai, bi, xi, yi;
                int x = x1, y =y1;
                if (x1 < x2)
                {
                    xi = 1;
                    dx = x2 - x1;
                }
                else
                {
                    xi = -1;
                    dx = x1 - x2;
                }
                if (y1 < y2)
                {
                    yi = 1;
                    dy = y2 - y1;
                }
                else
                {
                    yi = -1;
                    dy = y1 - y2;
                }
                g.FillRectangle(b, x, y, 1, 1);

                if (dx > dy)
                {
                    ai = (dy - dx) * 2;
                    bi = dy * 2;
                    d = bi - dx;
                    while (x != x2)
                    {
                        if (d >= 0)
                        {
                            x += xi;
                            y += yi;
                            d += ai;
                        }
                        else
                        {
                            d += bi;
                            x += xi;
                        }
                        g.FillRectangle(b, x, y, 1, 1);
                    }
                }
                else
                {
                    ai = (dx - dy) * 2;
                    bi = dx * 2;
                    d = bi - dy;
                    while (y != y2)
                    {
                        if (d >= 0)
                        {
                            x += xi;
                            y += yi;
                            d += ai;
                        }
                        else
                        {
                            d += bi;
                            y += yi;
                        }
                        g.FillRectangle(b, x, y, 1, 1);
                    }
                }
            }

        }
        private double calculateDist(Vertice v1, Vertice v2, int x, int y)//Done
        {
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            if ((dx == 0) && (dy == 0))
            {
                dx = x - v1.X;
                dy = y - v1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
            double t = ((x - v1.X) * dx + (y - v1.Y) * dy) / (dx * dx + dy * dy);
            if (t < 0)
            {
                dx = x - v1.X;
                dy = y - v1.Y;
            }
            else if (t > 1)
            {
                dx = x - v2.X;
                dy = y - v2.Y;
            }
            else
            {
                dx = x - (v1.X + t * dx);
                dy = y - (v1.Y + t * dy);
            }
            return Math.Sqrt(dx * dx + dy * dy);
        }
        private bool TurnsLeft(Vertice v1, Vertice v2, Vertice v3)
        {
            float u1 = v2.X - v1.X;
            float u2 = v2.Y - v1.Y;
            float w1 = v3.X - v2.X;
            float w2 = v3.Y - v2.Y;
            return u1 * w2 - u2 * w1 > 0;

        }//Done
        private double calculateCopiedAngle(Vertice v)//Done
        {
            if (copy.Count < 2)
                return 0;
            int i = copy.IndexOf(v);
            Vertice next = copy[(i + 1) % copy.Count];
            Vertice prev = copy[(i + copy.Count - 1) % copy.Count];
            (float x, float y) p2 = (v.X, v.Y), p3 = (next.X, next.Y), p1 = (prev.X, prev.Y);
            double angle = Math.Acos(DotProduct(p1, p2, p3) / (VectorLength(p2, p1) * VectorLength(p2, p3))) * 180 / Math.PI;
            if (copy.Count < 3)
                return angle;
            Vertice pprev = copy[(i + copy.Count - 2) % copy.Count];
            if ((TurnsLeft(pprev, prev, v) != TurnsLeft(prev, v, next)) && calculateCopiedAngle(prev) < 180)
                angle = 360 - angle;
            return angle;
        }
        private double calculateAngle(Vertice v)//Done
        {
            if (vertices.Count < 2)
                return 0;
            int i = vertices.IndexOf(v);
            Vertice next = vertices[(i + 1) % vertices.Count];
            Vertice prev = vertices[(i + vertices.Count - 1) % vertices.Count];
            (float x, float y) p2 = (v.X, v.Y), p3 = (next.X, next.Y), p1 = (prev.X, prev.Y);
            double angle = Math.Acos(DotProduct(p1, p2, p3) / (VectorLength(p2, p1) * VectorLength(p2, p3))) * 180 / Math.PI;
            if (vertices.Count < 3)
                return angle;
            Vertice pprev = vertices[(i + vertices.Count - 2) % vertices.Count];
            if ((TurnsLeft(pprev, prev, v) != TurnsLeft(prev, v, next)) && calculateAngle(prev)<180 )
                angle = 360 - angle;
            return angle;
        }
        private double DotProduct((float x, float y) p1, (float x, float y) p2, (float x, float y) p3)//Done
        {
            float u1 = p1.x - p2.x;
            float u2 = p1.y - p2.y; u2 = -u2;
            float v1 = p3.x - p2.x;
            float v2 = p3.y - p2.y; v2 = -v2;
            return u1 * v1 + u2 * v2;
        }
        private double VectorLength((float x, float y) p1, (float x, float y) p2)//Done
        {
            float u1 = p2.x - p1.x;
            float u2 = p2.y - p1.y;
            return Math.Sqrt(Math.Pow(u1, 2) + Math.Pow(u2, 2));
        }



        //-------------------------------------------------------------------------------------------------------
        //                              BUTTONS SEGMENT
        //-------------------------------------------------------------------------------------------------------


        private void button1_Click(object sender, EventArgs e)//Done
        {
            Vertice temp = new Vertice((int)(0.5 * (selectedEdge.vertice1.X + selectedEdge.vertice2.X)), (int)(0.5 * (selectedEdge.vertice1.Y + selectedEdge.vertice2.Y)));
            vertices.Insert(1 + vertices.IndexOf(selectedEdge.vertice1), temp);
            selectedEdge.vertice1.right = selectedEdge.vertice2.left = Condition.none;
            selectedEdge = null;
            selectedVertice = temp;
            textBox1.Text = calculateAngle(selectedVertice).ToString();
            button1.Enabled = button2.Enabled = button3.Enabled = false;
            button5.Enabled = button4.Enabled = true;
            pictureBox1.Refresh();
        }
        private void button2_Click(object sender, EventArgs e)//Sprobowac zaimplementowac jakby to byl mousemove
        {
            if(TryToSetEdge(selectedEdge,Condition.vertical))
            {
                button6.Enabled = true;
                ApplyCondition();
            }
            else
            {
                selectedEdge.SetCondtion(Condition.none);
                MessageBox.Show("Cannot make this edge " + Condition.vertical);
            }
            pictureBox1.Refresh();
        }
        private void button3_Click(object sender, EventArgs e)//Sprobowac zaimplementowac jakby to byl mousemove
        {
            if (TryToSetEdge(selectedEdge,Condition.horizontal))
            {
                button6.Enabled = true;
                ApplyCondition();
            }
            else
            {
                MessageBox.Show("Cannot make this edge " + Condition.horizontal);
            }
            pictureBox1.Refresh();
        }
        private void button4_Click(object sender, EventArgs e)//Done
        {
            if (selectedVertice.left != Condition.none && selectedVertice.right != Condition.none)
            {
                MessageBox.Show("Cannot set angle due to both adjacent edges having condition");
                return;
            }
            if (double.TryParse(textBox1.Text, out double r))
            {
                if(r<=0 || r>=360)
                {
                    MessageBox.Show("Incorrect angle value");
                    return;
                }
                if(textBox1.Text==string.Format("{0:0.0}",calculateAngle(selectedVertice)))
                {
                    textBox1.Text = string.Format("{0:0.0}", calculateAngle(selectedVertice));

                    button7.Enabled = true;
                    selectedVertice.conditionSet = true;
                    selectedVertice.angle = r;
                }
                else if(TryToSetAngle(selectedVertice,r))
                {
                    ApplyCondition();
                    button7.Enabled = true;
                    selectedVertice.conditionSet = true;
                    selectedVertice.angle = r;
                }
                else
                {
                    MessageBox.Show("Angle cannot be set to: " + r.ToString());
                    return;
                }
                pictureBox1.Refresh();
            }
            else
            {
                MessageBox.Show("Cannot parse " + textBox1.Text + " to double");
            }
        }
        private void button5_Click(object sender, EventArgs e)//Done
        {
            int i = vertices.IndexOf(selectedVertice);
            vertices[(i + 1) % vertices.Count].left = vertices[(i - 1 + vertices.Count) % vertices.Count].right = Condition.none;
            vertices[(i + 1) % vertices.Count].conditionSet = vertices[(i - 1 + vertices.Count) % vertices.Count].conditionSet = false;

            vertices.Remove(selectedVertice);
            if (vertices.Count < 3)
                finishedDrawing = false;
            selectedVertice = null;
            textBox1.Text = 0.ToString();
            button4.Enabled = button5.Enabled = button7.Enabled = false;
            pictureBox1.Refresh();
        }
        private void button6_Click(object sender, EventArgs e)//Done
        {
            selectedEdge.vertice1.right = selectedEdge.vertice2.left = Condition.none;
            button6.Enabled = false;
        }
         private void button7_Click(object sender, EventArgs e)//Done
        {
            selectedVertice.conditionSet = false;
            button7.Enabled = false;
            button4.Enabled = true;
            pictureBox1.Refresh();
        }
        private void button8_Click(object sender, EventArgs e)//Done
        {
            selectedEdge = null;
            selectedVertice = null;
            finishedDrawing = false;
            button1.Enabled = button2.Enabled = button3.Enabled = button4.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = false;
            vertices = new List<Vertice>();
            pictureBox1.Refresh();
        }
        private void button9_Click(object sender, EventArgs e)//Done
        {
            Predefined();
        }



        //-------------------------------------------------------------------------------------------------------
        //                              NOT NEEDED
        //-------------------------------------------------------------------------------------------------------


        private void Form1_KeyDown(object sender, KeyEventArgs e)//Done but button control is preferred
        {
            if (e.KeyCode == Keys.Delete && selectedVertice != null)
            {
                button1_Click(sender, e);
            }
            if (e.KeyCode == Keys.N && selectedEdge != null)
            {
                button5_Click(sender, e);
            }
        }
        private void pictureBox1_SizeChanged(object sender, EventArgs e)//Done
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        }

    }
}
