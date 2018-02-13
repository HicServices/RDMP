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

desc "Pushes the plugin packages into the specified folder"    
task :deployplugins, [:folder,:version] do |t, args|
    asminfoversion = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+(\.\d+)?/]
        
    major, minor, patch, build = asminfoversion.split(/\./)
   
    build = build.to_i + 1
    suffix = "-alpha"
    
    version = "#{major}.#{minor}.#{patch}.#{build}"
    puts "version: #{version}#{suffix}"
    
    Dir.chdir('Plugin/Plugin') do
        sh "./build-and-deploy-local.bat #{args.folder} '' #{version}#{suffix}"
    end
end

desc "Get the deploy and min-version for the app"    
task :publishall, [:folder, :url] do |t, args|
    asminfoversion = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+\.\d+(\-\w+)+/]
    
    if (asminfoversion.nil?) then
        asminfoversion = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+\.\d+/]
    end
    
    major, minor, patch, build, suffix = asminfoversion.split(/[\.\-]/)
    
    if (suffix)
        version = "#{major}.#{minor}.#{patch}.#{build}-#{suffix}"
    else
        version = "#{major}.#{minor}.#{patch}.#{build}"
    end
    
    minversion = "#{major}.#{minor}.#{patch}.0"
    
    puts "version: #{version}"
    puts "minversion: #{minversion}"
    
    Dir.chdir('Application/ResearchDataManagementPlatform') do
        #sh "./publish.bat #{version} #{minversion} #{args.folder}#{version}\\ #{args.url}"
    end
    
    # reset symlinks:
    File.delete "#{args.folder}ResearchDataManagementPlatform.application" if File.exists?("#{args.folder}ResearchDataManagementPlatform.application")
    sh "rd \"#{args.folder}Application Files\"" if Dir.exists?("#{args.folder}Application Files")
    #args.folder.gsub!("/","\\")
    #puts args.folder
    sh "call mklink \"#{args.folder}ResearchDataManagementPlatform.application\" \"#{args.folder}#{version}/ResearchDataManagementPlatform.application\""
    sh "call mklink /J \"#{args.folder}Application Files\" \"#{args.folder}#{version}/Application Files\""
end


# task :link do
# 	if File.exist?("DatabaseCreation.exe") 
# 		File.delete("DatabaseCreation.exe")
# 	end
# 	sh "call mklink DatabaseCreation.exe DatabaseCreation\\Release\\DatabaseCreation.exe"
# end
