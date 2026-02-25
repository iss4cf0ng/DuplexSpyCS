namespace ChromeDumper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ls = ChromeHistory.HistoryDumper();
            foreach (var entity in ls)
            {
                Console.WriteLine(entity._title);
                Console.WriteLine(entity._url);
                Console.ReadKey();
            }
        }
    }
}
