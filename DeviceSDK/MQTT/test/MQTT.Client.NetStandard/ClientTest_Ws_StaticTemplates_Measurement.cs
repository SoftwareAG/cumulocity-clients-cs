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
    internal class ClientTest_Ws_StaticTemplates_Measurement
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
        [Test]
        public void ClientTest_WsConnection_CreateCustomMeasurement()
        {
            var res2 = Task.Run(() => cl.CreateCustomMeasurementAsync("c8y_Temperature", "T","25", string.Empty, string.Empty, (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        [Test]
        public void ClientTest_WsConnection_CreateSignalStrengthMeasurement()
        {
            var res2 = Task.Run(() => cl.CreateSignalStrengthMeasurementAsync("-90", "23", "2017-09-13T14:00:14.000+02:00", (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        [Test]
        public void ClientTest_WsConnection_CreateTemperatureMeasurement()
        {
            var res2 = Task.Run(() => cl.CreateTemperatureMeasurementAsync("25", "2017-09-13T15:01:14.000+02:00", (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_CreateBatteryMeasurement()
        {
            var res2 = Task.Run(() => cl.CreateBatteryMeasurementAsync("95", "2017-09-13T15:01:14.000+02:00", (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }


    }
}
