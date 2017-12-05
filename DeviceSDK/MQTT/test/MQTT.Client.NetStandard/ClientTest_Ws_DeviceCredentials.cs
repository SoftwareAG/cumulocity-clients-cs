using Moq;
using Cumulocity.MQTT.Utils;
using MQTTnet.Core.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.MQTT.Test
{
    [TestFixture]
    internal class ClientTest_Ws_DeviceCredentials
    {
        private Mock<IIniParser> ini;
        private Mock<IMqttClient> mqttClient;
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
            clientId = "4927468bdd4b4171a23e31476ff82674";

            ini = new Mock<IIniParser>();
            ini.Setup(i => i.GetSetting("Device", "Server")).Returns("ws://piotr.staging.c8y.io/mqtt");
            ini.Setup(i => i.GetSetting("Device", "UserName")).Returns(@"management/devicebootstrap");
            ini.Setup(i => i.GetSetting("Device", "Password")).Returns(@"Fhdt1bb1f");
            ini.Setup(i => i.GetSetting("Device", "Port")).Returns("80");
            ini.Setup(i => i.GetSetting("Device", "ConnectionType")).Returns("WS");
            ini.Setup(i => i.GetSetting("Device", "ClientId")).Returns(clientId);

            cl = new Client(ini.Object);

            var res1 = Task.Run(() => cl.ConnectAsync()).Result;
        }

        [Test]
        public void ClientTest_WsConnection_UpdateDataAsync_Operation()
        {
            //SubscribeAsync
            //var res1 = Task.Run(() => cl.SubscribeAsync()).Result;
            var res2 = Task.Run(() => cl.RequestDeviceCredentials((e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
    }
    }
