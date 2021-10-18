using AnaliseServidores.Migracao.Info;
using AnaliseServidores.Migracao.Interfaces;
using AnaliseServidores.Migracao.Service;
using log4net;
using Ninject.Modules;

namespace AnaliseServidores.Migracao.Kernel
{
    public class IocConfiguration : NinjectModule
    {
        public override void Load()
        {
            Bind<ILog>().ToMethod(c => LogManager.GetLogger(c.Request.Target.Member.ReflectedType));

            Bind<ISistemaInfo>().To<SistemaInfo>().InTransientScope();
            Bind<IExecutarAppService>().To<ExecutarAppService>().InTransientScope();
        }
    }
}
