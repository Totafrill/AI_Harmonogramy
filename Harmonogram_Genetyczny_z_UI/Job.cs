
namespace Harmonogram_Genetyczny
{
    public class Job : ICloneable
    {
        // referencja do poprzedniego zadania na przypisanym procesorze
        public Job? Previous_job { get; set; }

        // referencja do nastepnego zadania na przypisanym procesorze
        public Job? Next_job { get; set; }

        // id zadania (jeżeli id jest równe -1, oznacza to, że zadanie
        // oznacza stan bezczynnosci)
        public int Id { get; set; }

        // czas wykonania zadania
        public int Duration { get; set; } = -1;

        // zależności
        public List<int> Dependencies { get; set; }

        // kiedy zadanie się zaczeło
        public int Start => Previous_job != null ? Previous_job.End : 0;

        // kiedy zadanie się skończyło
        public int End => Start + Duration;

        // konstruktor zadania z id, czasem trwania, i zależnościami
        // (innymi zadaniami, ktore musza sie wykonac przed obecnym)
        public Job(int id, int duration, List<int> dependencies)
        {
            Id = id;
            Duration = duration;
            Dependencies = dependencies;
        }

        // wyswietlanie informacji o zadaniu
        public void Show_job_details()
        {
            Console.Out.WriteLine();
            if (Id == -1) Console.Out.WriteLine("Procesor bezczynny...");
            else Console.Out.WriteLine("Zadanie: " + Id);
            Console.Out.WriteLine("Czas trwania: " + Duration + " s");
            Console.Out.WriteLine("Start: " + Start + " s");
            Console.Out.WriteLine("Koniec: " + End + " s");
            Console.Out.WriteLine();
        }

        // klonowanie zadania
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
