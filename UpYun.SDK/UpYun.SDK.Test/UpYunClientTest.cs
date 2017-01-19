using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UpYun.SDK;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class UpYunClientTest
    {
        
        string bicketName = "";
        string userName = "";
        string password = "";
        [TestMethod]
        public void 创建分段上传()
        {
            UpYunClient client = new UpYunClient(bicketName, userName, password);
            var task = client.MulitUploadInitiate("/testdir/xx.mp4", 1024 * 1024 * 100, "video/mp4", new Dictionary<string, string>() { { "X-Upyun-Meta-FileName", "xxx.mp4" } });

            var ss = task.Result;
            Console.WriteLine(ss.FileId + "---" + ss.PartId);
        }

        [TestMethod]
        public void 分段上传()
        {
            //固定上传分片1M
            UpYunClient client = new UpYunClient(bicketName, userName, password);
            var ss = client.MulitUploadPart("/testdir/xx.mp4", new UpYun.SDK.Model.MulitUploadResModel()
            {
                FileId = "1017aff1-1139-4859-9a50-c24dc99a46a1",
                PartId = "0"
            }, new byte[1048576]).Result;
            Console.WriteLine(ss.FileId + "---" + ss.PartId);

        }

        [TestMethod]
        public void 分段上传完成()
        {
            UpYunClient client = new UpYunClient(bicketName, userName, password);
            var ss = client.MulitUploadComplete("/testdir/xx.mp4", new UpYun.SDK.Model.MulitUploadResModel()
            {
                FileId = "1017aff1-1139-4859-9a50-c24dc99a46a1"
            }).Result;
            Console.WriteLine(ss.FileId + "---" + ss.PartId);
        }

        [TestMethod]
        public void 上传图片()
        {
            var cndHost = "http://note-video.b0.upaiyun.com";
            UpYunClient client = new UpYunClient(bicketName, userName, password);

            client.Upload("/a/b/c/tesr.png", new byte[] { 1, 2, 3, 4 }).GetAwaiter().GetResult();
        }
    }
}
