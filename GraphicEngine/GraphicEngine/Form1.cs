using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Project_4.Controllers;
using Project_4.Shaders;

namespace Project_4
{

    public partial class Form1 : Form
    {
        readonly double sphereR = 0.7;
        readonly int norm = 2;
        readonly int rscale = 2;
        Vector cameraFront = DenseVector.OfArray(new double[] { 0, 8, 0.5 });
        Vector cameraBirdsEye = DenseVector.OfArray(new double[] { 0.5, 0.5, 10 });
        Vector cameraPos = DenseVector.OfArray(new double[] { 8, 8, 2 });
        Vector upVector = DenseVector.OfArray(new double[] { 0, 0, 1 });
        Vector cameraTarget = DenseVector.OfArray(new double[] { 0, 0, 0 });

        Vector directionalLight = DenseVector.OfArray(new double[] { 0, 8, 8, 0 });

        Matrix[] models;

        List<Triangle> triangles,fillableTriangles;
        Matrix projectionM, viewM; 
        IShader shader = new GouraudShader();
        double alfa=0;
        FillController fillController;

        public Form1()
        {
            InitializeComponent();
            InitMatrixes();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            SelectViewMatrix();
            SelectShadingModel();

            fillController = new FillController();


            fillController.L = AetController.L = (Vector)viewM.Multiply(directionalLight);
            fillController.ReflectorPos = AetController.ReflectorPos = (Vector)viewM.Multiply(models[2]).Multiply(DenseVector.OfArray(new double[] { 0, 0, 0, 1 }));
            fillController.ReflectorTarget = AetController.ReflectorTarget = (Vector)viewM.Multiply(models[1]).Multiply(DenseVector.OfArray(new double[] { 0, 0, 0, 1 }));
            
            GenerateTrianglesList();
        }
        private void GenerateTrianglesList()
        {
            triangles = new List<Triangle>();
            int stepX = 10;
            int stepY = 10;
            for(int i=-180;i<180;i+=stepX)
            {
                for (int j = -90; j < 90; j += stepY)
                {
                    //i +=10 more to the right
                    //j +=10 higher
                    double x1 = sphereR * Math.Cos(Math.PI * i / 180) * Math.Cos(Math.PI * j / 180);
                    double y1= sphereR * Math.Sin(Math.PI * i / 180) * Math.Cos(Math.PI * j / 180);
                    double z1= sphereR*Math.Sin(Math.PI * j / 180);

                    double x2 = sphereR * Math.Cos(Math.PI * (i+stepX) / 180) * Math.Cos(Math.PI * j / 180);
                    double y2 = sphereR * Math.Sin(Math.PI * (i+stepX) / 180) * Math.Cos(Math.PI * j / 180);
                    double z2 = sphereR * Math.Sin(Math.PI * j / 180);

                    double x3 = sphereR * Math.Cos(Math.PI * i / 180) * Math.Cos(Math.PI * (j+stepY) / 180);
                    double y3 = sphereR * Math.Sin(Math.PI * i / 180) * Math.Cos(Math.PI * (j+stepY) / 180);
                    double z3 = sphereR * Math.Sin(Math.PI * (j+stepY) / 180);

                    double x4 = sphereR * Math.Cos(Math.PI * (i+stepX) / 180) * Math.Cos(Math.PI * (j+stepY) / 180);
                    double y4 = sphereR * Math.Sin(Math.PI * (i+stepX) / 180) * Math.Cos(Math.PI * (j+stepY) / 180);
                    double z4 = sphereR * Math.Sin(Math.PI * (j+stepY) / 180);

                    Vector v1= DenseVector.OfArray(new double[] { x1, y1, z1, 1 });
                    Vector v2 = DenseVector.OfArray(new double[] { x2, y2, z2, 1 });
                    Vector v3= DenseVector.OfArray(new double[] { x3, y3, z3, 1 });
                    Vector v4= DenseVector.OfArray(new double[] { x4, y4, z4, 1 });

                    Vector n1 = (Vector)v1.Divide(sphereR); n1.ClearSubVector(3, 1);
                    Vector n2 = (Vector)v2.Divide(sphereR); n2.ClearSubVector(3, 1);
                    Vector n3 = (Vector)v3.Divide(sphereR); n3.ClearSubVector(3, 1);
                    Vector n4 = (Vector)v4.Divide(sphereR); n4.ClearSubVector(3, 1);

                    triangles.Add(new Triangle(v3,v4,v1,Color.White,n3,n4,n1));
                    triangles.Add(new Triangle(v1,v2,v4,Color.White,n1,n2,n4));
                }
            }
                
        }
        private void InitMatrixes()
        {
            double fov = Math.PI / 4;
            double e = 1 / Math.Tan(fov / 2);
            double n = 1;
            double f = 100;

            double a = (double)pictureBox1.Height / (double)pictureBox1.Width;

            projectionM = DenseMatrix.OfArray(new double[,] { { e, 0, 0, 0 }, { 0, e / a, 0, 0 }, { 0, 0, -((f + n) / (f - n)), -2 * f * n / (f - n) }, { 0, 0, -1, 0 } });

            models = new Matrix[3];
            models[0] = DenseMatrix.OfArray(new double[,] { { 1, 0, 0, rscale * Math.Cos(alfa) }, { 0, 1, 0, rscale * Math.Sin(alfa) }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });//orbitin
            models[1]= DenseMatrix.OfArray(new double[,] { { Math.Cos(alfa), -Math.Sin(alfa), 0, 0 }, { Math.Sin(alfa), Math.Cos(alfa), 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
            models[2] = DenseMatrix.OfArray(new double[,] { { 1, 0, 0, Math.Cos(alfa) }, { 0, 1, 0, Math.Sin(alfa) }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
            models[2]= (DenseMatrix)(models[0] * DenseMatrix.OfArray(new double[,] { { 0.2, 0, 0, 0 }, { 0, 0.2, 0, 0 }, { 0, 0, 0.2, 0 }, { 0, 0, 0, 1 } }) * DenseMatrix.OfArray(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 5 }, { 0, 0, 0, 1 } }));

        }

        private void SelectShadingModel()
        {
            switch(comboBox2.SelectedIndex)
            {
                case 0:
                    shader = new GouraudShader();
                    break;
                case 1:
                    shader = new PhongShader();
                    break;
            }
            AetController.Shader = shader;
        }

        private void SelectViewMatrix()
        {
            switch(comboBox1.SelectedIndex)
            {
                case 0:
                    viewM = GenerateViewMatrix(cameraFront, DenseVector.OfArray(new double[]{ 0, 0, 0 }), upVector);
                    break;
                case 1:
                    viewM = GenerateViewMatrix(cameraBirdsEye, DenseVector.OfArray(new double[] {0, 0, 0 }), upVector);
                    break;
                case 2:
                    viewM = GenerateViewMatrix(DenseVector.OfArray(new double[] { cameraTarget[0] + 5, cameraTarget[1], cameraTarget[2] + 8 }),
                        cameraTarget, upVector);
                    break;
                case 3:
                    viewM = GenerateViewMatrix(cameraPos, cameraTarget, upVector);
                    break;
            }
        }
        private DenseMatrix GenerateViewMatrix(Vector cameraPos,Vector cameraTarget,Vector upVector)
        {
            upVector=(Vector)upVector.Normalize(norm);

            Vector zAxis = (Vector)(cameraPos - cameraTarget);
            zAxis = (Vector)zAxis.Normalize(norm);


            Vector xAxis = upVector.CrossProduct(zAxis);
            xAxis = (Vector)xAxis.Normalize(norm);

            Vector yAxis = zAxis.CrossProduct(xAxis);
            yAxis = (Vector)yAxis.Normalize(norm);

            DenseMatrix preresult = DenseMatrix.OfArray(new double[,] { { xAxis[0], yAxis[0], zAxis[0], cameraPos[0] },
                { xAxis[1], yAxis[1], zAxis[1], cameraPos[1] },
                { xAxis[2], yAxis[2], zAxis[2], cameraPos[2] },
                { 0, 0, 0, 1 } });

            DenseMatrix result =  (DenseMatrix)preresult.Inverse();

            return result;


        }
        private void Draw(Graphics g)
        {
            SelectViewMatrix();

            fillableTriangles = new List<Triangle>();
            Color[] colors = new Color[] { Color.FromArgb(240, 100, 100), Color.FromArgb(100, 240, 100), Color.FromArgb(100, 100, 240) };

            fillController.L = AetController.L = (Vector)viewM.Multiply(directionalLight);
            fillController.ReflectorPos = AetController.ReflectorPos = (Vector)viewM.Multiply(models[2]).Multiply(DenseVector.OfArray(new double[] { 0, 0, 0, 1 }));
            fillController.ReflectorTarget = AetController.ReflectorTarget = (Vector)viewM.Multiply(models[1]).Multiply(DenseVector.OfArray(new double[] { 0, 0, 0, 1 }));
            int counter = 0;
            foreach (Matrix model in models)
            {
                foreach(var triangle in triangles)
                {
                    Vector p1 = triangle.P1;
                    Vector p2 = triangle.P2;
                    Vector p3 = triangle.P3;

                    Matrix transform = (Matrix)viewM.Multiply(model);
                    Matrix normalTransform = (Matrix)transform.Transpose().Inverse();

                    Vector p1e = vertexShader(projectionM, viewM, model, p1);
                    Vector p2e = vertexShader(projectionM, viewM, model, p2);
                    Vector p3e = vertexShader(projectionM, viewM, model, p3);

                    double x1 = p1e[0] / p1e[3];
                    double y1 = p1e[1] / p1e[3];
                    double z1 = p1e[2] / p1e[3];

                    double x2 = p2e[0] / p2e[3];
                    double y2 = p2e[1] / p2e[3];
                    double z2 = p2e[2] / p2e[3];

                    double x3 = p3e[0] / p3e[3];
                    double y3 = p3e[1] / p3e[3];
                    double z3 = p3e[2] / p3e[3];

                    if (x1 <= 1 && x1 >= -1 && y1 <= 1 && y1 >= -1 && z1 <= 1 && z1 >= -1 &&
                       x2 <= 1 && x2 >= -1 && y2 <= 1 && y2 >= -1 && z2 <= 1 && z2 >= -1 &&
                       x3 <= 1 && x3 >= -1 && y3 <= 1 && y3 >= -1 && z3 <= 1 && z3 >= -1)
                    {
                        Vector p1new = new DenseVector(new double[] { ScaleX(x1), ScaleY(y1), z1 });
                        Vector p2new = new DenseVector(new double[] { ScaleX(x2), ScaleY(y2), z2 });
                        Vector p3new = new DenseVector(new double[] { ScaleX(x3), ScaleY(y3), z3 });

                        Vector v1 = (Vector)transform.Multiply(p1);
                        Vector v2 = (Vector)transform.Multiply(p2);
                        Vector v3 = (Vector)transform.Multiply(p3);

                        Vector n1 = (Vector)normalTransform.Multiply(triangle.N1);
                        Vector n2 = (Vector)normalTransform.Multiply(triangle.N2);
                        Vector n3 = (Vector)normalTransform.Multiply(triangle.N3);

                        fillableTriangles.Add(new Triangle(p1new, p2new, p3new, colors[counter], n1, n2, n3, v1, v2, v3));
                    }
                }
                counter++;
            }

            using (DirectBitmap b = new DirectBitmap(pictureBox1.Width, pictureBox1.Height))
            {
                fillController.InitBuffer(b.Width, b.Height);
                fillController.FillTriangles(b,fillableTriangles,g);
            }
                
        }

        private float ScaleX(double val)
        {
            //NewValue = (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin
            val = 0 + (val + 1) * (pictureBox1.Width - 0) / (1 + 1);
            return (float)val;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectShadingModel();
            pictureBox1.Refresh();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectViewMatrix();
            pictureBox1.Refresh();
        }

        private float ScaleY(double val)
        {
            //NewValue = (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin
            val = 0 + (val + 1) * (pictureBox1.Height - 0) / (1 + 1);
            val = pictureBox1.Height - val;
            return (float)val;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Draw(e.Graphics);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox3.SelectedIndex)
            {
                case 0:
                    ColorHelper.foggy = false;
                    ColorHelper.lightColor = Color.White;
                    break;
                case 1:
                    ColorHelper.foggy = false;
                    ColorHelper.lightColor = Color.Black;
                    break;
                case 2:
                    ColorHelper.foggy = true;
                    ColorHelper.lightColor = Color.White;
                    break;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            switch(checkBox1.Checked)
            {
                case true:
                    ColorHelper.reflectorColor = Color.White;
                    break;
                case false:
                    ColorHelper.reflectorColor = Color.Black;
                    break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            alfa += (5.0/180.0)*Math.PI;
            if (alfa >= 2 * Math.PI)
                alfa -= 2 * Math.PI;

            models[0] = DenseMatrix.OfArray(new double[,] { { 1, 0, 0, rscale * Math.Cos(alfa) }, { 0, 1, 0, rscale * Math.Sin(alfa) }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });//orbitin
            models[1] = DenseMatrix.OfArray(new double[,] { { Math.Cos(alfa), -Math.Sin(alfa), 0, 0 }, { Math.Sin(alfa), Math.Cos(alfa), 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
            models[2] = DenseMatrix.OfArray(new double[,] { { 1, 0, 0, Math.Cos(-alfa) }, { 0, 1, 0, Math.Sin(-alfa) }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
            models[2] = (DenseMatrix)(models[2] * DenseMatrix.OfArray(new double[,] { { 0.2, 0, 0, 0 }, { 0, 0.2, 0, 0 }, { 0, 0, 0.2, 0 }, { 0, 0, 0, 1 } }) * DenseMatrix.OfArray(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 5 }, { 0, 0, 0, 1 } }));

            cameraTarget = DenseVector.OfArray(new double[] { 0, 0, 0, 1 });
            cameraTarget =(Vector)(models[0] * cameraTarget);
            cameraTarget = DenseVector.OfArray(new double[] { cameraTarget[0], cameraTarget[1], cameraTarget[2] });

            pictureBox1.Refresh();
        }

        private Vector vertexShader(Matrix projectionM, Matrix viewM,Matrix modelM,Vector v)
        {
            return (Vector)(projectionM * viewM * modelM * v);
        }
    }
}
