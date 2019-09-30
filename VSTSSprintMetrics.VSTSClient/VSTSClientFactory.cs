using System;
using System.Collections.Generic;
using System.Text;
using VSTSSprintMetrics.VSTSClient.Interfaces;

namespace VSTSSprintMetrics.VSTSClient
{
    public class VSTSClientFactory : IVSTSClientFactory
    {
        public IVSTSClient CreateClient()
        {
            return new VSTSClient();
        }
    }
}
