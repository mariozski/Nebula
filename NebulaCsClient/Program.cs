using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula;


namespace NebulaCsClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new EveApi(@"http://api.eveonline.com",
                                 new APIKey(3806756, "Pc2FF5YnjFuRfHfEhda956k14x698J0FWcoP74Xtwom3EjaASKqGYumY3HrXn0p4"),
                                 MemoryCache.Instance);

            var accountStatus = api.AccountStatus();
            Console.WriteLine("{0}", accountStatus.CreateDate);
            var apiKeyInfo = api.APIKeyInfo();
            Console.WriteLine("{0}", apiKeyInfo);
            
            Console.ReadKey();

        }
    }
}
