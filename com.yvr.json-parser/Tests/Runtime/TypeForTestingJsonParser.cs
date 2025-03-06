using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace YVR.JsonParser.Tests
{
    public class AccountClassWithSerializedPrivate
    {
#pragma warning disable CS0414
        private string email = "ABC";
        private bool active = true;
#pragma warning restore CS0414
    }

    public class AccountClass
    {
        public string email;
        public bool active;
        public List<string> roles = new List<string>();
    }

    public class AccountClassWithoutRoles
    {
        public string email;
        public bool active;
    }

    public class AccountClassWithGender
    {
        public string email;
        public bool active;
        public List<string> roles = new List<string>();
        public string gender;
    }

    public class DummyBaseClass { }

    public class AccountDerivedClassWithDiffName : DummyBaseClass
    {
        [JsonProperty("email")] public string emailDiff;

        [JsonProperty("active")] public bool activeDiff;

        [JsonProperty("roles")] public List<string> rolesDiff = new List<string>();
    }

    public class AccountClassWithDiffName
    {
        [JsonProperty("email")] public string emailDiff;

        [JsonProperty("active")] public bool activeDiff;

        [JsonProperty("roles")] public List<string> rolesDiff = new List<string>();
    }

    public class AppDetail
    {
        public string scover;
        public string brief;

        public string extraField = "ArbitraryValue";
    }

    public class DownloadInfo
    {
        [JsonProperty("isSystem")] public bool isSystem;
        [JsonProperty("totalSize")] public long fileSize;
        [JsonProperty("downloadSize")] public long downloadLength;
        [JsonProperty("appId")] public long appId;
        [JsonProperty("name")] public string appName;
        [JsonProperty("pkg")] public string packageName;
        [JsonProperty("status")] public int initStatus = -1;

        private bool Equals(DownloadInfo other)
        {
            if (other == null) return false;
            return isSystem == other.isSystem &&
                   fileSize == other.fileSize &&
                   downloadLength == other.downloadLength &&
                   appId == other.appId &&
                   appName == other.appName &&
                   packageName == other.packageName &&
                   initStatus == other.initStatus;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj.GetType() == GetType() && Equals(obj as DownloadInfo);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(isSystem, fileSize, downloadLength, appId, appName, packageName, initStatus)
                        .GetHashCode();
        }
    }
}