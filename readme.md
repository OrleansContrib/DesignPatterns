# Orleans Design Patterns

* __[Reduce](Reduce.md)__ Provides a hierarchical structure to aggregate a value stored in many grains, which would unachievable with a fan-out.
* __[Smart Cache](Smart%20Cache.md)__ A performance optimization which uses Orleans as a distributed caching system in front of a storage system. Allows reads to be served from memory, and writes to be optionally buffered.
* __[Dispatcher](Dispatcher.md)__ A technique to send a batch of messages into Orleans in a single call, and have them distributed internally to the correct grains.
* __[Observer](Observer.md)__ Allows an observer to be notified of any state changes that a grain can choose to publish.

# Approximate Performance Expectations

Using X-Large VMs (8 CPU Cores / 14 GB RAM) on Microsoft Azure, with one silo per VM:

* A grain will handle a maximum of 1,000 requests per second.
* A silo will handle a maximum of 10,000 requests per second.
* A silo will hold 100,000 active grains.
