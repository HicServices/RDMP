require 'albacore'

load 'rakeconfig.rb'
$MSBUILD15CMD = MSBUILD15CMD.gsub(/\\/,"/")

task :ci_continuous, [:config] => [:setup_connection, :assemblyinfo, :bundlesource, :build, :tests]

task :plugins, [:config] => [:assemblyinfo, :build, :deployplugins]

task :release => [:assemblyinfo, :build_release, :squirrel, :github]

task :restorepackages do
    sh "nuget restore HIC.DataManagementPlatform.sln"
end

task :setup_connection do 
    File.open("Tests.Common/TestDatabases.txt", "w") do |f|
        f.write "ServerName: #{DBSERVER}\r\n"
        f.write "Prefix: #{DBPREFIX}\r\n"
        f.write "MySql: Server=#{MYSQLDB};Uid=#{MYSQLUSR};Pwd=#{MYSQLPASS};Ssl-Mode=Required\r\n"
    end
end

task :bundlesource do
	sh "powershell ./BundleSourceIntoZipFile.ps1"
end

msbuild :build, [:config] => :restorepackages do |msb, args|
	msb.command = $MSBUILD15CMD
    msb.properties = { :configuration => args.config }
    msb.targets = [ :Clean, :Build ]   
    msb.solution = SOLUTION
end

msbuild :build_release do |msb|
	msb.command = $MSBUILD15CMD
    msb.properties = { :configuration => :Release }
    msb.targets = [ :Clean, :Build ]   
    msb.solution = SOLUTION
end

task :tests, [:config] => [:createtestdb, :run_tests]

task :createtestdb, [:config] do |t, args|
	Dir.chdir("Tools/rdmp/bin/#{args.config}/netcoreapp2.2") do
        sh "dotnet ./rdmp.dll install #{DBSERVER} #{DBPREFIX} -D"
    end
end

task :run_tests do 
	sh 'dotnet test --no-build --logger:"nunit;LogFilePath=test-result.xml"'
end

desc "Sets the version number from SharedAssemblyInfo file"    
assemblyinfo :assemblyinfo do |asm|
	asm.input_file = "SharedAssemblyInfo.cs"
    asm.output_file = "SharedAssemblyInfo.cs"
    asminfoversion = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+/]
        
    major, minor, patch = asminfoversion.split(/\./)
   
    if PRERELEASE == "true"
        patch = patch.to_i + 1
        suffix = "-pre"
    elsif CANDIDATE == "true"
        patch = patch.to_i + 1
        suffix = "-rc"
    end

    version = "#{major}.#{minor}.#{patch}"
    puts "version: #{version}#{suffix}"
    # DO NOT REMOVE! needed by build script!
    f = File.new('version', 'w')
    f.write "#{version}#{suffix}"
    f.close
    # ----
    asm.version = version
    asm.file_version = version
    asm.informational_version = "#{version}#{suffix}"
end

desc "Pushes the plugin packages to nuget.org"    
task :deployplugins, [:config] do |t, args|
	version = File.open('version') {|f| f.readline}
    puts "version: #{version}"
	
	Dir.chdir('Plugins') do
        sh "nuget pack Plugin/Plugin.nuspec -Properties Configuration=#{args.config} -IncludeReferencedProjects -Symbols -Version #{version}"
        sh "nuget pack Plugin.UI/Plugin.UI.nuspec -Properties Configuration=#{args.config} -IncludeReferencedProjects -Symbols -Version #{version}"
        sh "nuget pack Plugin.Test/Plugin.Test.nuspec -Properties Configuration=#{args.config} -IncludeReferencedProjects -Symbols -Version #{version}"
		
        sh "nuget push HIC.RDMP.Plugin.#{version}.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey #{NUGETKEY}"
		sh "nuget push HIC.RDMP.Plugin.UI.#{version}.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey #{NUGETKEY}"
		sh "nuget push HIC.RDMP.Plugin.Test.#{version}.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey #{NUGETKEY}"
    end
end

