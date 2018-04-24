using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace WinForms2Lab
{
    public partial class Form1 : Form
    {
        #region Structures

        struct Vertex
        {
            public int X;
            public int Y;
            public Pen pen;

            public Vertex(int x, int y, Pen p)
            {
                X = x;
                Y = y;
                pen = p;
            }
        }

        struct Edge
        {
            public int From;
            public int To;

            public Edge(int from,int to)
            {
                From = from;
                To = to;
            }

        }

        #endregion

        #region Global Variables  
        
        int size = 30;
        static int thick = 2;
        Pen pen = new Pen(Color.Black, thick);
        Font font = new Font("Arial", 10);
        StringFormat stringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        int? selectedVertex = null;
        List<Vertex> vertices = new List<Vertex>();
        List<Edge> edges = new List<Edge>();
        bool movingVertex = false;
        int currentMousePosX;
        int currentMousePosY;

        #endregion

        public Form1()
        {
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture("pl-PL");
            InitializeComponent();
            
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e) //mouse click
        {
            switch(e.Button)
            {
                case MouseButtons.Left:
                    Vertex v = new Vertex(e.X, e.Y, pen);
                    bool colliding = false;
                    
                    for(int i=0; i<vertices.Count(); i++)
                    {
                        if ((Math.Pow((Math.Abs(vertices[i].X - v.X)), 2)) + (Math.Pow((Math.Abs(vertices[i].Y - v.Y)), 2)) < Math.Pow(size, 2)) // Pythagoras Theorem
                        {
                            if (selectedVertex.HasValue)
                            {
                                Edge edge = new Edge(selectedVertex.Value, i);
                                Edge symmetricEdge = new Edge(i, selectedVertex.Value);
                                if (edges.Contains(edge))
                                    edges.Remove(edge);
                                else if (edges.Contains(symmetricEdge))
                                    edges.Remove(symmetricEdge);
                                else
                                    edges.Add(edge);

                                pictureBox1.Refresh();
                            }
                                                                               
                            colliding = true;
                            break;                                                            
                        }
                    }
                    if (!colliding)
                    {
                        vertices.Add(v);
                        pictureBox1.Refresh();
                    }
                    break;

                case MouseButtons.Right:
                    bool change = false;
                    double distance = double.MaxValue;
                    double length;
                    for(int i=0; i<vertices.Count(); i++)
                    {
                        if ((length = (Math.Pow((Math.Abs(vertices[i].X - e.X)), 2)) + (Math.Pow((Math.Abs(vertices[i].Y - e.Y)), 2))) < Math.Pow(size, 2) && length < distance) // Pythagoras Theorem
                        {
                            change = true;
                            selectedVertex = i;
                            distance = length;
                            buttonDeleteVertex.Enabled = true;
                            
                        }
                    }
                    if (!change)
                    {
                        selectedVertex = null;
                        buttonDeleteVertex.Enabled = false;
                        movingVertex = false;
                    }
                    pictureBox1.Refresh();
                    break;                
            }                     
        }     

        private void buttonColor_Click(object sender, EventArgs e) //change color
        {
            ColorDialog cd = new ColorDialog();

            if (cd.ShowDialog() == DialogResult.OK)
            {
                pen = new Pen(cd.Color, thick);
                pictureBoxColor.BackColor = cd.Color;
                if (selectedVertex.HasValue)
                {
                    vertices[selectedVertex.Value] = new Vertex(vertices[selectedVertex.Value].X, vertices[selectedVertex.Value].Y, pen);
                    pictureBox1.Refresh();
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) //Painting
        {
            SolidBrush mySolidBrush = new SolidBrush(Color.White);
            foreach (Edge edge in edges)
            {
                Pen penEdges = new Pen(Color.Black, thick);
                e.Graphics.DrawLine(penEdges, vertices[edge.From].X, vertices[edge.From].Y, vertices[edge.To].X, vertices[edge.To].Y);
            }

            for (int i = 0; i < vertices.Count(); i++)
            {
                Rectangle rect = new Rectangle(vertices[i].X - size / 2, vertices[i].Y - size / 2, size, size);
                e.Graphics.FillEllipse(mySolidBrush, rect);

                if (selectedVertex.HasValue && selectedVertex.Value == i)
                {
                    Pen dottedpen = vertices[i].pen;                  
                    dottedpen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawEllipse(dottedpen, rect);
                    dottedpen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    
                }
                else
                    e.Graphics.DrawEllipse(vertices[i].pen, rect);
                               
                e.Graphics.DrawString($"{i + 1}", font, vertices[i].pen.Brush, rect, stringFormat);
            }

            
        }

        private void buttonLoad_Click(object sender, EventArgs e) //load graph
        {
            OpenFileDialog openGraph = new OpenFileDialog();
            openGraph.Filter = "Graph files (*.graph)|*.graph";
            if (openGraph.ShowDialog() == DialogResult.OK)
            {

                //string error;
                //string confirm;
                //if (CultureInfo.CurrentUICulture.Name == "pl-PL")
                //{
                //    error = "Błąd pliku.";
                //    confirm = "Graf wczytano pomyślnie.";
                //}
                //else
                //{
                //    error = "File error.";
                //    confirm = "Graph loaded successfully.";
                //}
                //
                ComponentResourceManager resourceManager = new ComponentResourceManager(typeof(Form1));

                List<Edge> tempEdges = new List<Edge>();
                List<Vertex> tempVertices = new List<Vertex>();
                FileStream fs = (FileStream)openGraph.OpenFile();
                try
                {
                    using (var fw = new StreamReader(fs))
                    {
                        int VerticesNumber = 0;
                        while (!fw.EndOfStream)
                        {
                            string[] parts = fw.ReadLine().Split(',');
                            if (parts.Count() == 3) //vertex
                            {
                                int x, y, color;

                                if (!(int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y) && int.TryParse(parts[2], out color)))
                                {
                                    MessageBox.Show(resourceManager.GetString("Error"));
                                    return;

                                }
                                tempVertices.Add(new Vertex(x, y, new Pen(Color.FromArgb(color), thick)));
                                VerticesNumber++;
                            }
                            else if (parts.Count() == 2) //edge
                            {
                                int from, to;
                                if (!(int.TryParse(parts[0], out from) && int.TryParse(parts[1], out to)))
                                {
                                    MessageBox.Show(resourceManager.GetString("Error"));
                                    return;
                                }
                                tempEdges.Add(new Edge(from, to));
                            }
                            else //something else, throw error
                            {
                                MessageBox.Show(resourceManager.GetString("Error"));
                                return;
                            }
                        }
                        foreach (Edge edge in tempEdges) //wrong number of vertices
                        {
                            if (edge.From >= VerticesNumber || edge.To >= VerticesNumber || edge.From < 0 || edge.To < 0)
                            {
                                MessageBox.Show(resourceManager.GetString("Error"));
                                return;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(resourceManager.GetString("Error"));
                    return;
                }
                
                edges = tempEdges;
                vertices = tempVertices;
                MessageBox.Show(resourceManager.GetString("ConfirmLoad"));
                pictureBox1.Refresh();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e) //save graph
        {
            SaveFileDialog saveGraph = new SaveFileDialog();
            saveGraph.Filter = "Graph files (*.graph)|*.graph";
            if (saveGraph.ShowDialog() == DialogResult.OK)
            {
                ComponentResourceManager resourceManager = new ComponentResourceManager(typeof(Form1));
                FileStream fs = (FileStream)saveGraph.OpenFile();
                using (var fw = new StreamWriter(fs))
                {
                    foreach (Vertex v in vertices)
                    {
                        fw.WriteLine($"{v.X},{v.Y},{v.pen.Color.ToArgb()}");
                    }
                    foreach (Edge edge in edges)
                    {
                        fw.WriteLine($"{edge.From},{edge.To}");
                    }
                    fw.Flush();
                }

                MessageBox.Show(resourceManager.GetString("ConfirmSave"));
                //if(CultureInfo.CurrentUICulture.Name ==  "pl-PL")
                //    MessageBox.Show("Graf zapisano pomyślnie.");
                //else
                //    MessageBox.Show("Graph saved successfully.");
            }
        }

        private void buttonClear_Click(object sender, EventArgs e) //clear graph
        {
            edges.Clear();
            vertices.Clear();
            pictureBox1.Refresh();
            selectedVertex = null;
        }

        private void buttonPolish_Click(object sender, EventArgs e) //polish culture
        {            
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture("pl-PL");
            ChangeNames();
            
        }

        private void buttonEnglish_Click(object sender, EventArgs e) //english culture
        {
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
            ChangeNames();  
            
        }

        private void buttonDeleteVertex_Click(object sender, EventArgs e) //delete vertex
        {
            DeleteVertex();
        }

        private void buttonVertex_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) //delete vertex by using delete button on keyboard
        {
            if (e.KeyCode != Keys.Delete || !selectedVertex.HasValue)
                return;
            DeleteVertex();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) //stop moving vertex
        {
            if (e.Button != MouseButtons.Left)
                movingVertex = false;

            if (!selectedVertex.HasValue)
                return;
            int vertexX = vertices[selectedVertex.Value].X;
            int vertexY = vertices[selectedVertex.Value].Y;

            if (vertexX < 0)
                vertexX = 0;
            if (vertexX > (pictureBox1.Right - pictureBox1.Left))
                vertexX = pictureBox1.Right - pictureBox1.Left;

            if (vertexY < 0)
                vertexY = 0;
            if (vertexY > (pictureBox1.Bottom - pictureBox1.Top))
                vertexY = pictureBox1.Bottom - pictureBox1.Top;
            Vertex replace = new Vertex(vertexX, vertexY, vertices[selectedVertex.Value].pen);
            vertices[selectedVertex.Value] = replace;
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) //moving vertex
        {
            if (!selectedVertex.HasValue || !movingVertex)
                return;
            
            int difX = pictureBox1.PointToClient(Cursor.Position).X - currentMousePosX;
            int difY = pictureBox1.PointToClient(Cursor.Position).Y - currentMousePosY;
            Vertex newLocalisation = new Vertex(vertices[selectedVertex.Value].X + difX, vertices[selectedVertex.Value].Y + difY, vertices[selectedVertex.Value].pen);
            vertices[selectedVertex.Value] = newLocalisation;
            currentMousePosX = e.X;
            currentMousePosY = e.Y;
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) //start moving vertex
        {
            if (selectedVertex.HasValue && e.Button == MouseButtons.Middle)
            {
                movingVertex = true;
                currentMousePosX = pictureBox1.PointToClient(Cursor.Position).X;
                currentMousePosY = pictureBox1.PointToClient(Cursor.Position).Y;
            }
        }

        //void ChangeName() //change control names function
        //{
        //    ComponentResourceManager resourceManager = new ComponentResourceManager(typeof(Form1));           
        //    resourceManager.ApplyResources(groupBox1, groupBox1.Name);
        //    resourceManager.ApplyResources(buttonColor, buttonColor.Name);
        //    resourceManager.ApplyResources(buttonDeleteVertex, buttonDeleteVertex.Name);
        //    resourceManager.ApplyResources(buttonClear, buttonClear.Name);
        //    resourceManager.ApplyResources(groupBox2, groupBox2.Name);
        //    resourceManager.ApplyResources(buttonPolish, buttonPolish.Name);
        //    resourceManager.ApplyResources(buttonEnglish, buttonEnglish.Name);
        //    resourceManager.ApplyResources(groupBox3, groupBox3.Name);
        //    resourceManager.ApplyResources(buttonSave, buttonSave.Name);
        //    resourceManager.ApplyResources(buttonLoad, buttonLoad.Name);

        //    Size before = this.Size;
        //    resourceManager.ApplyResources(this, "$this");
        //    this.Size = before;
                       
        //}

        void ChangeNames()
        {
            Size size = tableLayoutPanel1.Size;
            Controls.Clear();
            InitializeComponent();
            tableLayoutPanel1.Size = size;
        }

        void DeleteVertex() //delete vertex function
        {
            List<Edge> toDelete = new List<Edge>();

            for (int i = 0; i < edges.Count(); i++)
            {
                if (edges[i].From == selectedVertex.Value || edges[i].To == selectedVertex.Value)
                    toDelete.Add(edges[i]);
                else
                {
                    if (edges[i].From > selectedVertex.Value)
                        edges[i] = new Edge(edges[i].From - 1, edges[i].To);

                    if (edges[i].To > selectedVertex.Value)
                        edges[i] = new Edge(edges[i].From, edges[i].To - 1);
                }
            }
            foreach (Edge edge in toDelete)
                edges.Remove(edge);
            vertices.RemoveAt(selectedVertex.Value);

            buttonDeleteVertex.Enabled = false;
            selectedVertex = null;
            pictureBox1.Refresh();
        }
    }
}
