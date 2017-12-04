using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cumulocity.MQTT.Exceptions
{
    public class CommunicationException : Exception
    {
        protected CommunicationException()
        {
        }

        public CommunicationException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        public CommunicationException(string message)
            : base(message)
        {
        }

        public CommunicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
