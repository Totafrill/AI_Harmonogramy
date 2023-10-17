using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmonogram_Genetyczny
{
    internal class Scheduler
    {
        // zadania, liczba procesorow, kolejnosc zadan
        // do algorytmu genetycznego - wielkosc populacji i liczba generacji

        readonly List<Job> jobs;
        readonly int num_processors;
        readonly List<int> order_of_jobs = new();

        readonly int populationSize;
        readonly int generations;

        //konstruktor planisty
        public Scheduler(List<Job> jobs, int numProcessors, int population_size, int generations)
        {
            this.jobs = Schedule.Cloning_jobs(jobs);
            this.jobs.Shuffle();
            this.num_processors = numProcessors;
            this.populationSize = population_size;
            this.generations = generations;
        }

        // uruchomienie przydzielania zadan
        public void ScheduleTasks()
        {
            // tworzenie początkowej populacji losowych harmonogramów
            List<Schedule> population = GenerateInitialPopulation();

            for (int i = 0; i < generations; i++)
            {
                // wybor rodzicow na podstawie turnieju
                List<Schedule> parents = SelectParents(population);

                // krzyzowanie rodzicow
                List<Schedule> offspring = Crossover(parents);

                // mutacja
                Mutate(offspring);
                
                // budowa kazdego harmonogramu
                foreach (Schedule schedule in offspring) schedule.Add_jobs_to_processors();

                // zastapienie populacji potomkami - nowa generacja
                population = ReplacePopulation(population, offspring);
            }

            // wyswietlenie najlepszego harmonogramu z ostatniej generacji
            Schedule finalBestSchedule = GetBestSchedule(population);
            finalBestSchedule.Show_full_schedule();
        }
    
        // generowanie poczatkowej populacji
        List<Schedule> GenerateInitialPopulation()
        {
            List<Schedule> population = new();

            for (int i = 0; i < populationSize; i++)
            {
                // dla kazdego harmonogramu, losowana jest kolejnosc zadan
                Random_order_of_tasks();
                Schedule schedule = new(jobs, num_processors, order_of_jobs);
                schedule.Add_jobs_to_processors();
                population.Add(schedule);
            }

            return population;
        }

        // losowanie kolejnosci zadan
        void Random_order_of_tasks()
        {
            order_of_jobs.Clear();
            foreach (Job job in jobs) order_of_jobs.Add(jobs.FindIndex(a => a.Equals(job)));
            order_of_jobs.Shuffle();
        }

        List<Schedule> SelectParents(List<Schedule> population)
        {
            List<Schedule> parents = new();
            Random random = new();

            while (parents.Count < populationSize)
            {
                List<Schedule> tournament = new();

                while (tournament.Count < 5)
                {
                    Schedule candidate = population[random.Next(population.Count)];
                    tournament.Add(candidate);
                }

                Schedule selectedParent = tournament.OrderBy(schedule => schedule.CalculateFitness()).First();
                parents.Add(selectedParent);
            }

            return parents;
        }

        List<Schedule> Crossover(List<Schedule> parents)
        {
            List<Schedule> offspring = new();
            Random random = new();

            while (offspring.Count < populationSize)
            {
                Schedule parent1 = parents[random.Next(parents.Count)];
                Schedule parent2 = parents[random.Next(parents.Count)];

                Schedule child = parent1.Crossover(parent2, jobs);
                offspring.Add(child);
            }

            return offspring;
        }

        static void Mutate(List<Schedule> population)
        {
            Random random = new();

            foreach (Schedule schedule in population)
            {
                if (random.NextDouble() < 0.1)
                {
                    schedule.Mutate();
                }
            }
        }

        List<Schedule> ReplacePopulation(List<Schedule> population, List<Schedule> offspring)
        {
            List<Schedule> combinedPopulation = population.Concat(offspring).ToList();
            combinedPopulation = combinedPopulation.OrderBy(schedule => schedule.CalculateFitness()).ToList();

            return combinedPopulation.Take(populationSize).ToList();
        }

        static Schedule GetBestSchedule(List<Schedule> population)
        {
            return population.OrderBy(schedule => schedule.CalculateFitness()).First();
        }
    }
}
