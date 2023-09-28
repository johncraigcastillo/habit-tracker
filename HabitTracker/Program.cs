using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace HabitTracker;

public class Program
{
  static void Main(string[] args)
  {
    SQLitePCL.Batteries.Init();
    const string connectionString = @"Data Source=habit-tracker.db";

    using (var connection = new SqliteConnection(connectionString))
    {
      connection.Open();
      var tableCommand = connection.CreateCommand();
      tableCommand.CommandText =
          """
                CREATE TABLE IF NOT EXISTS drinking_water (
                    id INTEGER PRIMARY KEY,
                    date TEXT NOT NULL,
                    amount INTEGER NOT NULL
                    )
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
          // Update();
          break;
        case "Exit":
          closeApp = true;
          break;
      }
    }
  }

  private static void Delete()
  {
    AnsiConsole.MarkupLine("Not Yet Implemented!");
  }

  private static void Insert()
  {
    AnsiConsole.Clear();
    AnsiConsole.MarkupLine("\n[blue bold]INSERT RECORD[/]");
    var date = AnsiConsole.Ask<string>("Enter the date (YYYY-MM-DD): ");
    var amount = AnsiConsole.Ask<int>("Enter the amount (in oz): ");

    const string connectionString = @"Data Source=habit-tracker.db";
    using (var connection = new SqliteConnection(connectionString))
    {
      connection.Open();
      var insertCommand = connection.CreateCommand();
      insertCommand.CommandText =
          $@"
                INSERT INTO drinking_water (date, amount)
                VALUES ('{date}', {amount})
                ";
      insertCommand.ExecuteNonQuery();
      connection.Close();
    }

    AnsiConsole.MarkupLine("\n[green]Record inserted successfully![/]");
    AnsiConsole.MarkupLine("\n[green]Press any key to return to the main menu.[/]");
    Console.ReadKey();
  }

  private static void GetRecords()
  {
    AnsiConsole.Clear();
    AnsiConsole.MarkupLine("\n[blue bold]VIEW RECORDS[/]");
    const string connectionString = @"Data Source=habit-tracker.db";
    using (var connection = new SqliteConnection(connectionString))
    {
      connection.Open();
      var selectCommand = connection.CreateCommand();
      selectCommand.CommandText =
          """
                SELECT * FROM drinking_water
                """;
      var reader = selectCommand.ExecuteReader();
      while (reader.Read())
      {
        var id = reader.GetInt32(0);
        var date = reader.GetString(1);
        var amount = reader.GetInt32(2);
        AnsiConsole.MarkupLine($"[bold]ID:[/] {id} [bold]DATE:[/] {date} [bold]AMOUNT:[/] {amount}");
      }

      connection.Close();
    }

    AnsiConsole.MarkupLine("\n[green]Press any key to return to the main menu.[/]");
    Console.ReadKey();
  }
}
