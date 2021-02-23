require "net/http"
require 'uri'
require 'json'

load 'rakeconfig.rb'
$MSBUILD15CMD = MSBUILD15CMD.gsub(/\\/,"/")

task :ci_low_warnings, [:config,:level,:aserrors] => [:assemblyinfo, :build_low_warning]

task :ci_continuous, [:config] => [:setup_connection, :assemblyinfo, :build, :tests]

task :ci_integration, [:config] => [:setup_connection, :assemblyinfo, :build, :all_tests]

task :plugins, [:config] => [:assemblyinfo, :build, :deployplugins]

task :release => [:assemblyinfo, :build_release, :bundlesource, :build_cli, :squirrel, :github]

task :tests, [:config] => [:run_unit_tests]

task :all_tests, [:config] => [:createtestdb, :run_all_tests]

task :restorepackages do
    sh "nuget restore HIC.DataManagementPlatform.sln"
end

task :setup_connection do 
    File.open("Tests.Common/TestDatabases.txt", "w") do |f|
        f.write "ServerName: #{DBSERVER}\r\n"
        f.write "Prefix: #{DBPREFIX}\r\n"
        f.write "MySql: Server=#{MYSQLDB};Uid=#{MYSQLUSR};Pwd=#{MYSQLPASS};SslMode=Required\r\n"
    end
end

task :bundlesource, [:config] do |t, args|
	args.with_defaults(:config => :Release)
	sh "powershell ./BundleSourceIntoZipFile.ps1"
	FileUtils.cp "./Tools/BundleUpSourceIntoZip/output/SourceCodeForSelfAwareness.zip","./Application/ResearchDataManagementPlatform/bin/#{args.config}/net461/SourceCodeForSelfAwareness.zip"
end

task :build, [:config] => :restorepackages do |msb, args|
	sh "\"#{$MSBUILD15CMD}\" #{SOLUTION} \/t:Clean;Build \/p:Configuration=#{args.config}"
end

task :build_release => :restorepackages do
	sh "\"#{$MSBUILD15CMD}\" #{SOLUTION} \/t:Clean;Build \/p:Configuration=Release"
end

task :build_cli => :restorepackages do
	Dir.chdir("Tools/rdmp/") do
        sh "dotnet publish -r win-x64 -c Release -o PublishWindows"
		sh "dotnet publish -r linux-x64 -c Release -o PublishLinux --self-contained false"

		Dir.chdir("PublishWindows/") do
			sh "#{SQUIRREL}/signtool.exe sign /a /s MY /n \"University of Dundee\" /fd sha256 /tr http://timestamp.digicert.com /td sha256 /v *.dll"
			sh "#{SQUIRREL}/signtool.exe sign /a /s MY /n \"University of Dundee\" /fd sha256 /tr http://timestamp.digicert.com /td sha256 /v *.exe"
		end

		Dir.chdir("PublishLinux/") do
			sh "#{SQUIRREL}/signtool.exe sign /a /s MY /n \"University of Dundee\" /fd sha256 /tr http://timestamp.digicert.com /td sha256 /v *.dll"
		end
    end
	sh "powershell.exe -nologo -noprofile -command \"& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('Tools/rdmp/PublishWindows', 'Tools/rdmp/rdmp-cli-win-x64.zip'); }\""
	sh "powershell.exe -nologo -noprofile -command \"& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('Tools/rdmp/PublishLinux', 'Tools/rdmp/rdmp-cli-linux-x64.zip'); }\""
end

task :build_low_warning, [:config,:level,:aserrors] => :restorepackages do |msb, args|
	args.with_defaults(:level => 1)
	sh "\"#{$MSBUILD15CMD}\" #{SOLUTION} \/t:Clean;Build \/p:Configuration=#{args.config} \/v:detailed \/p:WarningLevel=#{args.level} \/p:TreatWarningsAsErrors=#{args.aserrors}"
end

task :createtestdb, [:config] do |t, args|
	Dir.chdir("Tools/rdmp/bin/#{args.config}/netcoreapp3.1") do
        sh "dotnet ./rdmp.dll install #{DBSERVER} #{DBPREFIX} -D"
    end
end

task :run_unit_tests, [:config] do |t,args|
	sh "dotnet test --no-build --filter TestCategory=Unit --logger:\"nunit;LogFilePath=test-result.xml\" --configuration #{args.config} -p:ParallelizeTestCollections=false"
end

task :run_all_tests, [:config] do |t,args|

	sh "dotnet test ./Reusable/Tests/ReusableCodeTests/ReusableCodeTests.csproj --no-build --logger:\"nunit;LogFilePath=test-result.xml\" --configuration #{args.config}"
	sh "dotnet test ./Rdmp.Core.Tests/Rdmp.Core.Tests.csproj --no-build --logger:\"nunit;LogFilePath=test-result.xml\" --configuration #{args.config}"
	sh "dotnet test ./Rdmp.UI.Tests/Rdmp.UI.Tests.csproj --no-build --logger:\"nunit;LogFilePath=test-result.xml\" --configuration #{args.config}"
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
	
	# Publish the CLI
	Dir.chdir('Tools/rdmp') do
		sh "dotnet publish -c #{args.config} --self-contained false"
	end
	
	#Package all the plugins
	Dir.chdir('Plugins') do
        sh "nuget pack Plugin/Plugin.nuspec -Properties Configuration=#{args.config} -IncludeReferencedProjects -Symbols -Version #{version}"
        sh "nuget pack Plugin.UI/Plugin.UI.nuspec -Properties Configuration=#{args.config} -IncludeReferencedProjects -Symbols -Version #{version}"
        sh "nuget pack Plugin.Test/Plugin.Test.nuspec -Properties Configuration=#{args.config} -IncludeReferencedProjects -Symbols -Version #{version}"
		
        sh "nuget push HIC.RDMP.Plugin.#{version}.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey #{NUGETKEY}"
		sh "nuget push HIC.RDMP.Plugin.UI.#{version}.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey #{NUGETKEY}"
		sh "nuget push HIC.RDMP.Plugin.Test.#{version}.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey #{NUGETKEY}"
    end
end

task :squirrel do
	version = File.open('version') {|f| f.readline}
    puts "version: #{version}"
	
	Dir.chdir "Application/ResearchDataManagementPlatform" do
		sh "nuget pack RDMP.nuspec -Properties Configuration=Release -Version #{version}"
		sh "#{SQUIRREL}/Squirrel.exe --releasify ResearchDataManagementPlatform.#{version}.nupkg -r Release_#{version} -n \"/a /s MY /n \"University of Dundee\" /fd sha256 /tr http://timestamp.digicert.com /td sha256 /v\""
	end
end

task :github do
	version = File.open('version') {|f| f.readline}
    puts "version: #{version}"
	branch = (ENV['BRANCH_SELECTOR'] || "origin/release/3.0.0.X").gsub(/origin\//, "")
	puts branch
	prerelease = branch.match(/master/) ? false : true	
	
	uri = URI.parse('https://api.github.com/repos/HicServices/RDMP/releases')
	body = { tag_name: "v#{version}", name: "RDMP v#{version}", body: ENV['MESSAGE'] || "Release version #{version}", target_commitish: branch, prerelease: prerelease }
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
	Dir.chdir("Tools/rdmp") do
		upload_to_github(upload_url, "rdmp-cli-win-x64.zip")
		upload_to_github(upload_url, "rdmp-cli-linux-x64.zip")
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

