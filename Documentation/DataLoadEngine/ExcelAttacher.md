## Excel Attacher
The Excel attacher is a convenient way to load your data from .xlsx files into RDMP.
It works out of the box with a single-sheet workbook.

### Simple Example
For example, the following Excel sheet would be transformed into a single database table with columns "COLUMN_1" and "COLUMN_2"
With a single entry ("some data","some other data").

|   | A         | B               |
|---|-----------|-----------------|
| 1 | COLUMN_1  | COLUMN_2        |
| 2 | some data | some other data |


## Configurable options
* Work Sheet Name - the name of the sheet you wish to import, only required if your Excel workbook has multiple sheets
* Add Filename Column Named - (Optional) If you want to store the source file location in the database, add the name of a new column to store this information in
* Force Replacement Headers - A comma separated list of headers to replace those found in your sheet
* Allow Extra Columns In Target Without Complaining of Column Mismatch - Allows for missing columns in the source file
* File Pattern - the file you wish to load
* Table To Load / Table Name - Which Table you wish to populate
* Send Load Not Required If File Not Found - If there is no file, we can skip the load
* Delay Load Failures - Wait till load complete to fail about missing files
* Culture / Explicit Date Time Format - Set the time format  of your data
* Row Offset - If your data doesn't start on the first row, set the Row Offset to the row that contains your data's headers. The first row is 1.
* Column Offset - If your data doesn't start on the first column, set the Column Offset to the column that contains your data's headers. The first Column is 'A' or '0'.


