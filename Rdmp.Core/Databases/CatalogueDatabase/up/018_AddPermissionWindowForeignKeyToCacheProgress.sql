--Version:1.13.0.0
--Description: Add PermissionWindow foreign key to CacheProgress

-- If ID is an identity column then this script has already run (providing someone hasn't been manually interfering with the database) 
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_CacheProgress_PermissionWindow]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN

ALTER TABLE [dbo].[CacheProgress]  WITH CHECK ADD  CONSTRAINT [FK_CacheProgress_PermissionWindow] FOREIGN KEY([PermissionWindow_ID])
REFERENCES [dbo].[PermissionWindow] ([ID])

ALTER TABLE [dbo].[CacheProgress] CHECK CONSTRAINT [FK_CacheProgress_PermissionWindow]

END