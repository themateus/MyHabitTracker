using System.Globalization;
using Microsoft.Data.Sqlite;

//E SE EU FIZER UM MENU PARA ESCOLHER O HABITO E ABRE O GETUSERINPUT
//PODENDO DIGITAR 0 PARA RETORNAR AO MENU DE ESCOLHA DE HABITO
//O MENU PARA ESCOLHER O HABITO PODEMOS USAR AS MESMAS FUNCOES DO GETUSERINPUT

namespace MyHabitTracker;

internal static class Program
{
    private const string ConnectionString = @"Data Source=MyHabitTracker.db";

    private static void Main()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        var tableCmd = connection.CreateCommand();

        // Criando tabela de hábitos com as unidades
        tableCmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS habits (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Name TEXT NOT NULL,
                                Unit INTEGER NOT NULL
                                )
            """;
        tableCmd.ExecuteNonQuery();
        
        // Criando tabela para registro dos hábitos
        tableCmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS register (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                HabitId TEXT NOT NULL,
                                Date TEXT NOT NULL,
                                Value INTEGER NOT NULL,
                                FOREIGN KEY (HabitId) REFERENCES habits(Id)
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
            Console.WriteLine("\n===== MAIN MENU =====");
            Console.WriteLine("\nHABITS:");
            Console.WriteLine("     1. Insert Habit.");
            Console.WriteLine("     2. Update Habit.");
            Console.WriteLine("     3. Delete Habit.");
            Console.WriteLine("     4. View All Habits.");
            
            Console.WriteLine("\nRECORDS:");
            Console.WriteLine("     5. Insert Record.");
            Console.WriteLine("     6. Update Record.");
            Console.WriteLine("     7. Delete Record.");
            Console.WriteLine("     8. View All Records.");

            Console.WriteLine("\nOTHER:");
            Console.WriteLine("     0.Close Application");
            Console.WriteLine("\n======================\n");
            Console.Write("Select an option: ");

            string? commandInput = Console.ReadLine();

            switch(commandInput)
            {
                case "0":
                    Console.WriteLine("\nGoodbye\n");
                    closeApp = true;
                    Environment.Exit(0);
                    break;
                case "1":
                    InsertHabit();
                    break;
                case "2":
                    UpdateHabit();
                    break;
                case "3":
                    DeleteHabit();
                    break;
                case "4":
                    GetAllHabits();
                    break;
                case "5":
                    InsertRecord();
                    break;
                case "6":
                    UpdateRecord();
                    break;
                case "7":
                    DeleteRecord();
                    break;
                case "8":
                    GetAllRecords();
                    break;
                default:
                    Console.WriteLine("\nInvalid Command. Write a number from 0 to 8.\n");
                    break;
            }
        }
    }

    private static void InsertHabit()
    {
        string? habitName = GetNameInput("\n\nWrite the habit name. Type 0 to return to main menu.\n\n");
        string? unitName = GetNameInput("\n\nWrite the unit of measurement. Type 0 to return to main menu.\n\n");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        
        //Parametrized query
        using var command = new SqliteCommand("INSERT INTO habits(Name, Unit) VALUES(@Name, @Unit)", connection);
        command.Parameters.AddWithValue("@Name", habitName);
        command.Parameters.AddWithValue("@Unit", unitName);
        command.ExecuteNonQuery();

        connection.Close();
    }

    private static void UpdateHabit()
    {
        GetAllHabits();
        var idSelected = GetNumberInput("\nWrite the number of the habit that you want to update. Type 0 to return to main menu\n");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("SELECT EXISTS(SELECT 1 FROM habits WHERE ID = @habitId)", connection);
        command.Parameters.AddWithValue("@habitId", idSelected);

        var checkQuery = Convert.ToInt32(command.ExecuteScalar());
        if (checkQuery == 0)
        {
            Console.WriteLine($"\nHabit with Id {idSelected} doesn't exists.");
            connection.Close();
            UpdateHabit();
        }
        
        string? habitName = GetNameInput("\n\nWrite the habit name. Type 0 to return to main menu.\n\n");
        string? unitName = GetNameInput("\n\nWrite the unit of measurement. Type 0 to return to main menu.\n\n");
        
        //Parametrized query
        using var updateCommand = new SqliteCommand("UPDATE habits SET Name = @Name, Unit = @Unit WHERE Id = @HabitId", connection);
        updateCommand.Parameters.AddWithValue("@Name", habitName);
        updateCommand.Parameters.AddWithValue("@Unit", unitName);
        updateCommand.Parameters.AddWithValue("@HabitId", idSelected);
        updateCommand.ExecuteNonQuery();

        connection.Close();
    }

    private static void DeleteHabit()
    {
        Console.Clear();
        GetAllHabits();

        var habitId = GetNumberInput("\nType the Id of the Habit to delete. Type 0 to return to main menu.");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("DELETE FROM habits WHERE Id = @HabitId",connection);
        command.Parameters.AddWithValue("@HabitId", habitId);

        var rowCount = command.ExecuteNonQuery();
        if (rowCount == 0)
        {
            Console.WriteLine($"\nHabbit with Id {habitId} doesn't exists.");
            connection.Close();
            DeleteHabit();
        }
        
        Console.WriteLine($"\nHabbit with id {habitId} was deleted.");
    }
    
    private static void GetAllHabits()
    {
        Console.Clear();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = $"SELECT * FROM habits";

        List<Habits> tableData = new();

        SqliteDataReader reader = tableCmd.ExecuteReader();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                tableData.Add(
                    new Habits
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Unit = reader.GetString(2)
                    });
            }
        }
        else
        {
            Console.WriteLine("No rows found.");
        }
        
        connection.Close();
        
        Console.WriteLine("---------------------------\n");
        Console.WriteLine("This is your habits:\n");
        foreach (var dw in tableData)
        {
            //Usar esse -n faz com que fique alinhado de forma fixa
            Console.WriteLine($"{dw.Id, -5} {dw.Name, -20} {dw.Unit, -10}");
        }

        Console.WriteLine("\n---------------------------\n");
    }
    
    private static void InsertRecord()
    {
        string date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease insert number of \n\n");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        
        //Parametrized query
        using var command = new SqliteCommand("INSERT INTO drinking_water(date, quantity) VALUES(@date, @quantity)", connection);

        command.Parameters.AddWithValue("@date", date);
        command.Parameters.AddWithValue("@quantity", quantity);
        command.ExecuteNonQuery();
        
        connection.Close();
    }

    private static void DeleteRecord()
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
            connection.Close();
            DeleteRecord();
        }

        Console.WriteLine($"\n\nRecord with Id {recordId} was deleted.\n\n");
    }

    private static void UpdateRecord()
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
            UpdateRecord();
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

    private static string? GetNameInput(string message)
    {
        Console.WriteLine(message);

        string? nameInput = Console.ReadLine();

        if (nameInput == "0") GetUserInput();
        
        while (double.TryParse(nameInput, out _))
        {
            Console.WriteLine("\n\nThat's a number. Try again.\n\n");
            nameInput = Console.ReadLine();
        }

        return nameInput;
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

public class Habits
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Unit { get; init; }
}