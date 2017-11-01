using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QRCodeCreator
{
    public class QRCodeModel
    {
        public Guid ID { get; set; }
        
        public string Content { get; set; }

        public string CharacterSet {get;set;}

        public ZXing.QrCode.Internal.ErrorCorrectionLevel ErrorCorrectionLevel { get; set; }

        public byte[] BitmapByteArray { get; set; }

        public string GroupName { get; set; }

        public DateTime UpdatedTime { get; set; }

    }
}
