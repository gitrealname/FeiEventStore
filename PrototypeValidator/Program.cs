namespace PrototypeValidator
{
    using System;
    using NLog;

    public interface IEvent
    {

    }

    public class EventX : IEvent
    {

    }

    public class EventY : IEvent { }

    public class Aggregate
    {
        public void Apply(EventX @event)
        {
            Console.WriteLine("EventX");
        }

        public void Apply(EventY @event)
        {
            Console.WriteLine("EventY");
        }

        public void Apply(object @event)
        {
            Console.WriteLine("Object");
        }
    }


    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Logger.Debug("Starting....");

            var x = new EventX();
            var y = new EventY();
            var o = new { a = 1 };

            var aggr = new Aggregate();

            IEvent e = x;
            aggr.Apply(e);

            e = y;
            aggr.Apply(e);

            aggr.Apply(o);
            Logger.Error("Done.");

        }
    }
}
