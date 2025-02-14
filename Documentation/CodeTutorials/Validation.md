# Validation

## Table of contents
1. [Background](#background)
2. [Configuring Validation](#configuring-validation)
   1. [Consequence](#consequence)
   1. [StandardRegex](#standardregex)
   1. [Results by Data Load Run ID](#results-by-data-load-run-id)
2. [Results Database](#results-database)
2. [Visualisation](#visualisation)
2. [Plugin Constraints](#plugin-constraints)

## Background
The RDMP Data Quality Engine (DQE) performs row level validation based on rules you set up on each column.  Validation is only run on extractable columns.  Results are persisted in a DQE results database designed to scale to very large datasets.

## Configuring Validation
Locate a [Catalogue] for which you want to run the DQE.  Right click it and select 'Data Quality Engine...' then from the top menu 'Validation Rules...':

![Launch Data Quality Engine Configuration Screen from context menu](Images/Validation/LaunchDqe.png)

This will let you choose which column in the dataset provides the time element (e.g. StudyDate) and what rules apply to which columns.


![ValidationRules](Images/Validation/ColumnValidationRules.png)

|Setting | Required | Role |
|--|---|---|
| Time Period Column |  Yes | Provides the time axis on which to arrange dataset data.  Should have a high completeness.  Records with a null value in the 'Time Period Column' will still be validated but will only have results in RowState and ColumnState (not PeriodicityState).| 
| Pivot Column |  No | Optional column for slicing results across.  For example by healthboard.  When set all results will be recorded against each pivot value seen.  Care should be taken not to pick a column with very large number of unique values| 
| Validation Rules | No | Per column rules that will be applied when running the DQE e.g. date in column X must be after date in column Y.  Each column can have one Primary Constraint and any number of Secondary Constraints.  Each constraint violation has a [consequence](#consequence)|


### Consequence

Each constraint has a 'Consequence'.  This describes how important/noteworthy it is when the rule is broken by data.  If a single value breaks 2+ constraints then only the highest Consequence is recorded.

### StandardRegex

Regular Expressions are a very powerful method of string validation.  
The StandardRegexConstraint allows you to centralise Regex rules.  This feature avoids replication of a given concept in multiple columns/datasets.  After defining a StandardRegexConstraint then you can reference it anywhere in RDMP (e.g. with a StandardRegexConstraint).

![ExampleStandardRegexConstraint](Images/Validation/StandardRegex.png)


### Results by Data Load Run ID

If your data is loaded through the DLE then you should have a`hic_dataLoadRunID` column.  If you make this column extractable (even with category Internal) then your results will be sliced by load ID.  This allows you to view DQE results by load.  

Using this feature you can diagnose whether 'bad' data is evenly distributed or was introduced by a single load (or began after a given load date).

## Results Database

Validation rules are applied by batch pulling all records from the source dataset table.  Each row is validated in isolation.  Results are compiled in memory then sent to the DQE results database in a transaction.  The DQE results database schema is designed to scale independently of the number of rows / rows failing validation.  To this end validation is recorded:

- By Column (Number of cells failing validation in this column)
- By Row (Number of rows failing validation)
- By Date (Number of rows failing validation per month)

Each of these is also subdivided by Pivot Value and Data Load Run ID (except By Date).

![DatabaseSchema](Images/Validation/ValidationResultsTables.png)

This schema scales well and allows you to persist DQE results for a large number of datasets and maintain a rewindable history for determining when a problem first manifested.

```
Example Dataset:
20,000,000 rows
20 columns
10 unique pivot values in pivot column
120 unique months of data (10 years)
60 unique data loads (one load every 2 months)

Number of records in ColumnState
10 (pivot values) x 20 (columns) x 60 (data loads) = 12,000 rows

Number of records in RowState
10 (pivot values) x 60 (data loads) = 600 rows

Number of records in PeriodicityState 
10 (pivot values) x 120 (months) x 4 (correct + the 3 failure severities) = 4,800 rows
```

## Visualisation

RDMP features a viewer which renders the By Date and By Column tables as graphs.  You can switch which Pivot Value you are looking at with the radio buttons and change which Evaluation (DQE run) you are viewing using the slider.  This allows you to look back in time at the dataset data quality in the past.

![Visualisation of dqe results](Images/Validation/Visualisation.png)


## Plugin Constraints
You can write new validation rules by inheriting from `HIC.Common.Validation.Constraints.Secondary.SecondaryConstraint`.  Implement the Validate method returning null if validation passed or a `ValidationFailure` if it didn't.  Public properties will be rendered as CheckBoxes, TextBoxes etc.

```csharp
public class NoFishConstraint : SecondaryConstraint
{
	//Will be exposed in user interface
	public bool IgnoreCaps { get; set; }

	public override void RenameColumn(string originalName, string newName){ }

	public override string GetHumanReadableDescriptionOfValidation()
	{
		return "Ensures the word Fish doesn't appear in the column";
	}

	public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
	{
		var s = value as string;
		if(s != null)
		{
			if(IgnoreCaps)
			{
				if (s.ToLowerInvariant().Contains("fish"))
					return new ValidationFailure("Value was Fish!", this);
			}
			else
			{
				if (s.Contains("Fish"))
					return new ValidationFailure("Value was Fish!",this);
			}
		}
		
		//validation passed
		return null;
	}
}

```

When uploaded as a plugin the above example would appear as:

![ValidationRules](Images/Validation/NoFish.png)


[Catalogue]: ./Glossary.md#Catalogue