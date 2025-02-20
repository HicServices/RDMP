# AWS S3 Bucket Release Destination
This release engine component allows you to write your flat files to an S3 bucket during a release.

## How does it work?
Using your local [AWS profile](https://docs.aws.amazon.com/cli/v1/userguide/cli-configure-files.html) it uses the AWs API to write your release data to an S3 Bucket

## Configuration
1. The first thing you'll need to do it set up your  [AWS profile](https://docs.aws.amazon.com/cli/v1/userguide/cli-configure-files.html) on your machine.
2. Next, we need to configure the release component (this can also be done interactively when the checks are ran)
    a. The name of theAWS profile you wish to use to perform the release. You can get this information during step 1
    b. The name of the S3 Bucket you wish to write to. This bucket must exist and you mist have list and write access to the bucket
    c. The AWS region the bucket exists in e.g. "eu-west-2"
    d. The bucket folder you wish to write to. This folder must have not previously been used for a release e.g. /my/release/folder/unique-release-name
3. With this all configured, you should be able to run the release.

## What gets written to AWS?
```
└── release folder/
    ├── contents.txt
    ├── Globals/
    │   └── any_global_files
    └── Extraction_Name/
        ├── Catalogue_folder/
        │   ├── extractionData.csv
        │   └── extractionData.docx
        └── ReleaseDocument.docx
```
The structure above is used to write data to the S3 Bucket.
contents.txt  give a tree view of all the data within the release
The Globals folder stores the released globals
Within the Extraction folder, there is a release document detailing what has been released.
Along with this, there are folders for each catalogue, containing the released data and any lookups associated with them