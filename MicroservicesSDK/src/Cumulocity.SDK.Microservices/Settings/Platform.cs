using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.Settings
{
    public class Platform
    {
        public Platform(PlatformSettings config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            MICROSERIVCE_ISOLATION = config.C8Y_MICROSERIVCE_ISOLATION;
            BASEURL = config.C8Y_BASEURL;
            BASEURL_MQTT = config.C8Y_BASEURL_MQTT;
            TENANT = config.C8Y_TENANT;
            PASSWORD = config.C8Y_PASSWORD;
            USERNAME = config.C8Y_USER;
            SERVER_PORT = config.SERVER_PORT;
            BOOTSTRAP_TENANT = config.C8Y_BOOTSTRAP_TENANT;
            BOOTSTRAP_USER = config.C8Y_BOOTSTRAP_USER;
            BOOTSTRAP_PASSWORD = config.C8Y_BOOTSTRAP_PASSWORD;
        }

        public string MICROSERIVCE_ISOLATION { get; protected set; }
        public string BASEURL { get; protected set; }
        public string BASEURL_MQTT { get; protected set; }
        public string TENANT { get; protected set; }
        public string PASSWORD { get; protected set; }
        public string USERNAME { get; protected set; }
        public string SERVER_PORT { get; protected set; }
        public string BOOTSTRAP_TENANT { get; protected set; }
        public string BOOTSTRAP_USER { get; protected set; }
        public string BOOTSTRAP_PASSWORD { get; protected set; }
    }

    public class PlatformSettings
    {
        public string C8Y_MICROSERIVCE_ISOLATION { get; set; }
        public string C8Y_BASEURL { get; set; }
        public string C8Y_BASEURL_MQTT { get; set; }
        public string C8Y_TENANT { get; set; }
        public string C8Y_PASSWORD { get; set; }
        public string C8Y_USER { get; set; }
        public string SERVER_PORT { get; set; }
        public string C8Y_BOOTSTRAP_TENANT { get; set; }
        public string C8Y_BOOTSTRAP_USER { get; set; }
        public string C8Y_BOOTSTRAP_PASSWORD { get; set; }
    }
}
