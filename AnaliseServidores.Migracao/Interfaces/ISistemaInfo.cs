using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnaliseServidores.Migracao.Interfaces
{
    public interface ISistemaInfo
    {
        void buscaNomeSistema();
        void buscaInfoOS();
        void buscaDataInstalacaoOS();
    }
}
