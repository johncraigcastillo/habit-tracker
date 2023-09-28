using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace HabitTracker;

public class Program
{
    private const string ConnectionString = @"Data Source=habit-tracker.db";

    static void Main(string[] args)
    {
        SQLitePCL.Batteries.Init();

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText =
                """
                CREATE TABLE IF NOT EXISTS drinking_water (
                    id     integer primary key autoincrement,
                    date   DATE not null,
                    amount int  not null,
                    check (amount = cast(amount as integer)),
                    check (amount >= 0));
                """;
            tableCommand.ExecuteNonQuery();
            connection.Close();
        }

        GetUserInput();
    }

    static void GetUserInput()
    {
        AnsiConsole.Clear();
        var closeApp = false;
        while (!closeApp)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("\n[blue bold]MAIN MENU[/]");
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .AddChoices(
                    "View Records", "Insert Record", "Delete Record", "Update Record", "Exit")
            );

            switch (selection)
            {
                case "View Records":
                    GetRecords();
                    break;
                case "Insert Record":
                    Insert();
                    break;
                case "Delete Record":
                    Delete();
                    break;
                case "Update Record":
                    Update();
                    break;
                case "Exit":
                    closeApp = true;
                    break;
            }
        }
    }

    private static void Update()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("\n[blue bold]UPDATE RECORD[/]");
        var recordId =
            AnsiConsole.Ask<int>(
                "[bold blue]Enter the id of the record you want to update[/] " +
                "[bold red]or Type 0 to cancel[/]: ");

        var selectCommand = $"SELECT * FROM drinking_water WHERE id = {recordId}";

        if (recordId == 0)
        {
            return;
        }

        if (!ExecuteSelectStatement(selectCommand))
        {
            AnsiConsole.MarkupLine("\n[red]Record not found![/]");
            AnsiConsole.MarkupLine("\n[green]Press any key to return to the main menu.[/]");
            Console.ReadKey();
            return;
        }


        var date = AnsiConsole.Ask<string>("Enter updated date (YYYY-MM-DD): ");
        date = EnsureDateFormat(date);

        var amount = AnsiConsole.Ask<int>("Enter updated amount (in oz): ");

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText =
                $"""
                 UPDATE drinking_water
                 SET date = '{date}', amount = {amount}
                 where id = {recordId}
                 """;
            updateCommand.ExecuteNonQuery();
            connection.Close();

            AnsiConsole.MarkupLine("\n[green]Record updated successfully[/]!");

            AnsiConsole.MarkupLine("\n[green]Press any key to return to the main menu.[/]");
            Console.ReadKey();
        }
    }

    private static void Delete()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("\n[blue bold]DELETE RECORD[/]");
        var recordId =
            AnsiConsole.Ask<int>(
                "[bold blue]Enter the id of the record you want to delete[/] " +
                "[bold red]or Type 0 to cancel[/]: ");
        if (recordId == 0)
        {
            return;
        }

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText =
                $"""
                 DELETE FROM drinking_water
                 WHERE id = {recordId};
                 """;
            var rowCount = deleteCommand.ExecuteNonQuery();
            connection.Close();
            if (rowCount == 0)
            {
                AnsiConsole.MarkupLine("\n[red]Record not found![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("\n[green]Record deleted successfully[/]!");
            }

            AnsiConsole.MarkupLine("\n[green]Press any key to return to the main menu.[/]");
            Console.ReadKey();
        }
    }

    private static void Insert()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("\n[blue bold]INSERT RECORD[/]");

        var date = AnsiConsole.Ask<string>("[bold blue]Enter the date (YYYY-MM-DD)[/] [bold red]or type q to cancel:[/] ");

        if (date == "q")
        {
            return;
        }

        date = EnsureDateFormat(date);


        var amount = AnsiConsole.Ask<int>("Enter the amount (in oz): ");

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText =
                $"""
                 INSERT INTO drinking_water (date, amount)
                 VALUES ('{date}', {amount})
                 """;
            insertCommand.ExecuteNonQuery();
            connection.Close();
        }

        AnsiConsole.MarkupLine("\n[green]Record inserted successfully![/]");
        AnsiConsole.MarkupLine("\n[green]Press any key to return to the main menu.[/]");
        Console.ReadKey();
    }

    private static string EnsureDateFormat(string date)
    {
        while (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                   out _))
        {
            AnsiConsole.MarkupLine("[red]Invalid input![/]");
            date = AnsiConsole.Ask<string>("Enter the date (YYYY-MM-DD): ");
        }

        return date;
    }

    private static void GetRecords()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("\n[blue bold]VIEW RECORDS[/]");
        const string selectAll = "SELECT * FROM drinking_water";
        ExecuteSelectStatement(selectAll);
        AnsiConsole.MarkupLine("\n[green]Press any key to return to the main menu.[/]");
        Console.ReadKey();
    }

    private static bool ExecuteSelectStatement(string commandText)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = commandText;
            var reader = selectCommand.ExecuteReader();
            if (!reader.HasRows)
            {
                return false;
            }

            BuildTable(reader);
            connection.Close();
            return true;
        }
    }

    private static void BuildTable(IDataReader reader)
    {
        var table = new Table
        {
            Border = TableBorder.Rounded
        };
        table.AddColumn("ID");
        table.AddColumn("DATE");
        table.AddColumn("AMOUNT");
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var date = reader.GetString(1);
            var amount = reader.GetInt32(2);
            table.AddRow(Convert.ToString(id), date, Convert.ToString(amount));
        }

        AnsiConsole.Write(table);
    }
}