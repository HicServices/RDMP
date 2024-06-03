# Cohort Versioning
Cohort Identitiy Configurations can be versioned, allowing you to store copies of cohort configurations prior to making updates.
you may want to use versioning instead of cloning the cohort to keep the tree list of cohort configurations simple, as versions do not appearin the traditional RDMP tree structure.

Cohort versioning is accessable via the Cohort creation page within RDMP, from here you can open existing versions of the cohort, or save the current configuration as a new version.

## Command Line
Versioning can be performed by using the CreateVersionOfCohortConfiguration command.
It can be ran with the following options 
```
CreateVersionOfCohortConfiguration cic:{some_id} name:{some_name}
```

Cohort versions can be used in the same way as a standard cohort identification configuration, their ID is accessible via the command
```
ListCohortVersions cic:{some_id}
```
This command will list the names and IDs of and versions associated with a cohort configuration

Within the CLI, cohort versions will appear alongside top-level cohort identity configurations when performing commands such as
```
List cic:*
```

## Cloning a cohort with Versions
Cloning a cohort configuration with versions does not clone the versions alongside the cohort configuration