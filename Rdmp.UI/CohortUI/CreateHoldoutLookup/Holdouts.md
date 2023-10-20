# Holdout Cohorts in RDMP
Holdout cohorts are groups of people that we want to exclude from extractions. This could be for a variety of reasons, notably as holdout data for ML model validation.
## How do I create a holdout?
1. Decide what you want to holdout
    
    This May be a certain number of people with a certain condition, or a complex distribution of attributes.
2. Create a Cohort that describes these people

    You may need to create multiple  cohorts depending on how complex your holdout criteria is.
    N.B. you can limit cohort sizes by editing the cohort aggregate filter to limit the count
3. Right click on the cohort and create a holdout from them
    This gives you a number of options to limit the holdout based on counts and dates. The cohort is shuffled before the holdout is selected.
4. This creates a catalogue containing the extraction identifiers for the holdout
5. Add the holdout catalogue(s) as exception clauses to your main extraction cohort to exclude them from the extraction

    