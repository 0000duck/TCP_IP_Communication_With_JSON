using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Clients
{
    public partial class Form1 : Form
    {
        List<SendingData> List_Data = new List<SendingData>();
        private Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 100;
        public Form1()
        {
            InitializeComponent();
        }
        private void GridviewDesign()
        {
            CString.Width = (dataGridView1.Width * 33) / 100;
            CInt.Width = (dataGridView1.Width * 33) / 100;
            CFloat.Width = (dataGridView1.Width * 34) / 100;

            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;
            dataGridView1.BackgroundColor = Color.White;

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            GridviewDesign();
            timer1.Start();
            ConnectToServer();
        }
        private void ConnectToServer()
        {
            if (!ClientSocket.Connected)
            {
                try
                {
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException ex)
                {
                    lblMessage.Text = ex.Message;
                }
            }
        }
        private string Create_Values()
        {
            string Data = "";
            int SendingInt = 0;
            float SendingFloat = 0;
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Data = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());

            SendingInt = random.Next(1, 40);

            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            SendingFloat = (float)(mantissa * exponent);

            SendingData sending_data = new SendingData();
            sending_data.FloatData = SendingFloat;
            sending_data.IntData = SendingInt;
            sending_data.StringData = Data;

            var content = JsonConvert.SerializeObject(sending_data);
            List_Data.Add(sending_data);

            return content.ToString();
        }
        public class SendingData
        {
            public string StringData { get; set; }
            public float FloatData { get; set; }
            public int IntData { get; set; }
        }
        private void SendString(string text)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(text);
                ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = List_Data;
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                timer1.Stop();
                if (!List_Data.Count.Equals(0))
                {
                    List_Data.RemoveAt(List_Data.Count - 1);
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = List_Data;
                }
                lblMessage.Text = ex.Message;
                ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ConnectToServer();
                timer1.Start();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            string values = Create_Values();
            SendString(values);
            timer1.Start();
        }
    }
}
