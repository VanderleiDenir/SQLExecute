using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace ScriptRunner
{
    public class SQLManager
    {
        public ISqlServer server;
        public BackgroundWorker backgroundScriptWorker;
        public List<FilePathData> filePaths;
        public DispatcherTimer Timer;
        public DateTime StartTime;
        public FileInfo CurrentFile;
        public DateTime TotalTime;
        public string CurrentFileUid;
        public bool finished;
        public bool continueWait;
        public bool error;
        public bool fileOpened;
        public bool stopped;

        public SQLManager()
        {
            this.backgroundScriptWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            this.filePaths = new List<FilePathData>();
            this.Timer = new DispatcherTimer();
            this.continueWait = true;
        }

        public void InitServer(SqlServer server)
        {
            this.server = (ISqlServer)server;
            server.SetInfoMessageEvent(new SqlInfoMessageEventHandler(this.OnInfoMessage));
        }

        public void RunScripts()
        {
            this.backgroundScriptWorker.DoWork += new DoWorkEventHandler(this.RunScriptsStart);
            this.backgroundScriptWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.RunScriptsCompleted);
            this.backgroundScriptWorker.RunWorkerAsync();
        }

        private void RunScriptsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.server.CloseServer();
            if (!this.Timer.IsEnabled)
                return;
            this.Timer.Stop();
        }

        private void RunScriptsStart(object sender, DoWorkEventArgs e)
        {
            if (!this.CheckServerAvailablity("logon", "password"))
                return;
            this.stopped = false;
            this.finished = false;
            List<FileInfo> files = new List<FileInfo>();
            this.TotalTime = DateTime.Now;
            Enumerable.ToList<FilePathData>((IEnumerable<FilePathData>)this.filePaths).ForEach((Action<FilePathData>)(filepath => files.Add(new FileInfo(filepath.fullFileName))));
            using (List<FileInfo>.Enumerator enumerator = Enumerable.ToList<FileInfo>((IEnumerable<FileInfo>)files).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    FileInfo file = enumerator.Current;
                    this.CurrentFile = file;
                    this.CurrentFileUid = Enumerable.FirstOrDefault<FilePathData>((IEnumerable<FilePathData>)this.filePaths, (Func<FilePathData, bool>)(x => x.fullFileName == file.FullName)).uid.ToString();
                    this.StartTime = DateTime.Now;
                    if (Enumerable.FirstOrDefault<FilePathData>((IEnumerable<FilePathData>)this.filePaths, (Func<FilePathData, bool>)(x => x.uid.ToString() == this.CurrentFileUid)).fileRunStatus == FileRunStatus.NotRun)
                    {
                        string input = file.OpenText().ReadToEnd();
                        input.Replace("^\\s*Go\\s*$", "^\\s*GO\\s*$");
                        input.Replace("^\\s*go\\s*$", "^\\s*GO\\s*$");
                        IEnumerable<string> source = (IEnumerable<string>)Regex.Split(input, "^\\s*GO\\s*$", RegexOptions.Multiline);
                        this.error = false;
                        this.backgroundScriptWorker.ReportProgress(0, (object)new MessageToReturn()
                        {
                            fileName = (this.CurrentFile != null ? this.CurrentFile.Name : ""),
                            message = (file.Name + "................Executando"),
                            messageType = MessageType.Running,
                            fileUid = this.CurrentFileUid
                        });
                        file.OpenText().Close();
                        this.Timer.Start();
                        foreach (string commandString in Enumerable.Where<string>((IEnumerable<string>)Enumerable.ToList<string>(source), (Func<string, bool>)(commandString => commandString.Trim() != "")))
                        {
                            try
                            {
                                this.server.ExecuteScript(commandString);
                            }
                            catch
                            {
                                if (!this.stopped)
                                    this.backgroundScriptWorker.ReportProgress(0, (object)new MessageToReturn()
                                    {
                                        fileName = (this.CurrentFile != null ? this.CurrentFile.Name : ""),
                                        message = "Sem conexão com Banco de Dados!",
                                        messageType = MessageType.DataBaseConnectionFailure
                                    });
                                if (this.stopped)
                                    break;
                            }
                        }
                        this.Timer.Stop();
                        if (!this.stopped)
                        {
                            if (!this.error)
                            {
                                Enumerable.FirstOrDefault<FilePathData>((IEnumerable<FilePathData>)this.filePaths, (Func<FilePathData, bool>)(x => x.uid.ToString() == this.CurrentFileUid)).fileRunStatus = FileRunStatus.Passed;
                                this.backgroundScriptWorker.ReportProgress(0, (object)new MessageToReturn()
                                {
                                    fileName = (this.CurrentFile != null ? this.CurrentFile.Name : ""),
                                    message = (file.Name + "...............Passou"),
                                    messageType = MessageType.PassMessage,
                                    fileUid = this.CurrentFileUid
                                });
                            }
                            else if (this.error)
                            {
                                Enumerable.FirstOrDefault<FilePathData>((IEnumerable<FilePathData>)this.filePaths, (Func<FilePathData, bool>)(x => x.uid.ToString() == this.CurrentFileUid)).fileRunStatus = FileRunStatus.Failed;
                                this.backgroundScriptWorker.ReportProgress(0, (object)new MessageToReturn()
                                {
                                    fileName = (this.CurrentFile != null ? this.CurrentFile.Name : ""),
                                    message = (file.Name + "................Falhou"),
                                    messageType = MessageType.FailedMessage,
                                    fileUid = this.CurrentFileUid
                                });
                                do
                                    ;
                                while (this.continueWait);
                                this.continueWait = true;
                                if (this.error)
                                    break;
                            }
                        }
                        else
                            break;
                    }
                }
            }
            this.finished = this.filePaths[this.filePaths.Count - 1].uid.ToString() == this.CurrentFileUid;
        }

        public void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            MessageToReturn messageToReturn = new MessageToReturn()
            {
                fileName = this.CurrentFile.Name
            };
            if ((int)e.Errors[0].Class != 0)
            {
                this.error = true;
                messageToReturn.message = string.Concat(new object[4]
        {
          (object) "Falha de Script: ",
          (object) e.Errors[0].Message,
          (object) " Linha No:",
          (object) e.Errors[0].LineNumber
        });
                messageToReturn.messageType = MessageType.Error;
                messageToReturn.fileUid = this.CurrentFileUid;
            }
            else
            {
                messageToReturn.message = e.Message;
                messageToReturn.messageType = MessageType.SQLMessage;
                messageToReturn.fileUid = this.CurrentFileUid;
            }
            this.backgroundScriptWorker.ReportProgress(0, (object)messageToReturn);
        }

        private bool CheckServerAvailablity(string logon, string password)
        {
            try
            {
                this.server.PingServer();
                return true;
            }
            catch (Exception ex)
            {
                this.backgroundScriptWorker.ReportProgress(0, (object)new MessageToReturn()
                {
                    fileName = (this.CurrentFile != null ? this.CurrentFile.Name : ""),
                    message = "Sem Conexão com Danco de Dados!",
                    messageType = MessageType.DataBaseConnectionFailure
                });
                return false;
            }
        }

        public void StopProcessing()
        {
            this.Timer.Stop();
            this.stopped = true;
            this.server.KillServer();
            this.backgroundScriptWorker.Dispose();
            this.backgroundScriptWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
        }
    }
}
