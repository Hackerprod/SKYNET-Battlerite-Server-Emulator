using System;

namespace SKYNET
{
    public class ServerAttribute : Attribute
    {
        public ServerAttribute(string configurationName, RequestType requestType, bool fallback = true)
        {
            this.ConfigurationName = configurationName;
            this.RequestType = requestType;
            this.Fallback = fallback;
        }

        public string ConfigurationName;

        public RequestType RequestType;

        public bool Fallback;
    }
}