using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;



namespace Project3
{
    public partial class Form1 : Form
    {
        private void Form1_Load_1(object sender, EventArgs e)
        {
           // AllocConsole();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        //-----------------------------------------DISABBLE CONSOLE


        private Profile sRGB, adobeRGB, appleRGB, cieRGB, wideGamut, pal_secam, custom;
        private Profile source, output;
        private DirectBitmap inBitmap, outBitmap;
        private bool generated;

        public Form1()
        {
            InitializeComponent();
            InitProfiles();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
            InitBitmap();
        }
        //--------INITIALIZATION OF VALUES AND TEXTFIELDS
        private void InitBitmap()
        {
            Bitmap temp = pictureBox1.Image as Bitmap;
            inBitmap = new DirectBitmap(temp.Width, temp.Height);

            for (int i = 0; i < temp.Width; i++)
            {
                for (int j = 0; j < temp.Height; j++)
                {
                    inBitmap.SetPixel(i, j, temp.GetPixel(i, j));
                }
            }
            pictureBox1.Image = inBitmap.Bitmap;
        }
        private void InitProfiles()
        {
            sRGB = new Profile(2.20,
                DenseVector.OfArray(new double[] { 0.3127, 0.3290, 0.3583 }),
                DenseVector.OfArray(new double[] { 0.64, 0.33, 0.03 }),
                DenseVector.OfArray(new double[] { 0.3, 0.6, 0.1 }),
                DenseVector.OfArray(new double[] { 0.15, 0.06, 0.79 }));
            adobeRGB = new Profile(2.20,
                DenseVector.OfArray(new double[] { 0.3127, 0.3290, 0.3583 }),
                DenseVector.OfArray(new double[] { 0.64, 0.33, 0.03 }),
                DenseVector.OfArray(new double[] { 0.21, 0.71, 0.08 }),
                DenseVector.OfArray(new double[] { 0.15, 0.06, 0.79 }));
            appleRGB = new Profile(1.80,
                DenseVector.OfArray(new double[] { 0.3127, 0.3290, 0.3583 }),
                DenseVector.OfArray(new double[] { 0.625, 0.34, 0.035 }),
                DenseVector.OfArray(new double[] { 0.28, 0.595, 0.125 }),
                DenseVector.OfArray(new double[] { 0.155, 0.07, 0.775 }));
            cieRGB = new Profile(2.20,
                DenseVector.OfArray(new double[] { 0.3333, 0.3333, 0.3334 }),//z=0.3333
                DenseVector.OfArray(new double[] { 0.7350, 0.2650, 0.0 }),
                DenseVector.OfArray(new double[] { 0.274, 0.717, 0.009 }),
                DenseVector.OfArray(new double[] { 0.167, 0.009, 0.824 }));
            wideGamut = new Profile(1.20,
                DenseVector.OfArray(new double[] { 0.3457, 0.3585, 0.2958 }),
                DenseVector.OfArray(new double[] { 0.7347, 0.2653, 0.0 }),
                DenseVector.OfArray(new double[] { 0.1152, 0.8264, 0.0584 }),//z=0.0177
                DenseVector.OfArray(new double[] { 0.1566, 0.0177, 0.8257 }));
            pal_secam = new Profile(2.20,
                DenseVector.OfArray(new double[] { 0.3127, 0.3290, 0.3583 }),
                DenseVector.OfArray(new double[] { 0.64, 0.33, 0.03 }),
                DenseVector.OfArray(new double[] { 0.29, 0.6, 0.11 }),
                DenseVector.OfArray(new double[] { 0.15, 0.06, 0.79 }));
            custom = new Profile(2.20,
                DenseVector.OfArray(new double[] { 0.3127, 0.3290, 0.3583 }),
                DenseVector.OfArray(new double[] { 0.64, 0.33, 0.03 }),
                DenseVector.OfArray(new double[] { 0.3, 0.6, 0.1 }),
                DenseVector.OfArray(new double[] { 0.15, 0.06, 0.79 }));
        }
        private bool CreateCustomSource(out Profile result)
        {
            result = new Profile();
            double gamma, wx, wy, wz, rx, ry, rz, gx, gy, gz, bx, by, bz;
            if (double.TryParse(textBox1.Text, out gamma) &&
                double.TryParse(textBox2.Text, out wx) && double.TryParse(textBox3.Text, out wy) &&
                double.TryParse(textBox4.Text, out rx) && double.TryParse(textBox5.Text, out ry) &&
                double.TryParse(textBox6.Text, out gx) && double.TryParse(textBox7.Text, out gy) &&
                double.TryParse(textBox8.Text, out bx) && double.TryParse(textBox9.Text, out by)
                )
            {
                if (gamma < 0.0 || wx < 0.0 || wy < 0.0 || rx < 0.0 || ry < 0.0 || gx < 0.0 || gy < 0.0 || bx < 0.0 || by < 0.0)
                {
                    MessageBox.Show("Gamma and colors x,y values cannot be negative");
                    return false;
                }
                wz = 1.0 - wx - wy;
                rz = 1.0 - rx - ry;
                gz = 1.0 - gx - gy;
                bz = 1.0 - bx - by;
                if (wx + wy > 1.0)
                {
                    MessageBox.Show("White x and y values are incorrect");
                    return false;
                }
                else if (rx + ry > 1.0)
                {
                    MessageBox.Show("Red x and y values are incorrect");
                    return false;
                }
                else if (gx + gy > 1.0)
                {
                    MessageBox.Show("Green x and y values are incorrect");
                    return false;
                }
                else if (bx + by > 1.0)
                {
                    MessageBox.Show("Blue x and y values are incorrect");
                    return false;
                }
                result = new Profile(gamma,
                    DenseVector.OfArray(new double[] { wx, wy, wz }),
                    DenseVector.OfArray(new double[] { rx, ry, rz }),
                    DenseVector.OfArray(new double[] { gx, gy, gz }),
                    DenseVector.OfArray(new double[] { bx, by, bz }));
            }
            else
            {
                MessageBox.Show("One of the values is not double and thus cannot be parsed");
                return false;
            }
            return true;
        }
        private bool CreateCustomOutput(out Profile result)
        {
            result = new Profile();
            double gamma, wx, wy, wz, rx, ry, rz, gx, gy, gz, bx, by, bz;
            if (double.TryParse(textBox10.Text, out gamma) &&
                double.TryParse(textBox11.Text, out wx) && double.TryParse(textBox12.Text, out wy) &&
                double.TryParse(textBox13.Text, out rx) && double.TryParse(textBox14.Text, out ry) &&
                double.TryParse(textBox15.Text, out gx) && double.TryParse(textBox16.Text, out gy) &&
                double.TryParse(textBox17.Text, out bx) && double.TryParse(textBox18.Text, out by)
                )
            {
                if (gamma < 0.0 || wx < 0.0 || wy < 0.0 || rx < 0.0 || ry < 0.0 || gx < 0.0 || gy < 0.0 || bx < 0.0 || by < 0.0)
                {
                    MessageBox.Show("Gamma and colors x,y values cannot be negative");
                    return false;
                }
                wz = 1.0 - wx - wy;
                rz = 1.0 - rx - ry;
                gz = 1.0 - gx - gy;
                bz = 1.0 - bx - by;
                if (wx + wy > 1.0)
                {
                    MessageBox.Show("White x and y values are incorrect");
                    return false;
                }
                else if (rx + ry > 1.0)
                {
                    MessageBox.Show("Red x and y values are incorrect");
                    return false;
                }
                else if (gx + gy > 1.0)
                {
                    MessageBox.Show("Green x and y values are incorrect");
                    return false;
                }
                else if (bx + by > 1.0)
                {
                    MessageBox.Show("Blue x and y values are incorrect");
                    return false;
                }
                result = new Profile(gamma,
                    DenseVector.OfArray(new double[] { wx, wy, wz }),
                    DenseVector.OfArray(new double[] { rx, ry, rz }),
                    DenseVector.OfArray(new double[] { gx, gy, gz }),
                    DenseVector.OfArray(new double[] { bx, by, bz }));
            }
            else
            {
                MessageBox.Show("One of the values is not double and thus cannot be parsed");
                return false;
            }
            return true;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var choice = (ProfileType)comboBox1.SelectedIndex;
            textBox1.Enabled = textBox2.Enabled = textBox3.Enabled = textBox4.Enabled = textBox5.Enabled = textBox6.Enabled = textBox7.Enabled = textBox8.Enabled = textBox9.Enabled = false;
            switch (choice)
            {
                case ProfileType.SRGB:
                    source = sRGB;
                    break;
                case ProfileType.AdobeRGB:
                    source = adobeRGB;
                    break;
                case ProfileType.AppleRGB:
                    source = appleRGB;
                    break;
                case ProfileType.CIERGB:
                    source = cieRGB;
                    break;
                case ProfileType.WideGamut:
                    source = wideGamut;
                    break;
                case ProfileType.PAL_SECAM:
                    source = pal_secam;
                    break;
                case ProfileType.Custom:
                    textBox1.Enabled = textBox2.Enabled = textBox3.Enabled = textBox4.Enabled = textBox5.Enabled = textBox6.Enabled = textBox7.Enabled = textBox8.Enabled = textBox9.Enabled = true;
                    source = custom;
                    break;
            }
            LoadSourceTextFields();
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox10.Enabled = textBox11.Enabled = textBox12.Enabled = textBox13.Enabled = textBox14.Enabled = textBox15.Enabled = textBox16.Enabled = textBox17.Enabled = textBox18.Enabled = false;
            var choice = (ProfileType)comboBox2.SelectedIndex;
            switch (choice)
            {
                case ProfileType.SRGB:
                    output = sRGB;
                    break;
                case ProfileType.AdobeRGB:
                    output = adobeRGB;
                    break;
                case ProfileType.AppleRGB:
                    output = appleRGB;
                    break;
                case ProfileType.CIERGB:
                    output = cieRGB;
                    break;
                case ProfileType.WideGamut:
                    output = wideGamut;
                    break;
                case ProfileType.PAL_SECAM:
                    output = pal_secam;
                    break;
                case ProfileType.Custom:
                    textBox10.Enabled = textBox11.Enabled = textBox12.Enabled = textBox13.Enabled = textBox14.Enabled = textBox15.Enabled = textBox16.Enabled = textBox17.Enabled = textBox18.Enabled = true;
                    output = custom;
                    break;
                default:
                    MessageBox.Show("Incorrect output profile!");
                    break;
            }
            LoadOutputTextFields();
        }
        private void LoadSourceTextFields()
        {
            textBox1.Text = source.Gamma.ToString();
            textBox2.Text = source.White[0].ToString();
            textBox3.Text = source.White[1].ToString();
            textBox4.Text = source.Red[0].ToString();
            textBox5.Text = source.Red[1].ToString();
            textBox6.Text = source.Green[0].ToString();
            textBox7.Text = source.Green[1].ToString();
            textBox8.Text = source.Blue[0].ToString();
            textBox9.Text = source.Blue[1].ToString();
        }
        private void LoadOutputTextFields()
        {
            textBox10.Text = output.Gamma.ToString();
            textBox11.Text = output.White[0].ToString();
            textBox12.Text = output.White[1].ToString();
            textBox13.Text = output.Red[0].ToString();
            textBox14.Text = output.Red[1].ToString();
            textBox15.Text = output.Green[0].ToString();
            textBox16.Text = output.Green[1].ToString();
            textBox17.Text = output.Blue[0].ToString();
            textBox18.Text = output.Blue[1].ToString();
        }
        //----------------------------------------BUTTONS SEGMENT--------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            using (var opf = new OpenFileDialog())
            {
                opf.InitialDirectory= System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Pictures");
                //MessageBox.Show(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Pictures"));
                opf.Filter = "Image files (*.bmp;*.jpg,*.jpeg,*.png) | *.bmp;*.jpg;*.jpeg;*.png";
                if (opf.ShowDialog() == DialogResult.OK)
                {
                    using (Bitmap temp = new Bitmap(opf.FileName))
                    {
                        inBitmap = new DirectBitmap(temp.Width, temp.Height);
                        for (int i = 0; i < temp.Width; i++)
                        {
                            for (int j = 0; j < temp.Height; j++)
                                inBitmap.SetPixel(i, j, temp.GetPixel(i, j));
                        }
                    }
                    pictureBox1.Image = inBitmap.Bitmap;
                    pictureBox2.Image = null;
                    generated = false;
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > 6 || comboBox1.SelectedIndex<0)
            {
                MessageBox.Show("Incorrect source profile");
                return;
            }
            if (comboBox2.SelectedIndex > 6 || comboBox2.SelectedIndex < 0)
            {
                MessageBox.Show("Incorrect output profile");
                return;
            }
            if(ProfileType.Custom == (ProfileType)comboBox1.SelectedIndex)
            {
                if (!CreateCustomSource(out source))
                    return;
            }
            if (ProfileType.Custom == (ProfileType)comboBox2.SelectedIndex)
            {
                if (!CreateCustomOutput(out output))
                    return;
            }
            GenerateOutput();
            generated = true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (!generated)
            {
                MessageBox.Show("You need to generate image to save it");
                return;
            }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Images|*.png;*.bmp;*.jpg;*.jpeg";
                ImageFormat format = ImageFormat.Png;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string ext = System.IO.Path.GetExtension(sfd.FileName);
                    switch (ext)
                    {
                        case ".png":
                            format = ImageFormat.Png;
                            break;
                        case ".jpg":
                        case ".jpeg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                    }
                    pictureBox2.Image.Save(sfd.FileName, format);
                }
            }
                
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Color c;
            DirectBitmap result = new DirectBitmap(inBitmap.Width, inBitmap.Height);
            for (int i = 0; i < result.Width; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    c = inBitmap.GetPixel(i, j);
                    int y = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                    result.SetPixel(i, j, Color.FromArgb(y, y, y));
                }
            }
            inBitmap = result;
            pictureBox1.Image = inBitmap.Bitmap;
        }
        //-----------------------------------------------------GENERATION OF OUTPUT
        private void GenerateOutput()
        {
            outBitmap = new DirectBitmap(inBitmap.Width, inBitmap.Height);
            Vector[,] tab;
            ReadInput(out tab);
            Decode(tab);
            ConvertToXYZ(tab);
            ConvertFromXYZ(tab);
            Encode(tab);
            WriteOutput(tab);
            pictureBox2.Image = outBitmap.Bitmap;
            generated = true;
            MessageBox.Show("Finished");
        }
        private void ReadInput(out Vector[,] tab)
        {
            tab = new Vector[inBitmap.Width, inBitmap.Height];
            for (int i = 0; i < inBitmap.Width; i++)
            {
                for (int j = 0; j < inBitmap.Height; j++)
                {
                    Color c = inBitmap.GetPixel(i, j);
                    tab[i, j] = DenseVector.OfArray(new double[] { c.R, c.G, c.B });
                }
            }
        }
        private void Decode(Vector[,] tab)
        {
            Parallel.For(0, inBitmap.Width, i =>
              {
                  for (int j = 0; j < inBitmap.Height; j++)
                  {
                      tab[i, j] = (Vector)tab[i, j].Divide(255.0).PointwisePower(source.Gamma);
                  }
              });
        }
        private void ConvertToXYZ(Vector[,] tab)
        {
            Matrix solver = DenseMatrix.OfColumnVectors(source.Red, source.Green, source.Blue);
            double X = source.White[0] * (1.0 / source.White[1]);
            double Y = 1.0;
            double Z = source.White[2]*(1.0/source.White[1]);
            Vector XYZ = DenseVector.OfArray(new double[] { X, Y, Z });
            Vector S = (Vector)solver.Solve(XYZ);
            Matrix converter = DenseMatrix.OfColumnVectors(source.Red*S[0],source.Green*S[1],source.Blue*S[2]);
            Parallel.For(0, inBitmap.Width, i =>
            {
                for (int j = 0; j < inBitmap.Height; j++)
                {
                    tab[i, j] = (Vector)(converter * tab[i, j]);
                }
            });
        }
        private void ConvertFromXYZ(Vector[,] tab)
        {
            Matrix solver = DenseMatrix.OfColumnVectors(output.Red, output.Green, output.Blue);
            double X = output.White[0] * (1.0 / output.White[1]);
            double Y = 1.0;
            double Z = output.White[2] * (1.0 / output.White[1]);
            Vector XYZ = DenseVector.OfArray(new double[] { X, Y, Z });
            Vector S=(Vector)solver.Solve(XYZ);
            Matrix converter = DenseMatrix.OfColumnVectors(output.Red * S[0], output.Green * S[1], output.Blue * S[2]);
            Parallel.For(0, inBitmap.Width, i =>
            {
                for (int j = 0; j < inBitmap.Height; j++)
                {
                    tab[i, j] = (Vector)(converter.Solve(tab[i, j]));
                }
            });
        }
        private void Encode(Vector[,] tab)
        {
            Parallel.For(0, inBitmap.Width, i =>
            {
                for (int j = 0; j < inBitmap.Height; j++)
                {
                    tab[i, j] = (Vector)tab[i, j].PointwisePower(1.0 / output.Gamma).Multiply(255.0);
                }
            });
        }
        private void WriteOutput(Vector[,] tab)
        {
            Vector v;
            for (int i = 0; i < inBitmap.Width; i++)
            {
                for (int j = 0; j < inBitmap.Height; j++)
                {
                    v = tab[i, j];
                    int r = (int)v[0], g = (int)v[1], b = (int)v[2];
                    if (r > 255 || r < 0)
                    {
                        if (r < 0)
                            r = 0;
                        else
                            r = 255;
                    }  
                    if (g > 255 || g<0)
                    {
                        if (g < 0)
                            g = 0;
                        else
                            g = 255;
                    }
                    if (b > 255 || b<0)
                    {
                        if (b < 0)
                            b = 0;
                        else
                            b = 255;
                    }
                    outBitmap.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }
        }
    }
}
