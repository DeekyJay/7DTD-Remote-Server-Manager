using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTD_Remote_Server_Manager
{
    class Player
    {
        public string sName { get; set; }
        public int userID { get; set; }

        public Player(string sName, int userID)
        {
            this.sName = sName;
            this.userID = userID;
        }

        public override string ToString()
        {
            return this.sName;
        }

        public string playerName
        {
            get { return this.sName; }
        }

        public int playerId
        {
            get { return this.userID; }
        }
    }
}
