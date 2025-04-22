using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MyHabitTracker;

class Program
{
    static string _connectionString = @"Data Source=MyHabitTracker.db";

    static void Main(string[] args)
    {
        using var connection = new SqliteConnection(_connectionString);
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
                    //Delete();
                    break;
                case "4":
                    //Update();
                    break;
                default:
                    System.Console.WriteLine("\nInvalid Command. Write a number from 0 to 4.\n");
                    break;
            }
        }
    }

    private static void Insert()
    {
        string date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = 
            $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";

        tableCmd.ExecuteNonQuery();
        connection.Close();
    }

    internal static string GetDateInput()
    {
        System.Console.WriteLine("\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.\n");

        string? dateInput = Console.ReadLine();

        if (dateInput == "0") GetUserInput();

        return dateInput;
    }

    internal static int GetNumberInput(string message)
    {
        System.Console.WriteLine(message);

        string? numberInput = Console.ReadLine();

        if (numberInput == "0") GetUserInput();

        int finalInput = Convert.ToInt32(numberInput);

        return finalInput;
    }

    internal static void GetAllRecords()
    {
        Console.Clear();
        using var connection = new SqliteConnection(_connectionString);
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
            Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MM-yyyy")} - Quantidade: {dw.Quantity}");
        }

        Console.WriteLine("---------------------------\n");
    }
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}