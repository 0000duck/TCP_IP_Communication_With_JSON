using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public partial class Form1 : Form
    {
        public List<SendingData> List_Data = new List<SendingData>();
        private readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 100;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
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
        public class SendingData
        {
            public string StringData { get; set; }
            public float FloatData { get; set; }
            public int IntData { get; set; }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            GridviewDesign();
            SetupServer();
        }
        private void SetupServer()
        {
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
        }
        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;
            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            serverSocket.BeginAccept(AcceptCallback, null);
        }
        public delegate void AddValueToGridviewDelegate();
        private void AddValueToGridview()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = List_Data;
        }
        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            SendingData IncomingData = JsonConvert.DeserializeObject<SendingData>(text);
            StringBuilder sb = new StringBuilder();
            sb.Append("Gelen String: ");
            sb.Append(IncomingData.StringData);
            sb.Append(",Gelen Float: ");
            sb.Append(IncomingData.FloatData.ToString());
            sb.Append(", Gelen İnt: ");
            sb.Append(IncomingData.IntData.ToString());
            List_Data.Add(IncomingData);
            if (dataGridView1.InvokeRequired)
                dataGridView1.Invoke(new AddValueToGridviewDelegate(AddValueToGridview));
            else
            {
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = List_Data;
            }
            byte[] data = Encoding.ASCII.GetBytes(sb.ToString());
            current.Send(data);
            sb = null;
            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
    }
}
