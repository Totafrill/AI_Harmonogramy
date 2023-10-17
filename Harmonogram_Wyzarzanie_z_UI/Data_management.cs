using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Harmonogram_Wyzarzanie
{
    public static class Data_management
    {
        static int num_processors = 0; // liczba procesorów

        // zmienne do algorytmu symulowanego wyzarzania
        public static List<Job> jobs = new();

        public static bool not_empty_data = jobs.Count > 0 ? true : false;

        // Harmonogram, konstruktor (zadania, liczba dostępnych procesorów)
        // Scheduler scheduler = new(jobs, num_processors, temperature, cooling_rate);

        // Uloz harmonogram, za pomocą algorytmu symulowanego wyżażania
        // scheduler.ScheduleTasks();


        public static void Create_data()
        {
            Console.Clear();
            bool is_ok = false;

            while (!is_ok)
            {
                Console.WriteLine("Podaj liczbe procesorow: ");
                string input = Console.ReadLine();
                bool is_valid_n = int.TryParse(input, out num_processors) && num_processors > 0;

                if (is_valid_n) is_ok = true;
                else
                {
                    Console.WriteLine("Nieprawidlowa ilośc procesorow.");
                    Console.WriteLine("Nacisnij klawisz, aby sprobowac ponownie");
                    Console.ReadLine();
                }
            }

            
            bool enough_tasks = false;
            
            while (!enough_tasks)
            {
                while (true)
                {
                    bool is_valid_n_id = true;
                    bool is_valid_n_duration = true;
                    bool is_valid_n_dependency = true;
                    int id = 0;
                    int duration = 0;
                    List<int> dependencies = new();

                    Console.Clear();
                    Console.WriteLine();
                    Console.WriteLine();
                    Display_current_data();


                    Console.WriteLine("Podaj zadanie w formacie (id czas_trwania zaleznosci).\n" +
                        "Jezeli nie ma zaleznosci, zostawic puste." + "    Przyklad: 0 12 2,4,5,6\n");

                    string input = Console.ReadLine();
                    string[] parts = input.Split(' ');

                    if (parts.Count() >= 2)
                    {
                        is_valid_n_id = int.TryParse(parts[0], out id) && id >= 0 && jobs.All(job => job.Id != id);
                        if (is_valid_n_id == false)
                        {
                            Console.WriteLine("Podane Id zadania jest nieprawidlowe albo sie powtarza.");
                            Console.WriteLine("Nacisnij klawisz, aby sprobowac ponownie");
                            Console.ReadLine();
                            continue;
                        }

                        is_valid_n_duration = int.TryParse(parts[1], out duration) && duration > 0;
                        if (is_valid_n_duration == false)
                        {
                            Console.WriteLine("Podany czas_trwania zadania jest nieprawidlowy.");
                            Console.WriteLine("Nacisnij klawisz, aby sprobowac ponownie");
                            Console.ReadLine();
                            continue;
                        }

                        if (parts.Count() == 3)
                        {
                            string[] part3 = parts[2].Split(',');
                            int depend_id;
                            dependencies.Clear();
                            foreach (string part in part3)
                            {
                                is_valid_n_dependency = int.TryParse(part, out depend_id) && depend_id != id && !jobs.Any(job => job.Id == depend_id && job.Dependencies.Contains(id));
                                if (is_valid_n_dependency == false)
                                {
                                    Console.WriteLine("Podane zaleznosci sa nieprawidlowe (mozliwe też przez mozliwosc zakleszczenia)");
                                    Console.WriteLine("Nacisnij klawisz, aby sprobowac ponownie");
                                    Console.ReadLine();
                                    
                                }
                                dependencies.Add(depend_id);
                            }
                            if (is_valid_n_dependency == false) continue;
                        }

                        Job job = new Job(id, duration, dependencies);
                        jobs.Add(job);
                        enough_tasks = true;

                        enough_tasks = jobs.All(job => job.Dependencies.All(d => jobs.Any(j => j.Id == d)));
                        if (enough_tasks == false)
                        {
                            Console.WriteLine("\nDodano zadanie" + id);
                            Console.WriteLine("Brakuje kilku zadan, od ktorych zaleza inne zadania. Aby stworzono dane, trzeba je podac");
                            Console.WriteLine("Nacisnij klawisz, aby kontynuowac");
                            Console.ReadLine();
                            continue;
                        }

                        is_ok = false;
                        while (!is_ok)
                        {
                            Console.WriteLine("\nDodano zadanie " + id);
                            Console.WriteLine("Chcesz dodac nowe zadania?");
                            Console.WriteLine("Wpisz 1 jezeli tak, 2 jezeli nie");

                            string choice = Console.ReadLine();

                            switch (choice)
                            {
                                case "1":
                                    is_ok = true;
                                    enough_tasks = false;
                                    Console.Clear();
                                    break;
                                case "2":
                                    is_ok = true;
                                    Console.Clear();
                                    break;
                                default:
                                    Console.WriteLine("Nieprawidlowy wybor. Sprobuj ponownie.");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nieprawidlowa ilosc argumentow. Sprobuj ponownie.");
                        continue;
                    }
                    break;
                }
            }

            Save_data_to_file();
        }

        public static void Save_data_to_file()
        {
            Console.WriteLine("Procedura zapisu do pliku ");
            Console.WriteLine("Podaj nazwe pliku z rozszerzeniem: ");
            string filePath = Console.ReadLine();

            // Zapis danych do pliku
            using (StreamWriter writer = new StreamWriter(@".\pliki\" + filePath))
            {
                writer.WriteLine("Procesors_number: " + num_processors);
                writer.WriteLine("Jobs_number: " + jobs.Count);

                foreach (Job job in jobs)
                {
                    writer.Write("Job: " + job.Id + " " + job.Duration);
                    foreach (int d in job.Dependencies)
                        writer.Write(" " + d);
                    writer.WriteLine();
                }
            }

            Console.WriteLine("Dane zostaly utworzone i zapisane do pliku.");
            Console.WriteLine("Nacisnij klawisz, aby kontynuowac");
            Console.ReadLine();
        }

        public static void Display_current_data()
        {
            Console.WriteLine("Ilosc procesorow: " + num_processors);
            Console.WriteLine("Ilosc obecnych zadan: " + jobs.Count());

            foreach (Job job in jobs)
            {
                Console.Write("Zadanie: " + job.Id + " Czas wykonywania: " + job.Duration + " Zaleznosci: ");
                int i = 1;
                foreach (int d in job.Dependencies)
                {
                    Console.Write(d);
                    if (i++ != job.Dependencies.Count()) Console.Write(", ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        public static void Load_data_from_file(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                int task_num;
                string line = reader.ReadLine();
                string[] split = line.Split(' ');
                int.TryParse(split[1], out num_processors);
                line = reader.ReadLine();
                split = line.Split(' ');
                int.TryParse(split[1], out task_num);
                jobs.Clear();

                for(int i = 0; i < task_num; i++)
                {
                    int id;
                    int duration;
                    List<int> dependencies = new();

                    line = reader.ReadLine();
                    split = line.Split(' ');
                    int.TryParse(split[1], out id);
                    int.TryParse(split[2], out duration);

                    for(int j = 3; j < split.Length; j++)
                    {
                        int d;
                        int.TryParse(split[j], out d);
                        dependencies.Add(d);
                    }

                    Job job = new Job(id,duration,dependencies);
                    jobs.Add(job);
                }
            }
        }

        public static void Create_scheduler(double temperature, double cooling_rate)
        {
            Console.Clear();
            Scheduler scheduler = new(jobs, num_processors, temperature, cooling_rate);
            scheduler.ScheduleTasks();
        }

        public static void Clear()
        {
            num_processors = 0;
            jobs.Clear();
        }
    }
}
