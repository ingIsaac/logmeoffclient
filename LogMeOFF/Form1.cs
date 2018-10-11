using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Net.Sockets;
using System.Net;
using System.Timers;

namespace LogMeOFF
{
    public partial class Form1 : Form
    {
        private string app_name = "LogMeOFF";
        private string user_name = string.Empty;
        private string _server = string.Empty;
        private Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private int port = 6401;
        private System.Timers.Timer timer = new System.Timers.Timer(30000);
        private bool enable = true;

        public Form1()
        {
            InitializeComponent();           
            timer.Elapsed += Timer_Elapsed;
            loadConfig();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            button1.BackColor = Color.Green;
            enable = true;
            button1.Enabled = true;
        }

        private void loadConfig()
        {
            try
            {
                XDocument config = XDocument.Load(Application.StartupPath + "\\config.xml");
                this.user_name = (from x in config.Descendants("Config") select x.Element("user_name")).SingleOrDefault().Value.ToString();
                this._server = (from x in config.Descendants("Config") select x.Element("server")).SingleOrDefault().Value.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show(this, "[Error] el archivo config.xml no se encuentra en la carpeta de instalación o se está dañado." + Application.StartupPath, app_name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }      

        private void button1_Click(object sender, EventArgs e)
        {
            if (enable)
            {
                try
                {
                    if (disconnect(user_name))
                    {
                        MessageBox.Show(this, "Has sido desconectado del servidor.", app_name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, err.ToString(), app_name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }           
        }

        public bool disconnect(string _data)
        {
            if (!server.Connected || server.Available == 0)
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var result = server.BeginConnect(new IPEndPoint(getSocketIPAddress(_server), port), null, null);
                bool success = result.AsyncWaitHandle.WaitOne(10000, true);
                if (success)
                {
                    try
                    {
                        byte[] data = System.Text.Encoding.Default.GetBytes(_data);
                        server.Send(data);
                        button1.BackColor = Color.Red;
                        enable = false;
                        button1.Enabled = false;
                        timer.Start();
                        return true;
                    }
                    catch (Exception)
                    {
                        server.Close();
                        return false;
                    }
                }
                else
                {
                    server.Close();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public IPAddress getSocketIPAddress(string p)
        {
            if (getIPformString(p) != null)
            {
                return getIPformString(p);
            }
            else if (getIPfromHost(p) != null)
            {
                return getIPfromHost(p);
            }
            else
            {
                MessageBox.Show(this, "[Error] no se ha podido definir la dirección IP del servidor", app_name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private IPAddress getIPformString(string ip)
        {
            IPAddress r;
            if (IPAddress.TryParse(ip, out r))
            {
                return r;
            }
            else
            {
                return null;
            }
        }

        private IPAddress getIPfromHost(string p)
        {
            IPAddress[] ips = null;
            try
            {
                ips = Dns.GetHostAddresses(p);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "[Error] no se ha podido definir el host de la IP del servidor.", app_name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (ips == null || ips.Length == 0)
            {
                ips[0] = null;
            }

            return ips[0];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (p.Length > 1)
            {
                MessageBox.Show("[Error] solo puede existir una instancia de este programa.", app_name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
    }
}
