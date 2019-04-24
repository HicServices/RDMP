--Version:1.0.0.1
--Description:Increases the length of the [DataSet].[dataSetID] column to match the length of [DataLoadTask].[name]

-- First drop the FK in DataLoadTask
ALTER TABLE [DataLoadTask] DROP CONSTRAINT FK_DataLoadTask_DataLoadTask

-- Now update the columns. First the column in DataSet.
ALTER TABLE [DataSet] ALTER COLUMN dataSetID varchar(255) not null

-- And then the FK column in DataLoadTask
ALTER TABLE [DataLoadTask] ALTER COLUMN dataSetID varchar(255) not null

-- Finally re-add the FK in DataLoadTask, also renaming it to not repeat the same table name twice
ALTER TABLE [DataLoadTask] ADD CONSTRAINT FK_DataLoadTask_DataSet FOREIGN KEY (dataSetID) REFERENCES DataSet (dataSetID)