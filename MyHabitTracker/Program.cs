using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MyHabitTracker;

internal static class Program
{
    private const string ConnectionString = @"Data Source=MyHabitTracker.db";

    private static void Main()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        var tableCmd = connection.CreateCommand();

        //Using @ allow to create multiline statement
        tableCmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS drinking_water (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Date TEXT NOT NULL,
                                Quantity INTEGER NOT NULL
                                )
            """;

        tableCmd.ExecuteNonQuery();

        connection.Close();

        GetUserInput();
    }

    private static void GetUserInput()
    {
        Console.Clear();
        bool closeApp = false;
        while (closeApp == false)
        {
            Console.WriteLine("\n\nMAIN MENU");
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("\nType 0 to Close Application.");
            Console.WriteLine("Type 1 to View All Record.");
            Console.WriteLine("Type 2 to Insert Record.");
            Console.WriteLine("Type 3 to Delete Record.");
            Console.WriteLine("Type 4 to Update Record.");
            Console.WriteLine("-------------------------------------\n");

            string? commandInput = Console.ReadLine();

            switch(commandInput)
            {
                case "0":
                    Console.WriteLine("\nGoodbye\n");
                    closeApp = true;
                    Environment.Exit(0);
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
                    Console.WriteLine("\nInvalid Command. Write a number from 0 to 4.\n");
                    break;
            }
        }
    }

    private static void Insert()
    {
        string date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        
        //Parametrized query
        using var command = new SqliteCommand("INSERT INTO drinking_water(date, quantity) VALUES(@date, @quantity)", connection);

        command.Parameters.AddWithValue("@date", date);
        command.Parameters.AddWithValue("@quantity", quantity);
        command.ExecuteNonQuery();
        
        connection.Close();
    }

    private static void Delete()
    {
        Console.Clear();
        GetAllRecords();

        var recordId = GetNumberInput("\n\nPlease type the Id of the record you want to delete. Type 0 to return to Main Menu\n\n");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        
        //Parametrized query
        using var command = new SqliteCommand("DELETE FROM drinking_water WHERE Id = @recordId", connection);

        command.Parameters.AddWithValue("@recordId", recordId);

        var rowCount = command.ExecuteNonQuery();

        if (rowCount == 0)
        {
            Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
            Delete();
        }

        Console.WriteLine($"\n\nRecord with Id {recordId} was deleted.\n\n");
    }

    private static void Update()
    {
        GetAllRecords();

        var recordId = GetNumberInput("\n\nPlease type the Id of the record would like to update. Type 0 to return to Main Menu");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        //Parametrized query
        using var command = new SqliteCommand("SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = @recordId)",connection);
        command.Parameters.AddWithValue("@recordId", recordId);
        
        var checkQuery = Convert.ToInt32(command.ExecuteScalar());

        if (checkQuery == 0)
        {
            Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
            connection.Close();
            Update();
        }

        string date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

        //Parametrized query
        using var updateCmd = new SqliteCommand("UPDATE drinking_water SET date = @date, quantity = @quantity  WHERE Id = @recordId", connection);
        updateCmd.Parameters.AddWithValue("@date", date);
        updateCmd.Parameters.AddWithValue("@quantity", quantity);
        updateCmd.Parameters.AddWithValue("@recordId", recordId);

        updateCmd.ExecuteNonQuery();

        connection.Close();
    }

    private static string GetDateInput()
    {
        Console.WriteLine("\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.\n");

        string? dateInput = Console.ReadLine();

        if (dateInput == "0") GetUserInput();

        while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
        {
            Console.WriteLine("\n\nInvalid date. (Format: dd-MM-yy). Type 0 to return to main menu.\n\n");
            dateInput = Console.ReadLine();
        }

        return dateInput;
    }

    private static int GetNumberInput(string message)
    {
        Console.WriteLine(message);

        string? numberInput = Console.ReadLine();

        if (numberInput == "0") GetUserInput();

        while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
        {
            Console.WriteLine("\n\nInvalid number. Try again.\n\n");
            numberInput = Console.ReadLine();
        }

        int finalInput = Convert.ToInt32(numberInput);

        return finalInput;
    }

    private static void GetAllRecords()
    {
        Console.Clear();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        
        
        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = $"SELECT * FROM drinking_water";

        List<DrinkingWater> tableData = new();

        SqliteDataReader reader = tableCmd.ExecuteReader();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                tableData.Add(
                    new DrinkingWater
                    {
                        Id = reader.GetInt32(0),
                        Date = DateTime.ParseExact(reader.GetString(1),"dd-mm-yy",new CultureInfo("en-US")),
                        Quantity = reader.GetInt32(2)
                    }
                );
            }
        }
        else
        {
            Console.WriteLine("No rows found.");
        }
        
        connection.Close();

        Console.WriteLine("---------------------------\n");
        foreach (var dw in tableData)
        {
            Console.WriteLine($"{dw.Id} - {dw.Date:dd-MM-yyyy} - Quantidade: {dw.Quantity}");
        }

        Console.WriteLine("---------------------------\n");
    }
}

public class DrinkingWater
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public int Quantity { get; init; }
}