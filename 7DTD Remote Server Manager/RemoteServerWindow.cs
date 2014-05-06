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
        public RemoteServerWindow()
        {
            InitializeComponent();

            if (File.Exists("user.conf"))
            {
                this.chkCredentials.Checked = true;

                string infoLine = File.ReadAllText("user.conf");

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
                File.Delete("user.conf");
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

                    System.IO.File.WriteAllText("user.conf", sAddress + "," + sUser + "," + sCode + "," + encryptPass + "," + encryptTelnetPass + "," + sTelnetPort);

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
    }
}
