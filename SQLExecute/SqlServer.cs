using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System.Data.SqlClient;

namespace SQLExecute
{
   public class SqlServer : ISqlServer
   {
      private readonly SqlConnection conn;
      private readonly Server serverConn;

      public SqlServer(string serverName, string userName, string password, string dataBase)
      {
         string str = "Initial Catalog=" + dataBase + "; Data Source=" + serverName + ";";
         if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
            str = str + " Integrated Security=True";
         else if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            str = str + "User ID=" + userName + ";password=" + password;
         this.conn = new SqlConnection(str + ";")
         {
            FireInfoMessageEventOnUserErrors = true
         };
         this.serverConn = new Server(new ServerConnection(this.conn))
         {
            ConnectionContext =
            {
               StatementTimeout = 99999
            }
         };
      }

      public SqlServer(string sqlConnectionString)
      {
         this.conn = new SqlConnection(sqlConnectionString + ";")
         {
            FireInfoMessageEventOnUserErrors = true
         };
         this.serverConn = new Server(new ServerConnection(this.conn))
         {
            ConnectionContext =
            {
               StatementTimeout = 99999
            }
         };
      }

      public void CloseServer()
      {
         if (this.serverConn == null || this.serverConn.ConnectionContext.SqlConnectionObject.State == ConnectionState.Closed)
            return;
         this.serverConn.ConnectionContext.SqlConnectionObject.Close();
      }

      public void ExecuteScript(string commandString)
      {
         this.serverConn.ConnectionContext.ExecuteWithResults(commandString);
      }

      public ServerVersion PingServer()
      {
         return this.serverConn.PingSqlServerVersion(this.serverConn.Name);
      }

      public void SetInfoMessageEvent(SqlInfoMessageEventHandler OnInfoMessage)
      {
         this.conn.InfoMessage += OnInfoMessage;
      }

      public void KillServer()
      {
         this.serverConn.ConnectionContext.Cancel();
      }
   }
}
