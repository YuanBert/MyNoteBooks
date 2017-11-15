using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace MotorTestPC
{
    public partial class MotorCtrl : Form
    {
        List<string>  uartName =new List<string>();
        string[] sUartName; //= new string[10];
        DataTable dt = new DataTable("Current_Temperture");
        bool listeningFlag = false;
        bool displayTempertureLineFlag = false;
        long gTimeCnt = 0;
        

        public MotorCtrl()
        {
            InitializeComponent();

            dt.Columns.Add("CurrentVale",typeof(Int16));
            dt.Columns.Add("TempertureValue",typeof(Int16));
            dt.Columns.Add("Date",System.Type.GetType("System.DateTime"));

            trackBarYmax.Value = Convert.ToInt32(chart1.ChartAreas[0].AxisY.Maximum);
            trackBarYlength.Value = Convert.ToInt32(chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum);
            buttonStart.Enabled = false;
            trackBarYmax.Enabled = false;
            trackBarYlength.Enabled = false;
            sUartName = SerialPort.GetPortNames();
            //foreach (string com in SerialPort.GetPortNames())
            //{
            //    uartName.Add(com);
            //}
            if (sUartName.Length > 0)
            {
                comboBox1.DataSource = sUartName;
                comboBox1.SelectedIndex = 0;
                buttonStart.Enabled = true;
            }
            else
            {
                MessageBox.Show("没有发现串口设备！");
            }
            
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if ("开始" == buttonStart.Text)
            {
                serialPortOfSenser.PortName = comboBox1.SelectedItem.ToString();
                try
                {
                    timer1.Start();
                    serialPortOfSenser.Open();
                    serialPortOfSenser.Write("S");
                    buttonStart.Text = "停止";
                    button1.BackColor = Color.Red;
                    trackBarYmax.Enabled = true;
                    trackBarYlength.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (true == serialPortOfSenser.IsOpen)
                {

                }
                else
                {
                    MessageBox.Show(" 设备打开失败！");
                }            
            }
            else
            {
                try
                {
                    timer1.Stop();
                    serialPortOfSenser.Write("P");
                    serialPortOfSenser.Close();
                    chart1.Series[0].Points.Clear();
                    chart1.Series[1].Points.Clear();
                    gTimeCnt = 0;
                    buttonStart.Text = "开始";
                    button1.BackColor = Color.Green;
                    trackBarYmax.Enabled = false;
                    trackBarYlength.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
        }

        private void serialPortOfSenser_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (false == serialPortOfSenser.IsOpen && listeningFlag)
            {
                MessageBox.Show("请打开某个串口","错误提示");
                return;
            }
            byte[] ReDatas = new byte[6];
            serialPortOfSenser.Read(ReDatas,0,6);
            serialPortOfSenser.DiscardInBuffer();
            if (ReDatas[0] != 0xFF || ReDatas[1] != 0xFF)
            {
                return;
            }
            int iCurrentValue = 0;
            int tTempertureValue = 0;

            iCurrentValue = ReDatas[2] << 8;
            iCurrentValue += ReDatas[3];

            tTempertureValue = ReDatas[4] << 8;
            tTempertureValue += ReDatas[5];
            this.BeginInvoke(new EventHandler(delegate
            {
                chart1.Series[0].Points.AddXY(gTimeCnt, iCurrentValue);
                if(displayTempertureLineFlag)
                {
                    chart1.Series[1].Points.AddXY(gTimeCnt, tTempertureValue);
                }
                gTimeCnt++;

                if (gTimeCnt > 500)
                {
                    gTimeCnt = 0;
                    chart1.Series[0].Points.Clear();
                    if (displayTempertureLineFlag)
                    {
                        chart1.Series[1].Points.Clear();
                    }
                    
                }
            }));

            dt.Rows.Add(iCurrentValue, tTempertureValue, DateTime.Now);
            if (dt.Rows.Count > 18000)
            {
                //this.BeginInvoke(new EventHandler(delegate
                //{
                    dt.Rows.Clear();
               // }));
            }


            //int i = 0;
            //for (i = 0; i < ReDatas.Length; )
            //{
            //    iCurrentValue = ReDatas[i++] << 8;
            //    iCurrentValue += ReDatas[i++];

            //    tTempertureValue = ReDatas[i++] << 8;
            //    tTempertureValue += ReDatas[i++];
            //    dt.Rows.Add(iCurrentValue, tTempertureValue, DateTime.Now);


            //}
            //dt.Rows.Add(iCurrentValue,tTempertureValue,DateTime.Now);
            

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listeningFlag = true;
            //DataTable tDt = new DataTable();
            //int i = 0;
            //int n = (dt.Rows.Count);
            //int[] iCurrentArry = new int[n];
            //int[] tTempertureArry = new int[n];

            //for (i = 0; i < dt.Rows.Count; i++)
            //{
            //    tTempertureArry[i] =Convert.ToInt16(dt.Rows[i][1].ToString());
            //    iCurrentArry[i] = Convert.ToInt16(dt.Rows[i][0].ToString());
            //}



            //if (dt.Rows.Count > 8000)
            //{
            //    this.BeginInvoke(new EventHandler(delegate
            //    {
            //        dt.Rows.Clear();
            //    })); 
            //}

            listeningFlag = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                displayTempertureLineFlag = true;
            }
            else
            {
                displayTempertureLineFlag = false;
                chart1.Series[1].Points.Clear();
            }
        }

        private void trackBarYmax_ValueChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisY.Maximum = trackBarYmax.Value;
            chart1.ChartAreas[0].AxisY.Minimum = trackBarYmax.Value - trackBarYlength.Value;
            textBox1.Text = trackBarYmax.Value.ToString();
        }

        private void trackBarYlength_ValueChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisY.Maximum = trackBarYmax.Value;
            chart1.ChartAreas[0].AxisY.Minimum = trackBarYmax.Value - trackBarYlength.Value;
            textBox2.Text = trackBarYlength.Value.ToString();
        }
    }
}
