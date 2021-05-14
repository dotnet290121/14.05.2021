using System;
using System.Threading.Tasks;

namespace MS_webapi_demi
{
    class Program
    {
        static void Main(string[] args)
        {

            var t1 =  Task.Run(() => {
                var flights = DbContext.ReadFromDb("sp_getflights"); 
            });
            var t2 = Task.Run(() => {
                var flights = DbContext.ReadFromDb("sp_getarilines");
            });
            var t3 = Task.Run(() => {
                var flights = DbContext.ReadFromDb("sp_gettickets");
            });
            var t4 = Task.Run(() => {
                var flights = DbContext.ReadFromDb("sp_getcountries");
            });
            var t5 = Task.Run(() => {
                var flights = DbContext.ReadFromDb("sp_getcustomers");
            });

            Task.WaitAll(new[] { t1, t2, t3, t4, t5 });

            //Console.ReadLine();
        }
    }
}
