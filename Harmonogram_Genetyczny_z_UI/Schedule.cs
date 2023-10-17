using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmonogram_Genetyczny
{
    public class Schedule
    {
        // lista zadań właściwa (wzorzec) i zadania 
        // tymczasowe do pracy na nich + slownik
        // aby jako argument mogla byc pomieszana lista
        readonly List<Job> jobs = new();
        readonly Dictionary<int, int> jobs_hashtable = new();

        // lista zadań oczekujacych
        readonly List<Job> jobs_wait = new();

        // lista procesorów do pracy
        readonly List<Processor> processors;

        // liczba procesorow
        readonly int num_processors = 0;

        // kolejność zadań do dodania do procesorow
        public List<int> Order_of_jobs { get; set; }

        // okresla czy mozna miec przerwy bezczynnosci pomiedzy zadaniami
        readonly bool can_processors_be_idle = false;

        // konstruktor harmonogramu
        public Schedule(List<Job> jobs, int num_processors, List<int> order_of_jobs, bool can_processors_be_idle = true)
        {
            // zadania, losowanie kolejnosci zadan, dla lepszego
            // dzialania algorytmu, liczba procesorow
            // tablica hash, do sprawnego wyszukania elementów w pomieszanej
            // liscie
            this.jobs = Cloning_jobs(jobs);

            if (jobs_hashtable.Count == 0) for (int i = 0; i < jobs.Count; i++) jobs_hashtable.Add(jobs[i].Id, i);


            // liczba procesorow i ich tworzenie
            this.num_processors = num_processors;
            processors = Get_new_processors(num_processors);
            this.Order_of_jobs = new List<int>(order_of_jobs);
            this.can_processors_be_idle = can_processors_be_idle;
        }

        // dodanie zadan do procesorow, wedlug kolejnosci, zaleznosci, jezeli
        // nie mozna dodac obecnie, zadanie idzie na kolejke czekania
        public void Add_jobs_to_processors()
        {
            Processor.processor_list_finish_Id = new List<int>();
            Processor? processor;

            // pierwsze przejscie po zadaniach, ktore mozna dodac
            // (mozliwosc zakleszczenia przy zlych danych)
            foreach (int i in Order_of_jobs)
            {
                processor = Get_processor_with_dependencies(processors, jobs[i]);
                if (processor == null) jobs_wait.Add(jobs[i]);
                else processor.Add_Job(jobs[i]);
            }

            // przejscie po kolejce czekania, dopoki wszystkie zadania, nie beda dodane
            // do wykonania przez procesory (mozliwosc zakleszczenia przy zlych danych)
            // W GetProcessorWithDependencies ustawiany dodatkowy argument mówi (przy false), że
            // nie ma już zadan niezaleznych i kazdy procesor moze byc wprowadzony
            // w stan bezczynnosci na okreslony czas

            while (jobs_wait.Count > 0)
            {
                List<Job> tasksWait_temp = new();

                foreach (Job job in jobs_wait)
                {
                    processor = Get_processor_with_dependencies(processors, job, can_processors_be_idle);
                    if (processor == null) tasksWait_temp.Add(job);
                    else processor.Add_Job(job);
                }

                // czyszczenie kolejki czekania i tworzenie nowej z pozostalymi zadaniami
                // do przydzielenia do procesorow
                jobs_wait.Clear();
                jobs_wait.AddRange(tasksWait_temp);
            }
        }

        // metoda dajaca procesor, ktory moze obecnie miec przypisane zadanie,
        // ze wzgledu na czas wykonywanych zadan na procesorze jak i zaleznosci,
        // miedzy innymi zadaniami
        Processor? Get_processor_with_dependencies(List<Processor> processors, Job job, bool only_dependent_tasks = false)
        {
            // lista procesorow, ktore moga miec przypisane zadanie ze wzgledu na
            // wykonane zadania zalezne i ich czas zakonczenia

            List<Processor> processors_depend = new();
            List<Processor> processors_depend_least_loaded = new();
            int time = 0;

            foreach (int dependency in job.Dependencies)
            {
                // sprawdzanie, czy kazdy z zadan zaleznych jest juz wykonany,
                // w przeciwnym razie zwraca null i zadanie trafia do kolejki oczekujacej 
                if (!Processor.processor_list_finish_Id.Contains(dependency)) return null;

                // uzyskanie czasu najpozniej wykonanego zadania zaleznego, aby
                // mozna bylo wykonac zadanie (tablica haszujaca, aby mozna bylo
                // wydobyc dobre indeksy zadan z pomieszanej tablicy)
                int t = jobs[(int)jobs_hashtable[dependency]].End;
                if (t > time) time = t;
            }

            // porownywanie czasu i tworzenie listy procesorow, do ktorych moze byc
            // przypisane zadanie (jezeli nie ma zaleznosci, lista procesorow bedzie
            // zawierac wszystkie mozliwe procesory)
            foreach (Processor p in processors)
            {
                //jezeli najpozniej wykonane zadanie na danym procesorze jest zadaniem zaleznym
                if (p.Time == time) return p;

                if (only_dependent_tasks && p.Time < time) processors_depend_least_loaded.Add(p);

                if (p.Time > time) processors_depend.Add(p);
            }

            if (only_dependent_tasks && processors_depend_least_loaded.Count > 0) return GetProcessorAndAddEmptyJob(processors_depend_least_loaded, time);

            // zwracanie wzglednie najlepszego procesora dla listy dostepnych procesorow
            return GetProcessorWithMinLoad(processors_depend);
        }

        // metoda dajaca procesor z listy, ktory ma najmniejszy czas wykonywania
        static Processor GetProcessorWithMinLoad(List<Processor> processors)
        {
            int minLoad = int.MaxValue;
            Processor best_processor = processors[0];

            foreach (Processor processor in processors)
            {
                if (processor.Time < minLoad)
                {
                    minLoad = processor.Time;
                    best_processor = processor;
                }
            }

            return best_processor;
        }

        // metoda dajaca procesor z listy, do ktorego jest dodawane zadanie bezczynne
        // i wybierane jest procesor, przy ktorym dodanie zadania z zadaniem bezczynnym
        // zajmnie najmniej czasu
        static Processor GetProcessorAndAddEmptyJob(List<Processor> processors, int time)
        {
            int min_lost_time = int.MaxValue;
            Processor best_processor = processors[0];

            foreach (Processor processor in processors)
            {
                if (time - processor.Time < min_lost_time)
                {
                    min_lost_time = time - processor.Time;
                    best_processor = processor;
                }
            }

            Job Wait_job = new(-1, min_lost_time, new List<int>());
            best_processor.Add_Job(Wait_job);

            return best_processor;
        }

        // tworzenie nowych procesorow
        static List<Processor> Get_new_processors(int numProcessors)
        {
            Processor.processor_list_finish_Id = new();
            List<Processor> processors = new(numProcessors);

            for (int i = 0; i < numProcessors; i++)
            {
                // inicjacja procesorow i dodanie ich do listy
                Processor processor = new(i);
                processors.Add(processor);
            }

            return processors;
        }

        // klonowanie zadan do nowej listy (listy zadan do opracowania harmonogramu)
        public static List<Job> Cloning_jobs(List<Job> tasks)
        {
            List<Job> list = new();

            foreach (Job job in tasks)
            {
                Job job_copy = (Job)job.Clone();
                list.Add(job_copy);
            }
            return list;
        }

        // wyswietlanie harmonogramu ostatecznego rozwiazania, razem z czasem
        // wykonywania harmonogramu
        public void Show_full_schedule()
        {
            Show_schedule();
            Show_total_time();
        }

        // wyswietlanie harmonogramu
        void Show_schedule()
        {
            Console.WriteLine("###########################################################################\n");
            Console.WriteLine("Harmonogram\n");
            // wyswietlanie informacji o kazdym procesorze
            foreach (Processor processor in processors)
            {
                processor.Show_processor_details();
            }
            Console.WriteLine("###########################################################################\n");
        }

        // wyswietlanie czasu harmonogramu
        void Show_total_time()
        {
            Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");

            int i = Get_schedule_execution_time();
            double j = Get_average_processor_idle_time();

            Console.WriteLine("Czas harmonogramu: " + i + " sekund    " + "Sredni czas bezczynnosci procesora: " + j + " sekund");
            Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\n");
        }


        // czas wykonania harmonogramu, czyli czas pracy najdluzej pracujacego procesora
        public int Get_schedule_execution_time()
        {
            int maxTime = processors.Max(processor => processor.Time);
            return maxTime;
        }

        // sredni czas bezczynnosci procesora, czyli ile
        // czasu kazdy procesor srednio jest bezczynny
        public double Get_average_processor_idle_time()
        {
            double i = 0;
            foreach (Processor processor in processors)
            {
                i += processor.Get_processor_idle_time();
            }
            i /= processors.Count;
            return i;
        }

        //// Metody do algorytmu genetycznego


        // maksymalny mozliwy czas harmonogramu, czyli przypisanie wszystkich
        // zadan do jednego procesora
        int Get_max_possible_schedule_execution_time()
        {
            int maxTime = jobs.Max(task => task.Duration);
            return maxTime;
        }

        // oblicz fitness do oceny osobnika z populacji
        public double CalculateFitness()
        {
            int aktTime = Get_schedule_execution_time();
            int maxTime = Get_max_possible_schedule_execution_time();
            // czas fitness dostosowany do zwracania wartosci od 0 do 1
            double Fitness = (double)(aktTime) / (double)(maxTime);
            return Fitness;
        }

        // operacja krzyzowanie dwoch rozwiazan, poprzez polaczenie dwoch list kolejnosci
        // zadan z losowym punktem przeciecia
        public Schedule Crossover(Schedule other, List<Job> jobs)
        {
            // losowo wybierany punkt krzyzowania
            Random random = new();
            int crossoverPoint = random.Next(1, Order_of_jobs.Count - 1);
            List<int> new_order_of_jobs = new();

            // Kopiowanie czesci listy od rodzica 1
            for (int j = 0; j < crossoverPoint; j++)
            {
                new_order_of_jobs.Add(Order_of_jobs[j]);
            }

            // Kopiowanie czesci listy od rodzica 2, jezeli ich jeszcze nie ma
            for (int j = 0; j < crossoverPoint; j++)
            {
                if (!new_order_of_jobs.Contains(other.Order_of_jobs[j]))
                {
                    new_order_of_jobs.Add(other.Order_of_jobs[j]);
                }
            }

            // Kopiowanie drugiej czesci listy od rodzica 2, jezeli ich jeszcze nie ma
            for (int j = crossoverPoint; j < Order_of_jobs.Count; j++)
            {
                if (!new_order_of_jobs.Contains(other.Order_of_jobs[j]))
                {
                    new_order_of_jobs.Add(other.Order_of_jobs[j]);
                }
            }
            
            // tworzenie dziecka na podstawie nowej kolejnosci zadan i domyslnej listy
            // nieprzypisanych zadan

            Schedule child = new(jobs, num_processors, new_order_of_jobs);
            return child;
        }

        // operacja mutacji poprzez zamiane kolejnoscia dwoch zadan
        public void Mutate()
        {
            Random random = new();
            Order_of_jobs.Swap(random.Next(0, jobs.Count), random.Next(0, jobs.Count));
        }
    }
}
