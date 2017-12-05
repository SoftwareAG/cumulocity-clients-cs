using Moq;
using Cumulocity.MQTT.Utils;
using MQTTnet.Core;
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
    internal class ClientTest_Ws_StaticTemplates_OperationsEvents
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
            //clientId = Guid.NewGuid().ToString("N");
            clientId = "4927468bdd4b4171a23e31476ff82674";

            ini = new Mock<IIniParser>();
            mqttClient = new Mock<IMqttClient>();

            ini.Setup(i => i.GetSetting("Device", "Server")).Returns("ws://piotr.staging.c8y.io/mqtt");
            ini.Setup(i => i.GetSetting("Device", "UserName")).Returns(@"piotr/pnow");
            ini.Setup(i => i.GetSetting("Device", "Password")).Returns(@"test1234");
            ini.Setup(i => i.GetSetting("Device", "Port")).Returns("80");
            ini.Setup(i => i.GetSetting("Device", "ConnectionType")).Returns("WS");
            ini.Setup(i => i.GetSetting("Device", "ClientId")).Returns(clientId);

            cl = new Client(ini.Object, mqttClient.Object);

        }

        //Restart (510)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_Restart()
        {
            cl.RestartEvt += (s, e) => {
                Assert.AreEqual(clientId,s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("510,{0}", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }

        //Command (511)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_Command()
        {
            cl.CommandEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("511,{0},execute this", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Configuration (513)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_Configuration()
        {
            cl.ConfigurationEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("513,{0},\"val1 = 1\nval2 = 2\"", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Firmware (515)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_Firmware()
        {
            cl.FirmwareEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("515,{0},myFimrware,1.0,http://www.my.url", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Software list (516)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_SoftwareList()
        {
            cl.SoftwareListEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("516,{0},softwareA,1.0,url1,softwareB,2.0,url2", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Measurement request operation (517)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_MeasurementRequestOperation()
        {
            cl.MeasurementRequestOperationEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("517,{0},LOGA", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Relay (518)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_Relay()
        {
            cl.RelayEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("518,{0},OPEN", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //RelayArray (519)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_RelayArray()
        {
            cl.RelayArrayEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("519,{0},OPEN,CLOSE,CLOSE,OPEN", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Upload configuration file (520)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_UploadConfigurationFile()
        {
            cl.UploadConfigurationFileEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("520,{0}", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Download configuration file (521)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_DownloadConfigurationFile()
        {
            cl.DownloadConfigurationFileEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("521,{0},http://www.my.url", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Logfile request (522)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_LogfileRequest()
        {
            cl.LogfileRequestEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("522,{0},logfileA,2013-06-22T17:03:14.000+02:00,2013-06-22T18:03:14.000+02:00,ERROR,1000", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
        //Communication mode (523)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_CommunicationMode()
        {
            cl.CommunicationModeEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("523,{0},SMS", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }

        //Get children of device (106)
        [Test, MaxTime(1000)]
        public void ClientTest_WsConnection_ChildrenOfDevice()
        {
            cl.ChildrenOfDeviceEvt += (s, e) => {
                Assert.AreEqual(clientId, s.ToString());
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/ds", Encoding.UTF8.GetBytes(String.Format("106,child1,child2,child3", clientId)), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }

    }
}
