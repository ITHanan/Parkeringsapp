using System;
using System.Collections.Generic;

class ParkingApp
{
    static Dictionary<int, ParkingSession> activeParkings = new Dictionary<int, ParkingSession>();
    static List<ParkingSession> parkingHistory = new List<ParkingSession>();
    static decimal hourlyRate = 20m; // Timtaxa (kr/timme)

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\nParkeringsapp");
            Console.WriteLine("1. Starta parkering");
            Console.WriteLine("2. Avbryt parkering");
            Console.WriteLine("3. Visa historik");
            Console.WriteLine("4. Avsluta");

            Console.Write("Välj ett alternativ: ");
            string choice = Console.ReadLine()!;

            switch (choice)
            {
                case "1":
                    StartParking();
                    break;
                case "2":
                    StopParking();
                    break;
                case "3":
                    ShowHistory();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Ogiltigt val, försök igen.");
                    break;
            }
        }
    }

    static void StartParking()
    {
        Console.Write("Ange parkeringsplats: ");
        string location = Console.ReadLine()!;

        int id = new Random().Next(1000, 9999); // Generera ett unikt ID
        ParkingSession session = new ParkingSession(id, location);

        activeParkings[id] = session; // Lägg till den i aktiva parkeringar
        Console.WriteLine($"Parkering startad med ID {id} på plats {location} vid {session.StartTime}.");
    }

    static void StopParking()
    {
        Console.Write("Ange parkerings-ID: ");
        int id = int.Parse(Console.ReadLine()!);

        if (activeParkings.TryGetValue(id, out ParkingSession session))
        {
            session.EndTime = DateTime.Now;
            session.Cost = CalculateCost(session.StartTime, session.EndTime.Value);

            activeParkings.Remove(id); // Ta bort från aktiva parkeringar
            parkingHistory.Add(session); // Flytta till historiken

            Console.WriteLine($"Parkering avslutad. Kostnad: {session.Cost} kr. Varaktighet: {(session.EndTime.Value - session.StartTime).TotalMinutes:F1} minuter.");
        }
        else
        {
            Console.WriteLine("Parkering med det ID:t hittades inte.");
        }
    }

    static void ShowHistory()
    {
        Console.WriteLine("\nParkeringshistorik:");
        foreach (var session in parkingHistory)
        {
            Console.WriteLine($"ID: {session.Id}, Plats: {session.Location}, Start: {session.StartTime}, Slut: {session.EndTime}, Kostnad: {session.Cost} kr");
        }
    }

    static decimal CalculateCost(DateTime start, DateTime end)
    {
        TimeSpan duration = end - start;
        double hours = duration.TotalHours;
        return (decimal)hours * hourlyRate;
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
