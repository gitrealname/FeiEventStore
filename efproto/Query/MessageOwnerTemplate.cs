using SqlFu.Builders;

namespace efproto.Query
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
