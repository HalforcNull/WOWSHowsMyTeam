using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WOWSHowsMyTeam
{
    public class ShipMaster
    {
        private static readonly ShipMaster instance = new ShipMaster();

        private Dictionary<string, string> IDNameTable = new Dictionary<string, string>();
        private Dictionary<string, int> IDShipSorting = new Dictionary<string, int>();

        private ShipMaster() { }

        public static ShipMaster Instance
        {
            get
            {
                return instance;
            }
        }

        public int getSortingbyID(string id)
        {
            if (!IDShipSorting.ContainsKey(id))
            {
                string rawJson = HttpManager.GetJsonShip(id);
                UpdateShip(rawJson, id);
            }

            return IDShipSorting[id];
        }

        public string getNamebyId(string id)
        {
            if(!IDNameTable.ContainsKey(id))
            {
                string rawJson = HttpManager.GetJsonShip(id);
                UpdateShip(rawJson, id);
            }

            return IDNameTable[id];
        }


        private void UpdateShip(string rawJson, string id)
        {
            int head = rawJson.IndexOf(id);
            string usefulData = rawJson.Substring(head + id.Length + 2);
            usefulData = usefulData.Substring(0, usefulData.Length - 2);

            dynamic o = JObject.Parse(usefulData);

            IDNameTable[id] = o.name.ToString();
            int tier = Convert.ToInt32(o.tier.ToString());
            string type = o.type.ToString();
            IDShipSorting[id] = CalculateShipSort(tier, type);
        }

        private int CalculateShipSort(int tier, string type)
        {
            switch(type.ToLower())
            {
                case "aircarrier":
                    return 40 + tier;
                case "battleship":
                    return 20 + tier;
                case "cruiser":
                    return 10 + tier;
                case "destroyer":
                    return tier;
                default:
                    return 0;
            }
        }

    }
}
