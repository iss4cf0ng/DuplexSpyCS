using System.Security.Policy;

namespace ChromeDumper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            clsDumper dumper = new clsDumper();
            var result = dumper.fnDumpHistory();

            foreach (var history in result)
            {
                if (history.URL.Contains("data"))
                    continue;

                Console.WriteLine(history.URL);
                Console.WriteLine(history.Title);
                Console.WriteLine("-------------------");
            }
        }
    }
}
