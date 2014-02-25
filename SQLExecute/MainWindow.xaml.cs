using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ScriptRunner;
using System.Collections.Generic;
using System.IO;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;

namespace ScriptRunner2
{
   public partial class MainWindow : Window, IComponentConnector, INotifyPropertyChanged
   {
      private readonly Options optionWindow;//= new Options();
      private List<ScriptDetails> scriptDetails = new List<ScriptDetails>();
      private SQLManager sqlRun;
      //private IExcelWriter excelwriter;

      public MainWindow()
      {
         this.InitializeComponent();
         optionWindow = new Options();


         //Marca o diretório a ser listado
         DirectoryInfo diretorio = new DirectoryInfo(Directory.GetCurrentDirectory());
         //Executa função GetFile(Lista os arquivos desejados de acordo com o parametro)
         //txtDirPath.Text = Directory.GetCurrentDirectory() + "\\" + diretorio.GetDirectories().LastOrDefault().Name;

      }

      private void btnRunScript_Click(object sender, RoutedEventArgs e)
      {
         if (string.IsNullOrEmpty(optionWindow.tbServerName.Text))
         {
            optionsButton_Click(null, null);
            return;
         }

         if (this.btnRunScript.Content.ToString() == ((object)BtnType.Iniciar).ToString())
            this.BtnRun_Click(false);
         else if (this.btnRunScript.Content.ToString() == ((object)BtnType.Reiniciar).ToString())
         {
            this.InitialiseSqlManager(true, this.sqlRun.filePaths);
            this.InitialiseServerConnection();
            this.BtnReRun_Click();
         }
         else if (this.btnRunScript.Content.ToString() == ((object)BtnType.Parar).ToString())
         {
            this.BtnStop_Click();
         }
         else
         {
            if (!(this.btnRunScript.Content.ToString() == ((object)BtnType.Continuar).ToString()))
               return;
            this.btnRunScript.Content = (object)BtnType.Parar;
            this.InitialiseSqlManager(true, this.sqlRun.filePaths);
            this.InitialiseServerConnection();
            this.BtnRun_Click(true);
         }
      }

      private void BtnRun_Click(bool isReRun)
      {
         if (!isReRun)
         {
            if (this.sqlRun != null && this.sqlRun.filePaths.Count > 0)
               this.InitialiseServerConnection();
            if (this.txtDirPath.Text != "" && Directory.Exists(this.txtDirPath.Text))
            {
               //this.lstDragDrop.Items.Clear();
               this.InitialiseSqlManager(isReRun, GetListOfFiles());//Enumerable.ToList<string>((IEnumerable<string>)Directory.GetFiles(this.txtDirPath.Text))));
               this.InitialiseServerConnection();
               this.AddFilesToList(this.sqlRun.filePaths, isReRun);
            }
         }
         if (this.sqlRun == null || this.sqlRun.filePaths.Count <= 0)
            return;
         this.btnRunScript.Content = (object)((object)BtnType.Parar).ToString();
         this.btnClear.IsEnabled = false;
         this.sqlRun.backgroundScriptWorker.ProgressChanged += new ProgressChangedEventHandler(this.backgroundScriptWorker_ProgressChanged);
         this.sqlRun.Timer.Tick += new EventHandler(this.WriteDurationTime);
         this.sqlRun.RunScripts();
      }

