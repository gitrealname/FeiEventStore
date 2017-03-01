# FeiEventStore

## Desing Notes and ideas

governance test cases shell enforce constraints listed below.

### Aggregate
* shell not handle event. only commands. (events should be handled by process managers)
* shell not emitt commands. only events.

### Process manager
* shell not handle commands
* shell emit commands only.
* must have final state after which process manager stops processing any events
* multiple process managers can spawned based on the same event (see IStartedByEvent)

### Domain Command Executor
* Coordinates in-domain commands execution
* tracks invloved aggregates, process managers and emmited events
* commit into event store once processing 

### Event

### SerializableType
* Events, Aggregates and Process manager must implement IPermanentlyTyped and IState

### Provision strategy

