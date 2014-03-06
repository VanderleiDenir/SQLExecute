using Microsoft.SqlServer.Management.Common;
using System.Data.SqlClient;

namespace SQLExecute
{
    public interface ISqlServer
    {
        void CloseServer();
        void ExecuteScript(string commandString);
        ServerVersion PingServer();
        void SetInfoMessageEvent(SqlInfoMessageEventHandler OnInfoMessage);
        void KillServer();
    }
}
