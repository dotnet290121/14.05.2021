using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitConsumerCore
{
    class Message
    {
        public string corr_id { get; set; }
        public string sp_name { get; set; }
        public string db_rows { get; set; }
    }
}
