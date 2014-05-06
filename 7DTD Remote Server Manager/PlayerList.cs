using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace _7DTD_Remote_Server_Manager
{
    class PlayerList : RemoteServerWindow
    {

        List<Player> lPlayers;

        public void PopulateList()
        {
            while (true && UserConfig.tnet.IsConnected)
            {
                refreshData();

                UserConfig.tnet.WriteLine("lp");

                string result = UserConfig.tnet.Read();

                if (result.Contains("1. id"))
                {
                    int find = result.IndexOf("1. id");
                    try
                    {
                        result = result.Substring(find);
                    }
                    catch (ArgumentOutOfRangeException exp)
                    {
                        UserConfig.tnet.WriteLine("exit");
                    }

                    string[] players = Regex.Split(result, ",|\n");

                    string[] playersFiltered = new string[(int)(players.Length / 5.5)];

                    UserConfig.players = new HashSet<Player>();

                    for (int i = 0; i < players.Length; i++)
                    {

                        if (players[i].Contains("id="))
                        {
                            //playersFiltered[index] = players[i].Substring(6);
                            //playersFiltered[index + 1] = players[i+1].TrimStart();
                            int equalsChar = players[i].IndexOf('=');

                            string sId = players[i].Substring(equalsChar + 1);
                            int id = Int32.Parse(sId);
                            string player = players[i + 1].TrimStart();
                            //string player = "Test123";

                            UserConfig.players.Add(new Player(player, id));

                        }

                    }

                    lPlayers = new List<Player>();

                    for (int i = 0; i < UserConfig.players.Count; i++)
                    {
                        lPlayers.Add(UserConfig.players.ElementAt(i));
                    }

                    //for (int i = 0; i < (playersFiltered.Length / 2) - 1; i++)
                    // {
                    // UserConfig.players.Add(new Player(playersFiltered[i + 1], Int32.Parse(playersFiltered[i])));
                    //}

                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        public void refreshData()
        {
            listPlayers.DataSource = null;

            listPlayers.DisplayMember = "userID";
            listPlayers.ValueMember = "sName";

            listPlayers.DataSource = lPlayers;
        }
    }
}
