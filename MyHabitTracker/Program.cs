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

    static void GetUserInput()
    {
        Console.Clear();
        bool closeApp = false;
        while (closeApp == false)
        {
            System.Console.WriteLine("\n\nMAIN MENU");
            System.Console.WriteLine("\nWhat would you like to do?");
            System.Console.WriteLine("\nType 0 to Close Application.");
            System.Console.WriteLine("Type 1 to View All Record.");
            System.Console.WriteLine("Type 2 to Insert Record.");
            System.Console.WriteLine("Type 3 to Delete Record.");
            System.Console.WriteLine("Type 4 to Update Record.");
            System.Console.WriteLine("-------------------------------------\n");

            string? commandInput = Console.ReadLine();

            switch(commandInput)
            {
                case "0":
                    System.Console.WriteLine("\nGoodbye\n");
                    closeApp = true;
                    break;
                case "1":
                    GetAllRecords();
                    break;
                case "2":
                    Insert();
                    break;
                case "3":
                    Delete();
                    break;
                case "4":
                    Update();
                    break;
                default:
                    System.Console.WriteLine("\nInvalid Command. Write a number from 0 to 4.\n");
                    break;
            }
        }
    }
}