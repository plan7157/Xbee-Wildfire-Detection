using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using XBee;


namespace Adhoc_Project_8031
{
    public partial class Form1 : Form
    {
        Bitmap newBitmap;
        Image file;
        bool checkmark = false;
        NodeData myNode = new NodeData();
        int TempMax = 41;
        bool logwrite = false;
        int nodeSize = 10;

        //Xbee
        public XBeeController controller;
        public event EventHandler<NodeDiscoveredEventArgs> NodeDiscovered1;


        public Form1()
        {
            InitializeComponent();
            controller = new XBeeController();
            //connect(controller);
            controller.NodeDiscovered += (sender, args) =>
            {
                //buffer1 = args.Node;
                //MessageBox.Show(args.Name);
                searchList(listBox1, args.Name,myNode,pictureBox1,1, controller, TempMax);
                
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if(dr == DialogResult.OK)
            {
                file = Image.FromFile(openFileDialog1.FileName);
                newBitmap = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = file;
            }
        }

        private void AddList_Click(object sender, EventArgs e)
        {
            if(AutoAddListrbt.Checked == true)
            {
                if(portstatus.Text != "-")
                {
                    getInfoNodeNow(controller, myNode, listBox1);
                    
                    portstatus.Text = "-";
                }
                else
                {
                    MessageBox.Show("Please connect port.");
                }
                
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(nodename.Text) && !string.IsNullOrWhiteSpace(address.Text))
                {
                    listBox1.Items.Add(nodename.Text);
                    myNode.pushNodeData(nodename.Text, address.Text);

                    address.Text = null;
                    nodename.Text = null;
                }
                else
                {
                    MessageBox.Show("please fill name and address.");
                }
            }
                
        }
        public static async void getInfoNodeNow(XBeeController x, NodeData node,ListBox list)
        {
            var localNode = x.Local;
            var serialNumber = await localNode.GetSerialNumber();
            var name = "-1";
            name= await localNode.GetNodeIdentifier();
            if (name != "-1")
            {
                x.Close();
                node.pushNodeData(name, serialNumber.ToString());
                list.Items.Add(name);
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String[] stpPortNames = SerialPort.GetPortNames();
            foreach (string x in stpPortNames)
            {
                comboBox1.Items.Add(x);
            }
            if(comboBox1.Items.Count == 0)
            {
                MessageBox.Show("Please plug in Xbee");
                Application.Exit();
            }
            else
            {
                comboBox1.SelectedIndex = 0;
            }
            
        }

        private void startFD_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false && portstatus.Text != "-")
            {
                logwrite = true;
                logtxt.Text+=(DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + System.Environment.NewLine+": Start Fire Detection"+ System.Environment.NewLine);
                workingstatus.Text = "WORKING";
                timer1.Interval = Convert.ToInt32(timeSet.Text)*1000;
                clearStatus(listBox1, myNode, pictureBox1);
                logtxt.Text += ("==>CLEAR<==" + System.Environment.NewLine);
                sentcommand(controller);
                timer1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Please connect port");
            }
            
        }

