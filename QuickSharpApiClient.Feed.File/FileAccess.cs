
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
        void Replace(string request, string response);
        void Create();
    }

    public class FileAccess : IDataAccess
    {
        private string _baseDirectory;
        public string BaseDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_baseDirectory))
                {
                    _baseDirectory = Path.GetTempPath();
                }
                return _baseDirectory;
            }
            set { _baseDirectory = value; }
        }

        public bool IsDefaultTempPath { get { return BaseDirectory == Path.GetTempPath(); } }

        public string FilePath { get { return Path.Combine(BaseDirectory, FileHelper.ReplaceSpecialChars(ApiUri.Host)); } }
        public string FileName
        {
            get
            {
                return Path.Combine(FilePath, string.Concat(FileHelper.ReplaceSpecialChars(ApiUri.AbsolutePath), "_", Method.ToString(), ".temp.apix")).ToLowerInvariant();
            }
        }
        public Uri ApiUri { get; set; }
        public MethodType? Method { get; set; }

        public FileAccess()
        {
        }

        public void Create()
        {
            FileHelper.CreateFile(FilePath, FileName);

            System.Diagnostics.Debug.Print("File Created at " + FileName);
        }

        public async Task<string> Read(string request = null)
        {
            try
            {
                return await FileHelper.ReadResponse(this.FileName, request);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public void Save(string request, string response)
        {
            Create();

            FileHelper.SaveResponse(this.FileName, request, response);
        }

        public void Replace(string request, string response)
        {
            FileHelper.ReplaceResponse(this.FileName, request, response);
        }
    }
}
