# RDMP Test Plan
The RDMP test plan outlines the questions that should be asked of new changes to help ensure the correct changes are being made at the correct time.
These questions are designed to help highlight any steps in the testing process that may have been missed.
## Questions To Ask
### Does this change add a database migration?
If the change adds a database migration, ask yourself the following questions:
* Is the change backwards compatable?
* Have I tested the change with a fresh install of RDMP?
* Have I tested the change through the upgrade path with populated data that my change affects?
* Should this change be included in a patch release? or be part of the next minor release? Or even a major release?
    * Patch releases contain small improvements and should be usable interchangably with other versions within the same minor version e.g v8.1.4 and v8.1.5
    * Minor released contain new functionality and should be backwards compatable with other versions in the same major version e.g. v8.1.0 and v8.2.0
    * Major releases contain changes that are not backwards compatable

### Does this change add new functionality?
If the change adds new functionality, ask yourself the following questions:
* Is the functionality usable via the GUI? If no ,why not?
* Is the functionality usable via the CLI? If no, why not?
* Has the functionalty been covered via unit tests?
* Has the functionalty been manually tested?
    * Has the happy path been tested?
        * This is the expected path, where users are paying attention and are on their best behaviour
    * Has the sad path been tested?
        * This is where the user tries to be as obtuse as possible
* Does this change do any data processing? If so, check the performance questions below

### Does this change have any impact on performance?
If the change adds or amends functionality that processes data
* Is the functionality performant?
    * Can the space/time complexity of the functions be reduced? 
* Does the functionality handle large datasets (>1GB) efficiently?


### What assumptions have been made?
* Have any assumptions about how this functionality will be used been made?
* Have any assumptions about the input data been made? Can these assumptions be extracted out into configuration?