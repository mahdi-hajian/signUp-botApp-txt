using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.Models
{
    class ChatLog
    {
        public long ChatId { get; set; }
        public ChatState State { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public int Id { get; set; }
        public string Reshteh { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
