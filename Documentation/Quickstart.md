# RDMP Quickstart Guide
## Prerequisites
- Access to an SQL Server
  - This is used for RDMPs internal starage
  - Any SQL Server will do, such as SQL Server Express

## Steps for Windows GUI Client
1. Download the latest release of RDMP from [GitHub](https://github.com/HicServices/RDMP/releases/latest). It will be called something like `rdmp-8.0.0-client.zip`
2. Unzip this file 
3. Inside this folder you should find and run a file called `ResearchDataManagmentPlatform.exe`
4. You should be prompted to input your platform databases
   * Set the server name, if you're using a locally hosted SQL server this will be something like `localhost\sqlexpress`
   * Set any login credentials required to access your database
   * If you want some fake data to test RDMP with, check the `Example Datasets` checkbox
   * All other options are for more advanced configurations, check out the [User Manual](./CodeTutorials/UserManual.md) for details
5. Press the `Create` button
    RDMP will now create the platform databases it requires. Any issues will be flagged in the results table.
6. RDMP will now restart, allow it to do its thing
7.  Congratualtions, you've successfully set up RDMP. Explore RDMP with your fake data, or check out  the [User Manual](./CodeTutorials/UserManual.md) for more info on what RDMP can do.
