using Moq;
using Cumulocity.MQTT.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.MQTT.Test
{
    [TestFixture]
    public class ClientTest_Ws
    {
        private Mock<IIniParser> ini;
        private Client cl;
        
        [SetUp]
        public void SetUp()
        {
            ini = new Mock<IIniParser>();
            ini.Setup(i => i.GetSetting("Device", "Server")).Returns("ws://piotr.staging.c8y.io/mqtt");
            ini.Setup(i => i.GetSetting("Device", "UserName")).Returns(@"piotr/pnow");
            ini.Setup(i => i.GetSetting("Device", "Password")).Returns(@"test1234");
            ini.Setup(i => i.GetSetting("Device", "Port")).Returns("80");
            ini.Setup(i => i.GetSetting("Device", "ConnectionType")).Returns("WS");
            ini.Setup(i => i.GetSetting("Device", "ClientId")).Returns("5b09e8a1c29948999d2e30430858c0ae");

            cl = new Client(ini.Object);
        }

        [Test]
        public void ClientTest_WsConnection_Connect()
        {
            var res = Task.Run(() => cl.ConnectAsync()).Result;

            Assert.IsTrue(cl.IsConnected);
        }
    }
}
