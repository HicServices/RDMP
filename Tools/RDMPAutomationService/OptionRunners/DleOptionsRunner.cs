using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using RDMPAutomationService.Logic.DLE;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.OptionRunners
{
    class DleOptionsRunner
    {
        public int Run(DleOptions opts)
        {
            opts.LoadFromAppConfig();
            var locator = opts.DoStartup();

            switch (opts.Command)
            {
                case DLECommands.run:

                    AutomatedDLELoad load = null;
                    if(opts.LoadMetadata != 0)
                    {
                        var lmd = locator.CatalogueRepository.GetObjectByID<LoadMetadata>(opts.LoadMetadata);
                        load = new AutomatedDLELoad(lmd);
                        
                    }
                    
                    if (load != null)
                        load.RunTask(locator);
                    else
                        Console.WriteLine("No load run");

                    return 0;
                case DLECommands.list:

                    Console.WriteLine(string.Format("[ID] - Name"));

                    if (opts.LoadMetadata != 0)
                    {
                        var l = locator.CatalogueRepository.GetObjectByID<LoadMetadata>(opts.LoadMetadata);
                        Console.WriteLine("[{0}] - {1}", l.ID, l.Name);
                    }
                    else
                    {
                        foreach (LoadMetadata l in locator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                            Console.WriteLine("[{0}] - {1}", l.ID, l.Name);
                    }
                    
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
