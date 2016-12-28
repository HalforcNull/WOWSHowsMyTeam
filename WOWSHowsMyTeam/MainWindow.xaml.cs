using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace WOWSHowsMyTeam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CheckData()
        {
            if (File.Exists("tempArenaInfo.json"))
            {
                string content = File.ReadAllText("tempArenaInfo.json");
                TempLeaderBoardRoot root = JsonConvert.DeserializeObject<TempLeaderBoardRoot>(content);
                updateView(root);
            }
        }

        private void updateView(TempLeaderBoardRoot root)
        {
            PlayerDatas pd = new PlayerDatas(root.vehicles, true);
            PlayerDatas pdE = new PlayerDatas(root.vehicles, false);
            
            dataGrid.ItemsSource = pd.OrderByDescending(t => t.requestSortIndex()) ;
            dataGridEnemy.ItemsSource = pdE.OrderByDescending(t => t.requestSortIndex());
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CheckData();
        }
    }

    public class PlayerDatas:List<PlayerData>
    {
        public PlayerDatas(List<Vehicle> Vehicles, bool ownTeam)
        {
            foreach (Vehicle v in Vehicles)
            {
                if (v.relation < 2 && ownTeam
                    ||v.relation == 2 && !ownTeam)
                { 
                    PlayerData p = new PlayerData(v);
                    this.Add(p);
                }
            }
        }

    }

    public class PlayerData 
    {
        public PlayerData(Vehicle v)
        {
            Name = v.name;
            ShipId = v.shipId.ToString();
            ShipName = ShipMaster.Instance.getNamebyId(ShipId);
            SortIndex = ShipMaster.Instance.getSortingbyID(ShipId);
            Team = v.relation;
            PlayerId = queryId();
            fillShipMastery();
        }

        private string queryId()
        {
            string response = HttpManager.GetJsonPlayerIDQuery(Name);
            dynamic o = JObject.Parse(response);
            return o.data.First.account_id.ToString();
        }

        private void fillShipMastery()
        {
            string response = HttpManager.GetJsonPlayerShipMastery(PlayerId, ShipId);
            int head = response.IndexOf("[{");
            int end = response.LastIndexOf("}]");
            string usefulData = response.Substring(head + 1, end - head);

            if (usefulData.Length == 0)
            {
                Hide = "是";
                return;
            }

            dynamic o = JObject.Parse(usefulData);
            BattleCount = Convert.ToInt32( o.pvp.battles );
            Wins = Convert.ToInt32( o.pvp.wins );
            Damage = Convert.ToInt32( o.pvp.damage_dealt );
            Exp = Convert.ToInt32( o.pvp.xp );
        }

        public int requestSortIndex()
        {
            return SortIndex;
        }

        public string Hide { get; private set; }
        public string Name { get; private set; }
        private string PlayerId { get; set; }
        public string ShipName { get; private set; }
        private string ShipId { get; set; }
        private int Wins { get; set; }
        public int BattleCount { get; private set; }
        private int Damage { get; set; }
        private int Exp { get; set; }
        private int SortIndex { get; set; }
        public int WinRate { get { return Wins * 100 / BattleCount; } }
        public int ExpAverage { get { return Exp / BattleCount; } }
        public int DmgAverage { get { return Damage / BattleCount; } }
        private int Team { get; set; }
    }

    public class Vehicle
    {
        public object shipId { get; set; }
        public int relation { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class TempLeaderBoardRoot
    {
        public string clientVersionFromXml { get; set; }
        public int gameMode { get; set; }
        public string clientVersionFromExe { get; set; }
        public string mapDisplayName { get; set; }
        public int mapId { get; set; }
        public string matchGroup { get; set; }
        public int duration { get; set; }
        public string gameLogic { get; set; }
        public string name { get; set; }
        public string scenario { get; set; }
        public int playerID { get; set; }
        public List<Vehicle> vehicles { get; set; }
        public int playersPerTeam { get; set; }
        public string dateTime { get; set; }
        public string mapName { get; set; }
        public string playerName { get; set; }
        public int scenarioConfigId { get; set; }
        public int teamsCount { get; set; }
        public string logic { get; set; }
        public string playerVehicle { get; set; }
    }
}
