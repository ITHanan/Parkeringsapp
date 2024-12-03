using Figgle;
using Spectre.Console;
using System.Text.Json;

class ParkingApp
{
    static string filePath = "parkingData.json";

    static Dictionary<int, ParkingSession> activeParkings = new Dictionary<int, ParkingSession>();
    static List<ParkingSession> parkingHistory = new List<ParkingSession>();
    static decimal hourlyRate = 20m; // Timtaxa (kr/timme)

    static void Main()
    {

        LoadData(); // Ladda data från JSON-fil vid start

        while (true)
        {
            Console.Clear();

            // ASCII Art for Header
            Console.WriteLine(FiggleFonts.Standard.Render("Parking App"));

            // Styled Menu using Spectre.Console
            AnsiConsole.Markup("[bold yellow]Choose an option:[/]\n");
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]What would you like to do?[/]")
                    .PageSize(4)
                    .AddChoices(new[]
                    {
                        "1. Start Parking",
                        "2. Stop Parking",
                        "3. Show History",
                        "4. Exit"
                    }));

            switch (choice[0]) // Extract the number from the choice (e.g., "1. Start Parking")
            {
                case '1':
                    StartParking();
                    break;
                case '2':
                    StopParking();
                    break;
                case '3':
                    ShowHistory();
                    break;
                case '4':
                    SaveData();
                    AnsiConsole.Markup("[bold green]Thank you for using the Parking App![/]\n");
                    return;
                default:
                    AnsiConsole.Markup("[bold red]Invalid choice, please try again.[/]\n");
                    break;
            }

