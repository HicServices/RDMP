using System.Data.SqlClient;

namespace DataLoadEngine.DatabaseManagement.Operations
{
    public interface IDatabaseOperation
    {
        void Execute();
        void Execute(SqlConnection connection);
        void Undo();

        string GetSQL();
    }
}