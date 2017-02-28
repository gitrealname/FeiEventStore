namespace FeiEventStore.Domain
{
    using FeiEventStore.Core;

    /// <summary>
    /// Indicates which command can create new Aggregate instance.
    /// Every aggregate must have one or more ICreatedByCommand
    /// </summary>
    /// <typeparam name="TCommand">The type of the event.</typeparam>
    public interface ICreatedByCommand<in TCommand>
        where TCommand : ICommand
    {
    }
}
