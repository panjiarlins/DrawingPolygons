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

namespace DrawingPolygons
{
    public partial class MainForm : Form
    {
        public PolyType polygon = new PolyType("Polygon");
        public PolyType polyline = new PolyType("Polyline");
        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            cbInitialPolyType.SelectedIndex = 0;
            cbEditPolyType.SelectedIndex = 0;
            cbWhatEdit.SelectedIndex = 0;
        }
        private void pnlDrawingwindow_MouseMove(object sender, MouseEventArgs e)
        {
            lblMouseX.Text = "x = " + e.Location.X.ToString();
            lblMouseY.Text = "y = " + e.Location.Y.ToString();
        }
        public void OnlyAcceptNumber(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void timerColor_Tick(object sender, EventArgs e)
        {
            pnlDisplayInitialColor.BackColor = Color.FromArgb(hsbInitalColorR.Value, hsbInitalColorG.Value, hsbInitalColorB.Value);
            lblInitalColorR.Text = hsbInitalColorR.Value.ToString();
            lblInitalColorG.Text = hsbInitalColorG.Value.ToString();
            lblInitalColorB.Text = hsbInitalColorB.Value.ToString();

            pnlDisplayChangeColor.BackColor = Color.FromArgb(hsbChangeColorR.Value, hsbChangeColorG.Value, hsbChangeColorB.Value);
            lblChangeColorR.Text = hsbChangeColorR.Value.ToString();
            lblChangeColorG.Text = hsbChangeColorG.Value.ToString();
            lblChangeColorB.Text = hsbChangeColorB.Value.ToString();

            Drawing();
        }
        private void btnClearscreen_Click(object sender, EventArgs e)
        {
            polygon.type = null;
            polyline.type = null;
            tabMenu.SelectedIndex = 0;
            cbEditPolyType_SelectedIndexChanged(sender, e);
            Reload();
        }
        private void pnlDrawingWindow_MouseDown(object sender, MouseEventArgs e)
        {
            string X = e.Location.X.ToString();
            string Y = e.Location.Y.ToString();
            txtInitialVertexX.Text = X;
            txtInitialVertexY.Text = Y;
            txtAddVertexX.Text = X;
            txtAddVertexY.Text = Y;
            txtEditVertexLocationX.Text = X;
            txtEditVertexLocationY.Text = Y;
        }
        private void cbWhatEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbWhatEdit.SelectedIndex == 0)
            {
                pnlAddVertex.Visible = true;
                pnlChangeColor.Visible = false;
                pnlDeletePoly.Visible = false;
                pnlDeleteVertex.Visible = false;
                pnlEditVertexLocation.Visible = false;
            }
            else if(cbWhatEdit.SelectedIndex == 1)
            {
                pnlAddVertex.Visible = false;
                pnlChangeColor.Visible = true;
                pnlDeletePoly.Visible = false;
                pnlDeleteVertex.Visible = false;
                pnlEditVertexLocation.Visible = false;
            }
            else if(cbWhatEdit.SelectedIndex == 2)
            {
                pnlAddVertex.Visible = false;
                pnlChangeColor.Visible = false;
                pnlDeletePoly.Visible = true;
                pnlDeleteVertex.Visible = false;
                pnlEditVertexLocation.Visible = false;
            }
            else if(cbWhatEdit.SelectedIndex == 3)
            {
                pnlAddVertex.Visible = false;
                pnlChangeColor.Visible = false;
                pnlDeletePoly.Visible = false;
                pnlDeleteVertex.Visible = true;
                pnlEditVertexLocation.Visible = false;
            }
            else if (cbWhatEdit.SelectedIndex == 4)
            {
                pnlAddVertex.Visible = false;
                pnlChangeColor.Visible = false;
                pnlDeletePoly.Visible = false;
                pnlDeleteVertex.Visible = false;
                pnlEditVertexLocation.Visible = true;
            }
        }
        private void cbEditPolyType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbEditPolyType.SelectedIndex == -1) { cbEditPolyType.SelectedIndex = 0; }
            else
            {
                cbEditPolyName.DataSource = null;
                cbEditPolyName.Items.Clear();

                NodePoly tempPoly;
                if (cbEditPolyType.SelectedIndex == 0) { tempPoly = polygon.type; }
                else { tempPoly = polyline.type; }

                List<string> items = new List<string>();
                while (tempPoly != null)
                {
                    items.Add(tempPoly.polyName);
                    tempPoly = tempPoly.nextPoly;
                }
                cbEditPolyName.DataSource = items;

                cbEditPolyName_SelectedIndexChanged(sender, e);
                DetailPolygon(cbEditPolyType.SelectedIndex == 0 ? polygon : polyline, cbEditPolyName.SelectedIndex);
            }
        }
        private void cbEditPolyName_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbAddVertexAfter.DataSource = null;
            cbAddVertexAfter.Items.Clear();
            cbEditVertexLocationWhatVertex.DataSource = null;
            cbEditVertexLocationWhatVertex.Items.Clear();
            cbDeleteVertexWhatVertex.DataSource = null;
            cbDeleteVertexWhatVertex.Items.Clear();

            PolyType Poly;
            if (cbEditPolyType.SelectedIndex == 0) { Poly = polygon; }
            else { Poly = polyline; }

            NodePoly tempPoly = Poly.type;
            if (tempPoly != null)
            {
                tempPoly = Poly.FindNodePolyBaseOnSelectedIndex(cbEditPolyName.SelectedIndex);

                NodeVertex tempVertex = tempPoly.poly;
                List<string> items = new List<string>();
                while (tempVertex != null)
                {
                    items.Add("(" + tempVertex.X + ", " + tempVertex.Y + ")");
                    tempVertex = tempVertex.nextVertex;
                }
                cbEditVertexLocationWhatVertex.DataSource = items;
                cbDeleteVertexWhatVertex.DataSource = items;
                items.Insert(0, "Add new vertex as first vertex");
                cbAddVertexAfter.DataSource = items;

                hsbChangeColorR.Value = tempPoly.Rcolor;
                hsbChangeColorG.Value = tempPoly.Gcolor;
                hsbChangeColorB.Value = tempPoly.Bcolor;

                DetailPolygon(cbEditPolyType.SelectedIndex == 0 ? polygon : polyline, cbEditPolyName.SelectedIndex);
            }
        }
        public void DetailPolygon(PolyType Poly, int index)
        {
            txtIsConvex.ResetText();
            txtAreaPolygon.ResetText();
            txtPerimeterPolygon.ResetText();
            txtLengthPolyline.ResetText();

            if (Poly.typeName == "Polygon")
            {
                int isConvex = Poly.IsConvex(index);
                double area = Poly.AreaOfPolygon(index);
                double perimeter = Poly.PerimeterOfPoly(index);
                if (isConvex != -2)
                {
                    if (isConvex == 1) { txtIsConvex.Text = "Yes"; }
                    else if (isConvex == 0) { txtIsConvex.Text = "No"; }
                    else if (isConvex == -1) { txtIsConvex.Text = "Something wrong with this Polygon's vertex!"; }
                }
                if (area != 0.0) { txtAreaPolygon.Text = area.ToString(); }
                if (perimeter != 0.0) { txtPerimeterPolygon.Text = perimeter.ToString(); }
                gbDetailPolygon.Visible = true;
                gbDetailPolyline.Visible = false;
            }
            else
            {
                double length = Poly.PerimeterOfPoly(index);
                if (length != 0.0) { txtLengthPolyline.Text = length.ToString(); }
                gbDetailPolygon.Visible = false;
                gbDetailPolyline.Visible = true;
            }
        }
        public void Drawing()
        {
            Graphics myGraphic = pnlDrawingWindow.CreateGraphics();
            myGraphic.DrawLine(new Pen(Color.White), 0, 0, 0, 0);
            for (int i = 0; i <= 1; i++)
            {
                NodePoly tempPoly;
                if (i == 0) { tempPoly = polygon.type; }
                else { tempPoly = polyline.type; }
                NodeVertex tempVertex;
                while (tempPoly != null)
                {
                    tempVertex = tempPoly.poly;
                    Pen myPen = new Pen(Color.FromArgb(tempPoly.Rcolor, tempPoly.Gcolor, tempPoly.Bcolor));
                    while (tempVertex.nextVertex != null)
                    {
                        myGraphic.DrawLine(myPen, tempVertex.X, tempVertex.Y, tempVertex.nextVertex.X, tempVertex.nextVertex.Y);
                        tempVertex = tempVertex.nextVertex;
                    }
                    if (i == 0) { myGraphic.DrawLine(myPen, tempVertex.X, tempVertex.Y, tempPoly.poly.X, tempPoly.poly.Y); };
                    tempPoly = tempPoly.nextPoly;
                }
            }
        }
        private void Reload()
        {
            pnlDrawingWindow.Refresh();
            txtDataFile.Text = polygon.PolyToString();
            txtDataFile.Text += polyline.PolyToString();
        }
        private void btnDraw_Click(object sender, EventArgs e)
        {
            PolyType Poly;
            if (cbInitialPolyType.SelectedIndex == 0) { Poly = polygon; }
            else { Poly = polyline; }

            if (txtInitialPolyName.Text == "" || txtInitialVertexX.Text == "" || txtInitialVertexY.Text == "")
            {
                MessageBox.Show("Error Input!");
            }
            else
            {
                Poly.AddInitial(txtInitialPolyName.Text, hsbInitalColorR.Value, hsbInitalColorG.Value, hsbInitalColorB.Value, Convert.ToInt32(txtInitialVertexX.Text), Convert.ToInt32(txtInitialVertexY.Text));

                txtInitialVertexX.ResetText();
                txtInitialVertexY.ResetText();
                txtInitialPolyName.ResetText();

                tabMenu.SelectedIndex = 1;
                cbEditPolyType.SelectedIndex = cbInitialPolyType.SelectedIndex;
                cbEditPolyType_SelectedIndexChanged(sender, e);
                cbEditPolyName.SelectedIndex = cbEditPolyName.Items.Count - 1;
                cbWhatEdit.SelectedIndex = 0;
                cbAddVertexAfter.SelectedIndex = cbAddVertexAfter.Items.Count - 1;
                Reload();
            }
        }
        private void btnApplyAddVertex_Click(object sender, EventArgs e)
        {
            PolyType Poly;
            if (cbEditPolyType.SelectedIndex == 0) { Poly = polygon; }
            else { Poly = polyline; }

            if (txtAddVertexX.Text == "" || txtAddVertexY.Text == "")
            {
                MessageBox.Show("Error Input!");
            }
            else
            {
                if (Poly.type != null)
                {
                    int i = cbEditPolyName.SelectedIndex;
                    int j = cbAddVertexAfter.SelectedIndex;
                    Poly.AddVertex(i, j, Convert.ToInt32(txtAddVertexX.Text), Convert.ToInt32(txtAddVertexY.Text));
                    txtAddVertexX.ResetText();
                    txtAddVertexY.ResetText();
                    cbEditPolyType_SelectedIndexChanged(sender, e);
                    cbEditPolyName.SelectedIndex = i;
                    cbAddVertexAfter.SelectedIndex = cbAddVertexAfter.Items.Count - 1;
                    Reload();
                }
            }
        }
        private void btnApplyChangeColor_Click(object sender, EventArgs e)
        {
            PolyType Poly;
            if (cbEditPolyType.SelectedIndex == 0) { Poly = polygon; }
            else { Poly = polyline; }

            if (Poly.type != null)
            {
                Poly.ChangeColor(cbEditPolyName.SelectedIndex, hsbChangeColorR.Value, hsbChangeColorG.Value, hsbChangeColorB.Value);
                Reload();
            }
        }
        private void btnDeletePoly_Click(object sender, EventArgs e)
        {
            PolyType Poly;
            if (cbEditPolyType.SelectedIndex == 0) { Poly = polygon; }
            else { Poly = polyline; }
            Poly.DeletePoly(cbEditPolyName.SelectedIndex);
            cbEditPolyType_SelectedIndexChanged(sender, e);
            Reload();
        }
        private void btnApplyDeleteVertex_Click(object sender, EventArgs e)
        {
            PolyType Poly;
            if (cbEditPolyType.SelectedIndex == 0) { Poly = polygon; }
            else { Poly = polyline; }

            NodePoly tempPoly;
            tempPoly = Poly.type;
            if (tempPoly != null)
            {
                int i = cbEditPolyName.SelectedIndex;
                int j = cbDeleteVertexWhatVertex.SelectedIndex;
                Poly.DeleteVertex(i, j);
                cbEditPolyType_SelectedIndexChanged(sender, e);
                cbEditPolyName.SelectedIndex = i;
                Reload();
            }
        }
        private void btnApplyEditVertexLocation_Click(object sender, EventArgs e)
        {
            PolyType Poly;
            if (cbEditPolyType.SelectedIndex == 0) { Poly = polygon; }
            else { Poly = polyline; }

            if(txtEditVertexLocationX.Text == "" || txtEditVertexLocationY.Text == "")
            {
                MessageBox.Show("Error Input!");
            }
            else
            {
                if (Poly.type != null)
                {
                    int i = cbEditPolyType.SelectedIndex;
                    int j = cbEditPolyName.SelectedIndex;
                    int k = cbEditVertexLocationWhatVertex.SelectedIndex;
                    Poly.EditVertexLocation(j, k, Convert.ToInt32(txtEditVertexLocationX.Text), Convert.ToInt32(txtEditVertexLocationY.Text));
                    txtEditVertexLocationX.ResetText();
                    txtEditVertexLocationY.ResetText();
                    cbEditPolyType_SelectedIndexChanged(sender, e);
                    cbEditPolyType.SelectedIndex = i;
                    cbEditPolyName.SelectedIndex = j;
                    cbEditVertexLocationWhatVertex.SelectedIndex = k;
                    Reload();
                }
            }
        }
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Open File";
            open.Filter = "Text Files (*.txt)|*.txt";
            if (open.ShowDialog() == DialogResult.OK)
            {
                StreamReader read = new StreamReader(File.OpenRead(open.FileName));
                txtDataFile.Text = read.ReadToEnd();
                read.Dispose();
            }
        }
        private void btnSaveFIle_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "Open File";
            save.Filter = "Text Files (*.txt)|*.txt";

            if(save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter write = new StreamWriter(File.Create(save.FileName));
                write.Write(txtDataFile.Text);
                write.Dispose();
            }
        }
        private void ReadingFile()
        {
            polygon.type = null;
            polyline.type = null;
            string text = txtDataFile.Text;
            string polyType = "";
            try
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == '<')
                    {
                        string polyName = "";
                        int j = i + 1;
                        polyType = text[j].ToString();
                        j += 2;
                        while (text[j] != '>')
                        {
                            polyName += text[j].ToString();
                            j++;
                        }
                        if (polyType == "0") { polygon.AddInitial(polyName); }
                        else if (polyType == "1") { polyline.AddInitial(polyName); }
                        else
                        {
                            polygon.type = null;
                            polyline.type = null;
                            pnlDrawingWindow.Refresh();
                            MessageBox.Show("Incorrect input in polyType!\r\n0 = Polygon\r\n1 = Polyline");
                            return;
                        }
                    }
                    else if (text[i] == '[')
                    {
                        string[] strcolor = { "", "", "" };
                        int index = 0;
                        int j = i + 1;
                        while (text[j] != ']')
                        {
                            if (text[j] == ' ') { }
                            else if (text[j] == ',') { index++; }
                            else
                            {
                                if (!char.IsDigit(text[j]))
                                {
                                    polygon.type = null;
                                    polyline.type = null;
                                    pnlDrawingWindow.Refresh();
                                    MessageBox.Show("Error input in RGB notation!");
                                    return;
                                }
                                strcolor[index] += text[j].ToString();
                            }
                            j++;
                        }
                        int[] color = new int[3];
                        color[0] = Convert.ToInt32(strcolor[0]);
                        color[1] = Convert.ToInt32(strcolor[1]);
                        color[2] = Convert.ToInt32(strcolor[2]);

                        if (color[0] < 0 || color[0] > 255 || color[1] < 0 || color[1] > 255 || color[2] < 0 || color[2] > 255)
                        {
                            polygon.type = null;
                            polyline.type = null;
                            pnlDrawingWindow.Refresh();
                            MessageBox.Show("RGB notation is out of range!");
                            return;
                        }
                        else
                        {
                            if (polyType == "0") { polygon.AddInitial(color[0], color[1], color[2]); }
                            else { polyline.AddInitial(color[0], color[1], color[2]); }
                        }

                    }
                    else if (text[i] == '{')
                    {
                        int j = i + 1;
                        while (text[j] != '}')
                        {
                            int index = 0;
                            if (text[j] == '(')
                            {
                                string strX = "";
                                string strY = "";
                                int k = j + 1;
                                while (text[k] != ')')
                                {
                                    if (text[k] == ' ') { }
                                    else if (text[k] == ',') { index++; }
                                    else
                                    {
                                        if (!char.IsDigit(text[k]))
                                        {
                                            polygon.type = null;
                                            polyline.type = null;
                                            pnlDrawingWindow.Refresh();
                                            MessageBox.Show("Error input in Coordinate Point!");
                                            return;
                                        }
                                        if (index == 0) { strX += text[k].ToString(); }
                                        else { strY += text[k].ToString(); }
                                    }
                                    k++;
                                }
                                int X = Convert.ToInt32(strX);
                                int Y = Convert.ToInt32(strY);
                                if (polyType == "0") { polygon.AddVertex(X, Y); }
                                else { polyline.AddVertex(X, Y); }
                            }
                            j++;
                        }
                    }
                }
                cbEditPolyType.SelectedIndex = 1;
                cbEditPolyType.SelectedIndex = 0;
                Reload();
            }
            catch
            {
                polygon.type = null;
                polyline.type = null;
                pnlDrawingWindow.Refresh();
                MessageBox.Show("Error Input!");
                return;
            }
        }
        private void btnRunFile_Click(object sender, EventArgs e)
        {
            ReadingFile();
        }
    }
    public class NodeVertex
    {
        public int X;
        public int Y;
        public NodeVertex nextVertex;
        public NodeVertex(int a, int b)
        {
            X = a;
            Y = b;
            nextVertex = null;
        }
    }
    public class NodePoly
    {
        public NodeVertex poly;
        public string polyName;
        public int Rcolor;
        public int Gcolor;
        public int Bcolor;
        public NodePoly nextPoly;
        
        public NodePoly()
        {
            poly = null;
        }
    }
    public class PolyType
    {
        public NodePoly type;
        public string typeName;
        public PolyType(string name)
        {
            type = null;
            typeName = name;
        }
        public void ChangeColor(int index, int R, int G, int B)
        {
            NodePoly tempPoly = FindNodePolyBaseOnSelectedIndex(index);
            tempPoly.Rcolor = R;
            tempPoly.Gcolor = G;
            tempPoly.Bcolor = B;
        }
        public void AddInitial(string polyName, int R, int G, int B, int X, int Y)
        {
            NodePoly newPoly = new NodePoly();
            newPoly.poly = new NodeVertex(X, Y);
            newPoly.Rcolor = R;
            newPoly.Gcolor = G;
            newPoly.Bcolor = B;
            newPoly.polyName = polyName;
            newPoly.nextPoly = null;

            if(type == null)
            {
                type = newPoly;
                return;
            }
            NodePoly p = type;
            while(p.nextPoly != null)
            {
                p = p.nextPoly;
            }
            p.nextPoly = newPoly;
        }
        public void AddInitial(string polyName)
        {
            NodePoly newPoly = new NodePoly();
            newPoly.polyName = polyName;
            newPoly.nextPoly = null;

            if (type == null)
            {
                type = newPoly;
                return;
            }
            NodePoly p = type;
            while (p.nextPoly != null)
            {
                p = p.nextPoly;
            }
            p.nextPoly = newPoly;
        }
        public void AddInitial(int R, int G, int B)
        {
            NodePoly tempPoly = type;
            while (tempPoly.nextPoly != null)
            {
                tempPoly = tempPoly.nextPoly;
            }
            tempPoly.Rcolor = R;
            tempPoly.Gcolor = G;
            tempPoly.Bcolor = B;
        }
        public void AddVertex(int indexPoly, int indexVertex, int X, int Y)
        {
            NodePoly tempPoly = FindNodePolyBaseOnSelectedIndex(indexPoly);
            if (tempPoly != null)
            {
                NodeVertex tempVertex = tempPoly.poly;
                indexVertex += 1;
                if (indexVertex == 1)
                {
                    NodeVertex temp = new NodeVertex(X, Y);
                    temp.nextVertex = tempVertex;
                    tempPoly.poly = temp;
                }
                else
                {
                    while (indexVertex-- != 0)
                    {
                        if (indexVertex == 1)
                        {
                            NodeVertex temp = new NodeVertex(X, Y);
                            temp.nextVertex = tempVertex.nextVertex;
                            tempVertex.nextVertex = temp;
                            break;
                        }
                        tempVertex = tempVertex.nextVertex;
                    }
                }
            }
        }
        public void AddVertex(int X, int Y)
        {
            NodePoly tempPoly = type;
            while(tempPoly.nextPoly != null)
            {
                tempPoly = tempPoly.nextPoly;
            }
            NodeVertex tempVertex = tempPoly.poly;
            if(tempVertex == null) { tempPoly.poly = new NodeVertex(X, Y); }
            else
            {
                while (tempVertex.nextVertex != null)
                {
                    tempVertex = tempVertex.nextVertex;
                }
                tempVertex.nextVertex = new NodeVertex(X, Y);
            }
        }
        public NodePoly FindNodePolyBaseOnSelectedIndex(int index)
        {
            NodePoly tempPoly = type;
            int i = 0;
            if(tempPoly == null) { return null; }
            else
            {
                while (tempPoly.nextPoly != null)
                {
                    if (i == index) { break; }
                    tempPoly = tempPoly.nextPoly;
                    i++;
                }
                return tempPoly;
            }
        }
        public void EditVertexLocation(int indexPoly, int indexVertex, int X, int Y)
        {
            NodePoly tempPoly = FindNodePolyBaseOnSelectedIndex(indexPoly);
            NodeVertex tempVertex = tempPoly.poly;
            int i = 0;
            while(tempVertex.nextVertex != null)
            {
                if(i == indexVertex) { break; }
                tempVertex = tempVertex.nextVertex;
                i++;
            }
            tempVertex.X = X;
            tempVertex.Y = Y;
        }
        public void DeleteVertex(int indexPoly, int indexVertex)
        {
            NodePoly tempPoly = FindNodePolyBaseOnSelectedIndex(indexPoly);
            NodeVertex tempVertex = tempPoly.poly;
            NodeVertex temp;
            if (indexVertex == 0)
            {
                temp = tempVertex.nextVertex;
                type.poly = temp;
            }
            else
            {
                int i = 0;
                while (tempVertex.nextVertex != null)
                {
                    if (i == indexVertex - 1)
                    {
                        temp = tempVertex.nextVertex.nextVertex;
                        tempVertex.nextVertex = temp;
                        break;
                    }
                    tempVertex = tempVertex.nextVertex;
                    i++;
                }
            }
        }
        public double AreaOfPolygon(int indexPoly)
        {
            if (typeName == "Polygon")
            {
                NodePoly tempPoly = FindNodePolyBaseOnSelectedIndex(indexPoly);
                if (tempPoly != null)
                {
                    NodeVertex tempVertex = tempPoly.poly;
                    int i = 0;
                    while (tempVertex != null)
                    {
                        tempVertex = tempVertex.nextVertex;
                        i++;
                    }
                    if (i > 1)
                    {
                        tempVertex = tempPoly.poly;
                        double x = 0.0, y = 0.0;
                        while(tempVertex.nextVertex != null)
                        {
                            x += tempVertex.X * tempVertex.nextVertex.Y;
                            y += tempVertex.Y * tempVertex.nextVertex.X;
                            tempVertex = tempVertex.nextVertex;
                        }
                        x += tempVertex.X * tempPoly.poly.Y;
                        y += tempVertex.Y * tempPoly.poly.X;
                        return Math.Abs(x - y) / 2.0;
                    }
                }
            }
            return 0.0;
        }
        public string PolyToString()
        {
            NodePoly tempPoly = type;
            if(tempPoly != null)
            {
                string text = "";
                string i = typeName == "Polygon" ? "0" : "1";
                while (tempPoly != null)
                {
                    text += "<" + i + "," + tempPoly.polyName + ">[" + tempPoly.Rcolor.ToString() + "," + tempPoly.Gcolor.ToString() + "," + tempPoly.Bcolor.ToString() + "]{";
                    NodeVertex tempVertex = tempPoly.poly;
                    while (tempVertex != null)
                    {
                        text += "(" + tempVertex.X.ToString() + "," + tempVertex.Y.ToString() + ")";
                        tempVertex = tempVertex.nextVertex;
                    }
                    text += "}\r\n";
                    tempPoly = tempPoly.nextPoly;
                }
                return text;
            }
            return "";
        }
        public void DeletePoly(int indexPoly)
        {
            NodePoly tempPoly = type;
            if(tempPoly != null)
            {
                if (indexPoly == 0)
                {
                    tempPoly = tempPoly.nextPoly;
                    type = tempPoly;
                }
                else
                {
                    NodePoly temp;
                    int i = 0;
                    while (tempPoly.nextPoly != null)
                    {
                        if (i == indexPoly - 1)
                        {
                            temp = tempPoly.nextPoly.nextPoly;
                            tempPoly.nextPoly = temp;
                            break;
                        }
                        tempPoly = tempPoly.nextPoly;
                        i++;
                    }
                }
            }
        }
        public double PerimeterOfPoly(int indexPoly)
        {
            NodePoly tempPoly = FindNodePolyBaseOnSelectedIndex(indexPoly);
            if(tempPoly != null && tempPoly.poly != null)
            {
                NodeVertex tempVertex = tempPoly.poly;
                if (tempVertex.nextVertex != null)
                {
                    double perimeter = 0.0;
                    double dx, dy;
                    if (tempVertex.nextVertex.nextVertex != null)
                    {
                        while (tempVertex.nextVertex != null)
                        {
                            dx = tempVertex.nextVertex.X - tempVertex.X;
                            dy = tempVertex.nextVertex.Y - tempVertex.Y;
                            perimeter += Math.Sqrt(((dx * dx) + (dy * dy)));
                            tempVertex = tempVertex.nextVertex;
                        }
                        if (typeName == "Polygon")
                        {
                            dx = tempPoly.poly.X - tempVertex.X;
                            dy = tempPoly.poly.Y - tempVertex.Y;
                            perimeter += Math.Sqrt(((dx * dx) + (dy * dy)));
                        }
                    }
                    else
                    {
                        dx = tempVertex.nextVertex.X - tempVertex.X;
                        dy = tempVertex.nextVertex.Y - tempVertex.Y;
                        perimeter += Math.Sqrt(((dx * dx) + (dy * dy)));
                    }
                    return perimeter;
                }
            }
            return 0.0;
        }
        public int IsConvex(int indexPoly)
        {
            NodePoly tempPoly = FindNodePolyBaseOnSelectedIndex(indexPoly);
            if (typeName == "Polygon" && tempPoly != null && tempPoly.poly != null)
            {
                NodeVertex tempVertex = tempPoly.poly;
                if (tempVertex.nextVertex != null && tempVertex.nextVertex.nextVertex != null)
                {
                    double ux, uy, vx, vy, sz;
                    bool indicator = true;
                    int i = 0;
                    while (tempVertex.nextVertex.nextVertex != null)
                    {
                        ux = tempVertex.nextVertex.X - tempVertex.X;
                        uy = tempVertex.nextVertex.Y - tempVertex.Y;
                        vx = tempVertex.nextVertex.nextVertex.X - tempVertex.nextVertex.X;
                        vy = tempVertex.nextVertex.nextVertex.Y - tempVertex.nextVertex.Y;
                        sz = (ux * vy) - (uy * vx);
                        if (i == 0) { indicator = sz > 0.0 ? true : false; }
                        if ((sz == 0.0)) { return -1; }
                        else if ((sz > 0.0 && !indicator) || (sz < 0.0 && indicator)) { return 0; }
                        tempVertex = tempVertex.nextVertex;
                        i++;
                    }
                    ux = tempVertex.nextVertex.X - tempVertex.X;
                    uy = tempVertex.nextVertex.Y - tempVertex.Y;
                    vx = tempPoly.poly.X - tempVertex.nextVertex.X;
                    vy = tempPoly.poly.Y - tempVertex.nextVertex.Y;
                    sz = (ux * vy) - (uy * vx);
                    if ((sz == 0.0)) { return -1; }
                    else if ((sz > 0.0 && !indicator) || (sz < 0.0 && indicator)) { return 0; }
                    else { return 1; }
                }
            }
            return -2;
        }
    }
}