using Microsoft.Data.Sqlite;

namespace MyHabitTracker;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = @"Data Source=MyHabitTracker.db";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var tableCmd = connection.CreateCommand();

        //Using @ allow to create multiline statement
        tableCmd.CommandText =
            @"CREATE TABLE IF NOT EXISTS drinking_water (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    Quantity INTEGER NOT NULL
                    )";

        tableCmd.ExecuteNonQuery();

        connection.Close();
    }
}