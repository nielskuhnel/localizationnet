using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Localization.Net.Exceptions;

namespace Localization.Net.Maintenance.Extraction
{
    //TODO: Make cool like MSBuild and support /**/foo/*.cs like constructs
    public class SourceFileList
    {

        private string _rootDir;
        public string RootDir
        {
            get { return _rootDir; }
            set
            {
                _rootDir = value; 
                if (!_rootDir.EndsWith("\\")) RootDir += "\\";
            }
        }

        
        public List<string> Extensions { get; set; }

        public SourceFileList(string rootDir, IEnumerable<string> extensions, IEnumerable<string> excludePatterns)
        {
            RootDir = rootDir;
            Extensions = new List<string>(extensions ?? new string[0]);
            ExcludePatterns = new List<string>(excludePatterns ?? new string[0]);
        }
        
        /// <summary>
        /// Files where the relative path starts with any of these will not be included
        /// </summary>
        public List<string> ExcludePatterns { get; set; }        

        public IEnumerable<SourceFile> GetFiles()
        {            
            var files = new List<SourceFile>();
            ProcessDirectory(new DirectoryInfo(_rootDir), files);
            return files;
        }

        void ProcessDirectory(DirectoryInfo dir, List<SourceFile> files)
        {
            foreach (var info in dir.GetFileSystemInfos())
            {
                if (ExcludePatterns != null && ExcludePatterns.Any(x => info.FullName.Contains(_rootDir + x)))
                {
                    continue;
                }
                var f = info as FileInfo;
                if (f != null)
                {
                    if (Extensions.Any(x => f.Extension.EndsWith(x)))
                    {
                        files.Add(new SourceFile
                        {
                            RelativePath = f.FullName.Substring(_rootDir.Length),
                            AbsolutePath = f.FullName,
                            Contents = File.ReadAllText(f.FullName)
                        });                        
                    }
                }
                else
                {
                    ProcessDirectory((DirectoryInfo)info, files);                    
                }
            }
        }

        #region Yeah. Thanks http://mrpmorris.blogspot.com/2007/05/convert-absolute-path-to-relative-path.html
        //This is currently not used but will definitely become handy if an IncludePatterns / ExcludePatterns scheme is implemented
        private string RelativePath(string absolutePath, string relativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split('\\');
            string[] relativeDirectories = relativeTo.Split('\\');

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                throw new LocalizedArgumentException("Exceptions.PathCommonBase", "Paths do not have a common base");

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }
        #endregion
    }

    public class SourceFile
    {
        public string AbsolutePath { get; set; }
        public string RelativePath { get; set; }

        public string Contents { get; set; }
    }
}
