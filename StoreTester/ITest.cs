using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStoreIntegrationTester
{
    public interface ITest
    {
        bool Run();

        string Name { get; }
    }
    public interface ITest<T> : ITest where T : class
    {
    }
}
