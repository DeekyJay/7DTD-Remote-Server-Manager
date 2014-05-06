using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTD_Remote_Server_Manager
{
    class SpawnEntity
    {
        public string entityName { get; set; }
        public int entityID { get; set; }

        public SpawnEntity(string eName, int eID)
        {
            this.entityName = eName;
            this.entityID = eID;
        }

        public override string ToString()
        {
            return this.entityName;
        }
    }
}