        private void stopFD_Click(object sender, EventArgs e)
        {
            if(timer1.Enabled == true)
            {
                //searchList(listBox1);
                logtxt.Text += (DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + System.Environment.NewLine + ": Stop Fire Detection" + System.Environment.NewLine);
                timer1.Enabled = false;
                workingstatus.Text = "OFF";
            }
            
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //int numOfCells = 200;
            //int cellSize = 10;
            Pen p = new Pen(Color.Black);

            /*for (int y = 0; y < numOfCells; ++y)
            {
                g.DrawLine(p, 0, y * cellSize, numOfCells * cellSize, y * cellSize);
            }

            for (int x = 0; x < numOfCells; ++x)
            {
                g.DrawLine(p, x * cellSize, 0, x * cellSize, numOfCells * cellSize);
            }*/
            foreach (DataRow row in myNode.getNodeTable().Rows)
            {
                if(row.Field<int>(3)!=-1 && row.Field<int>(4)!= -1)
                {

                    //MessageBox.Show(row.Field<int>(2).ToString());
                    SolidBrush sb;
                    if (row.Field<int>(2)==1)
                    {
                        sb = new SolidBrush(Color.Green);
                        if(logwrite==true)
                            logtxt.Text += (DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + System.Environment.NewLine + ": "+ row.Field<string>(0) + "==> PASS" + System.Environment.NewLine);
                    }
                    else if (row.Field<int>(2) == 2)
                    {
                        sb = new SolidBrush(Color.Red);
                        if (logwrite == true)
                            logtxt.Text += (DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + System.Environment.NewLine + ": " + row.Field<string>(0) + "==> WARNING" + System.Environment.NewLine);
                    }
                    else if (row.Field<int>(2) == 3)
                    {
                        sb = new SolidBrush(Color.Yellow);
                        if (logwrite == true)
                            logtxt.Text += (DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + System.Environment.NewLine + ": " + row.Field<string>(0) + "==> WARNING" + System.Environment.NewLine);
                    }
                    else
                    {
                        sb = new SolidBrush(Color.Black);
                        if (logwrite == true)
                            logtxt.Text += (DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + System.Environment.NewLine + ": " + row.Field<string>(0) + "==> DISCONNECT" + System.Environment.NewLine);
                    }
                    
                    Rectangle r = new Rectangle(row.Field<int>(3), row.Field<int>(4), nodeSize, nodeSize);
                    g.DrawRectangle(p, r);
                    g.FillRectangle(sb, r);
                }
                else
                {

                }
            }


            
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int xx;
            int yy;
            if (checkmark == true)
            {
                Point p = new Point(e.X, e.Y);
                xx = (p.X / 10) * 10;
                yy = (p.Y / 10) * 10;

                myNode.pushNodeAxis(listBox1.SelectedIndex, xx, yy);

                pictureBox1.Invalidate();
                checkmark = false;
            }
            
        }

