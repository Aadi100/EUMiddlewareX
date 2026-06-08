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
        /// Fingerprint (backupnum 0-9), Photo (50), Password (10), Rfid card (11)
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

        public string setusername(string sn, int enrollid, string name)
        {
            return setuserinfo(sn, enrollid, name, null, 0, null);
        }

        public async Task<string> setface(string sn, int enrollid, string name, object record)
        {
            string faceData = record?.ToString();
            if (!string.IsNullOrEmpty(faceData))
            {
                // Attempt to convert if 'record' is a URL or local file path
                var converted = await CommonUtils.ConvertImageToBase64Async(faceData);
                if (converted != null) faceData = converted;
            }
            return setuserinfo(sn, enrollid, name, 50, 0, faceData);
        }

        public string setrfid(string sn, int enrollid, string name, object record)
        {
            return setuserinfo(sn, enrollid, name, 11, 0, record);
        }

        public string setfingerprint(string sn, int enrollid, string name, object record)
        {
            return setuserinfo(sn, enrollid, name, 0, 0, record);
        }

        /// <summary>
        /// 4. Delete user information
        /// backupnum: 0-9 fp, 10 password, 11 card, 12 all fp, 13 all (fp, card, pwd)
        /// </summary>
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
            var payload = new
            {
                cmd = "getusername",
                sn = sn,
                enrollid = enrollid
            };
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

        /// <summary>
        /// 9. Clean all users
        /// </summary>
        public string cleanuser(string sn)
        {
            var payload = new { cmd = "cleanuser", sn = sn };
            return JsonConvert.SerializeObject(payload);
        }

        /// <summary>
        /// 11. Get all logs
        /// </summary>
        public string getalllog(string sn, bool stn, string from = null, string to = null)
        {
            JObject payload = new JObject();
            payload["cmd"] = "getalllog";
            payload["sn"] = sn;
            payload["stn"] = stn;
            if (!string.IsNullOrEmpty(from)) payload["from"] = from;
            if (!string.IsNullOrEmpty(to)) payload["to"] = to;

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
        /// 2. Get user information
        /// backupnum: 0 fp, 10 password, 11 card, 50 photo
        /// </summary>
        public string getuserinfo(string sn, int enrollid, int backupnum)
        {
            var payload = new
            {
                cmd = "getuserinfo",
                sn = sn,
                enrollid = enrollid,
                backupnum = backupnum
            };
            return JsonConvert.SerializeObject(payload);
        }
    }

    public class UserRecord
    {
        [JsonProperty("enrollid")]
        public int EnrollId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
