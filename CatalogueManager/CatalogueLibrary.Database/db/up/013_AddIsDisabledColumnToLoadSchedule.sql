--Version:1.11.0.0
--Description:Adds IsDisabled Column to LoadSchedule
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoadSchedule' AND COLUMN_NAME='IsDisabled')
BEGIN

ALTER TABLE LoadSchedule ADD IsDisabled bit NOT NULL
CONSTRAINT [DF_LoadSchedule_IsDisabled]  DEFAULT ((0))

END