      //
      public void backgroundScriptWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         MessageToReturn message = (MessageToReturn)e.UserState;
         if (message.messageType == MessageType.SQLMessage)
            this.messagesListBox.Items.Insert(0, (object)message.message);
         else if (message.messageType == MessageType.Error)
         {
            ItemCollection items = this.messagesListBox.Items;
            int insertIndex = 0;
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Width = 730.0;
            textBlock1.Text = message.message;
            textBlock1.Foreground = (Brush)new SolidColorBrush(Colors.Red);
            TextBlock textBlock2 = textBlock1;
            items.Insert(insertIndex, (object)textBlock2);
            this.WritePassedOrFailedMessageForReportGeneration(message, false);
         }
         else if (message.messageType == MessageType.DataBaseConnectionFailure)
         {
            ItemCollection items = this.messagesListBox.Items;
            int insertIndex = 0;
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Width = 730.0;
            textBlock1.Text = message.message;
            textBlock1.Foreground = (Brush)new SolidColorBrush(Colors.Red);
            TextBlock textBlock2 = textBlock1;
            items.Insert(insertIndex, (object)textBlock2);
            this.WritePassedOrFailedMessageForReportGeneration(message, false);
            this.btnRunScript.Content = (object)((object)BtnType.Iniciar).ToString();
         }
         else if (message.messageType == MessageType.PassMessage)
         {
            this.BindMessageToList(message, Colors.DarkGreen);
            this.WritePassedOrFailedMessageForReportGeneration(message, true);
            this.lblTime.Content = (object)"00:00:00";
         }
         else if (message.messageType == MessageType.FailedMessage)
         {
            this.BindMessageToList(message, Colors.Red);
            this.WritePassedOrFailedMessageForReportGeneration(message, false);
            this.lblTime.Content = (object)"00:00:00";
            bool? isChecked = this.optionWindow.cbContinueOnError.IsChecked;
            if ((isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?()).Value)
            {
               this.sqlRun.Timer.Stop();
               this.lblTime.Content = (object)"00:00:00";
               this.btnClear.IsEnabled = true;
               this.btnRunScript.Content = (object)((object)BtnType.Continuar).ToString();
            }
            else
               this.sqlRun.error = false;
            if (this.optionWindow.cbOpenScript.IsChecked.HasValue && this.optionWindow.cbOpenScript.IsChecked.Value)
            {
               if (!this.sqlRun.fileOpened)
               {
                  var currentScript = Enumerable.FirstOrDefault<FilePathData>((IEnumerable<FilePathData>)this.sqlRun.filePaths, (Func<FilePathData, bool>)(y => y.uid.ToString() == message.fileUid));
                  //try
                  //{
                  //var file = @"C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\Ssms.exe";
                  // http://stackoverflow.com/questions/4624113/how-to-process-start-with-impersonated-domain-user
                  //var sspw = new System.Security.SecureString();
                  //foreach (var c in optionWindow.tbPassword.Password.ToCharArray()) sspw.AppendChar(c);
                  //var proc = new Process();
                  //proc.StartInfo.UseShellExecute = false;
                  ////proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(file);
                  //proc.StartInfo.FileName = "Ssms.exe"; // Path.GetFileName(file);
                  //proc.StartInfo.Arguments = currentScript.fullFileName;
                  //proc.StartInfo.Domain = optionWindow.tbServerName.Text;
                  //proc.StartInfo.UserName = optionWindow.tbUserName.Text;
                  //proc.StartInfo.Password = sspw;

                  // http://stackoverflow.com/questions/4422084/impersonating-in-net-c-opening-a-file-via-process-start
                  //proc.StartInfo.LoadUserProfile = true;
                  //proc.Start();

                  //System.Security.SecureString password = new System.Security.SecureString();
                  //foreach (char c in optionWindow.tbPassword.Password.ToCharArray())
                  //{
                  //   password.AppendChar(c);
                  //}
                  //System.Diagnostics.ProcessStartInfo procInfo = new System.Diagnostics.ProcessStartInfo();
                  //procInfo.Arguments = " -file_name " + currentScript.fullFileName; //"/netonly";
                  //procInfo.FileName = "Ssms.exe";// @"C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\Ssms.exe"; ;
                  //procInfo.Domain = optionWindow.tbServerName.Text;
                  //procInfo.Verb = "runas";
                  //procInfo.UserName = optionWindow.tbUserName.Text;
                  //procInfo.Password = password;
                  //procInfo.UseShellExecute = false;
                  //System.Diagnostics.Process.Start(procInfo);

                  //Process.Start("Ssms.exe", " -nosplash" +
                  //   " -S " + optionWindow.tbServerName.Text +
                  //   " -D " + optionWindow.cbBancoDeDados.SelectedValue.ToString() +
                  //   (string.IsNullOrEmpty(optionWindow.tbUserName.Text) ? " -U " + optionWindow.tbUserName.Text + " -P " + optionWindow.tbPassword.Password : " -E ") +
                  //   " -file_name " + currentScript.fullFileName);
                  //}
                  //catch
                  //{
                  Process.Start("notepad.exe", currentScript.fullFileName);
                  //}
                  this.sqlRun.fileOpened = true;
               }
            }
            this.sqlRun.continueWait = false;
         }
         else if (message.messageType == MessageType.Running)
            this.BindMessageToList(message, Colors.Orange);
         if (!this.sqlRun.finished)
            return;
         this.btnRunScript.Content = (object)((object)BtnType.Reiniciar).ToString();
         this.btnClear.IsEnabled = true;
         this.sqlRun.finished = false;
      }

