using System;
using MQTTnet.Core.Adapter;
using MQTTnet.Core.Client;
using MQTTnet.Core.Serializer;
using MQTTnet.Implementations;
using MQTTnet.Core.Channel;

namespace MQTTnet
{
    public class MqttClientFactory
    {
        public IMqttClient CreateMqttClient(MqttClientOptions options)
        {
            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }

            return new MqttClient(options, new MqttChannelCommunicationAdapter(GetMqttCommunicationChannel(options), new MqttPacketSerializer()));
        }

        private static IMqttCommunicationChannel GetMqttCommunicationChannel(MqttClientOptions options)
        {
            switch (options.ConnectionType)
            {
                case ConnectionTypes.TCP:
                    return new MqttTcpChannel();
                case ConnectionTypes.TLS:
                    return new MqttTcpChannel();
                case ConnectionTypes.WS:
                    return new MqttWebSocketsChannel();
                case ConnectionTypes.WSS:
                    return new MqttWebSocketsChannel(true);

                default:
                    return null;
            }
        }
    }
}
