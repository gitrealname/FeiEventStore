using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace FeiEventStore.Ioc
{
    public interface IIocRegistrar
    {
        void Register(Type serviceType, Type implementationType, IocRegistrationLifetime lifetime);
    }
}