using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTD_Remote_Server_Manager
{
    class Player
    {
        protected string sName { get; set; }
        protected int userID { get; set; }

        public Player(string sName, int userID)
        {
            this.sName = sName;
            this.userID = userID;
        }

        public override string ToString()
        {
            return "ID: " + this.userID + ", Name: " + this.sName; 
        }
    }
}
