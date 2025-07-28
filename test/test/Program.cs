using Oracle.ManagedDataAccess.Client;
using test.Models;
using test.Repositories;
using test.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test 
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Starting Film Showings and Seat Query Function Test ---");

            string connectionString = "Data Source=//8.148.76.54:1524/orclpdb1;User Id=cbc;Password=123456;"; // Your cbc user connection string

            // !!! CRITICAL CONFIGURATION: The actual schema name where your tables reside !!!
            string schemaName = ""; // <-- !!! YOU MUST CHANGE THIS !!!

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Connection string is empty. Please set the correct connection string in Program.cs!");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            // Initial database connection test
            Console.WriteLine("\n--- Initial Database Connection Test ---");
            using (OracleConnection testConnection = new OracleConnection(connectionString))
            {
                try
                {
                    testConnection.Open();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Database connection successful!");
                    Console.ResetColor();
                }
                catch (OracleException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Database connection failed: {ex.Message}");
                    Console.WriteLine($"Oracle Error Code: {ex.Number}");
                    Console.WriteLine("Please check: connection string, port, service name, username, password, and cloud server firewall/whitelist settings.");
                    Console.ResetColor();
                    Console.ReadKey();
                    return;
                }
            }

            // Instantiate Repositories and Services
            IFilmRepository filmRepository = new OracleFilmRepository(connectionString);
            IFilmService filmService = new FilmService(filmRepository);
            IShowingRepository showingRepository = new OracleShowingRepository(connectionString);
            IShowingService showingService = new ShowingService(showingRepository, filmRepository); // Pass filmRepository to ShowingService

            // --- Test retrieving all films (from previous step, useful for finding a test film) ---
            Console.WriteLine("\n--- Test Retrieving All Films (for reference) ---");
            List<Film> films = new List<Film>();
            try
            {
                films = filmService.GetAvailableFilms();
                if (films.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Successfully retrieved {films.Count} film(s):");
                    Console.ResetColor();
                    foreach (var film in films)
                    {
                        Console.WriteLine($"- {film.FilmName}");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No films found. Please ensure the FILM table has data.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to retrieve film list: {ex.Message}");
                Console.ResetColor();
            }

            // --- Test retrieving showtimes for a specific film ---
            Console.WriteLine("\n--- Test Retrieving Showtimes for a Specific Film ---");
            string testFilmName = "星际穿越"; // <-- 已替换为你的电影名称
            // You can also specify a date, e.g., DateTime.Today or DateTime.Parse("2025-07-28")
            DateTime? testDate = null; // Set to null to get all showtimes, or specify a date

            try
            {
                List<Section> sections = showingService.GetFilmShowings(testFilmName, testDate);
                if (sections.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Successfully retrieved {sections.Count} showtime(s) for '{testFilmName}':");
                    Console.ResetColor();
                    foreach (var section in sections)
                    {
                        Console.WriteLine($"- {section.ToString()}");
                        if (section.MovieHall != null)
                        {
                            Console.WriteLine($"  Hall: {section.MovieHall.HallNo} ({section.MovieHall.Category}, Capacity: {section.MovieHall.Lines * section.MovieHall.ColumnsCount})");
                        }
                        if (section.TimeSlot != null)
                        {
                            Console.WriteLine($"  Timeslot: {section.TimeSlot.StartTime:hh\\:mm}-{section.TimeSlot.EndTime:hh\\:mm}");
                        }

                        // --- Test getting available seats for this section ---
                        Console.WriteLine($"  --- Getting Available Seats for Section {section.SectionID} ---");
                        try
                        {
                            // !!! 修复点：直接传递 section 对象 !!!
                            Dictionary<string, List<string>> availableSeats = showingService.GetAvailableSeats(section);
                            if (availableSeats.Any())
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"  Available Seats for Section {section.SectionID}:");
                                Console.ResetColor();
                                foreach (var row in availableSeats.OrderBy(r => r.Key)) // Order by row number
                                {
                                    Console.WriteLine($"    Row {row.Key}: {string.Join(", ", row.Value)}");
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"  No available seats found for Section {section.SectionID}. All seats might be sold or hall layout is empty.");
                                Console.ResetColor();
                            }
                        }
                        catch (Exception seatEx)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"  Failed to get available seats for Section {section.SectionID}: {seatEx.Message}");
                            Console.ResetColor();
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"No showtimes found for '{testFilmName}'. Please ensure SECTION table has data for this film.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to retrieve film showtimes: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\n--- Film Showings and Seat Query Function Test Completed. Press any key to exit. ---");
            Console.ReadKey();
        }
    }
}
