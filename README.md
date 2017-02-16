# PDEventStore

## Desing Notes and ideas

governance test cases shell enforce constraints listed below.

### Aggregate
* shell not handle event. only commands. (events should be handled by process managers)
* shell not emit commands. only events.

### Process manager
* shell not handle commands
* shell emit commands only.
* must have final state after which process manager stops processing any events
* multiple process managers can spawned based on the same event

### Event

### SerializableType
* Events, Aggregates and Process manager must implement ISerializableType
* consider to use attribute to associate strong type id with the concrete type
* consider to use attribute on Events and Aggregates to define that given type is a provision of 'retired' type.

### Provision strategy

#### Event and Aggregate
1) new event/aggregate type is implemented with IProvision<T> where T is type of 'retired' event
2) new type may need to be marked with attribute to tell the system 'retired' type id 
3) old type is moved into 'retired' folder but never deleted.
4) deletion of retired type can be done after upgrade/full-replay of the event store against new types

#### Process managers
1) IStartedBy interfaces removed from the implementation, to prevent creation of new instances
2) once all given process managers are complete, process manager type can be deleted.

#Notes

ReviseMessageBody(string, ISpellCheckerService, IMessage)

IResponse {
	Info(string)
	Warning(string)
	Error(string)
	EntityVersion(id, version)
	SetCommitId()
	//exceptions
	AccessDenied(string)
	NotFound(string)
	Exception(string)
	//Payload
	object Result {get;set;}
}

IExecutionContext {
	Guid OnBehalfOf {get} //returns user/sub-systemId
	Bool EmmitedBySystem 
}

ISessionCommandProcessor {
	IResponse publish(IMessage)
}

ICommandProcessor {
	async IResponse publishBatch(List<IMessage>)
}

IMessage {
	Guid? TargetProcessId,
}

ICommand : IMessage {
	Guid TargetAggregateId,
	long TargetAggregateVersion,
}

IEvent : IMessage, IPermanentTyped {
	Guid AggregateId, //issuer aggregate id
	int Version,
}

//TODO: re-think query
IQueryConstraint {
	Guid? CommitConstraint;
	Tuple<Guid,long> AggregateConstrain;
}

IQueryExecutor { ???? how to 
	async IResponse Execute<TQuery, TParam>()
}
