using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efproto.Model
{
    public class Message
    {
        public Message()
        {
        }
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public Guid CreatorId { get; set; }

    }
}
