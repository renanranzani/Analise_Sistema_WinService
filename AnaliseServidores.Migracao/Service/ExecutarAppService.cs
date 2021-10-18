using AnaliseServidores.Migracao.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnaliseServidores.Migracao.Service
{
    public class ExecutarAppService : IExecutarAppService
    {
        private readonly ISistemaInfo _sistemaInfo;
        private readonly ILog _log;

        public ExecutarAppService(ISistemaInfo sistemaInfo, ILog log)
        {
            _sistemaInfo = sistemaInfo;
            _log = log;
        }

        public void Start()
        {
            try
            {
                _log.Info("Executando a aplicação...");

                _sistemaInfo.buscaNomeSistema();
                _sistemaInfo.buscaInfoOS();
                _sistemaInfo.buscaDataInstalacaoOS();

                _log.Info("Aplicação finalizada.");
            }
            catch (Exception ex)
            {
                _log.Error("Erro ao executar aplicação: " + ex);
            }
        }
    }
}
