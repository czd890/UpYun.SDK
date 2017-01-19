using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpYun.SDK.Model
{
    public class MulitUploadResModel
    {
        public string FileId { get; set; }
        public string PartId { get; set; }
        public int PartSize { get; internal set; }
    }
}
