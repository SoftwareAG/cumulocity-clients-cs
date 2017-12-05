using Moq;
using Cumulocity.MQTT.Utils;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cumulocity.MQTT.Model;
using static Cumulocity.MQTT.Client;

namespace Cumulocity.MQTT.Test
{
    [TestFixture]
    internal class ClientTest_Ws_SmartREST_FullMock
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

        [Test, MaxTime(10000)]
        public void ClientTest_WsConnection_CheckTemplateCollectionExists_Exist_OnlyMock()
        {
            cl.IsExistTemplateCollectionEvt += (s, e) =>
            {
                Assert.AreEqual(123456, e.IdCollection);
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/dt", Encoding.UTF8.GetBytes("20,myExistingTemplateCollection,123456"), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }

        [Test, MaxTime(10000)]
        public void ClientTest_WsConnection_CheckTemplateCollectionExists_NotExist_OnlyMock()
        {
            cl.IsExistTemplateCollectionEvt += (s, e) =>
            {
                Assert.AreEqual(false, e.IsExist);
            };

            MqttApplicationMessage applicationMessage = new MqttApplicationMessage("s/dt", Encoding.UTF8.GetBytes("41,myExistingTemplateCollection"), MQTTnet.Core.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
            mqttClient.Raise(e => e.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(applicationMessage));
        }
    }

    [TestFixture]
    internal class ClientTest_Ws_SmartREST
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
            ini.Setup(i => i.GetSetting("Device", "UserName")).Returns(@"piotr/pnow");
            ini.Setup(i => i.GetSetting("Device", "Password")).Returns(@"test1234");
            ini.Setup(i => i.GetSetting("Device", "Port")).Returns("80");
            ini.Setup(i => i.GetSetting("Device", "ConnectionType")).Returns("WS");
            ini.Setup(i => i.GetSetting("Device", "ClientId")).Returns(clientId);

            cl = new Client(ini.Object);

            var res1 = Task.Run(() => cl.ConnectAsync()).Result;
        }

        [Test]
        public void ClientTest_WsConnection_CheckTemplateCollectionExists_Exist_IoTHub()
        {
            var autoEvent = new AutoResetEvent(false);

            cl.IsExistTemplateCollectionEvt += (s, e) =>
            {
                Assert.AreEqual(false, e.IsExist);
                autoEvent.Set();
            };

            Task.Run(() => cl.CheckTemplateCollectionExists("test2", (e) => { return Task.FromResult(false); }));

            autoEvent.WaitOne();
        }

        [Test]
        public void ClientTest_WsConnection_CheckTemplateCollectionExists_NotExist_IoTHub()
        {
            var autoEvent = new AutoResetEvent(false);

            cl.IsExistTemplateCollectionEvt += (s, e) =>
            {
                Assert.AreEqual(false, e.IsExist);
                autoEvent.Set();
            };

            Task.Run(() => cl.CheckTemplateCollectionExists("test2", (e) => { return Task.FromResult(false); }));

            autoEvent.WaitOne();
        }

        [Test]
        public void ClientTest_WsConnection_CheckTemplateCollectionExists_CreateGetInventoryDataAsync()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("GetTemplate4",
                                                                     new List<Request> {
                                                                        new InventoryGetRequest("9999",null, String.Empty, true),
                                                                        new InventoryGetRequest("9998",null, "c8y_Serial", false)
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" }),
                                                                         new Response("8888",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_CheckTemplateCollectionExists_GetInventoryDataAsync()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("GetTemplate4")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("GetTemplate4", "9998", new List<string> { "4927468bdd4b4171a23e31476ff82674" })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_CreatePostDataAsync_MeasurementRequest()
        {
            //(string messageId, bool? response, string type, string time, IList<CustomValue> customValues)
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("PostTemplateMeasurement",
                                                                     new List<Request> {
                                                                        new MeasurementRequest("7777",
                                                                        null,
                                                                        "c8y_CustomMeasurement",
                                                                        String.Empty,
                                                                        new List<CustomValue>{
                                                                            new CustomValue {Path = "c8y_MyMeasurement.M.value",
                                                                                Type = Enums.CustomValueType.NUMBER,
                                                                                Value = String.Empty
                                                                            }
                                                                        },Enums.HttpMethods.POST)
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" }),
                                                                         new Response("8888",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_CreatePostDataAsync_AlarmRequest()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("PostTemplateAlarm",
                                                                     new List<Request> {
                                                                        new AlarmRequest("6666",
                                                                        null,
                                                                        "c8y_CustomAlarm",
                                                                        "CustomAlarm",
                                                                        "ACTIVE",
                                                                        "MAJOR",
                                                                        String.Empty,
                                                                        new List<CustomValue>{
                                                                            new CustomValue {
                                                                                Path = "c8y_CustomAlarm.M.value",
                                                                                Type = Enums.CustomValueType.NUMBER,
                                                                                Value = String.Empty
                                                                            }
                                                                        },Enums.HttpMethods.POST)
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> {
                                                                             "type",
                                                                             "c8y_MQTTDevice",
                                                                             "c8y_Mobile.cellId" }),
                                                                         new Response("8888",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> {
                                                                             "type",
                                                                             "c8y_MQTTDevice",
                                                                             "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_CreatePostDataAsync_EventRequest()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("PostTemplateEvent",
                                                                     new List<Request> {
                                                                        new EventRequest("5555",
                                                                        null,
                                                                        "c8y_CustomEvent",
                                                                        "CustomEvent",
                                                                        String.Empty,
                                                                        new List<CustomValue>{
                                                                            new CustomValue {Path = "c8y_CustomEvent.M.value",
                                                                                Type = Enums.CustomValueType.NUMBER,
                                                                                Value = String.Empty
                                                                            }
                                                                        })
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> {
                                                                             "type",
                                                                             "c8y_MQTTDevice",
                                                                             "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_CreatePostDataAsync_InventoryRequest()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("PostTemplateInventory",
                                                                     new List<Request> {
                                                                        new InventoryRequest("4444",
                                                                        null,
                                                                        "c8y_MySerial",
                                                                        new List<CustomValue>{
                                                                            new CustomValue {Path = "c8y_CustomInventory.M.value",
                                                                                Type = Enums.CustomValueType.NUMBER,
                                                                                Value = String.Empty
                                                                            }
                                                                        },Enums.HttpMethods.POST)
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_AddUpdateDataAsync_InventoryRequest()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("UpdateTemplateInventory",
                                                                     new List<Request> {
                                                                        new InventoryRequest("3333",
                                                                        null,
                                                                        "c8y_MySerial",
                                                                        new List<CustomValue>{
                                                                            new CustomValue {Path = "c8y_CustomInventory.M.value",
                                                                                Type = Enums.CustomValueType.NUMBER,
                                                                                Value = String.Empty
                                                                            }
                                                                        },Enums.HttpMethods.PUT)
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_AddUpdateDataAsync_AlarmRequest()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("UpdateTemplateAlarm",
                                                                     new List<Request> {
                                                                        new AlarmUpdateRequest("2222",
                                                                        null,
                                                                        "c8y_CustomAlarm",
                                                                        new AlarmFragment("status",null),
                                                                        new List<CustomValue>{
                                                                        })
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }


        [Test]
        public void ClientTest_WsConnection_AddUpdateDataAsync_ClearingAlarmRequest()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("UpdateTemplateClearingAlarm ",
                                                                     new List<Request> {
                                                                        new AlarmUpdateRequest("0000",
                                                                        null,
                                                                        "c8y_CustomAlarm",
                                                                        new AlarmFragment("status",Enums.AlarmStatus.CLEARED),
                                                                        new List<CustomValue>{
                                                                           new CustomValue {
                                                                                Path = "c8y_CustomFragment",
                                                                                Type = Enums.CustomValueType.STRING,
                                                                                Value = String.Empty
                                                                            },

                                                                        })
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_AddUpdateDataAsync_OperationRequest()
        {
            var res2 = Task.Run(() => cl.CreateTemplateDataAsync("UpdateTemplateOperation",
                                                                     new List<Request> {
                                                                        new OperationRequest("1111",
                                                                        null,
                                                                        "com_cumulocity_model_WebCamDevice",
                                                                        new OperationFragment(
                                                                            "status",
                                                                             Enums.OperationStatus.SUCCESSFUL),
                                                                          new List<CustomValue>{
                                                                            new CustomValue {
                                                                                Path = "c8y_Fragment.val",
                                                                                Type = Enums.CustomValueType.NUMBER,
                                                                                Value = String.Empty
                                                                            } })
                                                                     },
                                                                     new List<Response> {
                                                                         new Response("8889",
                                                                         String.Empty,
                                                                         "c8y_IsDevice",
                                                                         new List<string> { "type", "c8y_MQTTDevice", "c8y_Mobile.cellId" })
                                                                     })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_PostDataAsync_SendMeasurement()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("PostTemplate")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("PostTemplate", "7777", new List<string> { "", "25" })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_PostDataAsync_SendAlarm()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("PostTemplateAlarm")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("PostTemplateAlarm", "6666", new List<string> { "ACTIVE", "MAJOR", "2017-09-20T09:03:14.000+02:00", "100" })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_PostDataAsync_SendEvent()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("PostTemplateEvent")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("PostTemplateEvent", "5555", new List<string> { "", "100" })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_PostDataAsync_AddToInventory()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("PostTemplateInventory")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("PostTemplateInventory", "4444", new List<string> {  "myImei300", "2"
        })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_UpdateDataAsync_Inventory()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("UpdateTemplateInventory")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("UpdateTemplateInventory", "3333", new List<string> { "myImei300", "2" })).Result;
            Assert.IsTrue(res1);
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_UpdateDataAsync_Alarm()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("UpdateTemplateAlarm")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("UpdateTemplateAlarm", "2222", new List<string> { "ACKNOWLEDGED" })).Result;
            Assert.IsTrue(res2);
        }


        [Test]
        public void ClientTest_WsConnection_UpdateDataAsync_Operation()
        {
            var res1 = Task.Run(() => cl.SubscribeSmartRestAsync("UpdateTemplateOperation")).Result;
            var res2 = Task.Run(() => cl.SendRequestDataAsync("UpdateTemplateOperation", "1111", new List<string> { "\"Take a picture!\"" })).Result;
            Assert.IsTrue(res2);
        }
    }
}