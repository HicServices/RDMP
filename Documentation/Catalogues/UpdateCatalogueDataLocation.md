# Update A [Catalogue](../CodeTutorials/Glossary.md#Catalogue) Data Location
It can be useful to migrate data from one database to another.
Instead of reimporting a [catalogue](../CodeTutorials/Glossary.md#Catalogue), RDMP allows you to repoint the [catalogue](../CodeTutorials/Glossary.md#Catalogue) to the location of your moved data.

## How to
* Right Click an a Catalogue > Catalogue Items > Update Catalogue Data Location
* This will open a new dialog
* From here, you can select which columns you wish to update and tell RDMP where the new location is
* RDMP will perform several checks to make sure the new data location is available and of the correct types
* It will inform you of any issues it comes across, otherwise will migrate the catalogue data references to the selected location

## What Does it change?
This functionality updates the underlying [ColumnInfo](../CodeTutorials/Glossary.md#ColumnInfo) to point to the new table. It also updates the extractionInformation SelectSQL to allow for future extractions to continue to work.
If you select a never-before-seen table, RDMP will also generate a new known table record in case of future use

[Catalogue]: ../CodeTutorials/Glossary.md#Catalogue
[ColumnInfo]: ../CodeTutorials/Glossary.md#ColumnInfo
