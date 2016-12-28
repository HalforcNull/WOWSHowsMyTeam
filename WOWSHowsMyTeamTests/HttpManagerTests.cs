using Microsoft.VisualStudio.TestTools.UnitTesting;
using WOWSHowsMyTeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WOWSHowsMyTeam.Tests
{
    [TestClass()]
    public class HttpManagerTests
    {
        [TestMethod()]
        public void GetJsonPlayerIDQueryTest()
        {
            string returnedJson = HttpManager.GetJsonPlayerIDQuery("HalforcNull");
            Assert.AreNotEqual("", returnedJson, "TestFail");
        }

        [TestMethod()]
        public void GetJsonPlayerIDQueryTest1()
        {
            string returnedJson = HttpManager.GetJsonPlayerIDQuery("HalforcNull");
            dynamic o = JObject.Parse(returnedJson);
            string id = o.data.First.account_id.ToString();
            Assert.AreEqual("1012710406", id, "TestFail");
        }

        [TestMethod()]
        public void GetJsonPlayerShipMasteryTest()
        {
            string returnedJson = HttpManager.GetJsonPlayerShipMastery("1012710406", "4182685136");
            Assert.AreNotEqual("", returnedJson, "TestFail");
        }

        [TestMethod()]
        public void GetJsonPlayerShipMasteryTest1()
        {
            string returnedJson = HttpManager.GetJsonPlayerShipMastery("1012710406", "4182685136");
            int head = returnedJson.IndexOf("[{");
            int end = returnedJson.LastIndexOf("}]");
            string usefulData = returnedJson.Substring(head + 1, end - head);
            dynamic o = JObject.Parse(usefulData);
            string id = o.data.First.account_id.ToString();
            Assert.AreEqual("1012710406", id, "TestFail");
        }

        [TestMethod()]
        public void GetJsonShipTest()
        {
            string returnedJson = HttpManager.GetJsonShip( "4182685136");
            int head = returnedJson.IndexOf("\"name\"");
            string usefulDataP1 = returnedJson.Substring(head);
            int end = usefulDataP1.IndexOf("\",");
            string usefulDataP2 = usefulDataP1.Substring(0, end );
            string name = usefulDataP2.Substring(usefulDataP2.LastIndexOf("\"")+1);


            Assert.AreEqual("Shchors", name, "TestFail");
        }

        [TestMethod()]
        public void GetJsonShipTest2()
        {
            string id = "4182685136";
            string returnedJson = HttpManager.GetJsonShip(id);
            int head = returnedJson.IndexOf(id);
            string usefulData = returnedJson.Substring(head + id.Length + 2);
            usefulData = usefulData.Substring(0, usefulData.Length - 2);

            dynamic o = JObject.Parse(usefulData);

            Assert.AreEqual("Shchors", o.name.ToString(), "TestFail");
        }
    }
}