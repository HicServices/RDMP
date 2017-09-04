require 'albacore'

load 'rakeconfig.rb'

task :ci_continuous, [:config] => [:setup_connection, :assemblyinfo, :deploy]

task :restorepackages do
    sh "nuget restore HIC.DataManagementPlatform.sln"
end

task :setup_connection do 
    File.open("Tests.Common/TestDatabases.txt", "w") do |f|
        f.write "ServerName:#{DBSERVER}\r\n"
        f.write "Prefix:#{DBPREFIX}\r\n"
    end
end

msbuild :build, [:config] => :restorepackages do |msb, args|
	args.with_defaults(:config => :Debug)
    msb.properties = { :configuration => args.config }
    msb.targets = [ :Clean, :Build ]   
    msb.solution = SOLUTION
end

msbuild :deploy, [:config] => :restorepackages do |msb, args|
	args.with_defaults(:config => :Release)
    msb.targets [ :Clean, :Build ]
    msb.properties = {
        :configuration => args.config,
        :outdir => "#{PUBLISH_DIR}/"
    }
    msb.solution = SOLUTION
end

desc "Sets the version number from GIT"    
assemblyinfo :assemblyinfo do |asm|
	asm.input_file = "SharedAssemblyInfo.cs"
    asm.output_file = "SharedAssemblyInfo.cs"
    version = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+(\.\d+)?/]
    describe = `git describe`.strip
    tag, rev, hash = describe.split(/-/)
    major, minor, patch, build = version.split(/\./)
    puts "version: #{major}.#{minor}.#{patch}.#{rev} build:#{build}"
    asm.version = "#{major}.#{minor}.#{patch}.#{rev}"
    asm.file_version = "#{major}.#{minor}.#{patch}.#{rev}"
    if PRERELEASE == "true"
        asm.informational_version = "#{major}.#{minor}.#{patch}.#{rev}-develop"
    else
        asm.informational_version = "#{major}.#{minor}.#{patch}.#{rev}"
    end
end

# task :link do
# 	if File.exist?("DatabaseCreation.exe") 
# 		File.delete("DatabaseCreation.exe")
# 	end
# 	sh "call mklink DatabaseCreation.exe DatabaseCreation\\Release\\DatabaseCreation.exe"
# end
