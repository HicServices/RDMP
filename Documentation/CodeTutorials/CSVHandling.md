# Delimited File Handling (e.g. CSV)
## Table of contents
1. [Background](#background)
1. [Scalability](#scalability)
1. [Type Determination](#type-determination)
1. [Corrupt Files](#corrupt-files)
1. [Resolved Automatically](#resolved-automatically)
	* [Blank Lines](#blank-lines)
	* [Null Values](#null-values)
	* [Trailing Nulls](#trailing-nulls)
1. [Resolved According To Strategy](#resolved-according-to-strategy)
	* [Empty Files](#empty-files)
1. [Unresolveable](#unresolveable)

## Background
CSV stands for 'Comma Separated Values'.  A CSV file is created by writting a text document in which the cells of the table are separated by a comma.  Here is an example CSV file:

```
CHI,StudyID,Date
0101010101,5,2001-01-05
0101010102,6,2001-01-05
```

CSV files usually end in the extension `.csv`.  Sometimes an alternate separator will be used e.g. pipe `|` or tab `\t`.  There is an [official ruleset](https://tools.ietf.org/html/rfc4180) for writting CSV files, this covers escaping, newlines etc.  However this ruleset is often not correctly implemented by data suppliers.  RDMP therefore supports the loading of corrupt/invalid CSV files.

The class that handles processing delimited files (CSV, TSV etc) is `DelimitedFlatFileDataFlowSource`.  This class is responsible for turning the CSV file into a series of `System.DataTable` chunks for upload to the database.

## Scalability
CSV processing is done iteratively and streamed into the database in chunks.  This has been tested with datasets of 800 million records without issue.  Chunk size is determined by `MaxBatchSize`, optionally the initial batch can be larger `StronglyTypeInputBatchSize` to streamline [Type descisions](./TypeTranslation.md) e.g. when sending data to a `DataTableUploadDestination`. 

## Type Determination
Type decisions [are handled seperately](./TypeTranslation.md) after the `System.DataTable` has been produced in memory from the CSV file.

## Corrupt Files
RDMP is able to detect and cope with some common problems with delimited (e.g. CSV) files.  These situations can be classified as 'Resolved Automatically', 'Resolved Accordly' and 'Unresolveable'

## Resolved Automatically
The following potential problems are automatically resolved by RDMP when processing delimited files (e.g. CSV).

### Blank Lines
In a CSV file blank lines are ignored unless they appear within an escaped string.

```
Name,Dob
Frank,2001-01-01

Herbert,2002-01-01
```
*NewLineInFile_Ignored*


```
Name,Dob,Description
Frank,2001-01-01,"Frank is

the best ever"
Herbert,2002-01-01,Hey
```
*NewLineInFile_RespectedWhenQuoted*

### Null Values
There are many ways that a database null can be represented in a CSV file.  The most common are to skip the value (e.g. `Bob,,32`) or use the value null (e.g. `Bob,null,32`).  

Any cell value that is blank, whitespace or 'null' (ignoring capitalisation / trim) will be treated as `DBNull.Value` and enter the database as a null

The unit test for this behaviour is _NullCellValues_ToDbNull_

### Trailing nulls
Sometimes one or more rows of a CSV will have trailing commas (or null values).  This usually happens when making a change in Microsoft Excel and saving as CSV.  An example is shown below.  In such a case the trailing null values are ignored.

```
CHI ,StudyID,Date
0101010101,5,2001-01-05
0101010101,5,2001-01-05,,  ,null,
0101010101,5,2001-01-05
0101010101,5,2001-01-05
```
_TrailingNulls_InRows_


This also applies to trailing nulls in the header of the file
```
CHI ,StudyID,Date,,
0101010101,5,2001-01-05
0101010101,5,2001-01-05
0101010101,5,2001-01-05
0101010101,5,2001-01-05
```
_TrailingNulls_InHeader_

### Empty Files
The `ThrowOnEmptyFiles` setting determines behaviour if a file empty.  True will throw an Exception (error), False will skip the file.

A file is considered empty if it:

* Has no text at all (_EmptyFile_TotallyEmpty_)
* Has only whitespace (i.e. space, tab, return) in it (_EmptyFile_AllWhitespace_)
* Has a header record but is otherwise only whitespace _EmptyFile_HeaderOnly_


## Unresolveable
