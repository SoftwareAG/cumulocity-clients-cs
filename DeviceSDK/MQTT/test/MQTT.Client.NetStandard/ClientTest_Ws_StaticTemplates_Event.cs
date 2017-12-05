using Moq;
using Cumulocity.MQTT.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.MQTT.Test
{
    [TestFixture]
    internal  class ClientTest_Ws_StaticTemplates_Event
    {
        private Mock<IIniParser> ini;
        private Client cl;
        private string clientId;

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [SetUp]
        public void SetUp()
        {
            //clientId = Guid.NewGuid().ToString("N");
            clientId = "4927468bdd4b4171a23e31476ff82674";

            ini = new Mock<IIniParser>();
            ini.Setup(i => i.GetSetting("Device", "Server")).Returns("ws://piotr.staging.c8y.io/mqtt");
            ini.Setup(i => i.GetSetting("Device", "UserName")).Returns(@"piotr/pnow");
            ini.Setup(i => i.GetSetting("Device", "Password")).Returns(@"test1234");
            ini.Setup(i => i.GetSetting("Device", "Port")).Returns("80");
            ini.Setup(i => i.GetSetting("Device", "ConnectionType")).Returns("WS");
            ini.Setup(i => i.GetSetting("Device", "ClientId")).Returns(clientId);

            cl = new Client(ini.Object);

            var res1 = Task.Run(() => cl.ConnectAsync()).Result;
            Assert.IsTrue(cl.IsConnected);
        }

        //Create basic event (400)
        [Test]
        public void ClientTest_WsConnection_CreateBasicEvent()
        {
            var res2 = Task.Run(() => cl.CreateBasicEventAsync("c8y_MyEvent", "Something was triggered", string.Empty, (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        //Create location update event (401)
       
        [Test]
        public void ClientTest_WsConnection_CreateLocationUpdateEvent()
        {
            var res2 = Task.Run(() => cl.CreateLocationUpdateEventAsync(
                 "52.209538",
                 "16.831992",
                 "76",
                 "134",
                 string.Empty,
                 (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        //Create location update event with device update (402)
        [Test]
        public void ClientTest_WsConnection_CreateLocationUpdateEventWithDeviceUpdate()
        {
            var res2 = Task.Run(() => cl.CreateLocationUpdateEventWithDeviceUpdateAsync(
                                 "52.209538",
                                 "16.831992",
                                 "76",
                                 "134",
                                 string.Empty,
                                 (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
    }
}
