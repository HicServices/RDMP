require 'rexml/document'

doc = REXML::Document.new File.new("ResearchDataManagementPlatform.csproj")
doc.context[:attribute_quote] = :quote
newgroup = doc.root.add_element 'ItemGroup'
doc.root.elements.each('ItemGroup/PublishFile') do |elem| 
    newpublish = newgroup.add_element elem.clone
    newpublish.attributes["Include"] = elem.attributes["Include"].gsub(/\.pdb/,'.xml')
    elem.elements.each() do |prop|
        newprop = newpublish.add_element prop.clone
        if prop.has_text? then
            newprop.text = prop.text
        end
    end
end

printer = REXML::Formatters::Pretty.new(2)
printer.compact = true
printer.write(doc, File.new("ResearchDataManagementPlatform.csproj", "w"))

fixed = File.read("ResearchDataManagementPlatform.csproj")
            .gsub(/\>\s/,'>')
            .gsub(/\s\<\//,'</')

puts fixed
