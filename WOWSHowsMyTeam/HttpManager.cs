using System;
using System.Net.Http;

namespace WOWSHowsMyTeam
{
    public static class HttpManager
    {
        public static string GetJsonPlayerIDQuery(string _playerName)
        {
            string url = @"https://api.worldofwarships.com/wows/account/list/";
            string applicationID = @"application_id=b3961d44aef965fc705e67a871cd5bbb";
            string searchCriteria = @"&search=" + _playerName;

            string parm = "?" + applicationID + searchCriteria;

            return GetJsonResponse(url, parm);
        }
        
        private static string GetJsonResponse(string url, string parm)
        {
            HttpClient c = new HttpClient();
            c.BaseAddress = new Uri(url);

            c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = c.GetAsync(parm).Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return "";
            }
        }

        public static string GetJsonShip(string id)
        {
            string url = @"https://api.worldofwarships.com/wows/encyclopedia/ships/";
            string applicationID = @"application_id=b3961d44aef965fc705e67a871cd5bbb";
            string criteria_ship_id = @"&ship_id=" + id;

            string parm = "?" + applicationID + criteria_ship_id;

            return GetJsonResponse(url, parm);
        }

        public static string GetJsonPlayerShipMastery(string playerId, string shipId)
        {
            string url = @"https://api.worldofwarships.com/wows/ships/stats/";
            string applicationID = @"application_id=b3961d44aef965fc705e67a871cd5bbb";
            string criteria_account_id = @"&account_id=" + playerId;
            string criteria_ship_id = @"&ship_id=" + shipId;

            string parm = "?" + applicationID + criteria_account_id + criteria_ship_id;

            return GetJsonResponse(url, parm);
        }
    }
}
