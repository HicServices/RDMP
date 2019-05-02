# Providers
This namespace deals with efficient discovery of all RDMP objects at once.  It aids in visualising them in user interface code e.g. in collections.

The namespace also includes problem provision (identifying easy to spot problems in objects such as missing fields).

## Nodes
Nodes are visual representations of object relationships or collections of objects.  Typically an `IChildProvider` returns a mixture of `DatabaseEntity` classes and nodes.