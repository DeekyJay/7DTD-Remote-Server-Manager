using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace _7DTD_Remote_Server_Manager
{
    class UserConfig
    {
        public static SshClient client;

        public static TelnetConnection tnet;

        public const string initVector = "tu89geji340t89u2";

        public const int keysize = 256;

        public static HashSet<Player> players;

    }
}