      private void WriteDurationTime(object sender, EventArgs e)
      {
         DateTime dateTime = DateTime.Now;
         dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
         this.sqlRun.StartTime = new DateTime(this.sqlRun.StartTime.Year, this.sqlRun.StartTime.Month, this.sqlRun.StartTime.Day, this.sqlRun.StartTime.Hour, this.sqlRun.StartTime.Minute, this.sqlRun.StartTime.Second);
         this.lblTime.Content = (object)dateTime.Subtract(this.sqlRun.StartTime).ToString();
         this.sqlRun.TotalTime = new DateTime(this.sqlRun.TotalTime.Year, this.sqlRun.TotalTime.Month, this.sqlRun.TotalTime.Day, this.sqlRun.TotalTime.Hour, this.sqlRun.TotalTime.Minute, this.sqlRun.TotalTime.Second);
         this.lblTotalTime.Content = (object)dateTime.Subtract(this.sqlRun.TotalTime).ToString();
      }

      private void btnClear_Click(object sender, RoutedEventArgs e)
      {
         this.InitialiseSqlManager(false, (List<FilePathData>)null);
         this.btnRunScript.Content = (object)((object)BtnType.Iniciar).ToString();
         this.Clear();
      }

      private void Clear()
      {
         this.messagesListBox.Items.Clear();
         carregaArquivosSql();
         //this.lstDragDrop.Items.Clear();
         //if (this.btnRunScript.Content.ToString() != ((object)BtnType.ReRun).ToString())
         //   this.lstDragDrop.Items.Insert(0, (object)this.dropFilesHere);
         //this.txtDirPath.Clear();
         this.lblTime.Content = (object)"00:00:00";
         this.lblTotalTime.Content = (object)"00:00:00";
         this.btnRunScript.Content = (object)((object)BtnType.Iniciar).ToString();
         this.scriptDetails = new List<ScriptDetails>();
      }

      private void optionsButton_Click(object sender, RoutedEventArgs e)
      {
         this.optionWindow.Show();
      }

      private void cancelButton_Click(object sender, RoutedEventArgs e)
      {
         if (this.sqlRun != null && this.sqlRun.server != null)
            this.sqlRun.server.KillServer();
         this.optionWindow.Close();
         this.Close();
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         if (this.sqlRun != null && this.sqlRun.server != null)
            this.sqlRun.server.CloseServer();
         this.optionWindow.Close();
      }

      private void BtnStop_Click()
      {
         CheckBox currentRunningCheckBox = Enumerable.FirstOrDefault<CheckBox>((IEnumerable<CheckBox>)Enumerable.ToList<CheckBox>(Enumerable.OfType<CheckBox>((IEnumerable)this.lstDragDrop.Items)), (Func<CheckBox, bool>)(x => x.Content.ToString().Contains("................Executando")));
         if (currentRunningCheckBox != null)
         {
            currentRunningCheckBox.Foreground = this.Foreground = (Brush)new SolidColorBrush(Colors.Blue);
            currentRunningCheckBox.Content = (object)Enumerable.FirstOrDefault<CheckBox>((IEnumerable<CheckBox>)Enumerable.ToList<CheckBox>(Enumerable.OfType<CheckBox>((IEnumerable)this.lstDragDrop.Items)), (Func<CheckBox, bool>)(x => x.Content.ToString().Contains("................Executando"))).Content.ToString().Replace("................Executando", "................Parado");
         }
         this.sqlRun.StopProcessing();
         this.btnRunScript.Content = (object)((object)BtnType.Continuar).ToString();
         this.btnClear.IsEnabled = true;
      }

      private void BtnReRun_Click()
      {
         this.Clear();
         this.AddFilesToList(this.sqlRun.filePaths, true);
         this.BtnRun_Click(true);
      }

