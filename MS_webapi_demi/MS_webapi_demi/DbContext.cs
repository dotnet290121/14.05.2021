using System;
using System.Collections.Generic;
using System.Text;

namespace MS_webapi_demi
{
    // define interface

    public static class DbContext
    {
        public static string WriteToDb(string sp_name, params string[] args)
        {

            return Guid.NewGuid().ToString();
        }

        public static List<Dictionary<string, string>> ReadFromDb(string sp_name, params string[] args)
        {
            RabbitClient.StartRead(sp_name, args);
            return new List<Dictionary<string, string>>();
        }
    }
}
