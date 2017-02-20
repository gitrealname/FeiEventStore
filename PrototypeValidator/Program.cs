using System.Linq;

namespace PrototypeValidator
{
    using System;
    using NLog;


    public interface inter1<T>
    {
        
    }

    public interface inter2<T>
    {
        
    }

    public class Entity : inter1<Entity>, inter2<Entity>
    {
    
    }

    


    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Logger.Debug("Starting....");

            var type = typeof(Entity);
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType)

                .First();
            //.Any(i => i.GetGenericTypeDefinition() == typeof(inter1<>));
            var interfaceArg = interfaces.GenericTypeArguments.First();
            var baseType = type.BaseType;

            Logger.Error("Done.");

        }
    }
}
