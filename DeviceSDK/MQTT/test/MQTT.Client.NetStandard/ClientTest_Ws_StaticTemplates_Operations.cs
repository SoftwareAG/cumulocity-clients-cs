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
    internal class ClientTest_Ws_StaticTemplates_Operations
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
        public void ClientTest_WsConnection_GetPendingOperations()
        {
            //Will trigger the sending of all PENDING operations for the agent.
            var res2 = Task.Run(() => cl.GetPendingOperationsAsync((e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_GetExecutingOperations()
        {
            //Will set the oldest EXECUTING operation with given fragment to FAILED
            var res2 = Task.Run(() => cl.SetExecutingOperationsAsync("c8y_Restart", (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_SetOperationToFailed()
        {
            //Will set the oldest EXECUTING operation with given fragment to SUCCESSFUL.
            //It enables the device to send additional parameters that trigger additional steps based on the type of operation send as fragment (see section Updating operations).
            var res2 = Task.Run(() => cl.SetOperationToFailedAsync("c8y_Restart", "Could not restart", (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }

        [Test]
        public void ClientTest_WsConnection_SetOperationToSuccessful()
        {
            //
            var res2 = Task.Run(() => cl.SetOperationToSuccessfulAsync("c8y_Restart", string.Empty, (e) => { return Task.FromResult(false); })).Result;
            Assert.IsTrue(res2);
        }
    }
}
