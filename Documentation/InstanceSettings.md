# Instance Settings
RDMP can be installed so that multiple users are connecting to the same instance of the RDMP databases.
When this happens, it may be required that users share certain configuration settings.
RDMP's instance settings are to unify the user experience when operating as a multi-user instance.
>>>>>>> b4beb28cbb98d941d82a1255afd33738488e91e6
Instance settings apply to all users who connect to the RDMP database, where as User Settings are used on a per-user basis.

| Setting Name | Description |
| -------------| ----------- |
|AutoSuggestProjectNumbers | If True will automatically populate a new project number with an +1 increment from the largest existing project |
