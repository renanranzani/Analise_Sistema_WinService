using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;

namespace AnaliseServidores.Migracao.Installer
{
    class Program
    {
        #if X86
                const string RootPath = "C:\\Program Files (x86)"; 
        #else
                const string RootPath = "C:\\Program Files";
        #endif

        const string ProgramPath = "AnaliseServidores.Migracao";
        const string ServiceName = "AnaliseServidores.Migracao";
        const string DisplayName = "Agente de Monitoração Migra Servidores";
        const string Description = "Serviço responsável por iniciar a aplicação de análise de migração de servidores.";
        const string ProgramName = "AnaliseServidores.Migracao.exe";

        static List<string> _outputlog = new List<string>();

        static void Main(string[] args)
        {
            string logOutputDirectory = null;

            try
            {
                logOutputDirectory = CreateLogOutputDirectory();

                Write("Verificando se o instalador foi executado com permissão de administrador...");

                if (!IsAdministrator())
                {
                    throw new Exception("O instalador não foi executado com permissão de administrador.");
                }

                string sourceDirName = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location) + "\\publish\\";

                Write($"Verificando se o serviço {ServiceName} já está instalado...");

                if (IsInstalled(ServiceName))
                {
                    Write($"O serviço {ServiceName} já está instalado. Você deseja reinstalar o serviço (S/N)?");

                    string answer = Console.ReadLine();

                    if (!string.IsNullOrEmpty(answer) && answer.ToLower().StartsWith("s"))
                    {
                        Uninstall(ServiceName);
                        InstallAndStart(ServiceName, DisplayName, Description, sourceDirName);

                        Write($"O serviço {ServiceName} foi reinstalado com sucesso.");
                    }
                }
                else
                {
                    InstallAndStart(ServiceName, DisplayName, Description, sourceDirName);
                }
            }
            catch (Exception ex)
            {
                Write(ex.ToString());
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(logOutputDirectory))
                {
                    File.WriteAllLines(logOutputDirectory + "logs.txt", _outputlog);
                }
                Console.ReadKey();
            }
        }

        static void Write(string value)
        {
            string message = DateTime.Now.ToString() + ": " + value;
            Console.WriteLine(message);
            _outputlog.Add(message);
        }

        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static bool IsInstalled(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.Any(x => x.ServiceName == serviceName);
        }

        static void Uninstall(string serviceName)
        {
            string command = "sc stop \"{serviceName}\"";
            ExecuteCommand(command);

            command = $"sc delete \"{ServiceName}\"";
            ExecuteCommand(command);

            command = $"rd /Q /S \"{RootPath}\\{ProgramPath}\"";
            ExecuteCommand(command);
        }

        static void ExecuteCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.StandardInput.WriteLine(command);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            string resultComand = process.StandardOutput.ReadToEnd();
            Write(resultComand);
        }

        static void InstallAndStart(string serviceName, string displayName, 
            string description ,string sourceDirName)
        {
            Write($"\n ------------------ Iniciando instalação do {serviceName} --------- \n");

            string destDirName = CreateFolderStructure(RootPath, ProgramPath);
            DirectoryCopy(sourceDirName, destDirName, copySubDirs: true);

            string command = $"sc create \"{serviceName}\" binPath= \"{destDirName + ProgramName}\"";
            ExecuteCommand(command);

            Write("\n ------------------ Iniciando configuração --------- \n");

            command = $"sc config \"{serviceName}\" start= auto";
            ExecuteCommand(command);

            command = $"sc config \"{serviceName}\" DisplayName= \"{displayName}\"";
            ExecuteCommand(command);

            command = $"sc description \"{serviceName}\" \"{description}\"";
            ExecuteCommand(command);

            command = $"sc start \"{serviceName}\" reset= 86400 actions= restart/60000/restart/60000/restart/60000/restart/60000";
            ExecuteCommand(command);

            Write("\n ------------------ Iniciando serviço --------- \n");

            command = $"sc start \"{serviceName}\"";
            ExecuteCommand(command);

            command = $"sc query \"{serviceName}\"";
            ExecuteCommand(command);

            Write("\n ------------------ Agendando serviço --------- \n");

            command = @"schtasks /create /tn AnaliseMigraServidores /tr AnaliseServidores.Migracao.Installer.exe /sc daily /st 18:00 /du 05:00";
            ExecuteCommand(command);

            Write("\n ------------------ Instalação completa --------- \n");
        
    }


        static string CreateLogOutputDirectory()
        {
            string path = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location) + "\\logInstalacao\\";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        static string CreateFolderStructure(string rootPath, string programPath)
        {
            string destDirName = rootPath + "\\" + programPath;

            if (Directory.Exists(destDirName))
            {
                Write($"O diretório {destDirName} já existe. Removendo...");
                Directory.Delete(destDirName, true);
            }

            Write($"Criando o diretório {destDirName}...");
            Directory.CreateDirectory(destDirName);

            return destDirName + "\\";
        }

        static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            Write($"Copiando arquivos de {sourceDirName} para {destDirName}...");

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Pasta não encontrada: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
