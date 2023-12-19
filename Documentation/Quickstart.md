# RDMP Quickstart Guide
## Prerequisites
- Access to an installation of Microsoft SQL Server, _or_ ability to run RDMP from the command line
  - SQL Server is used for RDMPs internal storage, or a directory on disk can be used instead
  - Any SQL Server edition will do, such as SQL Server Express (free)

## Steps for Windows GUI Client
1. Download the latest release of RDMP from [GitHub](https://github.com/HicServices/RDMP/releases/latest). It will be called something like `rdmp-8.0.0-client.zip`
2. Unzip this file 
3. Inside this folder you should find and run a file called `ResearchDataManagmentPlatform.exe`. Run this file with the `--dir` switch, followed by a location to use for storage, such as `ResearchDataManagmentPlatform.exe -dir d:\rdmpdata`. _OR_
4. You will be prompted with two buttons, select the `I want to create new Platform Databases` option
5. You should be prompted to input your platform databases
   * Set the server name, if you're using a locally hosted SQL server this will be something like `localhost\sqlexpress`
   * Set any login credentials required to access your database
   * If you want some fake data to test RDMP with, check the `Example Datasets` checkbox
   * All other options are for more advanced configurations, check out the [User Manual](./CodeTutorials/UserManual.md) for details
6. Press the `Create` button
    RDMP will now create the platform databases it requires. Any issues will be flagged in the results table.
7. RDMP will now restart, allow it to do its thing
8.  Congratulations, you've successfully set up RDMP. Explore RDMP with your fake data, or check out the [User Manual](./CodeTutorials/UserManual.md) for more info on what RDMP can do.


## Steps for CLI
1. Download the latest release of RDMP from [GitHub](https://github.com/HicServices/RDMP/releases/latest). It will be called something like `rdmp-8.1.1-cli-win-x64.zip` for windows and `rdmp-8.1.1-cli-linux-x64.zip` for linux
2. Unzip this file
3. Inside this folder you should find a file called `rdmp.exe` (for Windows) or `rdmp` (Linux). Run it as `rdmp.exe --dir d:\rdmpdata` (where d:\rdmpdata is where you want to store RDMP internal data), or if you prefer MS SQL Server, run the command `rdmp.exe install "{YOUR_SQL_SERVER_CONNECTION_STRING}" RDMP_ -e`
    * Replace `YOUR_SQL_SERVER_CONNECTION_STRING` with the connection string to your SQL Server
    * An example of this would be `localhost\sqlexpress;Uid=user;Pwd=password`
    * This command creates the platform databases RDMP requires. Any issues will appear in stdout
4. Congratulations, you've successfully set up RDMP using the CLI. Check out the [User Manual](./CodeTutorials/UserManual.md) for more info on what RDMP can do.
