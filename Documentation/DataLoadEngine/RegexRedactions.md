# Regex Redactions

RDMP has the ability to redact incoming and existing catalogue data.
It uses user-defined regex to do this.
This functionality may be useful if you know about problematic data that you wish to clean before it reaches RDMP

## Setup & Configuration
To begin, a redaction configuration is required.
This can be added in the "Configurations" section of RDMP.
Each configuration has:
* A Name
* A Description
* A Regex string to match
* A Replacement value

N.B. The replacement value cannot be longer than the redacted string i.e. "string" => "myRedactedString" will not work.

Once the configuration has been saved, it can be used to redact data.

## Usage in Data Loads
Within the Data Loads mutilators, there is a mutilator named "Regex Redaction Mutilator" that can be used in the RAW or STAGING areas of the load.
This mutilator will redact based on your selected configuration.
The configuration for this mutilator requires:
* A predefined regex redaction configuration (see above)
* Either a regex to select which columns to redact, or a selection of known columns

N.B. Primary Key columns will not be redacted and this functionality is intended for string fields.

Once the configuration has been set, it will redact the desired columns during a data load.

During a run, the mutilator will replace any regex matches with the desired string and store the now known redaction in RDMP.
To view the redactions made, see the section below.


## Usage within RDMP
You can view and restore existing redactions, along with adding new redactions within RDMP.
For a Catalogue, right-click and open the "Catalogue Items" submenu. 
From here, select the "Regex Redactions" option.
This will open an interface displaying existing redactions.
From here you can restore existing redactions either individually or in bulk.
You can also apply regex redactions to existing data by selecting a redaction configuration and which columns to redact.


[Catalogue](../CodeTutorials/Glossary.md#Catalogue)
