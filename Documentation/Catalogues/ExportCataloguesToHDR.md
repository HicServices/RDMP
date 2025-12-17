## Export Catalogues to HDR

The [HDR Gateway](https://healthdatagateway.org/en) is a metadata catalogue for all things health data.
RDMP has the ability to upload all non-internal, non-depricated, non-project specific catalogues to it using the following command
```
rdmp.exe exportcataloguestohdrgateway https://api.hdruk.cloud/api {teamID} {AppID} {ClientID} {additional config file}

```

Running this command will create new dataets on the HDR Gateway, as well as update an existing datasets with the same name as the catalogue.

### Command Variables
* teamId - the TeamID that you and the dataset(s) you want to create belong to
* AppID - This integration requires the use of a custom integration, the AppID is provided by this integration
* ClientID- This integration requires the use of a custom integration, the clientID is provided by this integration
* Additional config file - some details are not stored within RDMP, but are typically the same for all datasets that belong to a single owner. This file is used to populate these fields. Use the example below as a starting point

### Example Config File
 {
    "accessRights":"https://www.dundee.ac.uk/hic/governance-service",
    "accessService":"HIC has implemented a remote-access Trusted Research Environment to protect data confidentiality, satisfy public concerns about data loss and reassure Data Controllers about HIC’s secure management and processing of their data.\r\nData is not released externally to data users for analysis on their own computers but placed on a server at HIC, within a restricted, secure IT environment, where the data user is given secure remote access to carry out their analysis.\r\nFull details are available via the following link: https://www.dundee.ac.uk/hic/safe-haven",
    "accessServiceCategory":"TRE/SDE",
    "conformsTo":["LOCAL"],
    "vocabularyEncodingScheme":["LOCAL"],
    "language":["en"],
    "format":["CSV","Database"]
    "dataUseLimitation":["General research use"],
    "resourceCreator":"Please cite us!",
    "dataUseRequirements":["Disclosure control"]
 }