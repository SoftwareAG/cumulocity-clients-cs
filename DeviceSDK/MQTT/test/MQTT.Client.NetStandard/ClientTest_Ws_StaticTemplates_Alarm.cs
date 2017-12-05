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
    internal class ClientTest_Ws_StaticTemplates_Alarm
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

        //Create CRITICAL alarm (301)
        [Test]
        public void ClientTest_WsConnection_CreateCriticalAlarm()
        {
            var res2 = Task.Run(() => cl.CreateCriticalAlarmAsync("c8y_TemperatureAlarm", "Alarm of type c8y_TemperatureAlarm raised", string.Empty, (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        //Create MAJOR alarm (302)
        [Test]
        public void ClientTest_WsConnection_CreateMajorAlarm()
        {
            var res2 = Task.Run(() => cl.CreateMajorAlarmAsync("c8y_BatteryAlarm", " Major Alarm of type c8y_BatteryAlarm raised", string.Empty, (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        //Create MINOR alarm (303)
        [Test]
        public void ClientTest_WsConnection_CreateMinorAlarm()
        {
            var res2 = Task.Run(() => cl.CreateMinorAlarmAsync("c8y_WaterAlarm", "Alarm of type c8y_WaterAlarm raised", string.Empty, (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        //Create WARNING alarm (304)
        [Test]
        public void ClientTest_WsConnection_CreateWarningAlarm()
        {
            var res2 = Task.Run(() => cl.CreateWarningAlarmAsync("c8y_AirPressureAlarm", "Warning of type c8y_AirPressureAlarm raised", string.Empty, (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        //Update severity of existing alarm (305)
        [Test]
        public void ClientTest_WsConnection_UpdateSeverityOfExistingAlarm()
        {
            var res2 = Task.Run(() => cl.UpdateSeverityOfExistingAlarmAsync("c8y_AirPressureAlarm", "CRITICAL", (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
        //Clear existing alarm (306)
        [Test]
        public void ClientTest_WsConnection_ClearExistingAlarm()
        {
            var res2 = Task.Run(() => cl.ClearExistingAlarmAsync("c8y_TemperatureAlarm", (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
    }
}
