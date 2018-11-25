using System;
using System.Collections.Generic;
using System.Text;

namespace YTUploader.Core
{
    public struct PrivacyStatus
    {
        public static PrivacyStatus Private { get; }
        public static PrivacyStatus Public { get; }
        public static PrivacyStatus Unlisted { get; }

        static PrivacyStatus()
        {
            Private = new PrivacyStatus("private");
            Public = new PrivacyStatus("public");
            Unlisted = new PrivacyStatus("unlisted");
        }

        private readonly string status;

        private PrivacyStatus(string status)
            => this.status = status;

        public override string ToString() => status;

        public static implicit operator string(PrivacyStatus privacyStatus) 
            => privacyStatus.ToString();
    }
}
