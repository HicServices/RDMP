# DataTypeComputer
## Background
Relational databases require typed columns (`int`,`datetime`, `varchar(10)` etc).  Often when dealing with researcher datasets we are provided with untyped raw data (e.g. CSV).  The DataTypeComputer is responsible for picking a suitable `System.Type` based on arbitrary supplied objects as well as determining the number of decimal places and maximum lengths for strings.  A single DataTypeComputer expects to handle data only from a single column (or other logical source).  The DataTypeComputer does not itself generate Sql types, that is done by an `ITypeTranslater` via the interchange class `DataTypeRequest`.

DataTypeComputer deals with adjusting the estimate appropriately over time e.g. passing the value "-1000" might result in an estimate of `System.Int` but then passing "1.5" should result in a column estimate of `decimal(5,1)` (long enough to store both -1000 and 1.1).  If we then pass "A" then we have to change to `System.String` but the length will need to be 5 (long enough to store "-1000").  See test `TestDataTypeComputer_IntFloatString`.

## Fitting Strategy

### String Fitting
When trying to identify a `System.Type` suitable for an input string value (e.g. "2001-01-01"), only those in `DatabaseTypeRequest.PreferenceOrder` are considered.  The first input string is evaluated against each `System.Type` in preference order until a compatible `System.Type` is found (See `IsAcceptableAs`).  New strings are then evaluated against the `CurrentEstimate`, it is not possible to go back up the preference order.  `IsAcceptableAs` also computes the decimal places and length of string required to store the input if we have to fallback to `System.String`.

### Zero Prefixes
If an input string is a number that starts with zero e.g. "01" then the estimate will be changed to `System.String`.  This is intended behaviour since some codes e.g. CHI / Barcodes have valid zero prefixes.  If this is to be accurately preserved in the database then it must be stored as string (See `TestDatatypeComputer_PreeceedingZeroes`).  This also applies to values such as "-01"

### Whitespace
Leading and trailing whitespace is ignored for the purposes of determining Type.  E.g. " 0.1" is a valid `System.Decimal`.  However it is recorded for the maximum Length required if we later fallback to `System.String` (See Test `TestDatatypeComputer_Whitespace`).

### Fallback Strategy (Over Time)
DataTypeComputer will handle changes in `CurrentEstimate` over time based on new input values.  For example if you pass "1" (a valid `System.Int`) then "1.1" the estimate will fallback to `System.Decimal`.  This is done by recording the novel `System.Type` estimates of all previously seen objects in the column (See `_validTypesSeen`).  When updating the `CurrentEstimate` the Types of previous estimates are considered for compatibility e.g. it will be happy to go from `System.Int` to `System.Decimal` because you can store integers in a `System.Decimal`.  The same would not be true if after passing "1.1" (`System.Decimal`) you then passed "2001-01-01" since you couldn't store "1.1" in a `System.DateTime`, under these circumstances the DataTypeComputer would go straight to `System.String`

fallback in order of `System.Type` ) 

Since in this scenario Everything is ultimately compatible with `System.String` In order

### Strong Typed Objects
If you are passing objects that are not `System.String` e.g. from a DataColumn that has an actual Type on it (e.g. `System.Float`) then DataTypeComputer will set the `CurrentEstimate` to the provided object Type.  It will still calculate the `NumbersBeforeDecimalPlace` and `NumbersAfterDecimalPlace` properties if appropriate (See test `TestDatatypeComputer_HardTypeFloats`).

The first time you pass a typed object (excluding DBNull.Value) then it will assume the entire input stream is strongly typed (See `IsPrimedWithBonafideType`).  Any attempts to pass in different object Types in future (or if strings were previously passed in before) will result in a `DataTypeComputerException`.
