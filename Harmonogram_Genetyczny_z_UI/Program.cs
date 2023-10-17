using GAF;
using GAF.Operators;

namespace Harmonogram_Genetyczny
{
    class Program
    {
        // Definicja zadań
        // Job(Id_zadania, czas_wykonania_zadania,lista powiazan {id_zadań,
        // które muszą zostać wykonane przed wykonaniem zadania})
        // (w plikach txt)

        static void Main()
        {
            Main_screen();
        }


        static void Main_screen()
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Wybierz opcje:");
                Console.WriteLine("1. Wczytaj dane istniejace");
                Console.WriteLine("2. Stworz nowe dane");
                Console.WriteLine("3. Wyswietl obecne dane");
                Console.WriteLine("4. Stworz harmonogram na obecnych danych");
                Console.WriteLine("5. Wyjscie");

                Console.Write("Twój wybór: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Screens.Load_data_screen();
                        break;
                    case "2":
                        Screens.Create_new_data_screen();
                        break;
                    case "3":
                        Screens.Display_current_data_screen();
                        break;
                    case "4":
                        Screens.Create_schedule_screen();
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Nieprawidłowy wybór. Spróbuj ponownie.");
                        break;
                }
            }
        }
    }

    // klasa do roszerzen dzialan na listach
    public static class ListExtensions
    {
        private static readonly Random rng = new();

        // rozszerzenie do obiektu list, aby mozna bylo przetasowac elementy listy
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // zamiana dwoch elementow w liscie ze soba
        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

    }
}
