--Version:1.3.0.0
--Description: Prevents the user creating two ExtractableDatasets that both point to the same Catalogue ID
if not exists (select * from sys.indexes where name = 'PreventDoubleAddingCatalogueIdx')
begin

/****** Object:  Index [PreventDoubleAddingCatalogueIdx]    Script Date: 21/07/2015 12:37:17 ******/
CREATE UNIQUE NONCLUSTERED INDEX [PreventDoubleAddingCatalogueIdx] ON [dbo].[ExtractableDataSet]
(
	[Catalogue_ID] ASC
)
end
