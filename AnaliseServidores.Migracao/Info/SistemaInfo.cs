using AnaliseServidores.Migracao.Interfaces;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;

namespace AnaliseServidores.Migracao.Info
{
    internal class SistemaInfo : ISistemaInfo
    {
        private readonly ILog _log;

        public SistemaInfo(ILog log)
        {
            _log = log;
        }

        public void buscaNomeSistema()
        {
            try
            {
                String hostName = Dns.GetHostName();
                _log.Info("HostName do Servidor: " + hostName);
            }
            catch (Exception ex)
            {
                _log.Error("Erro na busca pelo Hostname! Erro: " + ex);
            }
        }

        public void buscaInfoOS()
        {
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
                foreach (ManagementObject managementObject in mos.Get())
                {
                    if (managementObject["Caption"] != null)
                    {
                        string nomeOS = managementObject["Caption"].ToString();
                        _log.Info("Sistema Operacional: " + nomeOS);
                    }
                    if (managementObject["OSArchitecture"] != null)
                    {
                        string arqOS = managementObject["OSArchitecture"].ToString();
                        _log.Info("Arquitetura do Sistema Operacional: " + arqOS);
                    }
                    if (managementObject["CSDVersion"] != null)
                    {
                        string packOS = managementObject["CSDVersion"].ToString();
                        _log.Info("Service Pack do Sistema Operacional: " + packOS);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Erro na busca por informações do Sistema Operacional! Erro: " + ex);
            }
        }

        public void buscaDataInstalacaoOS()
        {
            try
            {
                string nomeComputador = "";

                RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, nomeComputador);
                key = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false);

                if (key != null)
                {
                    DateTime dataInicial = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                    Int64 regVal = Convert.ToInt64(key.GetValue("InstallDate").ToString());

                    DateTime dataInstalacao = dataInicial.AddSeconds(regVal);

                    _log.Info("Data de instalação do Sistema Operacional: " + dataInstalacao);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Erro na busca pela data de instalação do Sistema Operacional! Erro: " + ex);
            }
        }

    }
}