      private void RunReport_Click(object sender, RoutedEventArgs e)
      {
         //if (this.scriptDetails.Count <= 0)
         //    return;
         //this.excelwriter = (IExcelWriter)new ExcelWriter();
         //this.excelwriter.WriteToExcel(this.scriptDetails, this.lblTotalTime.Content.ToString());
      }

      private List<FilePathData> GetListOfFiles()
      {
         List<FilePathData> filepathDataList = new List<FilePathData>();
         foreach (CheckBox item in this.lstDragDrop.Items)
         {
            if (item.IsChecked == true && item.IsEnabled)
            {
               filepathDataList.Add(new FilePathData()
               {
                  fullFileName = txtDirPath.Text + "\\" + item.Content,
                  uid = new Guid(item.Uid)
               });
            }
         }
         return filepathDataList;
         //List<FilePathData> listofString = new List<FilePathData>();
         //strings.ForEach((Action<string>)(x => listofString.Add(new FilePathData()
         //{
         //   uid = Guid.NewGuid(),
         //   fullFileName = x
         //})));
         //return listofString;
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         //if (!RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully)
         //    return;
         this.btnRunScript.Content = (object)((object)BtnType.Iniciar).ToString();
      }

      private void txtDirPath_Drop(object sender, DragEventArgs e)
      {
         string[] strArray = (string[])e.Data.GetData(DataFormats.FileDrop, false);
         this.txtDirPath.Text = strArray[0] ?? strArray[0];
      }

      private void txtDirPath_DragEnter(object sender, DragEventArgs e)
      {
         e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
      }

      private void txtDirPath_PreviewDragOver(object sender, DragEventArgs e)
      {
         e.Handled = true;
      }

      private void AddFilesToList(List<FilePathData> filepathDataList, bool isReRun)
      {
         //filepathDataList.ForEach((Action<FilePathData>)(x =>
         //{
         //   if (isReRun)
         //      x.fileRunStatus = FileRunStatus.NotRun;
         //   if (!x.fullFileName.ToUpper().Contains(".SQL"))
         //      return;
         //   ItemCollection temp_19 = this.lstDragDrop.Items;
         //   ListBoxItem temp_28 = new ListBoxItem()
         //   {
         //      Uid = x.uid.ToString(),
         //      Content = (object)x.fullFileName
         //   };
         //   temp_19.Add((object)temp_28);
         //}));
         filepathDataList = new List<FilePathData>();
         foreach (CheckBox item in this.lstDragDrop.Items)
         {
            if (item.IsChecked == true && item.IsEnabled)
            {
               filepathDataList.Add(new FilePathData()
               {
                  fullFileName = txtDirPath.Text + "\\" + item.Content,
                  fileRunStatus = FileRunStatus.NotRun,
                  uid = new Guid(item.Uid)
               });
            }
         }
      }

      private void BindMessageToList(MessageToReturn message, Color color)
      {
         CheckBox element = Enumerable.FirstOrDefault<CheckBox>((IEnumerable<CheckBox>)Enumerable.ToList<CheckBox>(
            Enumerable.OfType<CheckBox>((IEnumerable)this.lstDragDrop.Items)), (Func<CheckBox, bool>)(x => x.Uid == message.fileUid));

         if (element != null)
         {
            element.Content = (object)message.message;
            element.Foreground = (Brush)new SolidColorBrush(color);
            //element.Background = (Brush)new SolidColorBrush(color);
            element.IsEnabled = false; //Focus();
         }
      }

      private void WritePassedOrFailedMessageForReportGeneration(MessageToReturn message, bool isPassed)
      {
         if (!this.scriptDetails.Exists((Predicate<ScriptDetails>)(x => x.uid == message.fileUid)))
         {
            List<ScriptDetails> list1 = this.scriptDetails;
            ScriptDetails scriptDetails1 = new ScriptDetails();
            scriptDetails1.uid = message.fileUid ?? "";
            scriptDetails1.filename = message.fileUid == null ? "" : Enumerable.FirstOrDefault<FilePathData>((IEnumerable<FilePathData>)this.sqlRun.filePaths, (Func<FilePathData, bool>)(y => y.uid.ToString() == message.fileUid)).fileName();
            scriptDetails1.timetaken = this.lblTime.Content.ToString();
            scriptDetails1.status = isPassed ? "Passed" : "Failed";
            ScriptDetails scriptDetails2 = scriptDetails1;
            List<string> list2;
            if (!isPassed)
               list2 = new List<string>()
          {
            message.message
          };
            else
               list2 = new List<string>();
            scriptDetails2.remarks = list2;
            ScriptDetails scriptDetails3 = scriptDetails1;
            list1.Add(scriptDetails3);
         }
         else
         {
            if (message.messageType == MessageType.FailedMessage)
               return;

            var currentScript = Enumerable.FirstOrDefault<ScriptDetails>((IEnumerable<ScriptDetails>)this.scriptDetails, (Func<ScriptDetails, bool>)(x => x.uid == message.fileUid));
            if (currentScript.remarks == null)
               currentScript.remarks = new List<string>() { message.message };
            else
               currentScript.remarks.Add(message.message);
         }
      }

