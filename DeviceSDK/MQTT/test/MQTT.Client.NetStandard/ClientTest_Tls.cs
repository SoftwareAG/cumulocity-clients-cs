using Moq;
using Cumulocity.MQTT.Utils;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Cumulocity.MQTT.Test
{
    [TestFixture]
    public class ClientTest_Tls
    {
        private Mock<IIniParser> ini;
        private Client cl;

        [SetUp]
        public void SetUp()
        {
            ini = new Mock<IIniParser>();
            ini.Setup(i => i.GetSetting("Device", "Server")).Returns("piotr.staging.c8y.io");
            ini.Setup(i => i.GetSetting("Device", "UserName")).Returns(@"piotr/pnow");
            ini.Setup(i => i.GetSetting("Device", "Password")).Returns(@"test1234");
            ini.Setup(i => i.GetSetting("Device", "Port")).Returns("8883");
            ini.Setup(i => i.GetSetting("Device", "ConnectionType")).Returns("TLS");
            ini.Setup(i => i.GetSetting("Device", "ClientId")).Returns("5b09e8a1c29948999d2e30430858c0ae");

            cl = new Client(ini.Object);
        }

        [Test]
        public void ClientTest_TlsConnection_Connect()
        {
            var res = Task.Run(() => cl.ConnectAsync()).Result;

            Assert.IsTrue(cl.IsConnected);
        }
    }
}