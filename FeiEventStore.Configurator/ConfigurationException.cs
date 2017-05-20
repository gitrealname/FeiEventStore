using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeiEventStore.Configurator
{
    public class ConfigurationException : Exception
    {
        public List<string> ConfigurationErrors { get; private set; }

        public ConfigurationException(string message) : base(message)
        {
            ConfigurationErrors = new List<string>();
            ConfigurationErrors.Add(message);
        }

        public ConfigurationException(ICollection<string> messages ) : base(string.Join("; ", messages))
        {
            ConfigurationErrors = new List<string>();
            ConfigurationErrors.AddRange(messages);
        }
    }
}
