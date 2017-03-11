using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlFu.Builders;

namespace efproto
{
    public class MessageOwnerTemplate : IQueryTemplate<MessageOwner>
    {
        public string GetTemplate(ParametersManager paramManager)
        {
            var tmpl = @"select m.Subject, r.FirstName from Message m inner join Recipient r where m.CreatorId = r.Id";
            return tmpl;
        }
    }
}
