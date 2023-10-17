
namespace Harmonogram_Genetyczny
{
    public class Processor
    {
        // lista id zadan skonczonych (dla wszystkich procesorow w ramach
        // tworzenia jednego harmonogramu)
        public static List<int> processor_list_finish_Id = new();

        // id procesora
        public int Id { get; set; } = 0;

        // czas na procesorze
        public int Time { get; set; } = 0;

        // zadania na procesorze
        public List<Job> jobs = new();

        // konstruktor procesora
        public Processor(int id)
        {
            Id = id;
        }

        // dodawanie zadania do procesora, uwzgledniajac czy to zadanie jest dodawane
        // do pustej kolejki procesora. Kazdy zadanie na jednym procesorze jest zwiazane
        // z poprzednim i nastepnym, oczywiscie jezeli jest poprzednie lub nastepne
        public void Add_Job(Job job)
        {
            if (jobs.Count > 0)
            {
                job.Previous_job = jobs.Last();
                jobs.Last().Next_job = job;
            }

            jobs.Add(job);
            // gdy zadanie jest dodane, jest oznaczone (dodane do listy) jako zakonczone,
            // czyli rozpatrzone w harmonogramie
            processor_list_finish_Id.Add(job.Id);
            // do czasu procesora, dodawany jest czas zadania
            Time += job.Duration;
        }

        // wyswietlanie informacji o procesorze
        public void Show_processor_details()
        {
            Console.WriteLine("---------------------------------------------------------------------------");
            Console.WriteLine("Processor " + Id + "    " + Show_processor_time());
            Console.WriteLine("---------------------------------------------------------------------------");
            foreach (Job job in jobs)
            {
                job.Show_job_details();
            }
            Console.WriteLine();
        }

        // wyswietlanie czasu pracy procesora
        public String Show_processor_time()
        {
            return "Czas pracy procesora: " + Time + " s    " + "Czas bezczynnosci procesora: " + Get_processor_idle_time() + " s";
        }

        // pobiera czas bezczynnosci procesora, czyli czas wszystkich przerw
        // pomiedzy zadaniami na procesorze
        public double Get_processor_idle_time()
        {
            double i = 0;

            foreach (Job job in jobs)
            {
                // jezeli Id == -1, oznacza to, ze zadanie jest tak naprawde przerwa,
                // pomiedzy zadaniami, poniewaz -1 jako niedostepne Id oznacza taka beczynnosc
                if (job.Id == -1) i += job.Duration;
            }

            return i;
        }
    }
}
