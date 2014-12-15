using System;
using System.Diagnostics;
using System.Linq;
using System.Net;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

using Orleans.Host;

namespace Sample
{
    public class WorkerRole : RoleEntryPoint
    {
        OrleansAzureSilo silo;

        public override bool OnStart()
        {
            SetupWorker();
            return SetupSilo();
        }

        public override void Run()
        {
            RunSilo();
        }

        public override void OnStop()
        {
            StopSilo();
            StopWorker();
        }

        static void SetupWorker()
        {
            ServicePointManager.DefaultConnectionLimit = 100;
            RoleEnvironment.Changing += RoleEnvironmentChanging;
            
            SetupEnvironmentChangeHandlers();
            SetupDiagnostics();
        }

        void StopWorker()
        {
            RoleEnvironment.Changing -= RoleEnvironmentChanging;
            base.OnStop();
        }

        static void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            var i = 1;
            foreach (var c in e.Changes)
                Trace.WriteLine(string.Format("RoleEnvironmentChanging: #{0} Type={1} Change={2}", i++, c.GetType().FullName, c));

            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
                e.Cancel = true;
        }

        static void SetupEnvironmentChangeHandlers()
        {
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));

                RoleEnvironment.Changed += (sender, e) =>
                {
                    var configSettingsChanged = e.Changes
                        .OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any(change => (change.ConfigurationSettingName == configName));

                    if (!configSettingsChanged) 
                        return;

                    if (!configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)))
                        RoleEnvironment.RequestRecycle();
                };
            });
        }

        static bool CollectPerfCounters = false;
        static bool CollectWindowsEventLogs = false;
        static bool FullCrashDumps = false;

        static void SetupDiagnostics()
        {
            var cfg = DiagnosticMonitor.GetDefaultInitialConfiguration();

            if (CollectPerfCounters)
            {
                AddPerformanceCounter(cfg, @"\Processor(_Total)\% Processor Time", TimeSpan.FromSeconds(5));
                AddPerformanceCounter(cfg, @"\Memory\Available Mbytes", TimeSpan.FromSeconds(5));
            }

            if (CollectWindowsEventLogs)
            {
                AddEventSource(cfg, "System!*");
                AddEventSource(cfg, "Application!*");
            }

            cfg.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Information;
            cfg.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = TimeSpan.FromMinutes(5);

            CrashDumps.EnableCollection(FullCrashDumps);
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", cfg);
        }

        static void AddPerformanceCounter(DiagnosticMonitorConfiguration cfg, string counter, TimeSpan frequency)
        {
            cfg.PerformanceCounters.DataSources.Add(new PerformanceCounterConfiguration
            {
                CounterSpecifier = counter,
                SampleRate = frequency
            });
        }

        static void AddEventSource(DiagnosticMonitorConfiguration cfg, string source)
        {
            cfg.WindowsEventLog.DataSources.Add(source);
        }

        bool SetupSilo()
        {
            var siloEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["OrleansSiloEndpoint"];
            HubGateway.LocalEndPoint = siloEndpoint.IPEndpoint;

            silo = new OrleansAzureSilo();
            return silo.Start(RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance);
        }

        void StopSilo()
        {
            silo.Stop();
        }

        void RunSilo()
        {
            silo.Run();
        }
    }
}
