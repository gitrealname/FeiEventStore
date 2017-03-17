using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Domain
{
    public class InvalidDomainExecutionServiceExcepiton : Exception
    {
        public InvalidDomainExecutionServiceExcepiton() : base("IDomainExecutionScopeService must be of type: 'DomainExecutionScopeService'. Check IOC container configuration!!!")
        {
            
        }
    }
}
