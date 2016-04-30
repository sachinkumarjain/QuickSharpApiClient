
using QuickSharpApiClient.Common.Enums;
using System;
using System.IO;
using System.Threading.Tasks;
namespace QuickSharpApiClient.Feed.Files
{
    public interface IDataAccess
    {
        Task<string> Read(string request = null);
        void Save(string request, string response);
    }

    public class FileAccess : IDataAccess
    {
        private string _baseDirectory;
        public string BaseDirectory
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_baseDirectory))
                {
                    _baseDirectory = Path.GetTempPath();
                }
                return _baseDirectory;
            }
            set { _baseDirectory = value; }
        }
        public string FilePath { get { return Path.Combine(BaseDirectory, FileHelper.ReplaceSpecialChars(ApiUri.Host)); } }
        public string FileName { get { return Path.Combine(FilePath, string.Concat(FileHelper.ReplaceSpecialChars(ApiUri.AbsolutePath), "_", Method.ToString(), ".temp.apix")); } }
        public Uri ApiUri { get; set; }
        public MethodType Method { get; set; }

        public FileAccess()
        {
        }

        private void CreateDirectory()
        {
            if (!Directory.Exists(BaseDirectory))
            {
                try
                {
                    var dir = new DirectoryInfo(BaseDirectory);
                    dir.Create();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Unable to create file path to storage", ex);
                }
            }
        }

        public async Task<string> Read(string request = null)
        {
            return await FileHelper.ReadResponse(this.FileName, request);
        }

        public void Save(string request, string response)
        {
            CreateDirectory();

           FileHelper.SaveResponse(this.FileName, request, response);
        }
    }
}
