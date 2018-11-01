using System;
using System.IO;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CsvHelper;
using CsvHelper.Configuration;
using LoadModules.Generic.Exceptions;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowSources.SubComponents
{

    /// <summary>
    /// This class is a sub component of <see cref="DelimitedFlatFileDataFlowSource"/>, it is responsible for responding to errors processing the file
    /// being loaded according to the <see cref="BadDataHandlingStrategy"/>. It also includes settings for how to respond to empty files.
    /// </summary>
    public class FlatFileEventHandlers
    {
        private readonly FlatFileToLoad _fileToLoad;
        private readonly FlatFileToDataTablePusher _dataPusher;
        private readonly bool _throwOnEmptyFiles;
        private readonly BadDataHandlingStrategy _strategy;
        private readonly IDataLoadEventListener _listener;
        private int _maximumErrorsToReport;

        /// <summary>
        /// File where we put error rows
        /// </summary>
        public FileInfo DivertErrorsFile;

        public FlatFileEventHandlers(FlatFileToLoad fileToLoad, FlatFileToDataTablePusher dataPusher, bool throwOnEmptyFiles, BadDataHandlingStrategy strategy, IDataLoadEventListener listener, int maximumErrorsToReport)
        {
            _fileToLoad = fileToLoad;
            _dataPusher = dataPusher;
            _throwOnEmptyFiles = throwOnEmptyFiles;
            _strategy = strategy;
            _listener = listener;
            _maximumErrorsToReport = maximumErrorsToReport;
        }

        public void FileIsEmpty()
        {
            if (_throwOnEmptyFiles)
                throw new FlatFileLoadException("File " + _fileToLoad + " is empty");

            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "File " + _fileToLoad + " is empty"));
        }
        
        public void ReadingExceptionOccurred(CsvHelperException obj)
        {
            var line = new FlatFileLine(obj.ReadingContext);

            switch (_strategy)
            {
                case BadDataHandlingStrategy.IgnoreRows:
                    if (_maximumErrorsToReport-- >0)
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Ignored ReadingException on " + line.GetLineDescription(), obj));

                    //move to next line
                    _dataPusher.BadLines.Add(obj.ReadingContext.RawRow);

                    break;
                case BadDataHandlingStrategy.DivertRows:

                    DivertErrorRow(new FlatFileLine(obj.ReadingContext), obj);
                    break;

                case BadDataHandlingStrategy.ThrowException:
                    throw new FlatFileLoadException("Bad data found on li" + line.GetLineDescription(), obj);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void BadDataFound(FlatFileLine line)
        {
            switch (_strategy)
            {
                case BadDataHandlingStrategy.IgnoreRows:

                    if (_maximumErrorsToReport-- > 0)
                        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Ignored BadData on " + line.GetLineDescription()));

                    //move to next line
                    _dataPusher.BadLines.Add(line.LineNumber);

                    break;
                case BadDataHandlingStrategy.DivertRows:
                    DivertErrorRow(line, null);
                    break;

                case BadDataHandlingStrategy.ThrowException:
                    throw new FlatFileLoadException("Bad data found on " + line.GetLineDescription());


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void DivertErrorRow(FlatFileLine line, Exception ex)
        {
            if (DivertErrorsFile == null)
            {
                DivertErrorsFile = new FileInfo(Path.Combine(_fileToLoad.File.Directory.FullName, Path.GetFileNameWithoutExtension(_fileToLoad.File.Name) + "_Errors.txt"));

                //delete any old version
                if (DivertErrorsFile.Exists)
                    DivertErrorsFile.Delete();
            }

            if (_maximumErrorsToReport-- > 0)
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Diverting Error on " + line.GetLineDescription() + " to '" + DivertErrorsFile.FullName + "'", ex));

            File.AppendAllText(DivertErrorsFile.FullName, line.RawRecord);

            //move to next line
            _dataPusher.BadLines.Add(line.LineNumber);
        }

        public void RegisterEvents(IReaderConfiguration configuration)
        {
            configuration.BadDataFound = s=>BadDataFound(new FlatFileLine(s));
            configuration.ReadingExceptionOccurred = ReadingExceptionOccurred;
        }
    }
}
