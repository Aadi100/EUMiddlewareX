using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using System.Threading.Tasks;

namespace smart_attendance_middleware
{
    public class WebServerServices
    {
        /// <summary>
        /// 3. Send user information
        /// backupnum: 0-9 fp, 10 password, 11 card, 50 photo
        /// </summary>
        public string setuserinfo(string sn, int? enrollid, string name, int? backupnum, int? admin, object record)
        {
            var payload = new
            {
                cmd = "setuserinfo",
                sn = sn,
                enrollid = enrollid,
                name = name,
                backupnum = backupnum,
                admin = admin,
                record = record
            };
            return JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// 6. Set user name (Batch update)
        /// </summary>
        public string setusername(string sn, List<UserRecord> users)
        {
            var payload = new
            {
                cmd = "setusername",
                sn = sn,
                count = users.Count,
                record = users
            };
            return JsonConvert.SerializeObject(payload);
        }

        // Wrapper for single user name update
        public string setusername(string sn, int enrollid, string name)
        {
            return setusername(sn, new List<UserRecord> { new UserRecord { EnrollId = enrollid, Name = name } });
        }

        public async Task<string> setface(string sn, int enrollid, string name, object record)
        {
            string faceData = record?.ToString();
            if (!string.IsNullOrEmpty(faceData) && !faceData.StartsWith("/"))
            {
                var converted = await CommonUtils.ConvertImageToBase64Async(faceData);
                if (converted != null) faceData = converted;
            }
            return setuserinfo(sn, enrollid, name, 50, 0, faceData);
        }

        public string setrfid(string sn, int enrollid, string name, object record)
        {
            return setuserinfo(sn, enrollid, name, 11, 0, record);
        }

        public string setfingerprint(string sn, int enrollid, string name, object record, int fingerNum = 0)
        {
            return setuserinfo(sn, enrollid, name, fingerNum, 0, record);
        }

        /// <summary>   
        /// 4. Delete user information
        /// backupnum: 0-9 fp, 10 password, 11 card, 12 
        public string deleteuser(string sn, int enrollid, int backupnum)
        {
            var payload = new
            {
                cmd = "deleteuser",
                sn = sn,
                enrollid = enrollid,
                backupnum = backupnum
            };
            return JsonConvert.SerializeObject(payload);
        }

        /// <summary>
        /// 5. Get user name
        /// </summary>
        public string getusername(string sn, int enrollid)
        {
            var payload = new { cmd = "getusername", sn = sn, enrollid = enrollid };
            return JsonConvert.SerializeObject(payload);
        }

        /// <summary>
        /// 7 & 8. Enable/Disable user
        /// enflag: 1 to enable, 0 to disable
        /// </summary>
        public string enableuser(string sn, int enrollid, int enflag)
        {
            var payload = new
            {
                cmd = "enableuser",
                sn = sn,
                enrollid = enrollid,
                enflag = enflag
            };
            return JsonConvert.SerializeObject(payload);
        }

        // Wrapper for Enable (Protocol item 7)
        public string set_enable(string sn, int enrollid)
        {
            return enableuser(sn, enrollid, 1);
        }

        // Wrapper for Disable (Protocol item 8)
        public string set_disable(string sn, int enrollid)
        {
            // Note: Protocol doc snippet used 'enrolled' in one place and 'enrollid' in another.
            // Consistent with the rest of the commands, we use 'enrollid'.
            return enableuser(sn, enrollid, 0);
        }

        /// <summary>
        /// 9. Clean all users
        /// </summary>
        public string cleanuser(string sn)
        {
            var payload = new { cmd = "cleanuser", sn = sn };
            return JsonConvert.SerializeObject(payload);
        }

        /// <summary>
        /// Requests all logs from a device (stn hardcoded to true = incremental/new logs only).
        /// </summary>
        public string getalllogs(string sn)
        {
            var payload = new JObject();
            payload["cmd"] = "getalllog";
            payload["sn"]  = sn;
            payload["stn"] = true;
            return payload.ToString(Formatting.None);
        }

        /// <summary>
        /// 12. Clean all logs
        /// </summary>
        public string cleanlog(string sn)
        {
            var payload = new { cmd = "cleanlog", sn = sn };
            return JsonConvert.SerializeObject(payload);
        }

        /// <summary>
        /// 19. Open door
        /// </summary>
        public string opendoor(string sn, int? doornum = null)
        {
            JObject payload = new JObject();
            payload["cmd"] = "opendoor";
            payload["sn"] = sn;
            if (doornum.HasValue) payload["doornum"] = doornum.Value;
            return payload.ToString(Formatting.None);
        }

        /// <summary>
        /// 2. Get user information.
        /// backupnum: 0-9 fingerprint slot, 10 password, 11 card, 50 photo
        /// </summary>
        public string getuserinfo(string sn, int enrollid, int? backupnum = null)
        {
            var payload = new { cmd = "getuserinfo", sn = sn, enrollid = enrollid, backupnum = backupnum ?? 0 };
            return JsonConvert.SerializeObject(payload);
        }

        public string getuserdetails(string sn, int enrollid, int backupnum)
        {
            return getuserinfo(sn, enrollid, backupnum);
        }

    }

    public class UserRecord
    {
        [JsonProperty("enrollid")]
        public int EnrollId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class GetUserInfoResponse
    {
        public string Sn        { get; set; }
        public int    EnrollId  { get; set; }
        public string Name      { get; set; }
        public int    BackupNum { get; set; }
        public int    Admin     { get; set; }
        public string Record    { get; set; }
    }

}