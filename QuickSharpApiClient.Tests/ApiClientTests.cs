using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickSharpApiClient.Common.Enums;
using QuickSharpApiClient.Feed.Files;
using System;

namespace QuickSharpApiClient.Tests
{
    [TestClass]
    public class ApiClientTests
    {
        [TestMethod()]
        public void Should_return_valid_temp_file_name() 
        {
            var fa = new FileAccess();
            //setup
            fa.ApiUri = new Uri("https://api.github.com/users/sachinkumarjain");
            fa.Method = MethodType.Get;

            //action
            var filename = fa.FileName;

            //assert
            //https____api.github.com_users_sachinkumarjain_Get.temp.apix
            var expectedFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), string.Concat(fa.ApiUri.Host.Replace(":", "__").Replace("/", "_")), string.Concat(fa.ApiUri.AbsolutePath.Replace(":", "__").Replace("/", "_"), "_", "Get", ".temp.apix"));
            Assert.AreEqual(expectedFileName.ToLowerInvariant(), filename);

        }
    }
}
