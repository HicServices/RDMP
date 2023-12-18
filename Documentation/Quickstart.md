# RDMP Quickstart Guide
## Prerequisites
- Access to an SQL Server
  - This is used for RDMPs internal starage
  - Any SQL Server will do, such as SQL Server Express

## Steps for Windows GUI Client
1. Download the latest release of RDMP from [GitHub](https://github.com/HicServices/RDMP/releases/latest). It will be called something like `rdmp-8.0.0-client.zip`
2. Unzip this file 
3. Inside this folder you should find and run a file called `ResearchDataManagmentPlatform.exe`
4. You will be prompted with two buttons, select the `I want to create new Platform Databases` option
5. You should be prompted to input your platform databases
   * Set the server name, if you're using a locally hosted SQL server this will be something like `localhost\sqlexpress`
   * Set any login credentials required to access your database
   * If you want some fake data to test RDMP with, check the `Example Datasets` checkbox
   * All other options are for more advanced configurations, check out the [User Manual](./CodeTutorials/UserManual.md) for details
6. Press the `Create` button
    RDMP will now create the platform databases it requires. Any issues will be flagged in the results table.
7. RDMP will now restart, allow it to do its thing
8.  Congratualtions, you've successfully set up RDMP. Explore RDMP with your fake data, or check out the [User Manual](./CodeTutorials/UserManual.md) for more info on what RDMP can do.


## Steps for CLI
1. Download the latest release of RDMP from [GitHub](https://github.com/HicServices/RDMP/releases/latest). It will be called something like `rdmp-8.1.1-cli-win-x64.zip` for windows and `rdmp-8.1.1-cli-linux-x64.zip` for linux
2. Unzip this file
3. Inside this folder you should finda file called`rdmp.exe`. Run the command `rdmp.exe install "{YOUR_SQL_SERVER_CONNECTION_STRING}" RDMP_ -e`
    * Replace `YOUR_SQL_SERVER_CONNECTION_STRING` with the connection string to your SQL Server
    * An example of this would be `localhost\sqlexpress;Uid=user;Pwd=password`
    * This command creates the platform databases RDMP requires. Any issues will appear in stdout
4. Congratualtions, you've successfully set up RDMP using the CLI. Check out the [User Manual](./CodeTutorials/UserManual.md) for more info on what RDMP can do.
