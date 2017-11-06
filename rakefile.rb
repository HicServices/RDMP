require 'albacore'

load 'rakeconfig.rb'

task :ci_continuous, [:config] => [:setup_connection, :assemblyinfo, :deploy]

task :pluginbuild, [:folder] => [:assemblyinfo, :deployplugins]

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
    asminfoversion = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+(\.\d+)?/]
    
    major, minor, patch, build = asminfoversion.split(/\./)
    timestamp = Time.new.utc.to_i
    
    version = "#{major}.#{minor}.#{patch}"
    
    asm.version = version
    asm.file_version = version
    if PRERELEASE == "true"
        puts "version: #{version}-pre"
        asm.informational_version = "#{version}.#{timestamp}-pre"
    elsif CANDIDATE == "true"
        puts "version: #{version}-rc"
        asm.informational_version = "#{version}.#{timestamp}-rc"
    else
        puts "version: #{version}"
        asm.informational_version = "#{version}.#{timestamp}"
    end
end

task :deployplugins, [:folder] do |t, args|
    Dir.chdir('Plugin/Plugin') do
        sh "./build-and-deploy-local.bat #{args.folder}"
    end
end

# task :link do
# 	if File.exist?("DatabaseCreation.exe") 
# 		File.delete("DatabaseCreation.exe")
# 	end
# 	sh "call mklink DatabaseCreation.exe DatabaseCreation\\Release\\DatabaseCreation.exe"
# end
