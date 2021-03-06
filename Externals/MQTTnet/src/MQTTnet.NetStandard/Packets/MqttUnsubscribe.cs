using System.Collections.Generic;

namespace MQTTnet.Packets
{
    public sealed class MqttUnsubscribePacket : MqttBasePacket, IMqttPacketWithIdentifier
    {
        public ushort PacketIdentifier { get; set; }

        public IList<string> TopicFilters { get; set; } = new List<string>();

        public override string ToString()
        {
            var topicFiltersText = string.Join(",", TopicFilters);
            return "Subscribe: [PacketIdentifier=" + PacketIdentifier + "] [TopicFilters=" + topicFiltersText + "]";
        }
    }
}
