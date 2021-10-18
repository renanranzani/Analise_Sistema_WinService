using AnaliseServidores.Migracao.Interfaces;
using AnaliseServidores.Migracao.Kernel;
using AnaliseServidores.Migracao.Utils;
using log4net;
using System;
using System.ServiceProcess;

namespace AnaliseServidores.Migracao
{
    public static class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        public class Service : ServiceBase
        {

            public Service()
            {
                ServiceName = "Agente de Monitoração Migra Servidores";
            }

            protected override void OnStart(string[] args) => Program.Start(args);

            protected override void OnStop() => Program.Stop();
        }

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                // Running as SERVICE
                using (var service = new Service())
                    ServiceBase.Run(service);
            }
            else
            {
                // Running as CONSOLE
                try
                {
                    DisableConsoleQuickEdit.Go();

                    Start(args);

                    while (true)
                    {
                        Console.TreatControlCAsInput = true;
                        Console.ReadKey();
                    };
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Stop();
                }
            }
        }

        private static void Start(string[] args)
        {
            try
            {
                IocKernel.Initialize(new IocConfiguration());

                IocKernel.Get<IExecutarAppService>().Start();
            }
            catch (Exception ex)
            {
                _log.Info("Aplicação finalizada!" + ex);
            }

        }

        private static void Stop()
        {
            Environment.Exit(-1);
        }
    }
}