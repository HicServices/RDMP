# Data Load Chainer
The Data Load Chainer can be used to run multiple data loads in order.

## How to set it up
The Data Load Chainer is a "Data Provider" and should be added to the "Post Load" section of a data load.
Within the configuration, you can select which data load you want it to run and if it should automatically accept any changes the chained data load wishes to make.

## How does it work?
When running checks on the main Data Load, checks are also run an all chained Data Loads.
You main Data Load will run as normal. Once the Data Chainer is activated, it will run as a new Data Load, with a new Data Load ID.

### Why would I need to automatically accept checks?
We run the checks for all Data Loads prior to running, but we don't perform the tidy-up step of one data load ebfore running the next in the chain.
This can lead to issues where both data loads are trying to create the same resource, such as a temporary database.
Accepting checks means that you want RDMP to run any fixes it can to ensure your chained data load runs as expected.
If you don't check this option the Data Load may fail, but it is recommended to test each data load independently before attempting to chain them.