        private void AddMark_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                checkmark = true;
            }
            else
            {
                MessageBox.Show("please select node list");
            }
            
        }

        public static void searchList(ListBox nameList,string nodeName,NodeData node,PictureBox pic,int status, XBeeController controller,int tempmax)
        {
            int count = 0;
            foreach(var x in nameList.Items)
            {
                if (string.Compare(x.ToString(), nodeName)==0)
                {
                    node.statusUpdate(count, status);
                    checkTemp(controller, node , pic, count, 2,tempmax);
                    
                    break;
                }
                count++;
            }
            
        }
        public static void clearStatus(ListBox nameList,NodeData node, PictureBox pic)
        {
            
            int count = 0;
            foreach (var x in nameList.Items)
            {
                node.statusUpdate(count, 0);
                count++;
            }
            pic.Invalidate();
        }

        //XBEE Function

        public static async void sentcommand(XBeeController controller)
        {
            await controller.DiscoverNetwork();
        }

        public static async void connect(XBeeController controller,string port)
        {
            controller.Close();
            await controller.OpenAsync(port, 9600);
            

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            logtxt.Text += ("==>RECHECK<==" + System.Environment.NewLine);
            clearStatus(listBox1, myNode, pictureBox1);
            logtxt.Text += ("==>CLEAR<==" + System.Environment.NewLine);
            sentcommand(controller);
        }

        public static async void checkTemp(XBeeController controller, NodeData node, PictureBox pic, int index,int status,int tempmax)
        {
            var address = node.getAddress(index);
            LongAddress x = new LongAddress(Convert.ToUInt32(address.Substring(0, 8), 16), Convert.ToUInt32(address.Substring(8, 8), 16));
            NodeAddress a = new NodeAddress(x);
            //NodeAddress x = new NodeAddress(serialNumber1);
            var remoteNode = await controller.GetRemoteAsync(a);
            var serialNumber2 = await remoteNode.GetTemperature();
            /*if (!checkOver9(serialNumber2.ToString()))
            {*/
                if (Convert.ToInt32(serialNumber2.ToString(),16) >= tempmax)
                {
                    node.statusUpdate(index, status);
                }
                
            /*}
            else
            {
                node.statusUpdate(index, 3);
            }*/
            pic.Invalidate();
            //MessageBox.Show(x.ToString());
        }

        public static bool checkOver9(string x)
        {
            for (int i = x.Length-1; i >= 0; i--)
            {
                if (x[i] >= 'A')
                {
                    return true;
                }
            }
            return false;
        }

        private void connectbtn_Click(object sender, EventArgs e)
        {
            connect(controller, comboBox1.SelectedItem + "");
            portstatus.Text = comboBox1.SelectedItem + "";
        }

        private void editlistbtn_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                listBox1.Items[listBox1.SelectedIndex] = nodename.Text;
                myNode.updateNode(nodename.Text, address.Text, listBox1.SelectedIndex);
                /*foreach(DataRow x in myNode.getNodeTable().Rows)
                {
                    MessageBox.Show(x.Field<string>(0));
                }*/
            }
            else
            {
                MessageBox.Show("please select node list");
            }
        }

        private void deletelist_Click(object sender, EventArgs e)
        {

            if (listBox1.SelectedIndex > -1)
            {
                myNode.removeRow(listBox1.SelectedIndex);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                pictureBox1.Invalidate();
            }
            else
            {
                MessageBox.Show("please select node list");
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
            Graphics g = panel1.CreateGraphics();
            Pen p = new Pen(Color.Black);
            SolidBrush sb = new SolidBrush(Color.Black);
            Rectangle r = new Rectangle(3,3, (4+Convert.ToInt32(numericUpDown1.Value))*2, (4 + Convert.ToInt32(numericUpDown1.Value)) * 2);
            g.DrawRectangle(p, r);
            g.FillRectangle(sb, r);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void setNodeSizebtn_Click(object sender, EventArgs e)
        {
            nodeSize = (4 + Convert.ToInt32(numericUpDown1.Value)) * 2;
            pictureBox1.Invalidate();
        }

        private void exportLogbtn_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "txt files (*.txt)|*.txt";
                sfd.FilterIndex = 2;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, logtxt.Text);
                }
            }
        }

        private void saveSetingbtn_Click(object sender, EventArgs e)
        {
            string buffer="";
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "txt files (*.txt)|*.txt";
                sfd.FilterIndex = 2;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    buffer += (nodeSize-8)/2 + "@" + timeSet.Text+ Environment.NewLine;
                    foreach (DataRow row in myNode.getNodeTable().Rows)
                    {
                        buffer += row.Field<string>(0)+"@"+ row.Field<string>(1) + "@" + row.Field<int>(2) + "@" + row.Field<int>(3) + "@" + row.Field<int>(4)+Environment.NewLine;
                    }

                    File.WriteAllText(sfd.FileName, buffer);
                }
            }
        }

        private void loadSettingbtn_Click(object sender, EventArgs e)
        {
            
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                pictureBox1.Image = null;
                string[] buffer;
                listBox1.Items.Clear();
                myNode.getNodeTable().Clear();
                int index = -1;
                buffer= File.ReadAllLines(openFileDialog1.FileName);
                foreach(var x in buffer)
                {
                    var y = x.Split('@');
                    if (index == -1)
                    {
                        numericUpDown1.Value = Convert.ToInt32(y[0]);
                        nodeSize = (4 + Convert.ToInt32(numericUpDown1.Value)) * 2;
                        timeSet.Text = y[1];
                        index++;
                    }
                    else
                    {
                        myNode.pushNodeData(y[0], y[1]);
                        myNode.pushNodeAxis(index, Convert.ToInt32(y[3]), Convert.ToInt32(y[4]));
                        listBox1.Items.Add(y[0]);
                        index++;
                    }
                    
                }
                panel1.Invalidate();
                pictureBox1.Invalidate();
            }
            /*
            foreach (DataRow row in myNode.getNodeTable().Rows)
            {
                MessageBox.Show(row.Field<string>(0));
            }*/

        }

        private void reScanPortbtn_Click(object sender, EventArgs e)
        {
            controller.Close();
            portstatus.Text = "-";
            String[] stpPortNames = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            foreach (string x in stpPortNames)
            {
                comboBox1.Items.Add(x);
            }
            if (comboBox1.Items.Count == 0)
            {
                MessageBox.Show("Please plug in Xbee");
                Application.Exit();
            }
            else
            {
                comboBox1.SelectedIndex = 0;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            logtxt.Text = "";
        }
    }
}
