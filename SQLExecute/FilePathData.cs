using System;

namespace ScriptRunner
{
    public class FilePathData
    {
        public FileRunStatus fileRunStatus = FileRunStatus.NotRun;
        public Guid uid;
        public string fullFileName;

        public string fileName()
        {
            if (!string.IsNullOrEmpty(this.fullFileName))
                return this.fullFileName.Remove(0, this.fullFileName.LastIndexOf("\\") + 1);
            else
                return "";
        }
    }
}
