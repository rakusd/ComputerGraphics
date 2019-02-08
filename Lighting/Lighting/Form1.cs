using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project2
{
    //ZAPEWNE USUWANIE Z AET NIE DZIALA
    public partial class Form1 : Form
    {
        private DirectBitmap FilledWithColor;
        private DirectBitmap FilledWithTexture;
        private DirectBitmap texture;
        private DirectBitmap heightMap;
        private DirectBitmap normalMap;
        private bool firstColor=true, secondColor = true;
        private bool lightFixed = true, disturbanceFixed = true, normalFixed = true,normalFunction=false, rgbOn = false;
        float redLength, blueLength, greenLength;
        private int rgbPow=600;
        private int sphereR=100,sphereH,sphereHinit=40;
        private (float x, float y) spherePos=(303,310);

        private float fd = 0.01f,rgbHeight = 80f;
        (float x, float y, float z) redVector, greenVector, blueVector,redPos,greenPos,bluePos;
        private void UpdateBitmaps()
        {
            if (rgbOn)
            {
                redPos = (pictureBox1.Width / 2, 0, rgbHeight);
                bluePos = (0, pictureBox1.Height, rgbHeight);
                greenPos = (pictureBox1.Width, pictureBox1.Height, rgbHeight);
                redVector = (redPos.x - pictureBox1.Width / 2, redPos.y - pictureBox1.Height / 2, rgbHeight);
                blueVector = (bluePos.x - pictureBox1.Width / 2, bluePos.y - pictureBox1.Height / 2, rgbHeight);
                greenVector = (greenPos.x - pictureBox1.Width / 2, greenPos.y - pictureBox1.Height / 2, rgbHeight);
                redLength = Length(redVector);
                greenLength = Length(greenVector);
                blueLength = Length(blueVector);
            }
            for (int i = 0; i < FilledWithTexture.Width; i++)//ParallelFor not working for some reason
            {
                for (int j = 0; j < FilledWithTexture.Height; j++)
                {
                    FilledWithTexture.SetPixel(i, j, CalculateColor(i, j, false));
                }
            }
            //for (int i = 0; i < FilledWithTexture.Width; i++)//ParallelFor not working for some reason
            //{
            //    for (int j = 0; j < FilledWithTexture.Height; j++)
            //    {
            //        FilledWithColor.SetPixel(i, j, CalculateColor(i, j, true));
            //    }
            //}
            Parallel.For(0, FilledWithColor.Width, index =>
                  {
                      for (int k = 0; k<FilledWithColor.Height; k++)
                      {
                          FilledWithColor.SetPixel(index, k, CalculateColor(index, k, true));
                      }
            });

        }
        private void InitBitmaps()
        {
            Bitmap temp = new Bitmap(Project2.Properties.Resources.texture);
            texture = new DirectBitmap(temp.Width, temp.Height);
            for(int i=0;i<temp.Width;i++)
            {
                for (int j = 0; j < temp.Height; j++)
                    texture.SetPixel(i, j, temp.GetPixel(i, j));
            }
            temp.Dispose();

            temp = new Bitmap(Project2.Properties.Resources.brick_heightmap);
            heightMap = new DirectBitmap(temp.Width, temp.Height);
            for (int i = 0; i < temp.Width; i++)
            {
                for (int j = 0; j < temp.Height; j++)
                    heightMap.SetPixel(i, j, temp.GetPixel(i, j));
            }
            temp.Dispose();

            temp = new Bitmap(Project2.Properties.Resources.normal_map);
            normalMap = new DirectBitmap(temp.Width, temp.Height);
            for (int i = 0; i < temp.Width; i++)
            {
                for (int j = 0; j < temp.Height; j++)
                    normalMap.SetPixel(i, j, temp.GetPixel(i, j));
            }
            temp.Dispose();

            FilledWithColor = new DirectBitmap(pictureBox1.Width, pictureBox1.Height);
            FilledWithTexture = new DirectBitmap(pictureBox1.Width, pictureBox1.Height);
            UpdateBitmaps();
        }
        private Color objectColor = Color.Red;
        private Color lightColor = Color.White;
        private Point[] points;
        Point p;
        private int selectedPoint = -1;
        const int radius = 10;
        public Form1()
        {
 
            InitializeComponent();
            InitBitmaps();
            points = new Point[]  { new Point(10, 10),new Point(60, 30), new Point(30, 60), new Point(100, 150), new Point(150, 40), new Point(250, 80) };
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Refresh();
            comboBox1.SelectedIndex = 0;
        }
        //-------------------------------------------------------THREADING--------------------
        private Thread thread;
        delegate void invoker();
        private void StartDoing()
        {
            sphereH = sphereHinit;
            invoker f = new invoker(this.pictureBox1.Refresh);
            float deg = 0;
            int step = 0;
            while(true)
            {
                step++;
                if(step%12==0)
                    sphereH += 20;
                spherePos = ((float)(sphereR * Math.Cos(deg) +pictureBox1.Width / 2),(float)(pictureBox1.Height/2 -sphereR * Math.Sin(deg)));
                deg+=(float)(Math.PI*15/180);
                DateTime a = DateTime.Now;
                UpdateBitmaps();
                pictureBox1.Invoke(new MethodInvoker(f));
                DateTime b = DateTime.Now;
                Thread.Sleep(1000-(b-a).Milliseconds);
            }
        }
        //-------------------------------------------------------SELECTING POINTS--------------------
        private bool SelectPoint(int x,int y)
        {
            for(int i=0;i<points.Length;i++)
            {
                if (Math.Sqrt(Math.Pow(points[i].X - x, 2) + Math.Pow(points[i].Y - y, 2)) < radius)
                {
                    selectedPoint = i;
                    return true;
                }
            }
            return false;
        }
        //--------------------------------------------------------PAINT------------------------------
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            FillTriangles(g);
            PaintTriangles(g);

        }
        private void PaintTriangles(Graphics g)
        {
            using (Brush b = new SolidBrush(Color.White))
            {
                using (Pen p = new Pen(b))
                {
                    foreach (Point point in points)
                    {
                        g.FillEllipse(b, point.X-radius/2, point.Y-radius/2, radius, radius);
                    }
                    g.DrawLine(p, points[0], points[1]);
                    g.DrawLine(p, points[1], points[2]);
                    g.DrawLine(p, points[2], points[0]);
                    g.DrawLine(p, points[3], points[4]);
                    g.DrawLine(p, points[4], points[5]);
                    g.DrawLine(p, points[5], points[3]);
                }
            }
        }
        private void FillTriangles(Graphics g)
        {
            FillFirstTriangle(g);
            FillSecondTriangle(g);
        }
        private void FillFirstTriangle(Graphics g)
        {
            int[] ind = { 0, 1, 2 };
            SortIndexes(ind);
            int ymin = points[ind[0]].Y;
            int ymax = points[ind[ind.Length - 1]].Y;
            int k = 0;
            Aet head = null;
            using (DirectBitmap b = new DirectBitmap(pictureBox1.Width, pictureBox1.Height))
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    while (k < 3 && points[ind[k]].Y == y)
                    {
                        int prev = (ind[k] + 3 - 1) % 3;
                        int next = (ind[k] + 1) % 3;
                        if (points[next].Y > points[ind[k]].Y)
                        {
                            int ymaximum = points[next].Y;
                            float x = points[ind[k]].X;
                            float step = points[next].X == points[ind[k]].X  ? 0 : CalculateM(points[next], points[ind[k]]);
                            AddAet(ref head, new Aet(ymaximum, x, step));
                        }
                        else if(points[next].Y!=points[ind[k]].Y)
                        {
                            int ymaximum = points[ind[k]].Y;
                            float x = points[next].X;
                            float step = points[next].X == points[ind[k]].X  ? 0 : CalculateM(points[ind[k]], points[next]);
                            DeleteAet(ref head, ymaximum, x, step);
                        }
                        if (points[prev].Y > points[ind[k]].Y)
                        {
                            int ymaximum = points[prev].Y;
                            float x = points[ind[k]].X;
                            float step = points[prev].X == points[ind[k]].X  ? 0 : CalculateM(points[prev], points[ind[k]]);
                            AddAet(ref head, new Aet(ymaximum, x, step));
                        }
                        else if(points[prev].Y!=points[ind[k]].Y)
                        {
                            int ymaximum = points[ind[k]].Y;
                            float x = points[prev].X;
                            float step = points[prev].X == points[ind[k]].X ? 0 : CalculateM(points[ind[k]], points[prev]);
                            DeleteAet(ref head, ymaximum, x, step);
                        }
                        k++;
                    }
                    SortAet(ref head);
                    PaintAet(head, b, y,firstColor);
                    ActualizeAet(head);
                }
                g.DrawImage(b.Bitmap, 0, 0);
            }
        }
        private void FillSecondTriangle(Graphics g)
        {
            int[] ind = { 3, 4, 5 };
            SortIndexes(ind);
            int ymin = points[ind[0]].Y;
            int ymax = points[ind[ind.Length - 1]].Y;
            int k = 0;
            Aet head = null;
            using (DirectBitmap b = new DirectBitmap(pictureBox1.Width, pictureBox1.Height))
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    while (k < 3 && points[ind[k]].Y == y)
                    {
                        int prev = 3+(ind[k] + 3 - 1) % 3;
                        int next = 3+(ind[k] + 1) % 3;
                        if (points[next].Y > points[ind[k]].Y)
                        {
                            int ymaximum = points[next].Y;
                            float x = points[ind[k]].X;
                            float step = points[next].X == points[ind[k]].X ? 0 : CalculateM(points[next], points[ind[k]]);
                            AddAet(ref head, new Aet(ymaximum, x, step));
                        }
                        else if (points[next].Y != points[ind[k]].Y)
                        {
                            int ymaximum = points[ind[k]].Y;
                            float x = points[next].X;
                            float step = points[next].X == points[ind[k]].X ? 0 : CalculateM(points[ind[k]], points[next]);
                            DeleteAet(ref head, ymaximum, x, step);
                        }
                        if (points[prev].Y > points[ind[k]].Y)
                        {
                            int ymaximum = points[prev].Y;
                            float x = points[ind[k]].X;
                            float step = points[prev].X == points[ind[k]].X ? 0 : CalculateM(points[prev], points[ind[k]]);
                            AddAet(ref head, new Aet(ymaximum, x, step));
                        }
                        else if (points[prev].Y != points[ind[k]].Y)
                        {
                            int ymaximum = points[ind[k]].Y;
                            float x = points[prev].X;
                            float step = points[prev].X == points[ind[k]].X ? 0 : CalculateM(points[ind[k]], points[prev]);
                            DeleteAet(ref head, ymaximum, x, step);
                        }
                        k++;
                    }
                    SortAet(ref head);
                    PaintAet(head, b, y,secondColor);
                    ActualizeAet(head);
                }
                g.DrawImage(b.Bitmap, 0, 0);
            }
        }
        //-----------------------------------------------------------LOGIC
        private void DeleteAet(ref Aet head,int ymaximum,float x, float step)
        {
            if(head.Ymax==ymaximum && head.Step==step)
            {
                head = head.Next;
                return;
            }
            Aet temp = head;
            while(temp.Next!=null)
            {
                if(temp.Next.Ymax==ymaximum && temp.Next.Step==step)
                {
                    temp.Next = temp.Next.Next;
                    return;
                }
                temp = temp.Next;
            }
        }
        private float CalculateM(Point end,Point start)
        {
            float x2 = end.X;
            float y2 = end.Y;
            float x1 = start.X;
            float y1 = start.Y;
            return 1 / ((y2 - y1) / (x2 - x1));
        }
        private void AddAet(ref Aet head,Aet added)
        {
            if (head == null )
            {
                head = added;
                return;
            }
            Aet temp = head;
            while (temp.Next != null)
                temp = temp.Next;
            temp.Next = added;
        }
        private void SortAet(ref Aet head)
        {
            if (head == null || head.Next==null)
                return;
            if(head.X>head.Next.X)
            {
                Aet temp = head.Next;
                temp.Next = head;
                head.Next = null;
                head = temp;
            }
        }
        private void PaintAet(Aet head,DirectBitmap b,int y,bool colored)
        {
            if (head == null || head.Next==null)
                return;
            int start = (int)Math.Ceiling(head.X);
            int end = (int)Math.Ceiling(head.Next.X);
            if (colored)
                Parallel.For(start, end + 1, index => b.SetPixel(index, y, FilledWithColor.GetPixel(index, y)));
            else
                Parallel.For(start, end + 1, index => b.SetPixel(index, y, FilledWithTexture.GetPixel(index, y)));
        }
        private Color CalculateColor(int x,int y,bool colored)
        {
            (float x, float y, float z) N, L, D, Nprim;
            float cos;
            int red, green, blue;
            N = (0, 0, 1);
            L = (0, 0, 1);
            D = (0, 0, 0);
            if (!lightFixed)
                L = GetL(x, y);
            if (!normalFixed)
                N = GetN(x, y);

            if (normalFunction)
                N = GetFromNormalFunction(x, y);

            if (!disturbanceFixed)
                D = GetD(x, y,N);
            Nprim = Normalize((N.x+D.x,N.y+D.y,N.z+D.z));
            cos = CalculateCos(Nprim, L);
            if (cos < 0)
                return Color.Black;
            if (!colored)
            {
                objectColor = texture.GetPixel(x % texture.Width, y % texture.Height);
            }
            else
            {
                objectColor = pictureBox3.BackColor;
            }
            red = (int)(cos * objectColor.R * lightColor.R / 255);
            green = (int)(cos * objectColor.G * lightColor.G / 255);
            blue = (int)(cos * objectColor.B * lightColor.B / 255);
            if(rgbOn)
            {
                ApplyRGB(ref red, ref green, ref blue, x, y,objectColor,Nprim);
            }
            return Color.FromArgb(red, green, blue);
        }
        private void ApplyRGB(ref int red,ref int green,ref int blue,int x,int y,Color objectColor, (float x, float y, float z) Nprim)
        {
            int red2, green2, blue2;
            (float x, float y, float z) curRedVector, curGreenVector, curBlueVector;
            curRedVector = (redPos.x - x, redPos.y - y, redPos.z);
            curGreenVector = (greenPos.x - x, greenPos.y - y, greenPos.z);
            curBlueVector = (bluePos.x - x, bluePos.y - y, bluePos.z);
            
            red2 = (int)( 255 * Math.Pow(CalculateCos(redVector, curRedVector) / (redLength * Length(curRedVector)), rgbPow));
            green2= (int)( 255 * Math.Pow(CalculateCos(greenVector, curGreenVector)/(greenLength*Length(curGreenVector)), rgbPow));
            blue2 = (int)( 255 * Math.Pow(CalculateCos(blueVector, curBlueVector)/(blueLength*Length(curBlueVector)), rgbPow));

            float cosR = CalculateCos(Normalize(curRedVector), Nprim);
            float cosB =  CalculateCos(Normalize(curBlueVector), Nprim);
            float cosG = CalculateCos(Normalize(curGreenVector), Nprim);
            if (cosR>0)
                red += (int)(red2 * objectColor.R * cosR / 255);
            if(cosG>0)
                green += (int)(green2 * objectColor.G * cosG / 255);
            if(cosB>0)
                blue += (int)(blue2 * objectColor.B * cosB / 255);

            if (red > 255)
                red = 255;
            if (blue > 255)
                blue = 255;
            if (green > 255)
                green = 255;
                
        }
        private float Length((float x, float y, float z) v) => (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        private float CalculateCos((float x, float y, float z) u, (float x, float y,float z) v)
        {
            return u.x * v.x + u.y * v.y + u.z * v.z;
        }
        private (float x,float y,float z) Normalize((float x,float y,float z) v)
        {
            float len=(float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            if (len == 0)
                return (0, 0, 0);
            return (v.x / len, v.y / len, v.z / len);
        }
        private (float x,float y,float z) GetL(int x,int y)
        {
            (float x,float y,float z) L=(spherePos.x-x, spherePos.y-y, sphereH);
            return Normalize(L);
        }
        private (float x, float y, float z) GetN(int x, int y)
        {
            Color c = normalMap.GetPixel(x%normalMap.Width, y%normalMap.Height);
            int red = c.R, green = c.G, blue = c.B;

            if (green == 255)
                green--;
            if (red == 255)
                red--;

            float x3 = (float)blue / 255;
            float x2 = (float)green / 127 - 1;
            float x1 = (float)red / 127 - 1;
            if (x3 == 0)
                return (x1, x2, x3);
            return (x1 / x3, x2 / x3, 1);
        }

        float A=20f, alfa=0.01f, B=0.1f, beta=0.2f;

        private float FunctionZx(int x,int y)
        {
            //A*sin(alfa*x+B*sin(beta)*y)
            //A*cos(alfa*x+B*sin(beta*y)*alfa
            return (float)(A * Math.Cos(alfa * x + B * Math.Sin(beta) * y)*alfa);
            
        }
        private float FunctionZy(int x, int y)
        {
            //A*sin(alfa*x+B*sin(beta)*y)
            //A*cos(alfa*x+B*sin(beta)*y)+B*sin(beta)
            return (float)(A * Math.Cos(alfa * x + B * Math.Sin(beta) * y)*B*Math.Sin(beta));

        }
        private (float x, float y, float z) CrossProduct((float x, float y, float z) u, (float x, float y, float z) v)
        {
            (float x,float y,float z) result = (u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
            if (result.z == 0)
                return result;
            return (result.x / result.z, result.y / result.z, 1);
        }
        private (float x,float y,float z) GetFromNormalFunction(int x,int y)
        {
            (float x, float y, float z) Zx, Zy;
            Zx = (1, 0, FunctionZx(x, y));
            Zy = (0, 1, FunctionZy(x, y));
            return CrossProduct(Zx, Zy);

        }
        private (float x, float y, float z) GetD(int x, int y,(float x,float y,float z) N)
        {
            if(N.z==0)
                return (0, 0, 0);
            (float x, float y, float z) T, B;
            T = (1, 0, -N.x);
            B = (0, 1, -N.y);
            float dx, dy;
            Color cur=heightMap.GetPixel(x%heightMap.Width,y%heightMap.Height), nextX=heightMap.GetPixel((x+1)%heightMap.Width,y%heightMap.Height), nextY=heightMap.GetPixel(x%heightMap.Width,(y+1)%heightMap.Height);
            dx = nextX.R - cur.R;
            dy = nextY.R - cur.R;
            return (fd*(dx * T.x + dy * B.x), fd*(dx * T.y + dy * B.y),fd*( dx * T.z + dy * B.z));
            
        }
        private void ActualizeAet(Aet head)
        {
            if (head == null)
                return;
            Aet help = head;
            while(help!=null)
            {
                help.X += help.Step;
                help = help.Next;
            }
        }
        private void SortIndexes(int[] ind)
        {
            for(int i=0;i<ind.Length;i++)
            {
                for(int j=0;j<ind.Length-1;j++)
                {
                    if(points[ind[j]].Y>points[ind[j+1]].Y)
                    {
                        int swap = ind[j];
                        ind[j] = ind[j + 1];
                        ind[j + 1] = swap;
                    }
                }
            }
        }


        //-----------------------------------------------------------MOUSE EVENTS
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(selectedPoint>=0 && selectedPoint<=5)
            {
                points[selectedPoint].X = e.X;
                points[selectedPoint].Y = e.Y;
                if (e.X < 0)
                    points[selectedPoint].X = 0;
                if (e.X > pictureBox1.Width)
                    points[selectedPoint].X = pictureBox1.Width;
                if (e.Y < 0)
                    points[selectedPoint].Y = 0;
                if (e.Y > pictureBox1.Height)
                    points[selectedPoint].Y = pictureBox1.Height;
                pictureBox1.Refresh();
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            selectedPoint = -1;
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    SelectPoint(e.X, e.Y);
                    break;
            }
        }



        //-------------------------------------------------BUTTON EVENTS
        private void button2_Click(object sender, EventArgs e)
        {
            var temp = new ColorDialog();
            if (temp.ShowDialog() == DialogResult.OK)
            {
                objectColor = temp.Color;
                pictureBox3.BackColor = temp.Color;
                UpdateBitmaps();
                pictureBox1.Refresh();
            }
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            normalFixed = rb.Checked ? false : true;
            normalFunction = radioButton1.Checked ? true : false;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if(!int.TryParse(textBox1.Text,out int result))
            {
                MessageBox.Show("Invalid value. Only integers are allowed");
                return;
            }
            sphereR = result;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var temp = new ColorDialog();
            if (temp.ShowDialog() == DialogResult.OK)
            {
                lightColor = temp.Color;
                pictureBox2.BackColor = temp.Color;
                UpdateBitmaps();
                pictureBox1.Refresh();
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Image files (*.bmp;*.jpg,*.jpeg,*.png) | *.bmp;*.jpg;*.jpeg;*.png";
            if(opf.ShowDialog()==DialogResult.OK)
            {
                using (Bitmap temp = new Bitmap(opf.FileName))
                {
                    pictureBox4.ImageLocation = opf.FileName;
                    texture = new DirectBitmap(temp.Width, temp.Height);
                    for (int i = 0; i < temp.Width; i++)
                    {
                        for (int j = 0; j < temp.Height; j++)
                            texture.SetPixel(i, j, temp.GetPixel(i, j));
                    }
                }
                UpdateBitmaps();
            }
            pictureBox1.Refresh();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Image files (*.bmp;*.jpg,*.jpeg,*.png) | *.bmp;*.jpg;*.jpeg;*.png";
            if (opf.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap temp = new Bitmap(opf.FileName))
                {
                    pictureBox5.ImageLocation = opf.FileName;
                    heightMap = new DirectBitmap(temp.Width, temp.Height);
                    for (int i = 0; i < temp.Width; i++)
                    {
                        for (int j = 0; j < temp.Height; j++)
                            heightMap.SetPixel(i, j, temp.GetPixel(i, j));
                    }
                }
                UpdateBitmaps();
            }
            pictureBox1.Refresh();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Image files (*.bmp;*.jpg,*.jpeg,*.png) | *.bmp;*.jpg;*.jpeg;*.png";
            if (opf.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap temp = new Bitmap(opf.FileName))
                {
                    pictureBox6.ImageLocation = opf.FileName;
                    normalMap = new DirectBitmap(temp.Width, temp.Height);
                    for (int i = 0; i < temp.Width; i++)
                    {
                        for (int j = 0; j < temp.Height; j++)
                            normalMap.SetPixel(i, j, temp.GetPixel(i, j));
                    }
                }
                UpdateBitmaps();
            }
            pictureBox1.Refresh();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedIndex)
            {
                case 0:
                    firstColor = true;
                    secondColor = true;
                    break;
                case 1:
                    firstColor = true;
                    secondColor = false;
                    break;
                case 2:
                    firstColor = false;
                    secondColor = true;
                    break;
                case 3:
                    firstColor = false;
                    secondColor = false;
                    break;
            }
            pictureBox1.Refresh();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if(!float.TryParse(textBox3.Text,out float result))
            {
                MessageBox.Show("Invalid value.");
                return;
            }
            rgbHeight = result;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox4.Text, out int result))
            {
                MessageBox.Show("Invalid value. Only integers are allowed");
                return;
            }
            rgbPow = result;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if(!float.TryParse(textBox2.Text,out float result))
            {
                MessageBox.Show("Invalid value");
                textBox2.Text = fd.ToString();
                return;
            }
            fd = result;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            rgbOn = checkBox1.Checked == true ? true : false;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            lightFixed = rb.Checked ? true : false;
            if(!lightFixed)
            {
                thread = new Thread(new ThreadStart(this.StartDoing));
                thread.Start();
            }
            else
            {
                thread.Abort();
                UpdateBitmaps();
            }
            pictureBox1.Refresh();
        }
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            disturbanceFixed = rb.Checked ? true : false;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }
        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            normalFixed = rb.Checked ? true : false;
            normalFunction = radioButton1.Checked ? true : false;
            UpdateBitmaps();
            pictureBox1.Refresh();
        }
    }
}
