# Table of contents
1. [Hello World Plugin](#helloWorldPlugin)
2. [Attaching the Debugger](#debugging)
2. [Streamlining Build](#betterBuilding)

 <a name="helloWorldPlugin"></a>
 #Hello World Plugin
 Create a new Visual Studio Class Library Project targetting .Net Framework 4.5
 
 Add a reference to the HIC nuget server `https://hic.dundee.ac.uk/NuGet/nuget/` and add a reference to `HIC.RDMP.Plugin`
 
 Make sure that the major and minor version number (first two numbers) of the Nuget Package match your installed version of RDMP (Visible in the task bar of the main RDMP application)
 
 ![Versions must match](Images/NugetVersionMustMatchLive.png)
 
 
 Add a class called `MyPluginUserInterface` and inherit from `PluginUserInterface` override `GetAdditionalRightClickMenuItems`
 
```csharp
 public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            if (o is Catalogue)
                return new[] { new ToolStripMenuItem("Hello World", null, (s, e) => MessageBox.Show("Hello World")) };

            return null;
        }
 ```

 Launch Research Data Management Platform main application and launch Plugin Management from the Home screen (under Advanced).  Select Add Plugin..
 
  ![Adding a plugin via the RDMP user interface](Images/ManagePluginsAddingAPlugin.png)
 
 Next add a new empty Catalogue
 
 ![Add empty Catalogue](Images/AddEmptyCatalogue.png)
 
 Now right click it.  You should see your message appearing.
 
 ![What it should look like](Images/HelloWorldSuccess.png)
 
 <a name="debugging"></a>
 #Attaching the Debugger
 Sometimes you want to debug your plugin as it is running hosted by RDMP.  To do this simply launch `ResearchDataManagementPlatform.exe` manually (if you need to see where the exe is you can select Diagnostics=>Open exe directory at any time).  Next go into visual studio and select Debug=>Attach to Process
 
 <a name="betterBuilding"></a>
 #Better Build Process
 There are a couple of things you can do to streamline your plugin development process.  Firstly You can remove the requirement to launch 'Manage Plugins' every time you make a code change by setting up a post build step which runs PluginPackager.exe.  This will commit the plugin into the RMDP database.  Secondly you can add the ResearchDataManagementPlatform.exe as a startup project in your plugin solution.
 
 Copy the PluginPackager folder out of the nuget packages directory and into the root of your solution.
 
 Next right click your Project and select Properties
 
 Enter a Post-build script to run PluginPackager.exe e.g.
 
 ```
 
``` 
 
 