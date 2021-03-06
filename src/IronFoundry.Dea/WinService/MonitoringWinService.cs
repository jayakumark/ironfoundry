﻿namespace IronFoundry.Dea.WinService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Services;
    using IronFoundry.Dea.WcfInfrastructure;
    using IronFoundry.Dea.Configuration;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.WinService;

    public class MonitoringWinService : WcfWinService
    {
        private readonly IDeaConfig config;
        private readonly IFirewallService firewallService;

        public MonitoringWinService(ILog log, IDeaConfig config, IFirewallService firewallService) : base(log, true)
        {
            this.config = config;
            this.firewallService = firewallService;

            Uri baseAddress = config.MonitoringServiceUri;

            var webHttpBinding = new WebHttpBinding();
            webHttpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            webHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            var serviceHost =  new IocServiceHost(typeof(MonitoringService), baseAddress);               
            base.serviceHost = serviceHost;

            ServiceEndpoint endpoint = serviceHost.AddServiceEndpoint(typeof(IMonitoringService), webHttpBinding, baseAddress);
            endpoint.Behaviors.Add(new WebHttpBehavior());
            serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUserNamePasswordValidator(config.MonitoringCredentials);
            serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 
        }

        public override ushort StartIndex
        {
            get { return 2; }
        }

        public override StartServiceResult StartService(IntPtr ignored, string[] args)
        {
            firewallService.Open(config.MonitoringServicePort, Resources.MonitoringWinService_ServiceName);
            return base.StartService(ignored, args);
        }

        public override void StopService()
        {
            firewallService.Close(Resources.MonitoringWinService_ServiceName);
            base.StopService();
        }
    }
}