      private void InitialiseSqlManager(bool isReRunOrResume, List<FilePathData> renrunFilePath)
      {
         if (isReRunOrResume)
            renrunFilePath = this.sqlRun.filePaths;
         this.sqlRun = new SQLManager();
         if (renrunFilePath == null || renrunFilePath.Count <= 0)
            return;
         this.sqlRun.filePaths = Enumerable.ToList<FilePathData>((IEnumerable<FilePathData>)Enumerable.OrderBy<FilePathData,
                                       string>((IEnumerable<FilePathData>)renrunFilePath, (Func<FilePathData, string>)(x => x.fullFileName)));
      }

      private void InitialiseServerConnection()
      {
         this.sqlRun.InitServer(
            new SqlServer(this.optionWindow.tbServerName.Text,
                           this.optionWindow.tbUserName.Text,
                           this.optionWindow.tbPassword.Password,
                           this.optionWindow.cbBancoDeDados.SelectedValue.ToString()));
      }

      private void btBuscar_Click(object sender, RoutedEventArgs e)
      {
         ChooseFolder();
      }

      public void ChooseFolder()
      {
         var dialog = new System.Windows.Forms.FolderBrowserDialog();
         //System.Windows.Forms.DialogResult result = dialog.ShowDialog();

         // Set the help text description for the FolderBrowserDialog. 
         dialog.Description =
             "Selecione o Diretório onde estão localizados os Scripts SQL para atualização.";

         // Default to the My Documents folder. 
         //dialog.RootFolder = Environment.SpecialFolder.MyComputer;
         dialog.SelectedPath = txtDirPath.Text; //Directory.GetCurrentDirectory();

         if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            txtDirPath.Text = dialog.SelectedPath;
            carregaArquivosSql();
         }
      }

      private void carregaArquivosSql()
      {
         if (string.IsNullOrEmpty(txtDirPath.Text)) return;

         var checkBoxItens = new List<CheckBox>();
         DirectoryInfo di = new DirectoryInfo(txtDirPath.Text);
         foreach (FileInfo fi in di.GetFiles("*.sql"))
         {
            checkBoxItens.Add(new CheckBox
            {
               Content = fi.Name,
               IsChecked = true,
               IsEnabled = true,
               Uid = Guid.NewGuid().ToString(),
            });
         }
         CheckList = checkBoxItens;
      }


      #region Instance Variables
      List<CheckBox> checkList;
      #endregion /Instance Variables

      #region INotifyPropertyChanged
      //Add this bit of code to all your code behinds
      public event PropertyChangedEventHandler PropertyChanged;
      protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, e);
         }
      }
      #endregion /INotifyPropertyChanged

      //Property to bind to
      public List<CheckBox> CheckList
      {
         get { return checkList; }
         set
         {
            checkList = value;
            this.OnPropertyChanged(new PropertyChangedEventArgs("CheckList"));
         }
      }

      private void lstDragDrop_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         CheckBox chk;
         string nome;
         if (e.Device.Target is CheckBox)
         {
            chk = (CheckBox)e.Device.Target;
            nome = chk.Content.ToString();
         }
         else
         {
            //linha desabilitada
            chk = (CheckBox)((System.Windows.Controls.Panel)(e.Device.Target)).Children[0];
            nome = chk.Content.ToString().Replace("................Falhou", "").Replace("...............Passou", "");
         }
         Process.Start("notepad.exe", txtDirPath.Text + @"\" + nome);
         chk.IsChecked = true;
      }

   }
}