            // Wait for user to press a key before returning to menu
            AnsiConsole.Markup("[grey italic](Press any key to continue...)[/]");
            Console.ReadKey();
        }
    }

    static void StartParking()
    {
        Console.Clear();
        AnsiConsole.Markup("[bold green]Start Parking[/]\n");
        AnsiConsole.Markup("[bold yellow]Please enter the parking Location:[/] ");



        string location = Console.ReadLine()!;

        int id = new Random().Next(1000, 9999); // Generera ett unikt ID
        ParkingSession session = new ParkingSession(id, location);

        activeParkings[id] = session; // Lägg till den i aktiva parkeringar
        AnsiConsole.Markup($"[bold yellow]Parking started with the ID {id} in  {location} on  {session.StartTime}.![/]\n");


        SaveData();



    }

    static void StopParking()
    {
        Console.Clear();

        // Figgle ASCII header
        Console.WriteLine(FiggleFonts.Standard.Render("Stop Parking"));

        // Displaying the action message with styling
        AnsiConsole.Markup("[bold red]Stop Parking[/]\n");

        // Show active parkings
        ActiveParkings();

        // Prompt for Parking ID
        AnsiConsole.Markup("[bold cyan]Enter Parking ID:[/]");
        int id = int.Parse(Console.ReadLine()!);

        if (activeParkings.TryGetValue(id, out ParkingSession session))
        {
            // Set the end time and calculate the cost
            session.EndTime = DateTime.Now;
            session.Cost = CalculateCost(session.StartTime, session.EndTime.Value);

            // Move the session to history and remove it from active parkings
            activeParkings.Remove(id); // Remove from active parkings
            parkingHistory.Add(session); // Add to history

            // Provide feedback on successful parking stop
            AnsiConsole.Markup($"[bold yellow]Parking completed. Cost: {session.Cost:F2} SEK. Duration: {(session.EndTime.Value - session.StartTime).TotalMinutes:F1} minutes.[/]\n");
            AnsiConsole.Markup("[bold yellow]Parking stopped![/]\n");
        }
        else
        {
            // Provide feedback when the parking ID is not found
            AnsiConsole.Markup("[bold red]Parking with that ID not found.[/]\n");
        }

        // Save the updated data
        SaveData();
    }


    static void ActiveParkings()
    {
        Console.Clear();

        // Figgle ASCII header
        Console.WriteLine(FiggleFonts.Standard.Render("Active Parkings"));

        // Check if there are active parking sessions
        if (activeParkings == null || activeParkings.Count == 0)
        {
            AnsiConsole.Markup("[bold red]No active parking sessions found![/]\n");
            return;
        }

        // Create a table to display active parking sessions
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[bold cyan]ID[/]");
        table.AddColumn("[bold cyan]Location[/]");
        table.AddColumn("[bold cyan]Start Time[/]");

        // Add rows for each active session
        foreach (var session in activeParkings.Values)
        {
            table.AddRow(
                session.Id.ToString(),
                session.Location,
                session.StartTime.ToString("yyyy-MM-dd HH:mm")
            );
        }

        // Render the table
        AnsiConsole.Write(table);

    }
    static void ShowHistory()
    {
        Console.Clear();

        // Display ASCII header
        Console.WriteLine(FiggleFonts.Standard.Render("Parking History"));

        // Check if there is any history
        if (parkingHistory.Count == 0)
        {
            AnsiConsole.Markup("[bold red]No parking history available![/]\n");
            return;
        }

        // Create a Spectre Console Table
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[bold cyan]ID[/]");
        table.AddColumn("[bold cyan]Location[/]");
        table.AddColumn("[bold cyan]Start Time[/]");
        table.AddColumn("[bold cyan]End Time[/]");
        table.AddColumn("[bold cyan]Cost (SEK)[/]");

        // Populate the table with parking history
        foreach (var session in parkingHistory)
        {
            table.AddRow(
                session.Id.ToString(),
                session.Location,
                session.StartTime.ToString("yyyy-MM-dd HH:mm"),
                session.EndTime.HasValue
              ? session.EndTime.Value.ToString("yyyy-MM-dd HH:mm"):"N/A",
                $"{session.Cost:F2}"
            );
        }

        // Render the table
        AnsiConsole.Write(table);
    }


   


    static decimal CalculateCost(DateTime start, DateTime end)
    {
        TimeSpan duration = end - start;
        double hours = duration.TotalHours;
        return (decimal)hours * hourlyRate;

    }


    public static void SaveData()
    {
        Console.Clear();
        AnsiConsole.Markup("[bold green]Saving data...[/]\n");
        try
        {
            var data = new
            {
                ActiveParkings = activeParkings,
                ParkingHistory = parkingHistory
            };

            string dataJsonFilePath = "parkingData.json";

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dataJsonFilePath, json);

            MirrorChangesToProjectRoot("parkingData.json");


            AnsiConsole.Markup("[bold yellow]Data saved![/]\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with the saving of the data : {ex.Message}");
        }

    }

    static void LoadData()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<ParkingData>(json);

                if (data != null)
                {
                    activeParkings = data.ActiveParkings;
                    parkingHistory = data.ParkingHistory;
                    Console.WriteLine("Data laddad från fil.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid laddning av data: {ex.Message}");
        }
    }

    static void MirrorChangesToProjectRoot(string fileName)
    {
        // Get the path to the output directory
        string outputDir = AppDomain.CurrentDomain.BaseDirectory;

        // Get the path to the project root directory
        string projectRootDir = Path.Combine(outputDir, "../../../");

        // Define paths for the source (output directory) and destination (project root)
        string sourceFilePath = Path.Combine(outputDir, fileName);
        string destFilePath = Path.Combine(projectRootDir, fileName);

        // Copy the file if it exists
        if (File.Exists(sourceFilePath))
        {
            File.Copy(sourceFilePath, destFilePath, true); // true to overwrite
            Console.WriteLine($"{fileName} has been mirrored to the project root.");
        }
        else
        {
            Console.WriteLine($"Source file {fileName} not found.");
        }
    }

}
class ParkingSession
{
    public int Id { get; set; } // Unikt ID för parkeringen
    public DateTime StartTime { get; set; } // När parkeringen startades
    public DateTime? EndTime { get; set; } // När parkeringen slutade (null om aktiv)
    public string Location { get; set; } // Platsen för parkeringen
    public decimal Cost { get; set; } // Beräknad kostnad

    public ParkingSession(int id, string location)
    {
        Id = id;
        StartTime = DateTime.Now;
        Location = location;
        EndTime = null;
        Cost = 0;
    }



}


class ParkingData
{
    public Dictionary<int, ParkingSession> ActiveParkings { get; set; }
    public List<ParkingSession> ParkingHistory { get; set; }
}
