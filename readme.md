# Orleans Design Patterns

* __[Reduce](Reduce.md)__ Provides a hierarchical structure to aggregate a value stored in many grains, which would unachievable with a fan-out.
* __[Smart Cache](Smart%20Cache.md)__ A performance optimization which uses Orleans as a distributed caching system in front of a storage system. Allows reads to be served from memory, and writes to be optionally buffered.
* __[Dispatcher](Dispatcher.md)__ A technique to send a batch of messages into Orleans in a single call, and have them distributed internally to the correct grains.
