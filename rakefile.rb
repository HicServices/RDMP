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
   
    if PRERELEASE == "true"
        build = build.to_i + 1
        suffix = "-pre"
    elsif CANDIDATE == "true"
        build = build.to_i + 1
        suffix = "-rc"
    end

    version = "#{major}.#{minor}.#{patch}.#{build}"
    puts "version: #{version}"
    f = File.new('version', 'w')
    f.write "#{version}#{suffix}"
    asm.version = version
    asm.file_version = version
    asm.informational_version = "#{version}#{suffix}"
end

desc "Pushes the plugin packages into the specified folder"    
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
