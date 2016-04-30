using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickSharpApiClient.Common.Enums;
using QuickSharpApiClient.Feed.Files;
using System;
using System.IO; 

namespace QuickSharpApiClient.Tests
{
    [TestClass]
    public class ApiClientTests
    {
        [TestMethod()]
        public void Should_return_valid_temp_file_name() 
        {
            //setup
            var uri = new Uri("https://api.github.com/users/sachinkumarjain");

            //action
            var filename = FileHelper.CreateTempFile(uri, MethodType.Get);

            //assert
            //https____api.github.com_users_sachinkumarjain_Get.temp.apix
            var expectedFileName = Path.Combine(Path.GetTempPath(), string.Concat(uri.Host.Replace(":", "__").Replace("/", "_")), string.Concat(uri.AbsolutePath.Replace(":", "__").Replace("/", "_"), "_", "Get", ".temp.apix"));
            Assert.AreEqual(expectedFileName, filename);

        }
    }
}
