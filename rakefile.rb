require "net/http"
require 'uri'
require 'json'

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

task :build, [:config] => :restorepackages do |msb, args|
	sh "\"#{$MSBUILD15CMD}\" #{SOLUTION} \/t:Clean;Build \/p:Configuration=#{args.config}"
end

task :build_release => :restorepackages do
	sh "\"#{$MSBUILD15CMD}\" #{SOLUTION} \/t:Clean;Build \/p:Configuration=Release"
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
task :assemblyinfo do 
	asminfoversion = File.read("SharedAssemblyInfo.cs").match(/AssemblyInformationalVersion\("(\d+)\.(\d+)\.(\d+)(-.*)?"/)
    
	puts asminfoversion.inspect
	
    major = asminfoversion[1]
	minor = asminfoversion[2]
	patch = asminfoversion[3]
    suffix = asminfoversion[4]
	
	version = "#{major}.#{minor}.#{patch}"
    puts "version: #{version}#{suffix}"
    
	# DO NOT REMOVE! needed by build script!
    f = File.new('version', 'w')
    f.write "#{version}#{suffix}"
    f.close
    # ----
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
	version = File.open('version') {|f| f.readline}
    puts "version: #{version}"
	branch = ENV['BRANCH_SELECTOR'] || "origin/release/3.0.0.X"
	branch.gsub!(/origin\//, "")
	puts branch
	prerelease = branch.match(/master/) ? false : true	
	
	uri = URI.parse('https://api.github.com/repos/HicServices/RDMP/releases')
	body = { tag_name: "v#{version}", name: "RDMP v#{version}", target_commitish: branch, prerelease: prerelease }
    header = {'Content-Type' => 'application/json',
              'Authorization' => "token #{GITHUB}"}
	
	http = Net::HTTP.new(uri.host, uri.port)
	http.use_ssl = (uri.scheme == "https")
	request = Net::HTTP::Post.new(uri.request_uri, header)
	request.body = body.to_json

	# Send the request
	response = http.request(request)
    puts response.to_hash.inspect
    githubresponse = JSON.parse(response.body)
    puts githubresponse.inspect
    upload_url = githubresponse["upload_url"].gsub(/\{.*\}/, "")
    puts upload_url
    	
    Dir.chdir "Application/ResearchDataManagementPlatform/Release_#{version}" do
        upload_to_github(upload_url, "RELEASES")
        upload_to_github(upload_url, "ResearchDataManagementPlatform-#{version}-full.nupkg")
        upload_to_github(upload_url, "Setup.exe")
    end
end


def upload_to_github(upload_url, file_path)
    boundary = "AaB03x"
    uri = URI.parse(upload_url + "?name=" + file_path)
    
    header = {'Content-Type' => 'application/octet-stream',
              'Content-Length' => File.size(file_path).to_s,
              'Authorization' => "token #{GITHUB}"}

    http = Net::HTTP.new(uri.host, uri.port)
    http.use_ssl = (uri.scheme == "https")
    request = Net::HTTP::Post.new(uri.request_uri, header)

    file = File.open(file_path, "rb")
    request.body = file.read
    
    response = http.request(request)
    
    puts response.to_hash.inspect
    puts response.body

    file.close
end

# task :link do
# 	if File.exist?("DatabaseCreation.exe") 
# 		File.delete("DatabaseCreation.exe")
# 	end
# 	sh "call mklink DatabaseCreation.exe DatabaseCreation\\Release\\DatabaseCreation.exe"
# end
