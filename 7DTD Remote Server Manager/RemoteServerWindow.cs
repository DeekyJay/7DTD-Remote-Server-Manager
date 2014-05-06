using Renci.SshNet;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;

namespace _7DTD_Remote_Server_Manager
{
    public partial class RemoteServerWindow : Form
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "7DTD Server Manager");
        string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "7DTD Server Manager/user.conf");
        public RemoteServerWindow()
        {
            InitializeComponent();

            Directory.CreateDirectory(path);

            if (File.Exists(filename))
            {
                this.chkCredentials.Checked = true;

                string infoLine = File.ReadAllText(filename);

                string[] info = infoLine.Split(',');

                string decryptedPass = Decrypt(info[3], info[2]);
                string decryptedTelnetPass = Decrypt(info[4], info[2]);

                this.txtServerIP.Text = info[0];
                this.txtUser.Text = info[1];
                this.txtPass.Text = decryptedPass;
                this.txtTelnetPass.Text = decryptedTelnetPass;
                this.txtTelnetPort.Text = info[5];

                decryptedPass = null;
                decryptedTelnetPass = null;
            }

            List<SpawnEntity> se = new List<SpawnEntity>();

            se.Add(new SpawnEntity("Zombie 04", 1));
            se.Add(new SpawnEntity("Zombie 05", 2));
            se.Add(new SpawnEntity("Zombie 06", 3));
            se.Add(new SpawnEntity("Zombie 07", 4));
            se.Add(new SpawnEntity("Zombie Crawler", 5));
            se.Add(new SpawnEntity("Snow Zombie 01", 6));
            se.Add(new SpawnEntity("Snow Zombie 02", 7));
            se.Add(new SpawnEntity("Snow Zombie 03", 8));
            se.Add(new SpawnEntity("Spider Zombie", 9));
            se.Add(new SpawnEntity("Burnt Zombie", 10));
            se.Add(new SpawnEntity("Zombie Gal 01", 11));
            se.Add(new SpawnEntity("Zombie Gal 02", 12));
            se.Add(new SpawnEntity("Zombie Gal 03", 13));
            se.Add(new SpawnEntity("Zombie Gal 04", 14));
            se.Add(new SpawnEntity("Zombie 02", 15));
            se.Add(new SpawnEntity("Fat Zombie Cop", 16));
            se.Add(new SpawnEntity("Fat Zombie", 17));
            se.Add(new SpawnEntity("Hornet", 18));
            se.Add(new SpawnEntity("Zombie Dog", 19));
            se.Add(new SpawnEntity("Blue Car", 20));
            se.Add(new SpawnEntity("Orange Car", 21));
            se.Add(new SpawnEntity("Red Car", 22));
            se.Add(new SpawnEntity("White Car", 23));
            se.Add(new SpawnEntity("Stag (Deer)", 24));
            se.Add(new SpawnEntity("Rabbit", 25));
            se.Add(new SpawnEntity("Pig", 26));
            se.Add(new SpawnEntity("Melee Weapons", 27));
            se.Add(new SpawnEntity("Food", 28));
            se.Add(new SpawnEntity("Building Supplies", 29));
            se.Add(new SpawnEntity("Ranged Weapons", 30));
            se.Add(new SpawnEntity("Ranged Weapons Day 5", 31));
            se.Add(new SpawnEntity("Ranged Weapons Day 7", 32));
            se.Add(new SpawnEntity("Explosives", 33));
            se.Add(new SpawnEntity("General", 34));

            cboItems.DataSource = se;
            cboItems.DisplayMember = "entityName";
            cboItems.ValueMember = "entityID";

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.txtServerIP.Text = "";
            this.txtUser.Text = "";
            this.txtPass.Text = "";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            UserConfig.client = new SshClient(this.txtServerIP.Text, this.txtUser.Text, this.txtPass.Text);
            try
            {
                UserConfig.client.Connect();
            }
            catch (Renci.SshNet.Common.SshAuthenticationException exp)
            {
                UserConfig.client.Disconnect();
                System.Console.WriteLine("Authentication Error!");
                System.Console.WriteLine(exp);
            }

            if (UserConfig.client.IsConnected)
            {
                this.btnConnect.Enabled = false;
                this.btnDisconnect.Enabled = true;
                this.grpSSHCommands.Enabled = true;

                var cmdStatus = UserConfig.client.CreateCommand("/etc/init.d/7dtd.sh status");

                cmdStatus.Execute();

                string result = cmdStatus.Result;

                Console.Out.WriteLine(result);
                if (result.Contains("server is running"))
                {
                    this.btnStartServer.Enabled = false;
                    this.btnKillServer.Enabled = true;
                }
                else
                {
                    this.btnStartServer.Enabled = true;
                    this.btnKillServer.Enabled = false;
                }

            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (UserConfig.client.IsConnected)
            {
                UserConfig.client.RunCommand("logout");

                if (UserConfig.client.IsConnected)
                {
                    UserConfig.client.Disconnect();
                    this.btnDisconnect.Enabled = false;
                    this.btnConnect.Enabled = true;
                    this.grpSSHCommands.Enabled = false;
                    this.grpServerCommands.Enabled = false;
                }
                else
                {
                    Console.Out.WriteLine("Succesfully logged out and disconnected!");
                }
            }
        }

        private void chkCredentials_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkCredentials.Checked)
            {
                File.Delete(filename);
            }

            if (chkCredentials.Checked)
            {

                string sAddress = this.txtServerIP.Text;
                string sUser = this.txtUser.Text;
                string sPass = this.txtPass.Text;
                string s7DTDPass = this.txtTelnetPass.Text;
                string sTelnetPort = this.txtTelnetPort.Text;

                if (sPass != "")
                {
                    string sCode = sPass.Substring(sPass.Length - 3) + sPass.Substring(sPass.Length - 3, 2);

                    string encryptPass = Encrypt(sPass, sCode);
                    string encryptTelnetPass = Encrypt(s7DTDPass, sCode);

                    System.IO.File.WriteAllText(filename, sAddress + "," + sUser + "," + sCode + "," + encryptPass + "," + encryptTelnetPass + "," + sTelnetPort);

                    sPass = null;
                    encryptPass = null;
                }

            }

        }

        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(UserConfig.initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(UserConfig.keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(UserConfig.initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(UserConfig.keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            var cmdRun = UserConfig.client.CreateCommand("7dtd-start.sh");

            cmdRun.Execute();

            string result = cmdRun.Result;
            Console.Out.WriteLine(result);
            if (result.Contains("Done!"))
            {
                this.btnStartServer.Enabled = false;
                this.btnKillServer.Enabled = true;
            }
        }

        private void btnKillServer_Click(object sender, EventArgs e)
        {
            var cmdKill = UserConfig.client.CreateCommand("7dtd-kill.sh");

            cmdKill.Execute();

            string result = cmdKill.Result;
            Console.Out.WriteLine(result);
            if (result.Contains("Done!"))
            {
                this.btnStartServer.Enabled = true;
                this.btnKillServer.Enabled = false;
            }
        }

        private void btnTelnetConnect_Click(object sender, EventArgs e)
        {
            try
            {
                UserConfig.tnet = new TelnetConnection(this.txtServerIP.Text, Int32.Parse(this.txtTelnetPort.Text));
                string loginResult = UserConfig.tnet.Login(this.txtTelnetPass.Text);

                Console.WriteLine(loginResult);
                Console.Read();

                if (UserConfig.tnet.IsConnected && loginResult.Contains("password:"))
                {
                    this.btnTelnetConnect.Enabled = false;
                    this.btnDisconnect.Enabled = true;
                    this.grpServerCommands.Enabled = true;
                }
                else
                {
                    UserConfig.tnet.Disconnect();
                }
                
            }
            catch(SocketException exp)
            {
                Console.WriteLine(exp.Data);
            }
            finally
            {

            } 
        }

        private void btnSetTime_Click(object sender, EventArgs e)
        {
            UserConfig.tnet.WriteLine("st " + this.txtSetTime.Text);
        }

        private void btnSay_Click(object sender, EventArgs e)
        {
            UserConfig.tnet.WriteLine("say " + this.txtSay.Text);
        }
   

        private void btnTelnetShutdown_Click(object sender, EventArgs e)
        {
            UserConfig.tnet.WriteLine("shutdown");

            if (UserConfig.tnet.IsConnected)
            {
                UserConfig.tnet.Disconnect();
            }

            this.grpServerCommands.Enabled = false;
            this.btnTelnetConnect.Enabled = true;
        }

        private void btnTelnetExit_Click(object sender, EventArgs e)
        {
            UserConfig.tnet.WriteLine("exit");

            System.Threading.Thread.Sleep(500);

            if (UserConfig.tnet.IsConnected)
            {
                UserConfig.tnet.Disconnect();
            }

            this.btnTelnetConnect.Enabled = true;
            this.grpServerCommands.Enabled = false;
        }

        private void btnHorde_Click(object sender, EventArgs e)
        {
            UserConfig.tnet.WriteLine("spawnwanderinghorde");
        }

        private void btnSpawnEntity_Click(object sender, EventArgs e)
        {
            UserConfig.tnet.WriteLine("se " + txtPlayerNum.Text + " " + cboItems.SelectedValue);
        }
    }
}
