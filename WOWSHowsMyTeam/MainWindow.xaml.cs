using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WOWSHowsMyTeam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum ProgramStatus
        {
            NoGameDetected,
            Standby,
            Loading,
            Loaded
        }

        #region private property field
        private ProgramStatus Status;
        private string arenaInfoFile = "tempArenaInfo.json";
        DispatcherTimer Timer = new DispatcherTimer();

        #endregion private property field

        public MainWindow()
        {
            InitializeComponent();
            PersonalInitialize();
        }

        private void PersonalInitialize()
        {
            UpdateProgramStatus(ProgramStatus.Standby);
            Timer.Interval = new TimeSpan(0, 0, 5);
            Timer.Tick += Timer_Tick;
            Timer.Start();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ClientStarted())
            {
                arenaInfoFile = getArenaInfoPath();
                if (File.Exists(arenaInfoFile))
                {
                    CheckData();
                }
            }
            else
            {
                UpdateProgramStatus(ProgramStatus.NoGameDetected);
            }

        }

        private string getArenaInfoPath()
        {
            Process gameProcess = Process.GetProcessesByName("WorldOfWarships").FirstOrDefault();
            string exeFilePath = gameProcess.MainModule.FileName;
            return exeFilePath.Substring(0, exeFilePath.LastIndexOf('\\')) + @"\replays\tempArenaInfo.json";
        }

        private bool ClientStarted()
        {
            return Process.GetProcessesByName("WorldOfWarships").Length != 0;
        }

        private void UpdateProgramStatus(ProgramStatus newStatus)
        {
            Status = newStatus;
            textBox.Text = Status.ToString();
        }

        private async void CheckData()
        {
            if (Status == ProgramStatus.Loading ||
                Status == ProgramStatus.Standby && !File.Exists(arenaInfoFile) ||
                Status == ProgramStatus.Loaded && File.Exists(arenaInfoFile))
            {
                return;
            }


            if (Status == ProgramStatus.Loaded && !File.Exists(arenaInfoFile))
            {
                UpdateProgramStatus(ProgramStatus.Standby);
                return;
            }

            if (Status == ProgramStatus.Standby && File.Exists(arenaInfoFile))
            {
                UpdateProgramStatus(ProgramStatus.Loading);
                await UpdateData();
            }
        }

        private async Task UpdateData()
        {
            if (File.Exists(arenaInfoFile))
            {
                string content = File.ReadAllText(arenaInfoFile);
                TempLeaderBoardRoot root = JsonConvert.DeserializeObject<TempLeaderBoardRoot>(content);
                Task<PlayerDatas> ownTeamUpdate = new Task<PlayerDatas>(() => downloadData(root, true));
                Task<PlayerDatas> enemyTeamUpdate = new Task<PlayerDatas>(() => downloadData(root, false));
                Task[] alltask = { ownTeamUpdate, enemyTeamUpdate };
                ownTeamUpdate.Start();
                enemyTeamUpdate.Start();
                await Task.WhenAll(alltask);
                updateView(ownTeamUpdate.Result, enemyTeamUpdate.Result);
                UpdateProgramStatus(ProgramStatus.Loaded);
            }
            else
            {
                UpdateProgramStatus(ProgramStatus.Standby);
            }
        }

        private PlayerDatas downloadData(TempLeaderBoardRoot root, bool teammate)
        {
            return new PlayerDatas(root.vehicles, teammate);
        }

        private void updateView(PlayerDatas ownteam, PlayerDatas enemyteam)
        {
            dataGrid.ItemsSource = ownteam.OrderByDescending(t => t.requestSortIndex());
            dataGridEnemy.ItemsSource = enemyteam.OrderByDescending(t => t.requestSortIndex()).ToList();
        }
    }

    public class PlayerDatas : List<PlayerData>
    {
        public PlayerDatas(List<Vehicle> Vehicles, bool ownTeam)
        {
            foreach (Vehicle v in Vehicles)
            {
                if (v.relation < 2 && ownTeam
                    || v.relation == 2 && !ownTeam)
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
            if (!String.IsNullOrEmpty(PlayerId))
            {
                fillShipMastery();
            }
        }

        private string queryId()
        {
            string response = HttpManager.GetJsonPlayerIDQuery(Name);
            dynamic o = JObject.Parse(response);
            if (o.data.First == null)
            {
                return "";
            }
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
                Hide = "Y";
                return;
            }

            dynamic o = JObject.Parse(usefulData);
            BattleCount = Convert.ToInt32(o.pvp.battles);
            Wins = Convert.ToInt32(o.pvp.wins);
            Damage = Convert.ToInt32(o.pvp.damage_dealt);
            Exp = Convert.ToInt32(o.pvp.xp);
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
        public int WinRate { get { if (BattleCount != 0) return Wins * 100 / BattleCount; else return 0; } }
        public int ExpAverage { get { if (BattleCount != 0) return Exp / BattleCount; else return 0; } }
        public int DmgAverage { get { if (BattleCount != 0) return Damage / BattleCount; else return 0; } }
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