desc "Get the deploy and min-version for the app"    
task :publishall, [:folder, :url] do |t, args|
    asminfoversion = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+\.\d+(\-\w+)+/]
    
    if (asminfoversion.nil?) then
        asminfoversion = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+\.\d+/]
    end
    
    major, minor, patch, build, suffix = asminfoversion.split(/[\.\-]/)
        
    basefolder = args.folder.chomp("/").chomp("\\")
    baseurl = args.url.chomp("/").chomp("\\")
    version = "#{major}.#{minor}.#{patch}.#{build}"
    
    if (suffix)
        deployfolder = "#{basefolder}/#{major}.#{minor}.#{patch}.#{build}-#{suffix}"
        deployurl = "#{baseurl}/#{major}.#{minor}.#{patch}.#{build}-#{suffix}/"
    else
        deployfolder = "#{basefolder}/#{major}.#{minor}.#{patch}.#{build}"
        deployurl = "#{baseurl}/Stable/"
    end
    
    minversion = "#{major}.#{minor}.#{patch}.0"
    
    Dir.chdir('Application/ResearchDataManagementPlatform') do
        sh "./publish.bat #{version} #{minversion} #{deployfolder}/ #{deployurl}"
    end
    
    # reset symlinks, only if STABLE:
    if (!suffix)
        # delete old files
        File.delete "#{basefolder}/Stable/ResearchDataManagementPlatform.application" if File.exists?("#{basefolder}/Stable/ResearchDataManagementPlatform.application")
        File.delete "#{basefolder}/Stable/ResearchDataManagementPlatform.exe" if File.exists?("#{basefolder}/Stable/ResearchDataManagementPlatform.exe")
        File.delete "#{basefolder}/Stable/setup.exe" if File.exists?("#{basefolder}/Stable/setup.exe")
        sh "rd \"#{basefolder}/Stable/Application Files\"" if Dir.exists?("#{basefolder}/Stable/Application Files")
        # recreate symlinks
        sh "call mklink \"#{basefolder}/Stable/ResearchDataManagementPlatform.application\" \"#{deployfolder}/ResearchDataManagementPlatform.application\""
        sh "call mklink \"#{basefolder}/Stable/ResearchDataManagementPlatform.exe\" \"#{deployfolder}/ResearchDataManagementPlatform.exe\""
        sh "call mklink \"#{basefolder}/Stable/setup.exe\" \"#{deployfolder}/setup.exe\""        
        sh "call mklink /J \"#{basefolder}/Stable/Application Files\" \"#{deployfolder}/Application Files\""

        # zip up the standalone too
        File.delete "#{basefolder}/Stable/Standalone.zip" if File.exists?("#{basefolder}/Stable/Standalone.zip")
        sh "powershell.exe -nologo -noprofile -command \"& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('#{deployfolder}/standalone', '#{basefolder}/Stable/standalone.zip'); }\""
        
        # now we build the Automation Service
        Dir.chdir('Tools/RDMPAutomationService') do
            sh "./build.cmd #{deployfolder}/RDMPAutomationService/"
            File.delete "#{basefolder}/Stable/RDMPAutomationService.zip" if File.exists?("#{basefolder}/Stable/RDMPAutomationService.zip")
            sh "powershell.exe -nologo -noprofile -command \"& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('#{deployfolder}/RDMPAutomationService', '#{basefolder}/Stable/RDMPAutomationService.zip'); }\""
        end
    end    
end

task :squirrel do
	version = File.open('version') {|f| f.readline}
    puts "version: #{version}"
	Dir.chdir "Application/ResearchDataManagementPlatform" do
		sh "nuget pack RDMP.nuspec -Properties Configuration=Release -Version #{version}"
		sh "#{SQUIRREL} --releasify ResearchDataManagementPlatform.#{version}.nupkg -r Release_#{version}"
	end
end

task :github do

end

# task :link do
# 	if File.exist?("DatabaseCreation.exe") 
# 		File.delete("DatabaseCreation.exe")
# 	end
# 	sh "call mklink DatabaseCreation.exe DatabaseCreation\\Release\\DatabaseCreation.exe"
# end
