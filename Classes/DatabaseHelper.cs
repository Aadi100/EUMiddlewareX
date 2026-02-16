using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static smart_attendance_middleware.Classes.DeviceFunctions;


namespace smart_attendance_middleware
{
    // A simple model for Member
    public class Member
    {
        public int MemberID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string RFID { get; set; }
        public string Picture { get; set; }
        public int? EnrollmentID { get; set; }
        public bool Fingerprint { get; set; }
        public string PackageExpiry { get; set; }
        public Dictionary<int, bool> SyncStatus { get; set; } = new Dictionary<int, bool>();
    }

    public class Staff
    {
        public string StaffID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string RFID { get; set; }
        public string Picture { get; set; }
        public bool Fingerprint { get; set; }
        public int? EnrollmentID { get; set; }
        public Dictionary<int, bool> SyncStatus { get; set; } = new Dictionary<int, bool>();
    }


    // LogEntry model for attendance logs
    public class LogEntry
    {
        public int Id { get; set; }
        public int MemberID { get; set; }
        public int DeviceID { get; set; }
        public int EnrollmentID { get; set; }  // Keep enrollment ID
        public Dictionary<string, string> BiometricData { get; set; }
        public Dictionary<string, bool> SyncStatus { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }

    // StaffLog model 
    public class StaffLogEntry
    {
        public int Id { get; set; }
        public string StaffID { get; set; }
        public int StaffDeviceId { get; set; }
        public int? EnrollmentID { get; set; }
        public Dictionary<string, string> BiometricData { get; set; }
        public Dictionary<string, bool> SyncStatus { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }


    //Abdul Hadi Code
    public class Device
    {

        public int StaffDeviceId { get; set; }
        public int DeviceId { get; set; }
        public string Department { get; set; }
        public bool AttendanceType { get; set; }

        public string StaffAttendanceType { get; set; }
        public string DeviceSerial { get; set; }
        public string DeviceIP { get; set; }
        public int Port { get; set; }
        public string TimeZone { get; set; }
        public string Location { get; set; }
        public string ConnectionType { get; set; }
        public bool IsActive { get; set; }
        public string LastSyncTime { get; set; }
        public string Password { get; set; }

        public int FaceCapacity { get; set; }
        public int RFIDCapacity { get; set; }
        public int FingerCapacity { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }

        public int MachineNo { get; set; }

    }

    public class LocalUser
    {
        public int LocalUserID { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string BackupCode { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }

    public class FingerprintData
    {
        public int EnrollNumber { get; set; }      // User ID
        public int FingerNumber { get; set; }      // Which finger (0-9)
        public int Privilege { get; set; }         // Access level
        public byte[] TemplateData { get; set; }   // Binary template data
        public string TemplateString { get; set; } // Text template data
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }


    // Helper class to track biometric changes
    public class BiometricChanges
    {
        public bool HasChanges { get; set; }
        public bool FingerprintChanged { get; set; }
        public bool RfidChanged { get; set; }
        public bool PictureChanged { get; set; }
    }

    // Class to represent unsynced attendance logs
    public class UnsyncedAttendanceLog
    {
        public int LogId { get; set; }
        public int MemberId { get; set; }
        public int EnrollmentId { get; set; }

        public string AttendanceId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public bool NeedsCheckInSync { get; set; }
        public bool NeedsCheckOutSync { get; set; }
    }

    
        public class LogBiometricChange
        {
            public string BiometricType { get; set; }
            public int DeviceId { get; set; }
            public int? EnrollmentId { get; set; }
        }

    // Updated GLog record model
    public class GLogRecord
    {
        public int LogId { get; set; }
        public int DeviceId { get; set; }
        public string EnrollmentNumber { get; set; }
        public string DeviceTimeText { get; set; } // Store as string exactly from file
        public string OriginalLine { get; set; }
        public bool Sync { get; set; }
        public string DeviceType { get; set; } // "check-in" or "check-out"
        public bool Filter { get; set; } // Boolean filter field
        public string CreatedOn { get; set; }
        public string UpdatedOn { get; set; }

        // Helper property to convert string to DateTime when needed
        public DateTime DeviceTime
        {
            get
            {
                if (DateTime.TryParse(DeviceTimeText, out DateTime result))
                    return result;
                return DateTime.MinValue;
            }
        }
    }

    public class GateAccessed
    {
        public int GateId { get; set; }
        public string DeviceIP { get; set; }
        public int MemberId { get; set; }
        public bool IsVisitor { get; set; }
        public string VisitorName { get; set; }
        public string VisitorPhone { get; set; }
        public string Reason { get; set; }
        public DateTime AccessedTime { get; set; }
        public string Username { get; set; }
    }

    public class DatabaseHelper
    {

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["SmartAttendance"]?.ConnectionString;
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6");


        /// <summary>
        /// Initializes the SQLite database by creating the file (if needed) and the necessary tables.
        /// Tables:
        ///  - Settings: to store key-value pairs (e.g., APIKey, TimeZone, VersionTimestamp)
        ///  - Members: to store member details (MemberID, Name, Status, RFID, Picture, MachineID)
        ///  - AttendanceLogs: to store attendance records (LogID, MemberID, CheckInTime, CheckOutTime, Synced)
        /// </summary>
        public void InitializeDatabase()
        {

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Create the Settings table.
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT,
                        CreatedAt TEXT,
                        UpdatedAt TEXT
                    );";
                    command.ExecuteNonQuery();
                }

                // Create the Members table.
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Members (
                    MemberID INTEGER PRIMARY KEY,
                    Name TEXT,
                    Status TEXT,
                    RFID TEXT,
                    Picture TEXT,
                    Fingerprint BOOLEAN, 
                    EnrollmentID INTEGER,
                    PackageExpiry TEXT,
                    SyncStatus TEXT,
                    CreatedAt TEXT,
                    UpdatedAt TEXT
                );";
                    command.ExecuteNonQuery();
                }

                //Create the Staff table.
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS Staff (
        StaffID TEXT PRIMARY KEY,
        Name TEXT,
        Status TEXT,
        RFID TEXT,
        Picture TEXT,
        Fingerprint BOOLEAN,
        EnrollmentID INTEGER,
        SyncStatus TEXT,
        CreatedAt TEXT,
        UpdatedAt TEXT
    );";
                    command.ExecuteNonQuery();
                }

                // Create the AttendanceLogs table.
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS AttendanceLogs (
                        LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                        MemberID INTEGER,
                        AttendanceId TEXT,
                        CheckInTime TEXT,
                        CheckOutTime TEXT,
                        SyncStatus TEXT,-- JSON: { ""checkIn"": bool, ""checkOut"": bool }
                        CreatedAt TEXT,
                        UpdatedAt TEXT,
                        FOREIGN KEY(MemberID) REFERENCES Members(MemberID)
                    );";
                    command.ExecuteNonQuery();
                }

                //Abdul Hadi Code
                //Create the LocalUsers table.
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS LocalUsers (
                        LocalUserID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserName TEXT UNIQUE NOT NULL,
                        Role TEXT CHECK(Role IN ('admin', 'user', 'developer')) NOT NULL,
                        Password TEXT NOT NULL,
                        BackupCode TEXT NOT NULL,
                        CreatedAt TEXT,
                        UpdatedAt TEXT
                    );";
                    command.ExecuteNonQuery();
                }


                //Abdul Hadi Code
                // Create the Devices table.
                // 1 for check_in, 0 for check_out
                //unique add karna ip mai
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS Devices (
                        DeviceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Department VARCHAR(100) NULL,
                        AttendanceType BOOLEAN,        
                        DeviceSerial VARCHAR(20),
                        DeviceIP VARCHAR(50) UNIQUE NOT NULL,
                        Port INTEGER,
                        MachineNo INTEGER UNIQUE ,
                        TimeZone TEXT,
                        Location VARCHAR(100),
                        ConnectionType VARCHAR(20),
                        IsActive BOOLEAN,
                        LastSyncTime TEXT NULL,
                        Password TEXT NOT NULL,
                        FaceCapacity INTEGER NULL,
                        RFIDCapacity INTEGER NULL,
                        FingerCapacity INTEGER NULL,
                        CreatedAt TEXT,
                        UpdatedAt TEXT
                    );";
                    command.ExecuteNonQuery();
                }

                // Create the  Fingerprints table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS Fingerprints (
        EnrollNumber INTEGER PRIMARY KEY,        
        FingerNumber INTEGER NOT NULL,          
        Privilege INTEGER DEFAULT 0,              
        TemplateData BLOB,                        
        TemplateString TEXT,                      
        CreatedAt TEXT,
        UpdatedAt TEXT
    );";
                    command.ExecuteNonQuery();
                }


                // Create the GLog table for storing raw glog file data
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS GLog (
        LogId INTEGER PRIMARY KEY AUTOINCREMENT,
        DeviceId INTEGER NOT NULL,
        EnrollmentNumber TEXT NOT NULL,
        DeviceTime TEXT NOT NULL,
        OriginalLine TEXT NOT NULL,
        Sync BOOLEAN DEFAULT 0,
        DeviceType TEXT,
        Filter BOOLEAN DEFAULT 0,
        CreatedOn TEXT,
        UpdatedOn TEXT,
        FOREIGN KEY (DeviceId) REFERENCES Devices(DeviceId)
    );";
                    command.ExecuteNonQuery();
                }


                // Create the Sync Log table.
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS Log (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        MemberID INTEGER NOT NULL,
        DeviceID INTEGER NOT NULL,
        EnrollmentID INTEGER,
        BiometricData TEXT,
        SyncStatus TEXT,
        CreatedAt TEXT,
        UpdatedAt TEXT,
        FOREIGN KEY (MemberID) REFERENCES Members(MemberID),
        FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceId)
    );";
                    command.ExecuteNonQuery();
                }

                // Create the StaffDevices table 
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS StaffDevices (
        StaffDeviceId INTEGER PRIMARY KEY AUTOINCREMENT,
        Department VARCHAR(100) NULL,
        StaffAttendanceType TEXT,         -- keep TEXT
        DeviceSerial VARCHAR(20),
        DeviceIP VARCHAR(50) UNIQUE NOT NULL,
        Port INTEGER,
        MachineNo INTEGER UNIQUE,
        TimeZone TEXT,
        Location VARCHAR(100),
        ConnectionType VARCHAR(20),
        IsActive BOOLEAN,
        LastSyncTime TEXT NULL,
        Password TEXT NOT NULL,
        FaceCapacity INTEGER NULL,
        RFIDCapacity INTEGER NULL,
        FingerCapacity INTEGER NULL,
        CreatedAt TEXT,
        UpdatedAt TEXT
    );";
                    command.ExecuteNonQuery();
                }

                // Create the Staff Sync Log table 
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS StaffLog (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        StaffID TEXT NOT NULL,
        StaffDeviceId INTEGER NOT NULL,
        EnrollmentID INTEGER,
        BiometricData TEXT,
        SyncStatus TEXT,
        CreatedAt TEXT,
        UpdatedAt TEXT,
        FOREIGN KEY (StaffID) REFERENCES Staff(StaffID),
        FOREIGN KEY (StaffDeviceId) REFERENCES StaffDevices(StaffDeviceId)
    );";
                    command.ExecuteNonQuery();
                }

                // Create the StaffGLog table (like GLog, for staff)
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS StaffGLog (
        LogId INTEGER PRIMARY KEY AUTOINCREMENT,
        StaffDeviceId INTEGER NOT NULL,
        EnrollmentNumber TEXT NOT NULL,
        DeviceTime TEXT NOT NULL,
        OriginalLine TEXT NOT NULL,
        Sync BOOLEAN DEFAULT 0,
        DeviceType TEXT,
        Filter BOOLEAN DEFAULT 0,
        CreatedOn TEXT,
        UpdatedOn TEXT,
        FOREIGN KEY (StaffDeviceId) REFERENCES StaffDevices(StaffDeviceId)
    );";
                    command.ExecuteNonQuery();
                }

                // Create the StaffAttendanceLog table (no MemberID, no SyncStatus, use EnrollmentID)
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS StaffAttendanceLog (
        LogID INTEGER PRIMARY KEY AUTOINCREMENT,
        EnrollmentID INTEGER NOT NULL,
        AttendanceId TEXT,
        Name TEXT,
        CheckInTime TEXT,
        CheckOutTime TEXT,
        CreatedAt TEXT,
        UpdatedAt TEXT
    );";
                    command.ExecuteNonQuery();
                }

                // Create the GateAccessed table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
    CREATE TABLE IF NOT EXISTS GateAccessed (
        GateId INTEGER PRIMARY KEY AUTOINCREMENT,
        DeviceIP TEXT NOT NULL,
        MemberId INTEGER NOT NULL,
        IsVisitor BOOLEAN NOT NULL DEFAULT 0,
        VisitorName TEXT,
        VisitorPhone TEXT,
        Reason TEXT,
        AccessedTime TEXT,
        Username TEXT,
        CreatedAt TEXT,
        UpdatedAt TEXT,
        FOREIGN KEY (MemberId) REFERENCES Members(MemberID)
    );";
                    command.ExecuteNonQuery();
                }

                // Create the PendingJobs table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
CREATE TABLE IF NOT EXISTS PendingJobs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Action TEXT NOT NULL,       -- ""enable"" | ""disable"" | ""delete""
    MemberId INTEGER NOT NULL,
    EnrollmentId INTEGER NOT NULL,
    DeviceId INTEGER NULL,
    Ip TEXT NOT NULL,
    Port INTEGER NOT NULL,
    MachineNo INTEGER NOT NULL,
    Password TEXT NOT NULL,
    BackupNumber INTEGER NOT NULL,
    CreatedAt TEXT,
    FOREIGN KEY (DeviceId) REFERENCES Devices(DeviceId)
);";
                    command.ExecuteNonQuery();
                }



                DefaultDevUser(connection);
            }

        }

        private static byte[] GenerateSecureKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[32]; // 32 bytes = 256 bits
                rng.GetBytes(key);
                return key;
            }
        }

        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey; // Use the byte array directly
                aes.IV = new byte[16]; // Zero IV for simplicity; use random IV in production
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey; // Use the byte array directly
                aes.IV = new byte[16];
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }




        private string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        
        //Abdul Hadi Code
        //Hash the password
        private byte[] HashString(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        
        //Abdul Hadi Code
        //Generate a backup code
        private string GenerateBackupCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder backupCode = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < 10; i++)
            {
                backupCode.Append(chars[random.Next(chars.Length)]);
            }

            return backupCode.ToString();
        }

        private void DefaultDevUser(SQLiteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM LocalUsers WHERE UserName = @user", connection))
                    {
                        checkCommand.Transaction = transaction;
                        checkCommand.Parameters.AddWithValue("@user", "Dev");
                        long count = (long)checkCommand.ExecuteScalar();
                        if (count == 0)
                        {
                            const string defaultUser = "Dev";
                            const string defaultPassword = "DevCoder@123";
                            const string defaultRole = "developer";
                            const string plainBackupCode = "BackupDatabase@26";

                            string hashedPasswordBase64 = Convert.ToBase64String(HashString(defaultPassword));
                            string hashedBackupBase64 = Convert.ToBase64String(HashString(plainBackupCode));
                            string ts = GetTimestamp();

                            using (var insertCommand = new SQLiteCommand(connection))
                            {
                                insertCommand.Transaction = transaction;
                                insertCommand.CommandText =
                                @"
                        INSERT INTO LocalUsers (UserName, Role, Password, BackupCode, CreatedAt, UpdatedAt)
                        VALUES (@UserName, @Role, @Password, @BackupCode, @CreatedAt, @UpdatedAt);
                        ";
                                insertCommand.Parameters.AddWithValue("@UserName", defaultUser);
                                insertCommand.Parameters.AddWithValue("@Role", defaultRole);
                                insertCommand.Parameters.AddWithValue("@Password", hashedPasswordBase64);
                                insertCommand.Parameters.AddWithValue("@BackupCode", hashedBackupBase64);
                                insertCommand.Parameters.AddWithValue("@CreatedAt", ts);
                                insertCommand.Parameters.AddWithValue("@UpdatedAt", ts);
                                insertCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            Console.WriteLine($"Default developer user '{defaultUser}' created. Backup code: {plainBackupCode}");
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error creating default developer user: {ex.Message}");
                }
            }
        }



        //Abdul Hadi Code
        //Add a new user to the LocalUsers table
        public bool AddUser(string userName, string role, string password, out string plainBackupCode)
        {
            plainBackupCode = GenerateBackupCode();
            string hashedBackupCode = Convert.ToBase64String(HashString(plainBackupCode));

            if (role != "admin" && role != "user")
            {
                Console.WriteLine("Invalid role. Only 'admin' or 'user' are allowed.");
                return false;
            }

            string hashedPassword = Convert.ToBase64String(HashString(password));
            string timestamp = GetTimestamp();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand(connection))
                        {
                            command.CommandText =
                            @"
                            INSERT INTO LocalUsers (UserName, Role, Password, BackupCode, CreatedAt, UpdatedAt)
                            VALUES (@UserName, @Role, @Password, @BackupCode, @CreatedAt, @UpdatedAt);
                            ";

                            command.Parameters.AddWithValue("@UserName", userName);
                            command.Parameters.AddWithValue("@Role", role);
                            command.Parameters.AddWithValue("@Password", hashedPassword);
                            command.Parameters.AddWithValue("@BackupCode", hashedBackupCode);
                            command.Parameters.AddWithValue("@CreatedAt", timestamp);
                            command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.WriteLine($"User '{userName}' added successfully.");
                        return true;
                    }
                    catch (SQLiteException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error adding user: {ex.Message}");
                        return false;
                    }
                }
            }
        }
        //Abdul Hadi Code
        //Add a new user to the LocalUsers table and return the ID of the new user when adding first time
        public int? FirstAddUser(string userName, string role, string password, out string plainBackupCode)
        {
            plainBackupCode = GenerateBackupCode();
            string hashedBackupCode = Convert.ToBase64String(HashString(plainBackupCode));

            if (role != "admin" && role != "user")
            {
                Console.WriteLine("Invalid role. Only 'admin' or 'user' are allowed.");
                return null;
            }

            string hashedPassword = Convert.ToBase64String(HashString(password));
            string timestamp = GetTimestamp();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand(connection))
                        {
                            command.CommandText =
                            @"
                    INSERT INTO LocalUsers (UserName, Role, Password, BackupCode, CreatedAt, UpdatedAt)
                    VALUES (@UserName, @Role, @Password, @BackupCode, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();
                    ";

                            command.Parameters.AddWithValue("@UserName", userName);
                            command.Parameters.AddWithValue("@Role", role);
                            command.Parameters.AddWithValue("@Password", hashedPassword);
                            command.Parameters.AddWithValue("@BackupCode", hashedBackupCode);
                            command.Parameters.AddWithValue("@CreatedAt", timestamp);
                            command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                            long lastId = (long)command.ExecuteScalar();
                            transaction.Commit();
                            Console.WriteLine($"User '{userName}' added successfully.");
                            return (int)lastId;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error adding user: {ex.Message}");
                        return null;
                    }
                }
            }
        }



        //Abdul Hadi Code
        //Login user with username and password or backup code
        public (bool Success, string Role, int UserId) LoginUser(string userName, string passwordOrBackup)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand(connection))
                        {
                            command.CommandText =
                            @"
                    SELECT LocalUserID, Password, BackupCode, Role 
                    FROM LocalUsers 
                    WHERE UserName = @UserName;
                    ";
                            command.Parameters.AddWithValue("@UserName", userName);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    int userId = Convert.ToInt32(reader["LocalUserID"]);
                                    string storedPasswordBase64 = reader["Password"].ToString();
                                    string storedBackupCodeBase64 = reader["BackupCode"].ToString();
                                    string userRole = reader["Role"].ToString();

                                    byte[] inputHashedBytes = HashString(passwordOrBackup);
                                    string inputHashedBase64 = Convert.ToBase64String(inputHashedBytes);

                                    if (storedPasswordBase64 == inputHashedBase64 || storedBackupCodeBase64 == inputHashedBase64)
                                    {
                                        Console.WriteLine("✅ Login successful.");
                                        transaction.Commit();
                                        return (true, userRole, userId);
                                    }
                                    else
                                    {
                                        Console.WriteLine("❌ Invalid credentials.");
                                        transaction.Rollback();
                                        return (false, null, -1);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("❌ User not found.");
                                    transaction.Rollback();
                                    return (false, null, -1);
                                }
                            }
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"❌ Login error: {ex.Message}");
                        return (false, null, -1);
                    }
                }
            }
        }


        //Abdul Hadi Code

        public bool VerifyLoginUser(string userName, string passwordOrBackup)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand(connection))
                        {
                            command.CommandText =
                            @"
                    SELECT Password, BackupCode FROM LocalUsers WHERE UserName = @UserName;
                    ";
                            command.Parameters.AddWithValue("@UserName", userName);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string storedPasswordBase64 = reader["Password"].ToString();
                                    string storedBackupCodeBase64 = reader["BackupCode"].ToString();

                                    byte[] inputHashedBytes = HashString(passwordOrBackup);
                                    string inputHashedBase64 = Convert.ToBase64String(inputHashedBytes);

                                    if (storedPasswordBase64 == inputHashedBase64 || storedBackupCodeBase64 == inputHashedBase64)
                                    {
                                        Console.WriteLine("✅ Login successful.");
                                        transaction.Commit();
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("❌ Invalid credentials.");
                                        transaction.Rollback();
                                        return false;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("❌ User not found.");
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"❌ Login error: {ex.Message}");
                        return false;
                    }
                }
            }
        }


        //Abdul Hadi Code
        //Check if a user exists in the LocalUsers table
        public bool AdminUserExists()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM LocalUsers WHERE Role = 'admin'", connection))
                {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        //Abdul Hadi Code
        //Load all users

        public List<LocalUser> LoadUsers()
        {
            var users = new List<LocalUser>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT * FROM LocalUsers", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new LocalUser
                        {
                            LocalUserID = Convert.ToInt32(reader["LocalUserID"]),
                            UserName = reader["UserName"].ToString(),
                            Role = reader["Role"].ToString(),
                            Password = reader["Password"].ToString(),
                            BackupCode = reader["BackupCode"].ToString(),
                            CreatedAt = reader["CreatedAt"] as string,
                            UpdatedAt = reader["UpdatedAt"] as string
                        });
                    }
                }
            }

            return users;
        }

        //Abdul Hadi Code
        //Delete a user from the LocalUsers table
        public void DeleteUserById(int userId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("DELETE FROM LocalUsers WHERE LocalUserID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        //Abdul Hadi Code
        //Update user information in the LocalUsers table
        public void UpdateUserInfo(int userId, string newUsername, string newRole)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("UPDATE LocalUsers SET UserName = @username, Role = @role, UpdatedAt = @updated WHERE LocalUserID = @id", connection))
                {
                    command.Parameters.AddWithValue("@username", newUsername);
                    command.Parameters.AddWithValue("@role", newRole);
                    command.Parameters.AddWithValue("@updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@id", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        //Abdul Hadi Code
        //Update user password in the LocalUsers table
        public void UpdateUserPassword(int userId, string newPassword)
        {
            string hashedPassword = Convert.ToBase64String(HashString(newPassword));

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("UPDATE LocalUsers SET Password = @password, UpdatedAt = @updated WHERE LocalUserID = @id", connection))
                {
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@id", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Saves or updates a setting (as a key-value pair) in the Settings table.
        /// </summary>
        public void SaveSetting(string key, string value)
        {

            string timestamp = GetTimestamp();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    @"
                    INSERT INTO Settings (Key, Value, CreatedAt, UpdatedAt)
                    VALUES (@key, @value, @createdAt, @updatedAt)
                    ON CONFLICT(Key) DO UPDATE SET
                        Value = excluded.Value,
                        UpdatedAt = excluded.UpdatedAt;
                    ";
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@value", value);
                    command.Parameters.AddWithValue("@createdAt", timestamp);
                    command.Parameters.AddWithValue("@updatedAt", timestamp);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Retrieves the value of a setting from the Settings table.
        /// Returns null if the key does not exist.
        /// </summary>
        public string GetSetting(string key)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT Value FROM Settings WHERE Key = @key;";
                    command.Parameters.AddWithValue("@key", key);
                    object result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        
        public Member GetMember(int memberId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT MemberID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, PackageExpiry, SyncStatus " +
                    "FROM Members WHERE MemberID = @MemberID", connection))
                {
                    command.Parameters.AddWithValue("@MemberID", memberId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var syncStatusJson = reader.IsDBNull(8) ? "{}" : reader.GetString(8);
                            return new Member
                            {
                                MemberID = reader.GetInt32(0),
                                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Status = reader.IsDBNull(2) ? null : reader.GetString(2),
                                RFID = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Picture = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Fingerprint = !reader.IsDBNull(5) && reader.GetBoolean(5),
                                EnrollmentID = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                PackageExpiry = reader.IsDBNull(7) ? null : reader.GetString(7),
                                SyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>()
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Updated GetMembers method
        public List<Member> GetMembers()
        {
            var members = new List<Member>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT MemberID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, PackageExpiry, SyncStatus " +
                    "FROM Members", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var syncStatusJson = reader.IsDBNull(8) ? "{}" : reader.GetString(8);
                            members.Add(new Member
                            {
                                MemberID = reader.GetInt32(0),
                                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Status = reader.IsDBNull(2) ? null : reader.GetString(2),
                                RFID = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Picture = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Fingerprint = !reader.IsDBNull(5) && reader.GetBoolean(5),
                                EnrollmentID = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                PackageExpiry = reader.IsDBNull(7) ? null : reader.GetString(7),
                                SyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>()
                            });
                        }
                    }
                }
            }
            return members;
        }

        /// <summary>
        /// Inserts or updates a member record in the local DB using data received from the cloud.
        /// </summary>
        public void UpsertMember(JObject member)
        {
            // Extract member data
            int memberId = (int)member["id"];
            string name = member["name"]?.ToString() ?? string.Empty;
            string status = (string)member["status"];
            string rfid = member["rfid"]?.ToString() ?? string.Empty;
            string picture = member["picture"]?.ToString() ?? string.Empty;
            bool fingerprint = member["fingerprint"]?.ToObject<bool>() ?? false;
            int? enrollmentId = member["enrollment_id"]?.ToObject<int?>();
            string packageExpiry = member["package_expiry"]?.ToString();

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Get existing data
            var existingMember = GetMember(memberId);
            var devices = LoadDevices();
            var syncStatus = new Dictionary<int, bool>();

            // Determine sync status for each device
            foreach (var device in devices)
            {
                bool needsSync = true; // Default to false (needs sync)

                if (existingMember != null)
                {
                    // Check if this device needs sync based on its capabilities
                    bool fingerprintRelevant = device.FingerCapacity > 0;
                    bool rfidRelevant = device.RFIDCapacity > 0;
                    bool faceRelevant = device.FaceCapacity > 0;

                    // Check if relevant data changed
                    bool fingerprintChanged = fingerprintRelevant &&
                                           (fingerprint != existingMember.Fingerprint ||
                                            enrollmentId != existingMember.EnrollmentID);

                    bool rfidChanged = rfidRelevant &&
                                     (rfid != existingMember.RFID);

                    bool pictureChanged = faceRelevant &&
                                        (picture != existingMember.Picture);

                    needsSync = fingerprintChanged || rfidChanged || pictureChanged;
                }

                // NEVER SET TO TRUE HERE - must be updated after successful sync
                syncStatus[device.DeviceId] = false;

                // Log sync decision
                LogHelper.Log($"Member {memberId} sync status for device {device.DeviceId}: " +
                             $"{(needsSync ? "Needs sync" : "No changes detected")}");
            }

            // Serialize sync status
            string syncStatusJson = JsonConvert.SerializeObject(syncStatus);

            // Execute database operation
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SQLiteCommand(conn))
                        {
                            cmd.CommandText = @"
                        INSERT INTO Members 
                        (MemberID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, 
                         PackageExpiry, SyncStatus, CreatedAt, UpdatedAt)
                        VALUES (@id, @name, @status, @rfid, @picture, @fingerprint, 
                                @enrollment_id, @package_expiry, @syncStatus, @createdAt, @updatedAt)
                        ON CONFLICT(MemberID) DO UPDATE SET
                            Name = excluded.Name,
                            Status = excluded.Status,
                            RFID = excluded.RFID,
                            Picture = excluded.Picture,
                            Fingerprint = excluded.Fingerprint,
                            EnrollmentID = excluded.EnrollmentID,
                            PackageExpiry = excluded.PackageExpiry,
                            SyncStatus = excluded.SyncStatus,
                            UpdatedAt = excluded.UpdatedAt;";

                            cmd.Parameters.AddWithValue("@id", memberId);
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@rfid", string.IsNullOrEmpty(rfid) ? DBNull.Value : (object)rfid);
                            cmd.Parameters.AddWithValue("@picture", string.IsNullOrEmpty(picture) ? DBNull.Value : (object)picture);
                            cmd.Parameters.AddWithValue("@fingerprint", fingerprint);
                            cmd.Parameters.AddWithValue("@enrollment_id", enrollmentId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@package_expiry", string.IsNullOrEmpty(packageExpiry) ? DBNull.Value : (object)packageExpiry);
                            cmd.Parameters.AddWithValue("@syncStatus", syncStatusJson);
                            cmd.Parameters.AddWithValue("@createdAt", timestamp);
                            cmd.Parameters.AddWithValue("@updatedAt", timestamp);

                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error upserting member {memberId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a member record from the local SQLite DB based on MemberID.
        /// </summary>
        public void DeleteMemberById(int memberId)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Members WHERE MemberID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"🗑 Member {memberId} deleted from local DB.");
        }


        //Attendance Log Code
        public int InsertAttendanceLog(int memberId, string checkInValue)
        {
            string syncStatusJson = JsonConvert.SerializeObject(new { checkIn = false, checkOut = false });
            string timestamp = GetTimestamp();
            int newLogId = 0;
            string attendanceId = Guid.NewGuid().ToString();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Check if the provided check-in time is later than the member's last check-out time
                DateTime checkInDateTime;
                if (DateTime.TryParse(checkInValue, out checkInDateTime))
                {
                    DateTime lastCheckOutTime = GetLastCheckOutTime(memberId);
                    if (checkInDateTime <= lastCheckOutTime)
                    {
                        LogHelper.Log($"❌ Check-in time {checkInValue} is not later than the member's last check-out time {lastCheckOutTime}.");
                        return 0; // Or throw an exception, depending on your error handling policy
                    }
                }

                using (var command = new SQLiteCommand(
                    @"INSERT INTO AttendanceLogs
              (MemberID, AttendanceId, CheckInTime, CheckOutTime, SyncStatus, CreatedAt, UpdatedAt)
              VALUES (@memberId, @attendanceId, @checkIn, NULL, @syncStatus, @createdAt, @updatedAt);
              SELECT last_insert_rowid();", connection))
                {
                    command.Parameters.AddWithValue("@memberId", memberId);
                    command.Parameters.AddWithValue("@attendanceId", attendanceId);
                    command.Parameters.AddWithValue("@checkIn", (object)checkInValue ?? DBNull.Value);
                    command.Parameters.AddWithValue("@syncStatus", syncStatusJson);
                    command.Parameters.AddWithValue("@createdAt", timestamp);
                    command.Parameters.AddWithValue("@updatedAt", timestamp);

                    newLogId = Convert.ToInt32((long)command.ExecuteScalar());
                }
            }
            return newLogId;
        }


        public void UpdateAttendanceLogForCheckOut(int memberId, string checkOutValue)
        {
            string timestamp = GetTimestamp();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // First, validate the check-out time format
                DateTime checkOutDateTime;
                if (!DateTime.TryParse(checkOutValue, out checkOutDateTime))
                {
                    return; // Invalid check-out time format
                }

                // Check if there's any check-in record for this member on the check-out date
                bool hasCheckInForDate = HasAttendanceLogForDate(memberId, checkOutDateTime.Date, "check-in");

                int logId = 0;
                string existingJson = null;
                DateTime checkInTime = DateTime.MinValue;

                if (hasCheckInForDate)
                {
                    // Get latest open (no checkout) log for this member on the same date
                    using (var get = new SQLiteCommand(
                        @"SELECT LogID, CheckInTime, SyncStatus 
                  FROM AttendanceLogs 
                  WHERE MemberID = @memberId AND CheckOutTime IS NULL 
                  AND DATE(CheckInTime) = DATE(@checkOutDate)
                  ORDER BY CheckInTime DESC 
                  LIMIT 1", connection))
                    {
                        get.Parameters.AddWithValue("@memberId", memberId);
                        get.Parameters.AddWithValue("@checkOutDate", checkOutDateTime.ToString("yyyy-MM-dd"));
                        using (var reader = get.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                logId = reader.GetInt32(0);
                                checkInTime = DateTime.Parse(reader["CheckInTime"].ToString());

                                // Check that the check-out time is greater than the check-in time
                                if (checkOutDateTime <= checkInTime)
                                {
                                    LogHelper.Log($"❌ Check-out time {checkOutValue} is not greater than the check-in time {checkInTime} for same day.");
                                    return; // Prevent invalid check-out
                                }
                                existingJson = reader.IsDBNull(2) ? null : reader.GetString(2);
                            }
                            else
                            {
                                return; // No open attendance for this date
                            }
                        }
                    }
                }
                else
                {
                    // No check-in for current date, look for previous day only
                    DateTime previousDay = checkOutDateTime.Date.AddDays(-1);

                    using (var get = new SQLiteCommand(
                        @"SELECT LogID, CheckInTime, SyncStatus
                  FROM AttendanceLogs 
                  WHERE MemberID = @memberId AND CheckOutTime IS NULL 
                  AND DATE(CheckInTime) = DATE(@previousDay)
                  ORDER BY CheckInTime DESC 
                  LIMIT 1", connection))
                    {
                        get.Parameters.AddWithValue("@memberId", memberId);
                        get.Parameters.AddWithValue("@previousDay", previousDay.ToString("yyyy-MM-dd"));
                        using (var reader = get.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                logId = reader.GetInt32(0);
                                checkInTime = DateTime.Parse(reader["CheckInTime"].ToString());

                                // Check that the check-out time is greater than the check-in time
                                if (checkOutDateTime <= checkInTime)
                                {
                                    LogHelper.Log($"❌ Check-out time {checkOutValue} is not greater than the check-in time {checkInTime} for previous day.");
                                    return; // Prevent invalid check-out
                                }
                                existingJson = reader.IsDBNull(2) ? null : reader.GetString(2);
                            }
                            else
                            {
                                return; // No open attendance record found for previous day either
                            }
                        }
                    }
                }

                var syncDict = !string.IsNullOrWhiteSpace(existingJson)
                    ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(existingJson) ?? new Dictionary<string, bool>()
                    : new Dictionary<string, bool>();

                // Ensure keys exist & mark checkout as needing sync again
                if (!syncDict.ContainsKey("checkIn")) syncDict["checkIn"] = false;
                syncDict["checkOut"] = false;

                using (var update = new SQLiteCommand(
                    @"UPDATE AttendanceLogs 
              SET CheckOutTime = @checkOut, SyncStatus = @syncStatus, UpdatedAt = @updatedAt
              WHERE LogID = @logId", connection))
                {
                    update.Parameters.AddWithValue("@checkOut", (object)checkOutValue ?? DBNull.Value);
                    update.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncDict));
                    update.Parameters.AddWithValue("@updatedAt", timestamp);
                    update.Parameters.AddWithValue("@logId", logId);
                    update.ExecuteNonQuery();
                }
            }
        }

        private DateTime GetLastCheckOutTime(int memberId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT MAX(CheckOutTime) FROM AttendanceLogs WHERE MemberID = @memberId", connection))
                {
                    command.Parameters.AddWithValue("@memberId", memberId);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        return DateTime.Parse(result.ToString());
                    }
                    else
                    {
                        return DateTime.MinValue; // Or some other default value
                    }
                }
            }
        }


        

        /// <summary>
        /// /// Updates the sync status of an attendance log based on provided parameters.
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="attendanceId"></param>
        /// <param name="checkInTime"></param>
        /// <param name="checkOutTime"></param>
        /// <param name="checkIn"></param>
        /// <param name="checkOut"></param>
        /// <returns></returns>
        

        public bool UpdateAttendanceLogSyncStatus(
    int memberId,
    string attendanceId,
    DateTime? checkInTime = null,
    DateTime? checkOutTime = null,
    bool? checkIn = null,
    bool? checkOut = null)
        {
            // Must supply exactly one flag.
            bool hasCheckIn = checkIn.HasValue;
            bool hasCheckOut = checkOut.HasValue;
            if (!(hasCheckIn ^ hasCheckOut))
                return false;

            // For checkOut we require a non-empty attendanceId and checkOutTime
            if (hasCheckOut && (string.IsNullOrWhiteSpace(attendanceId) || !checkOutTime.HasValue))
                return false;

            // For checkIn with attendanceId, require checkInTime
            if (hasCheckIn && !string.IsNullOrWhiteSpace(attendanceId) && !checkInTime.HasValue)
                return false;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string existingJson = null;
                int targetLogId = 0;
                bool foundRecord = false;

                // For check-in: require both attendanceId and checkInTime to match
                if (hasCheckIn && !string.IsNullOrWhiteSpace(attendanceId) && checkInTime.HasValue)
                {
                    using (var select = new SQLiteCommand(
                        "SELECT LogID, SyncStatus FROM AttendanceLogs WHERE AttendanceId = @aid AND MemberID = @mid AND CheckInTime = @checkInTime LIMIT 1", connection))
                    {
                        select.Parameters.AddWithValue("@aid", attendanceId);
                        select.Parameters.AddWithValue("@mid", memberId);
                        select.Parameters.AddWithValue("@checkInTime", checkInTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        using (var reader = select.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                targetLogId = reader.GetInt32(0);
                                existingJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                                foundRecord = true;
                                LogHelper.Log($"Found attendance log with ID {targetLogId} for member {memberId} with AttendanceId {attendanceId} and CheckInTime {checkInTime.Value}");
                            }
                        }
                    }
                }
                // For check-out: require both attendanceId and checkOutTime to match
                else if (hasCheckOut && !string.IsNullOrWhiteSpace(attendanceId) && checkOutTime.HasValue)
                {
                    using (var select = new SQLiteCommand(
                        "SELECT LogID, SyncStatus FROM AttendanceLogs WHERE AttendanceId = @aid AND MemberID = @mid AND CheckOutTime = @checkOutTime LIMIT 1", connection))
                    {
                        select.Parameters.AddWithValue("@aid", attendanceId);
                        select.Parameters.AddWithValue("@mid", memberId);
                        select.Parameters.AddWithValue("@checkOutTime", checkOutTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        using (var reader = select.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                targetLogId = reader.GetInt32(0);
                                existingJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                                foundRecord = true;
                                LogHelper.Log($"Found attendance log with ID {targetLogId} for member {memberId} with AttendanceId {attendanceId} and CheckOutTime {checkOutTime.Value}");
                            }
                        }
                    }
                }
                // For check-in without attendanceId or checkInTime, fallback to latest log for member
                else if (hasCheckIn && string.IsNullOrWhiteSpace(attendanceId))
                {
                    using (var selectLatest = new SQLiteCommand(
                        @"SELECT LogID, SyncStatus 
                  FROM AttendanceLogs 
                  WHERE MemberID = @mid 
                  ORDER BY CheckInTime DESC 
                  LIMIT 1", connection))
                    {
                        selectLatest.Parameters.AddWithValue("@mid", memberId);
                        using (var reader = selectLatest.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                targetLogId = reader.GetInt32(0);
                                existingJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                                foundRecord = true;
                                LogHelper.Log($"Found latest attendance log with ID {targetLogId} for member {memberId} (no AttendanceId)");
                            }
                        }
                    }
                }

                if (!foundRecord)
                {
                    LogHelper.Log($"No matching attendance log found for member {memberId}" +
                                  (string.IsNullOrWhiteSpace(attendanceId) ? "" : $" with AttendanceId {attendanceId} and {(hasCheckIn ? $"CheckInTime {checkInTime}" : $"CheckOutTime {checkOutTime}")}"));
                    return false;
                }

                // Parse existing sync status
                Dictionary<string, bool> flags;
                try
                {
                    flags = JsonConvert.DeserializeObject<Dictionary<string, bool>>(existingJson ?? "") ?? new Dictionary<string, bool>();
                }
                catch
                {
                    flags = new Dictionary<string, bool>();
                }

                // Ensure keys exist
                if (!flags.ContainsKey("checkIn")) flags["checkIn"] = false;
                if (!flags.ContainsKey("checkOut")) flags["checkOut"] = false;

                // Only update the field we're directly changing, not both fields
                bool valueChanged = false;
                if (hasCheckIn)
                {
                    if (flags["checkIn"] != checkIn.Value)
                    {
                        flags["checkIn"] = checkIn.Value;
                        valueChanged = true;
                    }
                    else
                    {
                        LogHelper.Log($"Check-in sync status already set to {checkIn.Value} for log {targetLogId}");
                        return false;
                    }
                }
                else if (hasCheckOut)
                {
                    if (flags["checkOut"] != checkOut.Value)
                    {
                        flags["checkOut"] = checkOut.Value;
                        valueChanged = true;
                    }
                    else
                    {
                        LogHelper.Log($"Check-out sync status already set to {checkOut.Value} for log {targetLogId}");
                        return false;
                    }
                }

                // Only update if there was an actual change
                if (valueChanged)
                {
                    using (var update = new SQLiteCommand(
                        "UPDATE AttendanceLogs SET SyncStatus = @json, UpdatedAt = @ts WHERE LogID = @lid", connection))
                    {
                        update.Parameters.AddWithValue("@json", JsonConvert.SerializeObject(flags));
                        update.Parameters.AddWithValue("@ts", GetTimestamp());
                        update.Parameters.AddWithValue("@lid", targetLogId);
                        int rowsUpdated = update.ExecuteNonQuery();

                        if (rowsUpdated > 0)
                        {
                            LogHelper.Log($"Successfully updated {(hasCheckIn ? "check-in" : "check-out")} sync status for log {targetLogId}");
                            return true;
                        }
                        else
                        {
                            LogHelper.Log($"Failed to update {(hasCheckIn ? "check-in" : "check-out")} sync status for log {targetLogId}");
                            return false;
                        }
                    }
                }

                return false;
            }
        }

        public string GetDeviceAttendanceAction(int deviceMachineNo)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT AttendanceType FROM Devices WHERE MachineNo = @m LIMIT 1", connection))
                {
                    cmd.Parameters.AddWithValue("@m", deviceMachineNo);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return null;
                    return Convert.ToBoolean(result) ? "check-in" : "check-out";
                }
            }
        }

        public int? GetMemberIdByEnrollment(int enrollmentId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT MemberID FROM Members WHERE EnrollmentID = @enroll LIMIT 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@enroll", enrollmentId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetMemberIdByEnrollment][ERROR] {ex.Message}");
            }
            return null;
        }


        // Helper method to check if a record exists in the attendance log table
        public async Task<bool> RecordExistsInAttendanceLogAsync(int enrollNumber, DateTime deviceTime)
        {
            // Time tolerance (in seconds) to account for small differences in timestamps
            const int timeTolerance = 5;

            return await Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // First, get the memberId from enrollNumber
                    int? memberId = null;
                    using (var cmd = new SQLiteCommand("SELECT MemberID FROM Members WHERE EnrollmentID = @enrollId", connection))
                    {
                        cmd.Parameters.AddWithValue("@enrollId", enrollNumber);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            memberId = Convert.ToInt32(result);
                        }
                    }

                    if (!memberId.HasValue)
                    {
                        LogHelper.Log($"No member found for enrollment ID: {enrollNumber}");
                        return false;
                    }

                    // Format the date for comparison with a tolerance
                    DateTime startTime = deviceTime.AddSeconds(-timeTolerance);
                    DateTime endTime = deviceTime.AddSeconds(timeTolerance);

                    // Check if there's a matching record in AttendanceLogs
                    using (var cmd = new SQLiteCommand(@"
                    SELECT COUNT(*) FROM AttendanceLogs 
                    WHERE MemberID = @memberId AND 
                    ((datetime(CheckInTime) BETWEEN datetime(@startTime) AND datetime(@endTime)) OR 
                     (CheckOutTime IS NOT NULL AND datetime(CheckOutTime) BETWEEN datetime(@startTime) AND datetime(@endTime)))",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId.Value);
                        cmd.Parameters.AddWithValue("@startTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@endTime", endTime.ToString("yyyy-MM-dd HH:mm:ss"));

                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            });
        }


        /// <summary>
        /// Retrieves the ID of the most recent open attendance log for a member.
        /// An open log is one where CheckOutTime is NULL.
        /// </summary>
        /// <param name="memberId">The member ID to search for</param>
        /// <returns>The LogID of the open attendance log, or 0 if no open log exists</returns>
        public int GetOpenAttendanceLogId(int memberId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    @"SELECT LogID FROM AttendanceLogs 
              WHERE MemberID = @memberId AND CheckOutTime IS NULL 
              ORDER BY CheckInTime DESC LIMIT 1", connection))
                {
                    command.Parameters.AddWithValue("@memberId", memberId);
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                    return 0;
                }
            }
        }

        public DateTime? GetOpenAttendanceLogCheckInTime(int memberId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(@"
                    SELECT CheckInTime
                    FROM AttendanceLogs
                    WHERE MemberID = @memberId
                      AND CheckOutTime IS NULL
                    ORDER BY CheckInTime DESC
                    LIMIT 1;", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                            return null;

                        // Attendance times are stored as text; ensure robust parse
                        DateTime parsed;
                        if (DateTime.TryParse(Convert.ToString(result), out parsed))
                            return parsed;

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB][GetOpenAttendanceLogCheckInTime][ERROR] {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns the AttendanceId for a member.
        /// If checkIn is provided it matches by CheckInTime, else if checkOut is provided matches by CheckOutTime,
        /// otherwise returns the latest non-empty AttendanceId for the member (most recent CheckInTime).
        /// Returns null when no AttendanceId is found.
        /// </summary>
        public string GetAttendanceIdForMember(int memberId, DateTime? checkIn = null, DateTime? checkOut = null)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string sql;
                    if (checkIn.HasValue)
                    {
                        sql = "SELECT AttendanceId FROM AttendanceLogs WHERE MemberID = @mid AND CheckInTime = @checkIn LIMIT 1";
                    }
                    else if (checkOut.HasValue)
                    {
                        sql = "SELECT AttendanceId FROM AttendanceLogs WHERE MemberID = @mid AND CheckOutTime = @checkOut LIMIT 1";
                    }
                    else
                    {
                        // latest non-empty AttendanceId for member
                        sql = "SELECT AttendanceId FROM AttendanceLogs WHERE MemberID = @mid AND AttendanceId IS NOT NULL AND TRIM(AttendanceId) <> '' ORDER BY datetime(CheckInTime) DESC LIMIT 1";
                    }

                    using (var cmd = new System.Data.SQLite.SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@mid", memberId);
                        if (checkIn.HasValue) cmd.Parameters.AddWithValue("@checkIn", checkIn.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (checkOut.HasValue) cmd.Parameters.AddWithValue("@checkOut", checkOut.Value.ToString("yyyy-MM-dd HH:mm:ss"));

                        var result = cmd.ExecuteScalar();
                        return result == null || result == DBNull.Value ? null : result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"GetAttendanceIdForMember error for member {memberId}: {ex.Message}");
                return null;
            }
        }

        public DateTime? GetPairedCheckInTime(int memberId, DateTime checkOutTime)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Stored times are saved as text "yyyy-MM-dd HH:mm:ss"
                    string checkOutStr = checkOutTime.ToString("yyyy-MM-dd HH:mm:ss");

                    // Find the attendance row for this member with the provided checkout time
                    // and return its paired check-in time.
                    const string query = @"
                SELECT CheckInTime
                FROM AttendanceLogs
                WHERE MemberID = @memberId
                  AND CheckOutTime = @checkOutTime
                LIMIT 1";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@memberId", memberId);
                        command.Parameters.AddWithValue("@checkOutTime", checkOutStr);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            var checkInText = Convert.ToString(result);
                            if (DateTime.TryParse(checkInText, out var parsedCheckIn))
                                return parsedCheckIn;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error in GetPairedCheckInTime(memberId={memberId}, checkOut={checkOutTime}): {ex.Message}");
            }

            return null;
        }


        public bool UpdateAttendanceLogFromCloud(
    int memberId,
    DateTime checkInTime,
    DateTime? checkOutTime,
    string attendanceId,
    bool setCheckInSynced,
    bool setCheckOutSynced)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Find the log row by member + exact check-in time
                    int logId = 0;
                    string existingSyncStatusJson = null;

                    using (var cmd = new SQLiteCommand(
                        @"SELECT LogID, SyncStatus 
                  FROM AttendanceLogs
                  WHERE MemberID = @memberId
                    AND CheckInTime = @checkIn", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        cmd.Parameters.AddWithValue("@checkIn", checkInTime.ToString("yyyy-MM-dd HH:mm:ss"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                logId = reader.GetInt32(0);
                                existingSyncStatusJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                            }
                        }
                    }

                    if (logId == 0)
                    {
                        LogHelper.Log($"⚠️ No local AttendanceLog found for Member={memberId}, CheckIn={checkInTime:yyyy-MM-dd HH:mm:ss}");
                        return false;
                    }

                    // Parse / update SyncStatus JSON
                    var syncDict = !string.IsNullOrWhiteSpace(existingSyncStatusJson)
                        ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(existingSyncStatusJson) ?? new Dictionary<string, bool>()
                        : new Dictionary<string, bool>();

                    if (!syncDict.ContainsKey("checkIn")) syncDict["checkIn"] = false;
                    if (!syncDict.ContainsKey("checkOut")) syncDict["checkOut"] = false;

                    if (setCheckInSynced) syncDict["checkIn"] = true;
                    if (setCheckOutSynced) syncDict["checkOut"] = true;

                    string newSyncJson = JsonConvert.SerializeObject(syncDict);
                    string timestamp = GetTimestamp();

                    using (var update = new SQLiteCommand(
                        @"UPDATE AttendanceLogs
                  SET AttendanceId = @attendanceId,
                      SyncStatus   = @syncStatus,
                      UpdatedAt    = @updatedAt
                  WHERE LogID = @logId", connection))
                    {
                        update.Parameters.AddWithValue("@attendanceId", (object)attendanceId ?? DBNull.Value);
                        update.Parameters.AddWithValue("@syncStatus", newSyncJson);
                        update.Parameters.AddWithValue("@updatedAt", timestamp);
                        update.Parameters.AddWithValue("@logId", logId);

                        update.ExecuteNonQuery();
                    }
                }

                LogHelper.Log($"✅ Updated AttendanceLog from cloud for Member={memberId}, CheckIn={checkInTime:yyyy-MM-dd HH:mm:ss}, AttendanceId={attendanceId}");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error updating AttendanceLog from cloud for Member={memberId}: {ex.Message}");
                return false;
            }
        }

        public DateTime? GetLastCheckOutTimeForMember(int memberId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(@"
                    SELECT CheckOutTime
                    FROM AttendanceLogs
                    WHERE MemberID = @memberId
                      AND CheckOutTime IS NOT NULL
                    ORDER BY CheckOutTime DESC
                    LIMIT 1;", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                            return null;

                        DateTime parsed;
                        if (DateTime.TryParse(Convert.ToString(result), out parsed))
                            return parsed;

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB][GetLastCheckOutTimeForMember][ERROR] {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Checks if there are any attendance logs for a member on a specific date for a specific action.
        /// </summary>
        /// <param name="memberId">The member ID to check</param>
        /// <param name="date">The date to check (time portion is ignored)</param>
        /// <param name="action">The action to check for ("check-in" or "check-out")</param>
        /// <returns>True if attendance logs exist for that day and action, false otherwise</returns>
        public bool HasAttendanceLogForDate(int memberId, DateTime date, string action)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql;
                if (action == "check-in")
                {
                    sql = @"SELECT COUNT(*) FROM AttendanceLogs 
                           WHERE MemberID = @memberId 
                           AND DATE(CheckInTime) = DATE(@date)
                           AND CheckInTime IS NOT NULL";
                }
                else if (action == "check-out")
                {
                    sql = @"SELECT COUNT(*) FROM AttendanceLogs 
                           WHERE MemberID = @memberId 
                           AND DATE(CheckInTime) = DATE(@date)
                           AND CheckOutTime IS NOT NULL";
                }
                else
                {
                    // If action is not specified or invalid, check for any logs on that date
                    sql = @"SELECT COUNT(*) FROM AttendanceLogs 
                           WHERE MemberID = @memberId 
                           AND DATE(CheckInTime) = DATE(@date)";
                }

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@memberId", memberId);
                    command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }

        /// <summary>
        /// Checks if there are any attendance logs for a member on a specific date (backward compatibility).
        /// </summary>
        /// <param name="memberId">The member ID to check</param>
        /// <param name="date">The date to check (time portion is ignored)</param>
        /// <returns>True if attendance logs exist for that day, false otherwise</returns>
        public bool HasAttendanceLogForDate(int memberId, DateTime date)
        {
            return HasAttendanceLogForDate(memberId, date, null);
        }



        /// <summary>
        /// Unified attendance gate.
        /// Checks for any OPEN session (check-in without checkout) whose check-in date is TODAY or PREVIOUS DAY.
        /// Rules:
        ///  - action = "check-in": allowed ONLY if there is NO such open session (returns true if no open session).
        ///  - action = "check-out": allowed ONLY if there IS such an open session (returns true if at least one open session).
        ///  - any other action: returns false.
        /// This single function replaces previous multi-function logic.
        /// </summary>
        public bool CanProcessAttendance(int memberId, DateTime deviceTime, string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return false;

            action = action.Trim().ToLowerInvariant();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Count any open session whose check-in date is today or previous day
                const string sql = @"
                    SELECT COUNT(*) FROM AttendanceLogs
                    WHERE MemberID = @mid
                      AND CheckInTime IS NOT NULL
                      AND (CheckOutTime IS NULL OR TRIM(CheckOutTime) = '')
                      AND (
                            DATE(CheckInTime) = DATE(@dt)
                         OR DATE(CheckInTime) = DATE(@dt, '-1 day')
                          );";

                int openCount;
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@mid", memberId);
                    cmd.Parameters.AddWithValue("@dt", deviceTime.ToString("yyyy-MM-dd"));
                    openCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (action == "check-in")
                {
                    // Allow new check-in only if NO open session spanning today/previous
                    return openCount == 0;
                }
                if (action == "check-out")
                {
                    // Allow checkout only if an open session exists
                    return openCount > 0;
                }

                return false;
            }
        }


        /// <summary>
        /// Forces the Face sync status to false in the Log table for all devices associated with a member.
        /// This ensures re-enrollment is triggered when the face image URL changes.
        /// </summary>
        /// <param name="memberId">The member ID to update</param>
        public async Task ForceFaceSyncStatusFalseAsync(int memberId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get all log entries for the member
                    var logIds = new List<int>();
                    using (var cmd = new SQLiteCommand("SELECT Id, SyncStatus FROM Log WHERE MemberID = @memberId", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int logId = reader.GetInt32(0);
                                string syncStatusJson = reader.GetString(1);
                                var syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(syncStatusJson) ?? new Dictionary<string, bool>();

                                // Set Face to false if it exists
                                if (syncStatus.ContainsKey("Face"))
                                {
                                    syncStatus["Face"] = false;
                                }
                                else
                                {
                                    syncStatus["Face"] = false;  // Add if missing
                                }

                                // Update the log entry
                                using (var updateCmd = new SQLiteCommand("UPDATE Log SET SyncStatus = @syncStatus WHERE Id = @logId", connection))
                                {
                                    updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                                    updateCmd.Parameters.AddWithValue("@logId", logId);
                                    await updateCmd.ExecuteNonQueryAsync();
                                }

                                logIds.Add(logId);
                            }
                        }
                    }

                    if (logIds.Count > 0)
                    {
                        LogHelper.Log($"✅ Forced Face sync status to false for member {memberId} in {logIds.Count} log entries.");
                    }
                    else
                    {
                        LogHelper.Log($"ℹ️ No log entries found for member {memberId} to update Face sync status.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error forcing Face sync status for member {memberId}: {ex.Message}");
            }
        }

        public async Task<List<UnsyncedAttendanceLog>> CheckUnsyncedAttendanceLogsAsync()
        {
            var unsyncedLogs = new List<UnsyncedAttendanceLog>();

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Query now selects AttendanceId instead of LogId/EnrollmentId
                    string query = @"
                SELECT 
                    al.AttendanceId, 
                    al.MemberID, 
                    al.CheckInTime, 
                    al.CheckOutTime, 
                    al.SyncStatus
                FROM 
                    AttendanceLogs al
                WHERE 
                    al.SyncStatus LIKE '%false%'";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string attendanceId = reader.GetString(0);
                            int memberId = reader.GetInt32(1);

                            // Robust parsing of CheckInTime (skip row if invalid)
                            string checkInText = reader.IsDBNull(2) ? null : reader.GetString(2);
                            DateTime checkInTime;
                            if (string.IsNullOrWhiteSpace(checkInText) || !DateTime.TryParse(checkInText, out checkInTime))
                                continue;

                            DateTime? checkOutTime = null;
                            if (!reader.IsDBNull(3))
                            {
                                string checkOutText = reader.GetString(3);
                                DateTime tmp;
                                if (DateTime.TryParse(checkOutText, out tmp))
                                    checkOutTime = tmp;
                            }

                            string syncStatusJson = reader.IsDBNull(4) ? "{}" : reader.GetString(4);

                            // Parse the JSON in the SyncStatus field safely
                            Dictionary<string, bool> syncStatus;
                            try
                            {
                                syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(syncStatusJson) ?? new Dictionary<string, bool>();
                            }
                            catch
                            {
                                syncStatus = new Dictionary<string, bool>();
                            }

                            bool needsCheckInSync = syncStatus.ContainsKey("checkIn") && !syncStatus["checkIn"];
                            bool needsCheckOutSync = syncStatus.ContainsKey("checkOut") && !syncStatus["checkOut"] && checkOutTime.HasValue;

                            if (needsCheckInSync || needsCheckOutSync)
                            {
                                unsyncedLogs.Add(new UnsyncedAttendanceLog
                                {
                                    AttendanceId = attendanceId,   // ✅ new field
                                    MemberId = memberId,
                                    CheckInTime = checkInTime,
                                    CheckOutTime = checkOutTime ?? DateTime.MinValue,
                                    NeedsCheckInSync = needsCheckInSync,
                                    NeedsCheckOutSync = needsCheckOutSync
                                });
                            }
                        }
                    }
                }

                unsyncedLogs = unsyncedLogs
                    .OrderBy(u => u.CheckInTime)
                    .ThenBy(u => u.AttendanceId) // ✅ sort by AttendanceId instead of LogId
                    .ToList();

                LogHelper.Log($"Found {unsyncedLogs.Count} unsynced attendance logs");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error checking for unsynced attendance logs: {ex.Message}");
            }

            return unsyncedLogs;
        }

        // Get all attendance logs
        public List<dynamic> GetAllAttendanceLogs()
        {
            var logs = new List<dynamic>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"
                    SELECT 
                        LogID,
                        MemberID,
                        AttendanceId,
                        CheckInTime,
                        CheckOutTime,
                        CreatedAt,
                        UpdatedAt
                    FROM AttendanceLogs
                    ORDER BY CheckInTime ASC", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new
                        {
                            LogID = reader.GetInt32(0),
                            MemberID = reader.GetInt32(1),
                            AttendanceId = reader.IsDBNull(2) ? null : reader.GetString(2),
                            CheckInTime = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CheckOutTime = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CreatedAt = reader.IsDBNull(5) ? null : reader.GetString(5),
                            UpdatedAt = reader.IsDBNull(6) ? null : reader.GetString(6)
                        });
                    }
                }
            }

            return logs;
        }


        //Abdul Hadi Code
        public void SaveDevice(Device device)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    string timestamp = GetTimestamp();

                    // Encrypt sensitive fields
                    string encryptedSerial = Encrypt(device.DeviceSerial);
                    string encryptedIP = Encrypt(device.DeviceIP);

                    command.CommandText = @"
INSERT INTO Devices 
(Department, AttendanceType, DeviceSerial, DeviceIP, Port, TimeZone, Location, ConnectionType, IsActive, LastSyncTime, FaceCapacity, RFIDCapacity, FingerCapacity, MachineNo, CreatedAt, UpdatedAt, Password) 
VALUES 
(@Department, @AttendanceType, @DeviceSerial, @DeviceIP, @Port, @TimeZone, @Location, @ConnectionType, @IsActive, @LastSyncTime, @FaceCapacity, @RFIDCapacity, @FingerCapacity, @MachineNo, @CreatedAt, @UpdatedAt, @Password)";

                    command.Parameters.AddWithValue("@Department", (object)device.Department ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AttendanceType", device.AttendanceType);
                    command.Parameters.AddWithValue("@DeviceSerial", encryptedSerial);
                    command.Parameters.AddWithValue("@DeviceIP", encryptedIP);
                    command.Parameters.AddWithValue("@Port", device.Port);
                    command.Parameters.AddWithValue("@TimeZone", device.TimeZone);
                    command.Parameters.AddWithValue("@Location", device.Location);
                    command.Parameters.AddWithValue("@ConnectionType", device.ConnectionType);
                    command.Parameters.AddWithValue("@IsActive", device.IsActive);
                    command.Parameters.AddWithValue("@LastSyncTime", (object)device.LastSyncTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FaceCapacity", (object)device.FaceCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RFIDCapacity", (object)device.RFIDCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FingerCapacity", (object)device.FingerCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MachineNo", device.MachineNo);
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);
                    command.Parameters.AddWithValue("@UpdatedAt", timestamp);
                    command.Parameters.AddWithValue("@Password", device.Password);

                    command.ExecuteNonQuery();
                }
            }
        }



        //Abdul Hadi Code
        public void UpdateDevice(Device device)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        string timestamp = GetTimestamp();
                        // Encrypt sensitive fields
                        string encryptedSerial = Encrypt(device.DeviceSerial);
                        string encryptedIP = Encrypt(device.DeviceIP);

                        command.CommandText = @"
UPDATE Devices SET 
    Department = @Department,
    AttendanceType = @AttendanceType,
    DeviceSerial = @DeviceSerial,
    DeviceIP = @DeviceIP,
    Port = @Port,
    TimeZone = @TimeZone,
    Location = @Location,
    ConnectionType = @ConnectionType,
    IsActive = @IsActive,
    LastSyncTime = @LastSyncTime,
    FaceCapacity = @FaceCapacity,
    RFIDCapacity = @RFIDCapacity,
    FingerCapacity = @FingerCapacity,
    MachineNo = @MachineNo,
    Password = @Password,
    UpdatedAt = @UpdatedAt
WHERE DeviceId = @DeviceId";

                        command.Parameters.AddWithValue("@DeviceId", device.DeviceId);
                        command.Parameters.AddWithValue("@Department", (object)device.Department ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AttendanceType", device.AttendanceType);
                        command.Parameters.AddWithValue("@DeviceSerial", encryptedSerial);
                        command.Parameters.AddWithValue("@DeviceIP", encryptedIP);
                        command.Parameters.AddWithValue("@Port", device.Port);
                        command.Parameters.AddWithValue("@TimeZone", device.TimeZone);
                        command.Parameters.AddWithValue("@Location", device.Location);
                        command.Parameters.AddWithValue("@ConnectionType", device.ConnectionType);
                        command.Parameters.AddWithValue("@IsActive", device.IsActive);
                        command.Parameters.AddWithValue("@LastSyncTime", (object)device.LastSyncTime ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FaceCapacity", (object)device.FaceCapacity ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RFIDCapacity", (object)device.RFIDCapacity ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FingerCapacity", (object)device.FingerCapacity ?? DBNull.Value);
                        command.Parameters.AddWithValue("@MachineNo", device.MachineNo);
                        command.Parameters.AddWithValue("@Password", device.Password);
                        command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SQLiteException ex) when (ex.ErrorCode == 19)
            {
                string detail = "Update failed due to a unique constraint violation.";

                if (ex.Message.Contains("DeviceSerial"))
                    detail += " A device with the same *DeviceSerial* already exists.";
                else if (ex.Message.Contains("DeviceIP"))
                    detail += " A device with the same *DeviceIP* already exists.";
                else if (ex.Message.Contains("MachineNo"))
                    detail += " A device with the same *Machine No* already exists.";

                throw new InvalidOperationException(detail, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the device.", ex);
            }
        }

        //Abdul Hadi Code // This code updated by Aamir. Port excluded from check
        // Check if a device with the same IP or Machine No already exists
        public string CheckDeviceExists(string deviceIP, int machineNo)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                SELECT DeviceIP, MachineNo
                FROM Devices
                WHERE DeviceIP = @DeviceIP OR MachineNo = @MachineNo;
            ";

                    command.Parameters.AddWithValue("@DeviceIP", Encrypt(deviceIP));
                    command.Parameters.AddWithValue("@MachineNo", machineNo);

                    using (var reader = command.ExecuteReader())
                    {
                        bool ipExists = false, machineExists = false;

                        while (reader.Read())
                        {
                            if (Decrypt(reader["DeviceIP"].ToString()) == deviceIP) ipExists = true;
                            if (Convert.ToInt32(reader["MachineNo"]) == machineNo) machineExists = true;
                        }

                        if (ipExists && machineExists)
                            return "Both Device IP and Machine No exist.";
                        else if (ipExists)
                            return "Device IP exists.";
                        else if (machineExists)
                            return "Machine No exists.";
                    }
                }
            }

            return "Neither IP nor Machine No exists.";
        }



        //Abdul Hadi Code
        // Check if a device with the same IP or Machine No already exists, excluding the current deviceId
        // Updated version - Port uniqueness check removed
        public string IDCheckDeviceExists(string deviceIP, int machineNo, int? deviceId = null)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    // Check for matching DeviceIP or MachineNo, but optionally skip for a specific deviceId
                    command.CommandText = @"
                SELECT DeviceId, DeviceIP, MachineNo
                FROM Devices
                WHERE (DeviceIP = @DeviceIP OR MachineNo = @MachineNo)
                AND (@DeviceId IS NULL OR DeviceId != @DeviceId);";

                    // Adding parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@DeviceIP", Encrypt(deviceIP));
                    command.Parameters.AddWithValue("@MachineNo", machineNo);
                    command.Parameters.AddWithValue("@DeviceId", deviceId.HasValue ? (object)deviceId.Value : DBNull.Value);

                    // Execute the query
                    using (var reader = command.ExecuteReader())
                    {
                        bool ipExists = false, machineExists = false;

                        // Check if the fields exist
                        while (reader.Read())
                        {
                            string existingIP = Decrypt(reader["DeviceIP"].ToString());
                            int existingMachineNo = Convert.ToInt32(reader["MachineNo"]);

                            if (existingIP == deviceIP) ipExists = true;
                            if (existingMachineNo == machineNo) machineExists = true;
                        }

                        // Construct the response based on which fields exist
                        if (ipExists && machineExists)
                        {
                            return "Device IP and Machine No exist.";
                        }
                        else if (ipExists)
                        {
                            return "Device IP exists.";
                        }
                        else if (machineExists)
                        {
                            return "Machine No exists.";
                        }
                    }
                }
            }

            return "";
        }


        public void DeleteDevice(int deviceId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM Devices WHERE DeviceId = @DeviceId";
                    command.Parameters.AddWithValue("@DeviceId", deviceId);
                    command.ExecuteNonQuery();
                }
            }
        }

        //Abdul Hadi Code
        public List<Device> LoadDevices()
        {
            var devices = new List<Device>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT * FROM Devices", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        devices.Add(new Device
                        {
                            DeviceId = Convert.ToInt32(reader["DeviceId"]),
                            Department = reader["Department"] as string,
                            AttendanceType = Convert.ToBoolean(reader["AttendanceType"]),
                            DeviceSerial = Decrypt(reader["DeviceSerial"].ToString()),
                            DeviceIP = Decrypt(reader["DeviceIP"].ToString()),
                            Port = Convert.ToInt32(reader["Port"]),
                            TimeZone = reader["TimeZone"].ToString(),
                            Location = reader["Location"].ToString(),
                            ConnectionType = reader["ConnectionType"].ToString(),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            LastSyncTime = reader["LastSyncTime"] as string,
                            FaceCapacity = Convert.ToInt32(reader["FaceCapacity"]),
                            RFIDCapacity = Convert.ToInt32(reader["RFIDCapacity"]),
                            FingerCapacity = Convert.ToInt32(reader["FingerCapacity"]),
                            MachineNo = Convert.ToInt32(reader["MachineNo"]),
                            CreatedAt = reader["CreatedAt"] as string,
                            UpdatedAt = reader["UpdatedAt"] as string,
                            Password = reader["Password"].ToString()
                        });
                    }
                }
            }

            return devices;
        }

        public async Task InsertLogsForMemberAcrossAllDevicesAsync(int memberId)
        {
            string faceDir = Path.Combine(Directory.GetCurrentDirectory(), "FACE");
            string facePath = Path.Combine(faceDir, $"{memberId}.jpg");
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get member biometric data
                string rfid = null;
                bool hasFingerprint = false;
                int? enrollmentId = null;
                bool hasFace = false;

                using (var cmd = new SQLiteCommand(
                    "SELECT RFID, Fingerprint, EnrollmentID FROM Members WHERE MemberID = @memberId",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rfid = reader["RFID"] != DBNull.Value ? reader["RFID"].ToString() : null;
                            hasFingerprint = reader["Fingerprint"] != DBNull.Value && Convert.ToBoolean(reader["Fingerprint"]);
                            enrollmentId = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : (int?)null;
                        }
                    }
                }

                // Check for face image
                hasFace = File.Exists(facePath);

                // Skip if no biometric data
                if (string.IsNullOrWhiteSpace(rfid) && !hasFace && !hasFingerprint)
                {
                    LogHelper.Log($"No biometric data for MemberID {memberId}. Skipping.");
                    return;
                }

                // Prepare biometric data
                var biometricData = new Dictionary<string, string>();
                var syncStatus = new Dictionary<string, bool>();

                if (!string.IsNullOrWhiteSpace(rfid))
                {
                    biometricData.Add("RFID", rfid);
                    syncStatus.Add("RFID", false);
                }

                if (hasFace)
                {
                    biometricData.Add("Face", facePath);
                    syncStatus.Add("Face", false);
                }

                if (hasFingerprint && enrollmentId.HasValue)
                {
                    biometricData.Add("Fingerprint", enrollmentId.Value.ToString());
                    syncStatus.Add("Fingerprint", false);
                }

                // Get all active devices
                List<Device> devices = new List<Device>();
                using (var cmd = new SQLiteCommand(
                    "SELECT DeviceId FROM Devices WHERE IsActive = 1;",
                    connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        devices.Add(new Device
                        {
                            DeviceId = reader.GetInt32(0)
                        });
                    }
                }

                // Process each device
                foreach (var device in devices)
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check for existing log
                            bool logExists = false;
                            Dictionary<string, string> existingBiometrics = null;
                            Dictionary<string, bool> existingSync = null;

                            using (var checkCmd = new SQLiteCommand(
                                "SELECT BiometricData, SyncStatus FROM Log " +
                                "WHERE MemberID = @memberId AND DeviceID = @deviceId",
                                connection, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@memberId", memberId);
                                checkCmd.Parameters.AddWithValue("@deviceId", device.DeviceId);

                                using (var reader = await checkCmd.ExecuteReaderAsync())
                                {
                                    if (await reader.ReadAsync())
                                    {
                                        logExists = true;
                                        existingBiometrics = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                            reader["BiometricData"].ToString());
                                        existingSync = JsonConvert.DeserializeObject<Dictionary<string, bool>>(
                                            reader["SyncStatus"].ToString());
                                    }
                                }
                            }

                            // Check if update needed
                            bool needsUpdate = !logExists;
                            if (logExists)
                            {
                                foreach (var bio in biometricData)
                                {
                                    if (!existingBiometrics.ContainsKey(bio.Key) ||
                                        existingBiometrics[bio.Key] != bio.Value)
                                    {
                                        needsUpdate = true;
                                        break;
                                    }
                                }
                            }

                            if (needsUpdate)
                            {
                                // Merge with existing data
                                if (logExists)
                                {
                                    foreach (var existing in existingBiometrics)
                                    {
                                        if (!biometricData.ContainsKey(existing.Key))
                                        {
                                            biometricData.Add(existing.Key, existing.Value);
                                            syncStatus.Add(existing.Key, existingSync[existing.Key]);
                                        }
                                    }
                                }

                                string bioJson = JsonConvert.SerializeObject(biometricData);
                                string syncJson = JsonConvert.SerializeObject(syncStatus);

                                if (logExists)
                                {
                                    // Update existing
                                    using (var updateCmd = new SQLiteCommand(
                                        "UPDATE Log SET " +
                                        "EnrollmentID = @enrollmentId, " +
                                        "BiometricData = @biometricData, " +
                                        "SyncStatus = @syncStatus, " +
                                        "UpdatedAt = @updatedAt " +
                                        "WHERE MemberID = @memberId AND DeviceID = @deviceId",
                                        connection, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId ?? (object)DBNull.Value);
                                        updateCmd.Parameters.AddWithValue("@biometricData", bioJson);
                                        updateCmd.Parameters.AddWithValue("@syncStatus", syncJson);
                                        updateCmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                        updateCmd.Parameters.AddWithValue("@memberId", memberId);
                                        updateCmd.Parameters.AddWithValue("@deviceId", device.DeviceId);
                                        await updateCmd.ExecuteNonQueryAsync();
                                    }
                                }
                                else
                                {
                                    // Insert new
                                    using (var insertCmd = new SQLiteCommand(
                                        "INSERT INTO Log (" +
                                        "MemberID, DeviceID, EnrollmentID, " +
                                        "BiometricData, SyncStatus, CreatedAt, UpdatedAt" +
                                        ") VALUES (" +
                                        "@memberId, @deviceId, @enrollmentId, " +
                                        "@biometricData, @syncStatus, @createdAt, @updatedAt)",
                                        connection, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@memberId", memberId);
                                        insertCmd.Parameters.AddWithValue("@deviceId", device.DeviceId);
                                        insertCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId ?? (object)DBNull.Value);
                                        insertCmd.Parameters.AddWithValue("@biometricData", bioJson);
                                        insertCmd.Parameters.AddWithValue("@syncStatus", syncJson);
                                        insertCmd.Parameters.AddWithValue("@createdAt", timestamp);
                                        insertCmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                        await insertCmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogHelper.Log($"Error processing device {device.DeviceId} for member {memberId}: {ex.Message}");
                        }
                    }
                }
            }
        }

        
        // Get active members with biometrics but no logs
        public List<int> GetActiveMemberIdsWithBiometricsAndNoLogs()
        {
            var memberIds = new List<int>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                const string sql = @"
SELECT m.MemberID
FROM Members m
WHERE LOWER(TRIM(IFNULL(m.Status, ''))) = 'active'
  AND m.EnrollmentID IS NOT NULL
  AND (
        TRIM(IFNULL(m.RFID, '')) <> ''
     OR m.Fingerprint = 1
     OR TRIM(IFNULL(m.Picture, '')) <> ''
  )
  AND NOT EXISTS (
        SELECT 1
        FROM Log l
        WHERE l.MemberID = m.MemberID
  );";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        memberIds.Add(reader.GetInt32(0));
                }
            }

            return memberIds;
        }

        public async Task InsertLogsForAllMembersAsync(int memberId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                try
                {
                    await InsertLogsForMemberAcrossAllDevicesAsync(memberId);
                    LogHelper.Log($"Log processing complete for MemberID {memberId}. Success: 1, Failed: 0, Total: 1");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error inserting logs for MemberID {memberId}: {ex.Message}");
                    LogHelper.Log($"Log processing complete for MemberID {memberId}. Success: 0, Failed: 1, Total: 1");
                }
            }
        }

        public async Task UpdateBiometricSyncStatus(int logId, string biometricType, bool isSynced)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Get current sync status from Log table
                        Dictionary<string, bool> syncStatus = null;
                        using (var cmd = new SQLiteCommand(
                            "SELECT SyncStatus FROM Log WHERE Id = @logId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@logId", logId);
                            var result = await cmd.ExecuteScalarAsync();

                            if (result != null && result != DBNull.Value)
                            {
                                syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(result.ToString());
                            }
                            else
                            {
                                throw new Exception($"Log entry with ID {logId} not found");
                            }
                        }

                        // 2. Update the specific biometric type status
                        if (syncStatus.ContainsKey(biometricType))
                        {
                            syncStatus[biometricType] = isSynced;
                        }
                        else
                        {
                            syncStatus.Add(biometricType, isSynced);
                        }

                        // 3. Update only the Log table
                        using (var updateCmd = new SQLiteCommand(
                            "UPDATE Log SET SyncStatus = @syncStatus WHERE Id = @logId",
                            connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                            updateCmd.Parameters.AddWithValue("@logId", logId);
                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        LogHelper.Log($"Updated {biometricType} sync status for log {logId} to {isSynced}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error updating sync status for log {logId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }


        public async Task<bool> CheckAndUpdateMemberDeviceSyncStatus(int memberId, int deviceId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                bool logExists = false;
                using (var checkCmd = new SQLiteCommand(
                    "SELECT 1 FROM Log WHERE MemberID = @memberId AND DeviceID = @deviceId",
                    connection))
                {
                    checkCmd.Parameters.AddWithValue("@memberId", memberId);
                    checkCmd.Parameters.AddWithValue("@deviceId", deviceId);
                    logExists = (await checkCmd.ExecuteScalarAsync()) != null;
                }

                if (!logExists) return false;

                bool allSynced = true;
                using (var cmd = new SQLiteCommand(
                    "SELECT BiometricData, SyncStatus FROM Log WHERE MemberID = @memberId AND DeviceID = @deviceId",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    cmd.Parameters.AddWithValue("@deviceId", deviceId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var biometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                reader["BiometricData"].ToString());
                            var syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(
                                reader["SyncStatus"].ToString());

                            allSynced = biometricData.Keys.All(key =>
                                syncStatus.ContainsKey(key) && syncStatus[key]);
                        }
                    }
                }

                if (!allSynced) return false;

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<int, bool> memberSyncStatus = new Dictionary<int, bool>();

                        using (var cmd = new SQLiteCommand(
                            "SELECT SyncStatus FROM Members WHERE MemberID = @memberId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@memberId", memberId);
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                memberSyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(result.ToString())
                                                  ?? new Dictionary<int, bool>();
                            }
                        }

                        // ✅ Skip DB write if already true
                        bool alreadyTrue;
                        if (memberSyncStatus.TryGetValue(deviceId, out alreadyTrue) && alreadyTrue)
                        {
                            transaction.Commit();
                            return false; // no change performed
                        }

                        memberSyncStatus[deviceId] = true;

                        using (var updateCmd = new SQLiteCommand(
                            "UPDATE Members SET SyncStatus = @syncStatus WHERE MemberID = @memberId",
                            connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(memberSyncStatus));
                            updateCmd.Parameters.AddWithValue("@memberId", memberId);
                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        LogHelper.Log($"Updated member {memberId} sync status for device {deviceId}");

                        transaction.Commit();
                        return true; // changed
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error updating member sync status: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public int GetFullySyncedMembersCountAgainstAllDevices(bool onlyActiveDevices = true)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 1) Load device IDs (active only by default)
                var deviceIds = new List<int>();
                using (var deviceCmd = new SQLiteCommand(
                    onlyActiveDevices
                        ? "SELECT DeviceId FROM Devices WHERE IsActive = 1"
                        : "SELECT DeviceId FROM Devices",
                    connection))
                using (var reader = deviceCmd.ExecuteReader())
                {
                    while (reader.Read())
                        deviceIds.Add(reader.GetInt32(0));
                }

                // If no devices, no one can be "synced against all devices"
                if (deviceIds.Count == 0)
                    return 0;

                // 2) Scan all members and count those whose SyncStatus has TRUE for every deviceId
                int fullySyncedCount = 0;

                using (var memberCmd = new SQLiteCommand(
                    "SELECT MemberID, SyncStatus FROM Members",
                    connection))
                using (var reader = memberCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string syncStatusJson = reader.IsDBNull(1) ? "{}" : reader.GetString(1);

                        Dictionary<int, bool> syncDict;
                        try
                        {
                            syncDict = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson)
                                       ?? new Dictionary<int, bool>();
                        }
                        catch
                        {
                            syncDict = new Dictionary<int, bool>();
                        }

                        bool allDevicesTrue = true;
                        for (int i = 0; i < deviceIds.Count; i++)
                        {
                            int deviceId = deviceIds[i];
                            bool value;
                            if (!syncDict.TryGetValue(deviceId, out value) || !value)
                            {
                                allDevicesTrue = false;
                                break;
                            }
                        }

                        if (allDevicesTrue)
                            fullySyncedCount++;
                    }
                }

                return fullySyncedCount;
            }
        }

        //Abdul Hadi Code
        // Get unsynced log entries
        public async Task<List<LogEntry>> GetUnsyncedLogEntriesAsync()
        {
            var unsyncedEntries = new List<LogEntry>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(
                    "SELECT Id, MemberID, DeviceID, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt " +
                    "FROM Log WHERE SyncStatus LIKE '%false%'", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var entry = new LogEntry
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            DeviceID = Convert.ToInt32(reader["DeviceID"]),
                            EnrollmentID = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : 0,
                            BiometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader["BiometricData"].ToString()),
                            SyncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(reader["SyncStatus"].ToString()),
                            CreatedAt = reader["CreatedAt"].ToString(),
                            UpdatedAt = reader["UpdatedAt"].ToString()
                        };
                        if (entry.SyncStatus.Any(s => !s.Value)) unsyncedEntries.Add(entry);
                    }
                }
            }
            return unsyncedEntries;
        }

        public bool AddOrUpdateFingerprint(FingerprintData fingerprintdata)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                INSERT INTO Fingerprints 
                (EnrollNumber, FingerNumber, Privilege, TemplateData, TemplateString, CreatedAt, UpdatedAt) 
                VALUES 
                (@EnrollNumber, @FingerNumber, @Privilege, @TemplateData, @TemplateString, @CreatedAt, @UpdatedAt)
                ON CONFLICT(EnrollNumber) DO UPDATE SET 
                    FingerNumber = excluded.FingerNumber,
                    Privilege = excluded.Privilege,
                    TemplateData = excluded.TemplateData,
                    TemplateString = excluded.TemplateString,
                    UpdatedAt = excluded.UpdatedAt";

                        var now = GetTimestamp();

                        command.Parameters.AddWithValue("@EnrollNumber", fingerprintdata.EnrollNumber);
                        command.Parameters.AddWithValue("@FingerNumber", fingerprintdata.FingerNumber);
                        command.Parameters.AddWithValue("@Privilege", fingerprintdata.Privilege);
                        command.Parameters.AddWithValue("@TemplateData", fingerprintdata.TemplateData ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TemplateString", fingerprintdata.TemplateString ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedAt", fingerprintdata.CreatedAt ?? now);
                        command.Parameters.AddWithValue("@UpdatedAt", now);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddOrUpdateFingerprint: {ex.Message}");
                return false;
            }
        }


        public List<FingerprintData> GetAllFingerprints()
        {
            var fingerprints = new List<FingerprintData>();
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Fingerprints", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fingerprints.Add(new FingerprintData
                            {
                                EnrollNumber = Convert.ToInt32(reader["EnrollNumber"]),
                                FingerNumber = Convert.ToInt32(reader["FingerNumber"]),
                                Privilege = Convert.ToInt32(reader["Privilege"]),
                                TemplateData = reader["TemplateData"] as byte[],
                                TemplateString = reader["TemplateString"]?.ToString(),
                                CreatedAt = reader["CreatedAt"]?.ToString(),
                                UpdatedAt = reader["UpdatedAt"]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllFingerprints: {ex.Message}");
            }
            return fingerprints;
        }

        public List<byte[]> GetAllBinaryTemplates()
        {
            var templates = new List<byte[]>();
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(
                        "SELECT TemplateData FROM Fingerprints WHERE TemplateData IS NOT NULL", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["TemplateData"] != DBNull.Value)
                            {
                                templates.Add((byte[])reader["TemplateData"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllBinaryTemplates: {ex.Message}");
            }
            return templates;
        }

        public List<string> GetAllStringTemplates()
        {
            var templates = new List<string>();
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(
                        "SELECT TemplateString FROM Fingerprints WHERE TemplateString IS NOT NULL", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["TemplateString"] != DBNull.Value)
                            {
                                templates.Add(reader["TemplateString"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllStringTemplates: {ex.Message}");
            }
            return templates;
        }

        public bool DeleteFingerprint(int enrollNumber)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(
                        "DELETE FROM Fingerprints WHERE EnrollNumber = @EnrollNumber", connection))
                    {
                        command.Parameters.AddWithValue("@EnrollNumber", enrollNumber);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteFingerprint: {ex.Message}");
                return false;
            }
        }

        public bool DeleteAllFingerprints()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("DELETE FROM Fingerprints", connection))
                    {
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteAllFingerprints: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Member>> GetMembersWithMissingFingerprintsAsync()
        {
            List<Member> members = new List<Member>();

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                SELECT m.MemberID, m.Name, m.Status, m.RFID, m.Picture, 
                       m.Fingerprint, m.EnrollmentID, m.PackageExpiry
                FROM Members m
                LEFT JOIN Fingerprints f ON m.EnrollmentID = f.EnrollNumber
                WHERE m.Fingerprint = 1 
                AND f.EnrollNumber IS NULL
                AND m.EnrollmentID IS NOT NULL";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    Member member = new Member
                                    {
                                        MemberID = Convert.ToInt32(reader["MemberID"]),
                                        Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : null,
                                        Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                        RFID = reader["RFID"] != DBNull.Value ? reader["RFID"].ToString() : null,
                                        Picture = reader["Picture"] != DBNull.Value ? reader["Picture"].ToString() : null,
                                        Fingerprint = Convert.ToBoolean(reader["Fingerprint"]),
                                        EnrollmentID = reader["EnrollmentID"] != DBNull.Value
                                                    ? Convert.ToInt32(reader["EnrollmentID"])
                                                    : (int?)null,
                                        PackageExpiry = reader["PackageExpiry"] != DBNull.Value
                                                    ? reader["PackageExpiry"].ToString()
                                                    : null
                                    };
                                    members.Add(member);

                                    // Log each found member
                                    LogHelper.Log($"Found member with missing fingerprint - " +
                                                $"MemberID: {member.MemberID}, " +
                                                $"Name: {member.Name}, " +
                                                $"EnrollmentID: {member.EnrollmentID}, " +
                                                $"RFID: {member.RFID}");
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Log($"Error processing member record: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                LogHelper.Log($"Completed missing fingerprints check. Found {members.Count} members.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in GetMembersWithMissingFingerprintsAsync: {ex.Message}");
                throw; // Re-throw if you want calling code to handle it
            }

            return members;
        }


        public (bool shouldDisable, List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)> deviceInfo)
        CheckMemberAccessStatus(int memberId)
        {
            var deviceInfo = new List<(int, string, int, string, int, int)>();
            bool shouldDisable = false;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = @"
                SELECT m.Status, m.PackageExpiry, m.EnrollmentID,
                       d.DeviceIP, d.Port, d.MachineNo
                FROM Members m
                JOIN Log l ON m.MemberID = l.MemberID
                JOIN Devices d ON l.DeviceID = d.DeviceId
                WHERE m.MemberID = @memberId";
                    cmd.Parameters.AddWithValue("@memberId", memberId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Safe reading with null checks
                            string status = reader["Status"]?.ToString() ?? "active";
                            string packageExpiry = reader["PackageExpiry"]?.ToString();
                            int enrollmentNumber = reader["EnrollmentID"] != DBNull.Value ?
                                Convert.ToInt32(reader["EnrollmentID"]) : 0;

                            // Parse expiry date safely
                            bool isExpired = false;
                            if (!string.IsNullOrEmpty(packageExpiry) &&
                                DateTime.TryParse(packageExpiry, out DateTime expiryDate))
                            {
                                isExpired = expiryDate < DateTime.Today;
                            }

                            // If member already marked "Disabled" in DB, do not attempt disable again
                            if (status.Equals("disabled", StringComparison.OrdinalIgnoreCase))
                            {
                                shouldDisable = false;
                            }
                            else
                            {
                                // Also disable for overdue/inactive/expired status
                                bool isInactive = status.Equals("inactive", StringComparison.OrdinalIgnoreCase);
                                bool isOverdue = status.Equals("overdue", StringComparison.OrdinalIgnoreCase);

                                shouldDisable = isInactive || isOverdue || isExpired;
                            }

                            // Reset reader to process all rows
                            do
                            {
                                int machineNo = reader["MachineNo"] != DBNull.Value ?
                                    Convert.ToInt32(reader["MachineNo"]) : 0;
                                string ip = Decrypt(reader["DeviceIP"]?.ToString()) ?? "0.0.0.0";
                                int port = reader["Port"] != DBNull.Value ?
                                    Convert.ToInt32(reader["Port"]) : 0;

                                deviceInfo.Add((
                                    machineNo,
                                    ip,
                                    port,       // Now taken from Devices table
                                    "0",        // Fixed password
                                    enrollmentNumber,
                                    0           // Fixed backupNumber
                                ));
                            } while (reader.Read());
                        }
                    }
                }
            }

            return (shouldDisable, deviceInfo);
        }

        public Dictionary<int, (bool shouldDisable, List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)> deviceInfo)>
            GetMembersAccessStatus()
        {
            var results = new Dictionary<int, (bool, List<(int, string, int, string, int, int)>)>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Get only members who have logs
                using (var cmd = new SQLiteCommand(@"
            SELECT DISTINCT m.MemberID
            FROM Members m
            JOIN Log l ON m.MemberID = l.MemberID", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int memberId = Convert.ToInt32(reader["MemberID"]);
                        results[memberId] = CheckMemberAccessStatus(memberId);
                    }
                }
            }

            return results;
        }

        public List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)>
            GetDetails(int memberId)
        {
            var deviceDetails = new List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)>();
            int totalDevices = 0;

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(@"
                SELECT 
                    m.EnrollmentID,
                    d.DeviceIP, 
                    d.Port, 
                    d.MachineNo
                FROM Members m
                JOIN Log l ON m.MemberID = l.MemberID
                JOIN Devices d ON l.DeviceID = d.DeviceId
                WHERE m.MemberID = @memberId", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                LogHelper.Log($"⚠️ No devices found for member {memberId}");
                                return deviceDetails;
                            }

                            LogHelper.Log($"\n📋 Device Details for Member {memberId}:");
                            LogHelper.Log($"----------------------------------------");

                            while (reader.Read())
                            {
                                int enrollmentNumber = reader["EnrollmentID"] != DBNull.Value ?
                                    Convert.ToInt32(reader["EnrollmentID"]) : 0;
                                int machineNo = reader["MachineNo"] != DBNull.Value ?
                                    Convert.ToInt32(reader["MachineNo"]) : 0;
                                string ip = Decrypt(reader["DeviceIP"]?.ToString()) ?? "0.0.0.0";
                                int port = reader["Port"] != DBNull.Value ?
                                    Convert.ToInt32(reader["Port"]) : 0;

                                deviceDetails.Add((
                                    machineNo: machineNo,
                                    ip: ip,
                                    port: port,
                                    password: "0",
                                    enrollmentNumber: enrollmentNumber,
                                    backupNumber: 0
                                ));
                                totalDevices++;

                                // Log each device
                                LogHelper.Log($"🔧 Device {totalDevices}:");
                                LogHelper.Log($"   ├─ Machine No: {machineNo}");
                                LogHelper.Log($"   ├─ IP:Port: {ip}:{port}");
                                LogHelper.Log($"   └─ Enrollment: {enrollmentNumber}");
                            }

                            LogHelper.Log($"\n📊 Summary:");
                            LogHelper.Log($"   Total Devices: {totalDevices}");
                            LogHelper.Log($"----------------------------------------");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error fetching details for member {memberId}: {ex.Message}");
                LogHelper.Log($"🔍 Stack Trace: {ex.StackTrace}");
            }

            return deviceDetails;
        }


        public List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)>
    GetAvailableDeviceDetails(int memberId)
        {
            var deviceDetails = new List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)>();
            int totalDevices = 0;

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Check if the member exists in the Log table
                    using (var checkCmd = new SQLiteCommand(@"
                SELECT 1 
                FROM Log 
                WHERE MemberID = @memberId", connection))
                    {
                        checkCmd.Parameters.AddWithValue("@memberId", memberId);
                        var exists = checkCmd.ExecuteScalar();

                        if (exists == null)
                        {
                            LogHelper.Log($"⚠️ Member {memberId} does not exist in the Log table.");
                            return deviceDetails;
                        }
                    }

                    // Retrieve device details based on availability
                    using (var cmd = new SQLiteCommand(@"
                SELECT 
                    m.EnrollmentID,
                    d.DeviceIP, 
                    d.Port, 
                    d.MachineNo,
                    d.IsActive
                FROM Members m
                JOIN Log l ON m.MemberID = l.MemberID
                JOIN Devices d ON l.DeviceID = d.DeviceId
                WHERE m.MemberID = @memberId AND d.IsActive = 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                LogHelper.Log($"⚠️ No active devices found for member {memberId}");
                                return deviceDetails;
                            }

                            LogHelper.Log($"\n📋 Active Device Details for Member {memberId}:");
                            LogHelper.Log($"----------------------------------------");

                            while (reader.Read())
                            {
                                int enrollmentNumber = reader["EnrollmentID"] != DBNull.Value ?
                                    Convert.ToInt32(reader["EnrollmentID"]) : 0;
                                int machineNo = reader["MachineNo"] != DBNull.Value ?
                                    Convert.ToInt32(reader["MachineNo"]) : 0;
                                string ip = Decrypt(reader["DeviceIP"]?.ToString()) ?? "0.0.0.0";
                                int port = reader["Port"] != DBNull.Value ?
                                    Convert.ToInt32(reader["Port"]) : 0;

                                deviceDetails.Add((
                                    machineNo: machineNo,
                                    ip: ip,
                                    port: port,
                                    password: "0",
                                    enrollmentNumber: enrollmentNumber,
                                    backupNumber: 0
                                ));
                                totalDevices++;

                                // Log each device
                                LogHelper.Log($"🔧 Device {totalDevices}:");
                                LogHelper.Log($"   ├─ Machine No: {machineNo}");
                                LogHelper.Log($"   ├─ IP:Port: {ip}:{port}");
                                LogHelper.Log($"   └─ Enrollment: {enrollmentNumber}");
                            }

                            LogHelper.Log($"\n📊 Summary:");
                            LogHelper.Log($"   Total Active Devices: {totalDevices}");
                            LogHelper.Log($"----------------------------------------");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error fetching available device details for member {memberId}: {ex.Message}");
                LogHelper.Log($"🔍 Stack Trace: {ex.StackTrace}");
            }

            return deviceDetails;
        }


        public (bool deviceExists, bool syncStatus, string memberStatus) GetMemberDeviceSyncStatus(int memberId, int deviceId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(@"
                SELECT SyncStatus, Status 
                FROM Members 
                WHERE MemberID = @memberId", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        LogHelper.Log($"⚡ Checking sync status for member {memberId} and device {deviceId}");

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string syncStatusJson = reader["SyncStatus"]?.ToString();
                                string memberStatus = reader["Status"]?.ToString() ?? "Unknown";

                                if (string.IsNullOrEmpty(syncStatusJson))
                                {
                                    LogHelper.Log($"⚠️ No sync status found for member {memberId}");
                                    return (false, false, memberStatus);
                                }

                                try
                                {
                                    // Parse the JSON sync status
                                    var syncStatusDict = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson);

                                    if (syncStatusDict != null && syncStatusDict.ContainsKey(deviceId))
                                    {
                                        bool syncStatus = syncStatusDict[deviceId];
                                        LogHelper.Log($"📊 Member {memberId}, Device {deviceId}: {(syncStatus ? "✅ Synced" : "❌ Not Synced")} (Member Status: {memberStatus})");
                                        return (true, syncStatus, memberStatus);
                                    }
                                    else
                                    {
                                        LogHelper.Log($"⚠️ Device {deviceId} not found in sync status for member {memberId}");
                                        return (false, false, memberStatus);
                                    }
                                }
                                catch (JsonException ex)
                                {
                                    LogHelper.Log($"❌ Error parsing sync status JSON for member {memberId}: {ex.Message}");
                                    return (false, false, memberStatus);
                                }
                            }
                            else
                            {
                                LogHelper.Log($"⚠️ Member {memberId} not found in Members table");
                                return (false, false, "Not Found");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error checking member-device sync status for member {memberId}, device {deviceId}: {ex.Message}");
                LogHelper.Log($"🔍 Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public bool UpdateMemberStatus(int memberId, string newStatus)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    LogHelper.Log($"ℹ️ Updating status for member {memberId} to '{newStatus}'");

                    using (var cmd = new SQLiteCommand(@"
                UPDATE Members 
                SET Status = @status,
                    UpdatedAt = CURRENT_TIMESTAMP
                WHERE MemberID = @memberId", connection))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@memberId", memberId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            LogHelper.Log($"✅ Successfully updated status for member {memberId}");
                            return true;
                        }
                        else
                        {
                            LogHelper.Log($"⚠️ No member found with ID {memberId}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error updating status for member {memberId}: {ex.Message}");
                LogHelper.Log($"🔍 Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool DeleteMemberLogs(int memberId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    LogHelper.Log($"ℹ️ Preparing to delete logs for member {memberId}");

                    using (var transaction = connection.BeginTransaction())
                    using (var cmd = new SQLiteCommand(@"
                DELETE FROM Log 
                WHERE MemberID = @memberId", connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        transaction.Commit();

                        LogHelper.Log($"🗑️ Deleted {rowsAffected} log entries for member {memberId}");
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error deleting logs for member {memberId}: {ex.Message}");
                LogHelper.Log($"🔍 Stack Trace: {ex.StackTrace}");
                return false;
            }
        }


        // Check if this is the first device in the database
        public bool IsFirstDevice()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Devices", connection))
                    {
                        int deviceCount = Convert.ToInt32(cmd.ExecuteScalar());
                        return deviceCount == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error checking device count: {ex.Message}");
                return false;
            }
        }

        // Check if there are any unsynced logs
        public async Task<bool> CheckForUnsyncedLogs()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SQLiteCommand(@"
                SELECT EXISTS(
                    SELECT 1 FROM Log 
                    WHERE SyncStatus LIKE '%false%' 
                    LIMIT 1
                )", connection))
                    {
                        return Convert.ToBoolean(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error checking for unsynced logs: {ex.Message}");
                return false;
            }
        }



        public async Task<List<LogBiometricChange>> VerifyAndUpdateLogBiometricsAsync(int memberId)
        {
            var changes = new List<LogBiometricChange>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                // Check if the member exists in the Log table
                bool memberExistsInLog = false;
                using (var checkCmd = new SQLiteCommand("SELECT 1 FROM Log WHERE MemberID = @memberId LIMIT 1", connection))
                {
                    checkCmd.Parameters.AddWithValue("@memberId", memberId);
                    memberExistsInLog = (await checkCmd.ExecuteScalarAsync()) != null;
                }

                if (!memberExistsInLog)
                {
                    LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Member {memberId} not found in Log table. Skipping biometric verification/update.");
                    return changes;
                }

                // Get member's biometric data and SyncStatus
                string rfid = null, picture = null, syncStatusJson = null;
                bool hasFingerprint = false;
                int? enrollmentId = null;
                string facePath = Path.Combine(Directory.GetCurrentDirectory(), "FACE", $"{memberId}.jpg");
                bool hasFace = File.Exists(facePath);

                using (var cmd = new SQLiteCommand("SELECT RFID, Fingerprint, EnrollmentID, Picture, SyncStatus FROM Members WHERE MemberID = @memberId", connection))
                {
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rfid = reader["RFID"] != DBNull.Value ? reader["RFID"].ToString() : null;
                            hasFingerprint = reader["Fingerprint"] != DBNull.Value && Convert.ToBoolean(reader["Fingerprint"]);
                            enrollmentId = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : (int?)null;
                            picture = reader["Picture"] != DBNull.Value ? reader["Picture"].ToString() : null;
                            syncStatusJson = reader["SyncStatus"] != DBNull.Value ? reader["SyncStatus"].ToString() : null;
                        }
                    }
                }

                // Parse device list from SyncStatus JSON
                var deviceIds = new List<int>();
                if (!string.IsNullOrWhiteSpace(syncStatusJson))
                {
                    var syncStatusDict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(syncStatusJson);
                    if (syncStatusDict != null)
                    {
                        foreach (var key in syncStatusDict.Keys)
                        {
                            if (int.TryParse(key, out int id))
                                deviceIds.Add(id);
                        }
                    }
                }

                //LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Processing member {memberId} for devices: {string.Join(", ", deviceIds)}");

                // For each device, ensure log exists and is up to date
                foreach (var deviceId in deviceIds)
                {
                    int? logEnrollmentId = null;
                    Dictionary<string, string> biometricData = null;
                    Dictionary<string, bool> syncStatus = null;
                    int logId = 0;
                    bool logExists = false;

                    using (var logCmd = new SQLiteCommand("SELECT Id, EnrollmentID, BiometricData, SyncStatus FROM Log WHERE MemberID = @memberId AND DeviceID = @deviceId", connection))
                    {
                        logCmd.Parameters.AddWithValue("@memberId", memberId);
                        logCmd.Parameters.AddWithValue("@deviceId", deviceId);
                        using (var reader = await logCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                logExists = true;
                                logId = reader.GetInt32(0);
                                logEnrollmentId = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : (int?)null;
                                biometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader["BiometricData"].ToString());
                                syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(reader["SyncStatus"].ToString());
                            }
                        }
                    }

                    if (!logExists)
                    {
                        biometricData = new Dictionary<string, string>();
                        syncStatus = new Dictionary<string, bool>();
                        if (!string.IsNullOrEmpty(rfid))
                        {
                            biometricData["RFID"] = rfid;
                            syncStatus["RFID"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "RFID", DeviceId = deviceId, EnrollmentId = enrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will insert RFID for member {memberId} on device {deviceId}");
                        }
                        // Fixed: Check if picture field has value AND face file exists
                        if (!string.IsNullOrEmpty(picture) && hasFace)
                        {
                            biometricData["Face"] = facePath;
                            syncStatus["Face"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "Face", DeviceId = deviceId, EnrollmentId = enrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will insert Face for member {memberId} on device {deviceId}");
                        }
                        if (hasFingerprint && enrollmentId.HasValue)
                        {
                            biometricData["Fingerprint"] = enrollmentId.Value.ToString();
                            syncStatus["Fingerprint"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "Fingerprint", DeviceId = deviceId, EnrollmentId = enrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will insert Fingerprint for member {memberId} on device {deviceId}");
                        }
                        if (biometricData.Count > 0)
                        {
                            using (var insertCmd = new SQLiteCommand(
                                "INSERT INTO Log (MemberID, DeviceID, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt) VALUES (@memberId, @deviceId, @enrollmentId, @biometricData, @syncStatus, @createdAt, @updatedAt)", connection))
                            {
                                insertCmd.Parameters.AddWithValue("@memberId", memberId);
                                insertCmd.Parameters.AddWithValue("@deviceId", deviceId);
                                insertCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId ?? (object)DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@biometricData", JsonConvert.SerializeObject(biometricData));
                                insertCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                                var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                                insertCmd.Parameters.AddWithValue("@createdAt", now);
                                insertCmd.Parameters.AddWithValue("@updatedAt", now);
                                await insertCmd.ExecuteNonQueryAsync();
                                LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Inserted new log for member {memberId} on device {deviceId} with biometrics: {string.Join(", ", biometricData.Keys)}");
                            }
                        }
                        continue;
                    }

                    // Update existing log if needed
                    bool updated = false;

                    // RFID
                    if (biometricData.ContainsKey("RFID"))
                    {
                        if (string.IsNullOrEmpty(rfid))
                        {
                            biometricData.Remove("RFID");
                            syncStatus.Remove("RFID");
                            changes.Add(new LogBiometricChange { BiometricType = "RFID", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will remove RFID for member {memberId} on device {deviceId}");
                            updated = true;
                        }
                        else if (biometricData["RFID"] != rfid)
                        {
                            biometricData["RFID"] = rfid;
                            syncStatus["RFID"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "RFID", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will update RFID for member {memberId} on device {deviceId}");
                            updated = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(rfid))
                    {
                        biometricData["RFID"] = rfid;
                        syncStatus["RFID"] = false;
                        changes.Add(new LogBiometricChange { BiometricType = "RFID", DeviceId = deviceId, EnrollmentId = logEnrollmentId ?? enrollmentId });
                        LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will add RFID for member {memberId} on device {deviceId}");
                        updated = true;
                    }

                    // Face - FIXED LOGIC
                    if (biometricData.ContainsKey("Face"))
                    {
                        // Remove Face if picture field is empty/null OR face file doesn't exist
                        if (string.IsNullOrEmpty(picture) || !hasFace)
                        {
                            biometricData.Remove("Face");
                            syncStatus.Remove("Face");
                            changes.Add(new LogBiometricChange { BiometricType = "Face", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will remove Face for member {memberId} on device {deviceId} (picture={picture ?? "NULL"}, hasFace={hasFace})");
                            updated = true;
                        }
                        // Update Face if path changed
                        else if (biometricData["Face"] != facePath)
                        {
                            biometricData["Face"] = facePath;
                            syncStatus["Face"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "Face", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will update Face for member {memberId} on device {deviceId}");
                            updated = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(picture) && hasFace)
                    {
                        // Add Face if picture field has value AND face file exists
                        biometricData["Face"] = facePath;
                        syncStatus["Face"] = false;
                        changes.Add(new LogBiometricChange { BiometricType = "Face", DeviceId = deviceId, EnrollmentId = logEnrollmentId ?? enrollmentId });
                        LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will add Face for member {memberId} on device {deviceId}");
                        updated = true;
                    }

                    // Fingerprint
                    if (biometricData.ContainsKey("Fingerprint"))
                    {
                        if (!hasFingerprint || !enrollmentId.HasValue)
                        {
                            biometricData.Remove("Fingerprint");
                            syncStatus.Remove("Fingerprint");
                            changes.Add(new LogBiometricChange { BiometricType = "Fingerprint", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will remove Fingerprint for member {memberId} on device {deviceId}");
                            updated = true;
                        }
                        else if (biometricData["Fingerprint"] != enrollmentId.Value.ToString())
                        {
                            biometricData["Fingerprint"] = enrollmentId.Value.ToString();
                            syncStatus["Fingerprint"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "Fingerprint", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will update Fingerprint for member {memberId} on device {deviceId}");
                            updated = true;
                        }
                    }
                    else if (hasFingerprint && enrollmentId.HasValue)
                    {
                        biometricData["Fingerprint"] = enrollmentId.Value.ToString();
                        syncStatus["Fingerprint"] = false;
                        changes.Add(new LogBiometricChange { BiometricType = "Fingerprint", DeviceId = deviceId, EnrollmentId = logEnrollmentId ?? enrollmentId });
                        LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Will add Fingerprint for member {memberId} on device {deviceId}");
                        updated = true;
                    }

                    // If after update there is no biometric data left, delete the log and log the action
                    if (biometricData.Count == 0)
                    {
                        using (var deleteCmd = new SQLiteCommand("DELETE FROM Log WHERE Id = @logId", connection))
                        {
                            deleteCmd.Parameters.AddWithValue("@logId", logId);
                            await deleteCmd.ExecuteNonQueryAsync();
                        }
                        LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Deleted log for member {memberId} on device {deviceId} because no biometric data remains.");
                        continue;
                    }

                    if (updated)
                    {
                        using (var updateCmd = new SQLiteCommand("UPDATE Log SET BiometricData = @biometricData, SyncStatus = @syncStatus, UpdatedAt = @updatedAt WHERE Id = @logId", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@biometricData", JsonConvert.SerializeObject(biometricData));
                            updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                            updateCmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                            updateCmd.Parameters.AddWithValue("@logId", logId);
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                        LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Updated log for member {memberId} on device {deviceId}. Remaining biometrics: {string.Join(", ", biometricData.Keys)}");
                    }
                }
            }

            //LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Completed for member {memberId}. Total changes: {changes.Count}");
            return changes;
        }


        // Query function to get device details by DeviceId
        public (string ip, int machineNo, int port) GetDeviceDetails(int deviceId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT DeviceIP, MachineNo, Port FROM Devices WHERE DeviceId = @deviceId", connection))
                {
                    cmd.Parameters.AddWithValue("@deviceId", deviceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string ip = Decrypt(reader["DeviceIP"].ToString());
                            int machineNo = Convert.ToInt32(reader["MachineNo"]);
                            int port = Convert.ToInt32(reader["Port"]);
                            return (ip, machineNo, port);
                        }
                    }
                }
            }
            return (null, 0, 0);
        }

        public (bool exists, bool synced) CheckAttendanceDuplicateAndSynced(int memberId, DateTime deviceTime, string action)
        {
            string timeStr = deviceTime.ToString("yyyy-MM-dd HH:mm:ss");
            string column = action == "check-in" ? "CheckInTime" : "CheckOutTime";
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand($@"
            SELECT SyncStatus 
            FROM AttendanceLogs
            WHERE MemberID=@mid AND {column}=@ts
            LIMIT 1;", conn))
                {
                    cmd.Parameters.AddWithValue("@mid", memberId);
                    cmd.Parameters.AddWithValue("@ts", timeStr);
                    var raw = cmd.ExecuteScalar();
                    if (raw == null || raw == DBNull.Value) return (false, false);
                    try
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(raw.ToString())
                                   ?? new Dictionary<string, bool>();
                        bool synced = false;
                        if (action == "check-in" && dict.ContainsKey("checkIn")) synced = dict["checkIn"];
                        if (action == "check-out" && dict.ContainsKey("checkOut")) synced = dict["checkOut"];
                        return (true, synced);
                    }
                    catch
                    {
                        return (true, false);
                    }
                }
            }
        }




        // Helper to delete logs for a device
        public void DeleteLogsByDeviceId(int deviceId)
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection("Data Source=SmartAttendance.db;Version=3;"))
            {
                connection.Open();

                // 1. Delete logs for the device
                using (var command = new System.Data.SQLite.SQLiteCommand("DELETE FROM Log WHERE DeviceId = @DeviceId", connection))
                {
                    command.Parameters.AddWithValue("@DeviceId", deviceId);
                    command.ExecuteNonQuery();
                }

                // 2. Update SyncStatus in Members table for all members
                using (var selectCmd = new System.Data.SQLite.SQLiteCommand("SELECT MemberID, SyncStatus FROM Members", connection))
                using (var reader = selectCmd.ExecuteReader())
                {
                    var updates = new List<(int memberId, string newSyncStatus)>();

                    while (reader.Read())
                    {
                        int memberId = reader.GetInt32(0);
                        string syncStatusJson = reader.IsDBNull(1) ? "{}" : reader.GetString(1);

                        var syncStatusDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>();

                        if (syncStatusDict.ContainsKey(deviceId))
                        {
                            syncStatusDict.Remove(deviceId);
                            string newSyncStatusJson = Newtonsoft.Json.JsonConvert.SerializeObject(syncStatusDict);
                            updates.Add((memberId, newSyncStatusJson));
                        }
                    }

                    // Apply updates
                    foreach (var update in updates)
                    {
                        using (var updateCmd = new System.Data.SQLite.SQLiteCommand("UPDATE Members SET SyncStatus = @SyncStatus WHERE MemberID = @MemberID", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@SyncStatus", update.newSyncStatus);
                            updateCmd.Parameters.AddWithValue("@MemberID", update.memberId);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }


        // Update the InsertGLogRecordAsync method to include the new fields
        // Enhanced InsertGLogRecordAsync method with duplicate prevention
        public async Task InsertGLogRecordAsync(int deviceId, string enrollmentNumber, string deviceTimeText, string originalLine, bool filter = false)
        {
            try
            {
                string timestamp = GetTimestamp();

                // Get device type from Devices table
                string deviceType = "unknown";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand("SELECT AttendanceType FROM Devices WHERE DeviceId = @DeviceId", connection))
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            bool attendanceType = Convert.ToBoolean(result);
                            deviceType = attendanceType ? "check-in" : "check-out";
                        }
                    }
                }

                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Check for existing record to prevent duplicates
                    using (var checkCommand = new SQLiteCommand(@"
                SELECT COUNT(*) FROM GLog 
                WHERE DeviceId = @DeviceId 
                AND EnrollmentNumber = @EnrollmentNumber 
                AND DeviceTime = @DeviceTime 
                AND OriginalLine = @OriginalLine", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@DeviceId", deviceId);
                        checkCommand.Parameters.AddWithValue("@EnrollmentNumber", enrollmentNumber);
                        checkCommand.Parameters.AddWithValue("@DeviceTime", deviceTimeText);
                        checkCommand.Parameters.AddWithValue("@OriginalLine", originalLine);

                        int existingCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                        if (existingCount > 0)
                        {
                            LogHelper.Log($"Duplicate GLog record detected - skipping insert for enrollment {enrollmentNumber} at {deviceTimeText}");
                            return;
                        }
                    }

                    using (var command = new SQLiteCommand(@"
                INSERT INTO GLog (DeviceId, EnrollmentNumber, DeviceTime, OriginalLine, Sync, DeviceType, Filter, CreatedOn, UpdatedOn)
                VALUES (@DeviceId, @EnrollmentNumber, @DeviceTime, @OriginalLine, @Sync, @DeviceType, @Filter, @CreatedOn, @UpdatedOn)", connection))
                    {
                        command.Parameters.AddWithValue("@DeviceId", deviceId);
                        command.Parameters.AddWithValue("@EnrollmentNumber", enrollmentNumber);
                        command.Parameters.AddWithValue("@DeviceTime", deviceTimeText);
                        command.Parameters.AddWithValue("@OriginalLine", originalLine);
                        command.Parameters.AddWithValue("@Sync", false);
                        command.Parameters.AddWithValue("@DeviceType", deviceType);
                        command.Parameters.AddWithValue("@Filter", filter);
                        command.Parameters.AddWithValue("@CreatedOn", timestamp);
                        command.Parameters.AddWithValue("@UpdatedOn", timestamp);

                        await command.ExecuteNonQueryAsync();
                        LogHelper.Log($"Successfully inserted GLog record for enrollment {enrollmentNumber} at {deviceTimeText}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error inserting GLog record: {ex.Message}");
                throw;
            }
        }

        public List<GLogRecord> FilterAndUpdateGLogRecordsByDateTwoWay()
        {
            var filteredRecords = new List<GLogRecord>();

            try
            {
                // Hardcode case as "2way" for this function
                string attendanceCase = "2way";
                LogHelper.Log($"[GLogPair] Starting two-way pairing for all devices. Case={attendanceCase}");

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    try
                    {
                        // Re-open previously filtered rows for dates that received new logs
                        string reopenSql = @"
                UPDATE GLog
                SET Sync = 0,
                    Filter = 0
                WHERE EXISTS (
                    SELECT 1
                    FROM GLog g2
                    WHERE g2.EnrollmentNumber = GLog.EnrollmentNumber
                      AND DATE(g2.DeviceTime) = DATE(GLog.DeviceTime)
                      AND g2.Sync = 0
                );";

                        using (var reopenTransaction = connection.BeginTransaction())
                        {
                            using (var reopenCmd = new SQLiteCommand(reopenSql, connection, reopenTransaction))
                            {
                                int reopened = reopenCmd.ExecuteNonQuery();
                                LogHelper.Log($"[GLogPair] Reopened {reopened} previously processed rows for re-evaluation.");
                            }
                            reopenTransaction.Commit();
                        }

                        // Get device type mapping for all devices
                        var deviceTypeMap = new Dictionary<int, string>();
                        using (var mapCmd = new SQLiteCommand("SELECT DeviceId, AttendanceType FROM Devices", connection))
                        {
                            using (var r = mapCmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    int deviceId = r.GetInt32(r.GetOrdinal("DeviceId"));
                                    bool attendanceType = Convert.ToBoolean(r["AttendanceType"]);
                                    string deviceTypeStr = attendanceType ? "Check_In" : "Check_Out";
                                    deviceTypeMap[deviceId] = deviceTypeStr;
                                }
                            }
                        }

                        // Load all unsynced records from all devices
                        string selectSql = @"
                        SELECT LogId, DeviceId, EnrollmentNumber, DeviceTime, OriginalLine,
                               Sync, DeviceType, Filter, CreatedOn, UpdatedOn,
                               DATE(DeviceTime) AS RecordDate
                        FROM GLog
                        WHERE Sync = 0
                        ORDER BY EnrollmentNumber, DATE(DeviceTime), DeviceTime";

                        var allRecords = new List<dynamic>();
                        using (var cmd = new SQLiteCommand(selectSql, connection))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int deviceId = reader.GetInt32(reader.GetOrdinal("DeviceId"));

                                    // Determine effective type based on device configuration and stored DeviceType
                                    string effectiveType = "Unknown";
                                    if (deviceTypeMap.ContainsKey(deviceId))
                                    {
                                        effectiveType = deviceTypeMap[deviceId];
                                    }
                                    else
                                    {
                                        // Fallback to stored DeviceType
                                        string storedType = reader["DeviceType"]?.ToString()?.ToLower() ?? "";
                                        if (storedType == "check-in") effectiveType = "Check_In";
                                        else if (storedType == "check-out") effectiveType = "Check_Out";
                                    }

                                    allRecords.Add(new
                                    {
                                        LogId = reader.GetInt32(reader.GetOrdinal("LogId")),
                                        DeviceId = deviceId,
                                        EnrollmentNumber = reader["EnrollmentNumber"].ToString(),
                                        DeviceTime = DateTime.Parse(reader["DeviceTime"].ToString()),
                                        DeviceTimeText = reader["DeviceTime"].ToString(),
                                        OriginalLine = reader["OriginalLine"].ToString(),
                                        Sync = Convert.ToBoolean(reader["Sync"]),
                                        OriginalDeviceType = reader["DeviceType"]?.ToString() ?? "unknown",
                                        EffectiveType = effectiveType,
                                        Filter = reader["Filter"] != DBNull.Value && Convert.ToBoolean(reader["Filter"]),
                                        CreatedOn = reader["CreatedOn"].ToString(),
                                        UpdatedOn = reader["UpdatedOn"].ToString(),
                                        RecordDate = reader["RecordDate"].ToString()
                                    });
                                }
                            }
                        }

                        LogHelper.Log($"[GLogPair] Loaded {allRecords.Count} records for pairing across all devices.");

                        // Group by enrollment and date
                        var grouped = allRecords.GroupBy(g => new { g.EnrollmentNumber, g.RecordDate }).ToList();
                        var selectedIds = new HashSet<int>();
                        var openIds = new HashSet<int>();
                        var ignoredIds = new HashSet<int>();

                        // Process each group
                        foreach (var group in grouped)
                        {
                            LogHelper.Log($"[GLogPair] Group Enroll={group.Key.EnrollmentNumber} Date={group.Key.RecordDate} Total={group.Count()}");
                            PairTwoWayGLog(group.ToList(), selectedIds, openIds, ignoredIds, filteredRecords);
                        }

                        // Update database with results in batches to reduce lock duration
                        string timestamp = GetTimestamp();
                        const int batchSize = 200;

                        if (selectedIds.Count > 0)
                        {
                            UpdateGLogSyncFlagsInBatches(connection, selectedIds.ToList(), true, timestamp, batchSize);
                            LogHelper.Log($"[GLogPair] Marked {selectedIds.Count} paired/open rows Filter=1 Sync=1.");
                        }

                        var toIgnore = ignoredIds.Except(selectedIds).ToList();
                        if (toIgnore.Count > 0)
                        {
                            UpdateGLogSyncFlagsInBatches(connection, toIgnore, false, timestamp, batchSize);
                            LogHelper.Log($"[GLogPair] Marked {toIgnore.Count} ignored rows Filter=0 Sync=1.");
                        }

                        LogHelper.Log($"[GLogPair] Finished two-way filtering for all devices. Output filtered rows={filteredRecords.Count}");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error in FilterAndUpdateGLogRecordsByDateTwoWay: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in two-way filtering for all devices: {ex.Message}");
                throw;
            }

            return filteredRecords;
        }

        private void UpdateGLogSyncFlagsInBatches(SQLiteConnection connection, List<int> ids, bool filterValue, string timestamp, int batchSize)
        {
            if (ids == null || ids.Count == 0)
                return;

            for (int i = 0; i < ids.Count; i += batchSize)
            {
                var batch = ids.Skip(i).Take(batchSize).ToList();
                var idParameters = batch.Select((_, index) => $"@id{index}").ToList();
                string updateSql = $"UPDATE GLog SET Sync=1, Filter=@filter, UpdatedOn=@ts WHERE LogId IN ({string.Join(", ", idParameters)})";

                using (var transaction = connection.BeginTransaction())
                using (var cmd = new SQLiteCommand(updateSql, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@filter", filterValue ? 1 : 0);
                    cmd.Parameters.AddWithValue("@ts", timestamp);
                    for (int index = 0; index < batch.Count; index++)
                    {
                        cmd.Parameters.AddWithValue(idParameters[index], batch[index]);
                    }

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Two-way pairing core for normal GLog records (cross-device logic adapted for single device).
        /// Produces pairs and at most one trailing open check-in.
        /// </summary>
        private void PairTwoWayGLog(
            List<dynamic> rows,
            HashSet<int> selectedIds,
            HashSet<int> openIds,
            HashSet<int> ignoredIds,
            List<GLogRecord> filteredRecords)
        {
            var ins = rows.Where(r => r.EffectiveType == "Check_In")
                          .OrderBy(r => r.DeviceTime).ToList();
            var outs = rows.Where(r => r.EffectiveType == "Check_Out")
                           .OrderBy(r => r.DeviceTime).ToList();

            if (ins.Count == 0 && outs.Count == 0)
            {
                LogHelper.Log("   [PairGLog2Way] No usable records.");
                foreach (var r in rows)
                {
                    ignoredIds.Add(r.LogId);
                }
                return;
            }

            int i = 0; // index into ins
            int j = 0; // index into outs
            DateTime lastOut = DateTime.MinValue;
            int session = 1;

            while (i < ins.Count)
            {
                var inRec = ins[i];

                // Skip check-in that is <= lastOut (ignored because it is before/at previous checkout)
                if (inRec.DeviceTime <= lastOut)
                {
                    ignoredIds.Add(inRec.LogId);
                    i++;
                    continue;
                }

                // Find earliest checkout > check-in
                while (j < outs.Count && outs[j].DeviceTime <= inRec.DeviceTime) j++;

                if (j >= outs.Count)
                {
                    // No checkout available; this becomes the single open check-in.
                    LogHelper.Log($"   [PairGLog2Way] Open check-in retained: {inRec.DeviceTimeText} (LogId={inRec.LogId})");
                    selectedIds.Add(inRec.LogId);
                    openIds.Add(inRec.LogId);
                    filteredRecords.Add(CreateGLogRecordFromDynamic(inRec, "check-in"));
                    // Remaining later check-ins (if any) are ignored
                    for (int k = i + 1; k < ins.Count; k++)
                        ignoredIds.Add(ins[k].LogId);
                    break;
                }

                var outRec = outs[j];
                // Create pair
                LogHelper.Log($"   [PairGLog2Way] Session {session}: {inRec.DeviceTimeText} -> {outRec.DeviceTimeText} (InId={inRec.LogId}, OutId={outRec.LogId})");
                session++;
                selectedIds.Add(inRec.LogId);
                selectedIds.Add(outRec.LogId);
                filteredRecords.Add(CreateGLogRecordFromDynamic(inRec, "check-in"));
                filteredRecords.Add(CreateGLogRecordFromDynamic(outRec, "check-out"));
                lastOut = outRec.DeviceTime;
                i++;
                j++;
            }

            // Add all checkouts not selected as ignored (standalone or before first usable pairing)
            foreach (var o in outs)
                if (!selectedIds.Contains(o.LogId))
                    ignoredIds.Add(o.LogId);

            // Also mark any non-selected check-ins as ignored (already added some above)
            foreach (var ci in ins)
                if (!selectedIds.Contains(ci.LogId) && !ignoredIds.Contains(ci.LogId))
                    ignoredIds.Add(ci.LogId);

            // Any other rows (Mixed/Unknown) become ignored
            foreach (var r in rows)
                if (!selectedIds.Contains(r.LogId) && !ignoredIds.Contains(r.LogId))
                    ignoredIds.Add(r.LogId);
        }



        /// <summary>
        /// Helper method to create GLogRecord from dynamic record for normal GLog
        /// </summary>
        private GLogRecord CreateGLogRecordFromDynamic(dynamic record, string deviceType)
        {
            return new GLogRecord
            {
                LogId = (int)record.LogId, // Fixed: Explicit cast to int
                DeviceId = (int)record.DeviceId,
                EnrollmentNumber = record.EnrollmentNumber,
                DeviceTimeText = record.DeviceTimeText,
                OriginalLine = record.OriginalLine,
                Sync = record.Sync,
                DeviceType = deviceType,
                Filter = record.Filter,
                CreatedOn = record.CreatedOn,
                UpdatedOn = record.UpdatedOn
            };
        }



        public int? GetDeviceIdByMachineNo(int machineNo)
        {
            try
            {
                var devices = LoadDevices();
                var device = devices.FirstOrDefault(d => d.MachineNo == machineNo);
                return device?.DeviceId;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error getting device ID for machine number {machineNo}: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Gets the name of a member by their ID
        /// </summary>
        /// <param name="memberId">The member ID to look up</param>
        /// <returns>The member's name, or null if not found</returns>
        public string GetMemberName(int memberId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT Name FROM Members WHERE MemberID = @memberId LIMIT 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetMemberName][ERROR] {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Two-way pairing core (cross-device).
        /// Produces pairs and at most one trailing open check-in.
        /// </summary>
        private void PairTwoWay(
            List<dynamic> rows,
            HashSet<int> selectedIds,
            HashSet<int> openIds,
            HashSet<int> ignoredIds,
            List<GLogRecord> filteredRecords)
        {
            var ins = rows.Where(r => r.EffectiveType == "Check_In")
                          .OrderBy(r => r.DeviceTime).ToList();
            var outs = rows.Where(r => r.EffectiveType == "Check_Out")
                           .OrderBy(r => r.DeviceTime).ToList();

            if (ins.Count == 0 && outs.Count == 0)
            {
                LogHelper.Log("   [Pair2Way] No usable records.");
                foreach (var r in rows) ignoredIds.Add(r.LogId);
                return;
            }

            int i = 0; // index into ins
            int j = 0; // index into outs
            DateTime lastOut = DateTime.MinValue;
            int session = 1;

            while (i < ins.Count)
            {
                var inRec = ins[i];

                // Skip in that is <= lastOut (ignored because it is before/at previous checkout)
                if (inRec.DeviceTime <= lastOut)
                {
                    ignoredIds.Add(inRec.LogId);
                    i++;
                    continue;
                }

                // Find earliest out > inRec
                while (j < outs.Count && outs[j].DeviceTime <= inRec.DeviceTime) j++;

                if (j >= outs.Count)
                {
                    // No checkout available; this becomes the single open check-in.
                    LogHelper.Log($"   [Pair2Way] Open check-in retained: {inRec.DeviceTimeText} (LogId={inRec.LogId})");
                    selectedIds.Add(inRec.LogId);
                    openIds.Add(inRec.LogId);
                    filteredRecords.Add(CreateGLogRecord(inRec, "Check_In"));
                    // Remaining later check-ins (if any) are ignored
                    for (int k = i + 1; k < ins.Count; k++)
                        ignoredIds.Add(ins[k].LogId);
                    break;
                }

                var outRec = outs[j];
                // Pair
                LogHelper.Log($"   [Pair2Way] Session {session}: {inRec.DeviceTimeText} -> {outRec.DeviceTimeText} (InId={inRec.LogId}, OutId={outRec.LogId})");
                session++;
                selectedIds.Add(inRec.LogId);
                selectedIds.Add(outRec.LogId);
                filteredRecords.Add(CreateGLogRecord(inRec, "Check_In"));
                filteredRecords.Add(CreateGLogRecord(outRec, "Check_Out"));
                lastOut = outRec.DeviceTime;
                i++;
                j++;
            }

            // Add all outs not selected as ignored (standalone or before first usable pairing)
            foreach (var o in outs)
                if (!selectedIds.Contains(o.LogId))
                    ignoredIds.Add(o.LogId);

            // Also mark any non selected ins (already added some above)
            foreach (var ci in ins)
                if (!selectedIds.Contains(ci.LogId) && !ignoredIds.Contains(ci.LogId))
                    ignoredIds.Add(ci.LogId);

            // Any other rows (Mixed/Unknown) become ignored
            foreach (var r in rows)
                if (!selectedIds.Contains(r.LogId) && !ignoredIds.Contains(r.LogId))
                    ignoredIds.Add(r.LogId);
        }

        /// <summary>
        /// Processes 1-way attendance with sequential time-based pairing
        /// </summary>
        private void PairOneWay(
    List<dynamic> rows,
    HashSet<int> selectedIds,
    HashSet<int> ignoredIds,
    List<GLogRecord> filteredRecords)
        {
            if (rows == null || rows.Count == 0) return;

            // All records (single device) – order by time
            var ordered = rows.OrderBy(r => r.DeviceTime).ToList();
            LogHelper.Log($"   [Pair1Way] Processing {ordered.Count} records.");

            int i = 0;
            int session = 1;

            while (i < ordered.Count)
            {
                var inRec = ordered[i];

                // Find next record strictly AFTER this one (treat as checkout)
                int j = i + 1;

                // Skip any exact duplicate timestamps (rare) – mark them ignored
                while (j < ordered.Count && ordered[j].DeviceTime <= inRec.DeviceTime)
                {
                    if (ordered[j].DeviceTime == inRec.DeviceTime)
                    {
                        ignoredIds.Add(ordered[j].LogId);
                        LogHelper.Log($"      [Pair1Way] Ignoring duplicate timestamp {ordered[j].DeviceTimeText} (LogId={ordered[j].LogId})");
                    }
                    j++;
                }

                if (j >= ordered.Count)
                {
                    // No checkout available -> open check-in
                    LogHelper.Log($"   [Pair1Way] Open check-in retained: {inRec.DeviceTimeText} (LogId={inRec.LogId})");
                    filteredRecords.Add(CreateGLogRecord(inRec, "Check_In"));
                    selectedIds.Add(inRec.LogId);
                    break; // done
                }

                var outRec = ordered[j];

                // Pair formed
                LogHelper.Log($"   [Pair1Way] Session {session}: {inRec.DeviceTimeText} -> {outRec.DeviceTimeText} (InId={inRec.LogId}, OutId={outRec.LogId})");
                session++;

                filteredRecords.Add(CreateGLogRecord(inRec, "Check_In"));
                filteredRecords.Add(CreateGLogRecord(outRec, "Check_Out"));
                selectedIds.Add(inRec.LogId);
                selectedIds.Add(outRec.LogId);

                // Move past the checkout
                i = j + 1;
            }

            // Mark any rows not selected as ignored
            foreach (var r in ordered)
                if (!selectedIds.Contains(r.LogId))
                    ignoredIds.Add(r.LogId);

            LogHelper.Log($"   [Pair1Way] Sessions={(session - 1)}, Selected={selectedIds.Count}, Ignored={ignoredIds.Count}");
        }

        /// <summary>
        /// Helper method to create GLogRecord from dynamic record
        /// </summary>
        private GLogRecord CreateGLogRecord(dynamic record, string deviceType)
        {
            return new GLogRecord
            {
                LogId = record.LogId,
                DeviceId = record.StaffDeviceId,
                EnrollmentNumber = record.EnrollmentNumber,
                DeviceTimeText = record.DeviceTimeText,
                OriginalLine = record.OriginalLine,
                Sync = record.Sync,
                DeviceType = deviceType,
                Filter = record.Filter,
                CreatedOn = record.CreatedOn,
                UpdatedOn = record.UpdatedOn
            };
        }
        

        /// <summary>
        /// Deletes all GLog records from the database.
        /// </summary>
        public void DeleteAllGLogs()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM GLog";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                LogHelper.Log("✅ Successfully deleted all GLog records from the database");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error deleting all GLog records from the database: {ex.Message}");
            }
        }

        
        public List<GateAccessed> GetAllGateAccesseds()
        {
            var gateAccesseds = new List<GateAccessed>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT GateId, DeviceIP, MemberId, IsVisitor, VisitorName, VisitorPhone, Reason, AccessedTime, Username, CreatedAt, UpdatedAt " +
                    "FROM GateAccessed", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            gateAccesseds.Add(new GateAccessed
                            {
                                GateId = reader.GetInt32(0),
                                DeviceIP = reader.GetString(1),
                                MemberId = reader.GetInt32(2),
                                IsVisitor = reader.GetBoolean(3),
                                VisitorName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                VisitorPhone = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Reason = reader.IsDBNull(6) ? null : reader.GetString(6),
                                AccessedTime = DateTime.Parse(reader.GetString(7)),
                                Username = reader.IsDBNull(8) ? null : reader.GetString(8)
                            });
                        }
                    }
                }
            }
            return gateAccesseds;
        }

        public void InsertGateAccessed(GateAccessed gateAccessed)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    string timestamp = GetTimestamp();
                    string accessedTime = gateAccessed.AccessedTime != DateTime.MinValue
                        ? gateAccessed.AccessedTime.ToString("yyyy-MM-dd HH:mm:ss")
                        : timestamp;

                    command.CommandText = @"
            INSERT INTO GateAccessed 
            (DeviceIP, MemberId, IsVisitor, VisitorName, VisitorPhone, Reason, AccessedTime, Username, CreatedAt, UpdatedAt) 
            VALUES 
            (@DeviceIP, @MemberId, @IsVisitor, @VisitorName, @VisitorPhone, @Reason, @AccessedTime, @Username, @CreatedAt, @UpdatedAt)";

                    command.Parameters.AddWithValue("@DeviceIP", gateAccessed.DeviceIP);
                    command.Parameters.AddWithValue("@MemberId", gateAccessed.MemberId);
                    command.Parameters.AddWithValue("@IsVisitor", gateAccessed.IsVisitor);
                    command.Parameters.AddWithValue("@VisitorName", (object)gateAccessed.VisitorName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VisitorPhone", (object)gateAccessed.VisitorPhone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Reason", (object)gateAccessed.Reason ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AccessedTime", accessedTime);
                    command.Parameters.AddWithValue("@Username", (object)gateAccessed.Username ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);
                    command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                    command.ExecuteNonQuery();
                }
            }
        }

        
        public string GetDatabaseFilePath()
        {
            try
            {
                var builder = new SQLiteConnectionStringBuilder(connectionString);
                var dataSource = builder.DataSource;
                if (string.IsNullOrWhiteSpace(dataSource)) return null;

                // If relative, resolve against application base directory
                if (!Path.IsPathRooted(dataSource))
                    dataSource = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dataSource));

                return dataSource;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ GetDatabaseFilePath error: {ex.Message}");
                return null;
            }
        }

        // Creates a read-only snapshot of the SQLite DB to attach in email
        public string CreateDatabaseSnapshot()
        {
            try
            {
                var dbPath = GetDatabaseFilePath();
                if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
                    return null;

                var tempDir = Path.Combine(Path.GetTempPath(), "SmartAttendanceBackup");
                Directory.CreateDirectory(tempDir);
                var snapshotPath = Path.Combine(tempDir, $"SmartAttendance_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                using (var src = new SQLiteConnection(connectionString))
                using (var dest = new SQLiteConnection($"Data Source={snapshotPath};Version=3;"))
                {
                    src.Open();
                    dest.Open();
                    src.BackupDatabase(dest, "main", "main", -1, null, 0);
                }

                return snapshotPath;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Database snapshot error: {ex.Message}");
                return null;
            }
        }



        ////////////////////////PENDING JOBS////////////////////////////////

        /// <summary>
        /// Inserts a new pending job into the database.
        /// </summary>
        public void InsertPendingJob(PendingJob job)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    @"INSERT INTO PendingJobs (Action, MemberId, EnrollmentId, DeviceId, Ip, Port, MachineNo, Password, BackupNumber, CreatedAt)
              VALUES (@action, @memberId, @enrollmentId, @deviceId, @ip, @port, @machineNo, @password, @backupNumber, @createdAt)", connection))
                {
                    command.Parameters.AddWithValue("@action", job.Action ?? string.Empty);
                    command.Parameters.AddWithValue("@memberId", job.MemberId);
                    command.Parameters.AddWithValue("@enrollmentId", job.EnrollmentId);

                    // Nullable DeviceId support via reflection (works even if PendingJob doesn't define DeviceId)
                    var deviceIdProp = job.GetType().GetProperty("DeviceId");
                    if (deviceIdProp != null)
                    {
                        var deviceIdVal = deviceIdProp.GetValue(job, null);
                        command.Parameters.AddWithValue("@deviceId", deviceIdVal ?? (object)DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@deviceId", DBNull.Value);
                    }

                    command.Parameters.AddWithValue("@ip", job.Ip ?? string.Empty);
                    command.Parameters.AddWithValue("@port", job.Port);
                    command.Parameters.AddWithValue("@machineNo", job.MachineNo);
                    command.Parameters.AddWithValue("@password", job.Password ?? string.Empty);
                    command.Parameters.AddWithValue("@backupNumber", job.BackupNumber);
                    command.Parameters.AddWithValue("@createdAt", job.CreatedAt.ToString("o"));  // ISO 8601 format
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<PendingJob> GetAllPendingJobs()
        {
            var jobs = new List<PendingJob>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT Id, Action, MemberId, EnrollmentId, DeviceId, Ip, Port, MachineNo, Password, BackupNumber, CreatedAt FROM PendingJobs ORDER BY CreatedAt ASC",
                    connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var job = new PendingJob
                        {
                            Action = reader.IsDBNull(reader.GetOrdinal("Action")) ? null : reader.GetString(reader.GetOrdinal("Action")),
                            MemberId = reader.IsDBNull(reader.GetOrdinal("MemberId")) ? 0 : reader.GetInt32(reader.GetOrdinal("MemberId")),
                            EnrollmentId = reader.IsDBNull(reader.GetOrdinal("EnrollmentId")) ? 0 : reader.GetInt32(reader.GetOrdinal("EnrollmentId")),
                            Ip = reader.IsDBNull(reader.GetOrdinal("Ip")) ? null : reader.GetString(reader.GetOrdinal("Ip")),
                            Port = reader.IsDBNull(reader.GetOrdinal("Port")) ? 0 : reader.GetInt32(reader.GetOrdinal("Port")),
                            MachineNo = reader.IsDBNull(reader.GetOrdinal("MachineNo")) ? 0 : reader.GetInt32(reader.GetOrdinal("MachineNo")),
                            Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? null : reader.GetString(reader.GetOrdinal("Password")),
                            BackupNumber = reader.IsDBNull(reader.GetOrdinal("BackupNumber")) ? 0 : reader.GetInt32(reader.GetOrdinal("BackupNumber")),
                            CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
                        };

                        // Try assign DeviceId if property exists on PendingJob
                        int ordDeviceId = reader.GetOrdinal("DeviceId");
                        if (!reader.IsDBNull(ordDeviceId))
                        {
                            var deviceIdProp = job.GetType().GetProperty("DeviceId");
                            if (deviceIdProp != null && deviceIdProp.CanWrite)
                            {
                                deviceIdProp.SetValue(job, reader.GetInt32(ordDeviceId));
                            }
                        }

                        jobs.Add(job);
                    }
                }
            }
            return jobs;
        }

        public void DeletePendingJob(PendingJob job)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    @"DELETE FROM PendingJobs 
              WHERE Action = @action 
                AND MemberId = @memberId 
                AND EnrollmentId = @enrollmentId 
                AND Ip = @ip 
                AND Port = @port 
                AND MachineNo = @machineNo
                AND (DeviceId = @deviceId OR (DeviceId IS NULL AND @deviceId IS NULL))", connection))
                {
                    command.Parameters.AddWithValue("@action", job.Action ?? string.Empty);
                    command.Parameters.AddWithValue("@memberId", job.MemberId);
                    command.Parameters.AddWithValue("@enrollmentId", job.EnrollmentId);
                    command.Parameters.AddWithValue("@ip", job.Ip ?? string.Empty);
                    command.Parameters.AddWithValue("@port", job.Port);
                    command.Parameters.AddWithValue("@machineNo", job.MachineNo);

                    // Provide @deviceId parameter with null-awareness
                    var deviceIdProp = job.GetType().GetProperty("DeviceId");
                    if (deviceIdProp != null)
                    {
                        var deviceIdVal = deviceIdProp.GetValue(job, null);
                        command.Parameters.AddWithValue("@deviceId", deviceIdVal ?? (object)DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@deviceId", DBNull.Value);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }



        /////////////////////////////////////////////////////////////////////////////////////
































































        ////////////////////////////////////// STAFF //////////////////////////////////////////

        public Staff GetStaff(string staffId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT StaffID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, SyncStatus FROM Staff WHERE StaffID = @StaffID", connection))
                {
                    command.Parameters.AddWithValue("@StaffID", staffId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var syncStatusJson = reader.IsDBNull(7) ? "{}" : reader.GetString(7);
                            return new Staff
                            {
                                StaffID = reader.GetString(0),
                                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Status = reader.IsDBNull(2) ? null : reader.GetString(2),
                                RFID = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Picture = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Fingerprint = !reader.IsDBNull(5) && reader.GetBoolean(5),
                                EnrollmentID = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                SyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<Staff> GetStaffs()
        {
            var staffs = new List<Staff>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT StaffID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, SyncStatus FROM Staff", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var syncStatusJson = reader.IsDBNull(7) ? "{}" : reader.GetString(7);
                        staffs.Add(new Staff
                        {
                            StaffID = reader.GetString(0),
                            Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Status = reader.IsDBNull(2) ? null : reader.GetString(2),
                            RFID = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Picture = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Fingerprint = !reader.IsDBNull(5) && reader.GetBoolean(5),
                            EnrollmentID = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                            SyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>()
                        });
                    }
                }
            }
            return staffs;
        }

        public void UpsertStaff(JObject staff)
        {
            string staffId = staff["id"]?.ToString(); // GUID or string
            string name = staff["name"]?.ToString() ?? string.Empty;
            string status = (string)staff["status"];
            string rfid = staff["rfid"]?.ToString() ?? string.Empty;
            string picture = staff["picture"]?.ToString() ?? string.Empty;
            bool fingerprint = staff["fingerprint"]?.ToObject<bool>() ?? false;
            int? enrollmentId = staff["enrollment_id"]?.ToObject<int?>();

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var existing = !string.IsNullOrEmpty(staffId) ? GetStaff(staffId) : null;
            var staffDevices = LoadStaffDevices() ?? new List<Device>();
            var syncStatus = new Dictionary<int, bool>();

            foreach (var d in staffDevices)
            {
                bool needsSync = true;
                if (existing != null)
                {
                    bool fingerprintRelevant = d.FingerCapacity > 0;
                    bool rfidRelevant = d.RFIDCapacity > 0;
                    bool faceRelevant = d.FaceCapacity > 0;

                    bool fingerprintChanged = fingerprintRelevant && (fingerprint != existing.Fingerprint || enrollmentId != existing.EnrollmentID);
                    bool rfidChanged = rfidRelevant && (rfid != existing.RFID);
                    bool pictureChanged = faceRelevant && (picture != existing.Picture);

                    needsSync = fingerprintChanged || rfidChanged || pictureChanged;
                }
                if (d.StaffDeviceId != 0)
                    syncStatus[d.StaffDeviceId] = false;
            }

            string syncStatusJson = JsonConvert.SerializeObject(syncStatus);

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SQLiteCommand(conn))
                        {
                            cmd.CommandText = @"
INSERT INTO Staff 
(StaffID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, SyncStatus, CreatedAt, UpdatedAt)
VALUES (@id, @name, @status, @rfid, @picture, @fingerprint, @enrollment_id, @syncStatus, @createdAt, @updatedAt)
ON CONFLICT(StaffID) DO UPDATE SET
    Name = excluded.Name,
    Status = excluded.Status,
    RFID = excluded.RFID,
    Picture = excluded.Picture,
    Fingerprint = excluded.Fingerprint,
    EnrollmentID = excluded.EnrollmentID,
    SyncStatus = excluded.SyncStatus,
    UpdatedAt = excluded.UpdatedAt;";
                            cmd.Parameters.AddWithValue("@id", staffId ?? "");
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@rfid", string.IsNullOrEmpty(rfid) ? DBNull.Value : (object)rfid);
                            cmd.Parameters.AddWithValue("@picture", string.IsNullOrEmpty(picture) ? DBNull.Value : (object)picture);
                            cmd.Parameters.AddWithValue("@fingerprint", fingerprint);
                            cmd.Parameters.AddWithValue("@enrollment_id", enrollmentId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@syncStatus", syncStatusJson);
                            cmd.Parameters.AddWithValue("@createdAt", timestamp);
                            cmd.Parameters.AddWithValue("@updatedAt", timestamp);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        LogHelper.Log($"Error upserting staff {staffId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Delete staff by ID
        public void DeleteStaffById(string staffId)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Staff WHERE StaffID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", staffId);
                    cmd.ExecuteNonQuery();
                }
            }
            LogHelper.Log($"🗑 Staff {staffId} deleted from local DB.");
        }

        // Get staff ID by Enrollment
        public int? GetStaffIdByEnrollment(int enrollmentId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT StaffID FROM Staff WHERE EnrollmentID = @enroll LIMIT 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@enroll", enrollmentId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetStaffIdByEnrollment][ERROR] {ex.Message}");
            }
            return null;
        }

        // Insert a new staff device
        public void SaveStaffDevice(Device device)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    string timestamp = GetTimestamp();

                    string encryptedSerial = Encrypt(device.DeviceSerial);
                    string encryptedIP = Encrypt(device.DeviceIP);

                    command.CommandText = @"
INSERT INTO StaffDevices 
(Department, StaffAttendanceType, DeviceSerial, DeviceIP, Port, TimeZone, Location, ConnectionType, IsActive, LastSyncTime, FaceCapacity, RFIDCapacity, FingerCapacity, MachineNo, CreatedAt, UpdatedAt, Password) 
VALUES 
(@Department, @StaffAttendanceType, @DeviceSerial, @DeviceIP, @Port, @TimeZone, @Location, @ConnectionType, @IsActive, @LastSyncTime, @FaceCapacity, @RFIDCapacity, @FingerCapacity, @MachineNo, @CreatedAt, @UpdatedAt, @Password)";
                    command.Parameters.AddWithValue("@Department", (object)device.Department ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StaffAttendanceType", device.StaffAttendanceType ?? "");
                    command.Parameters.AddWithValue("@DeviceSerial", encryptedSerial);
                    command.Parameters.AddWithValue("@DeviceIP", encryptedIP);
                    command.Parameters.AddWithValue("@Port", device.Port);
                    command.Parameters.AddWithValue("@TimeZone", device.TimeZone);
                    command.Parameters.AddWithValue("@Location", device.Location);
                    command.Parameters.AddWithValue("@ConnectionType", device.ConnectionType);
                    command.Parameters.AddWithValue("@IsActive", device.IsActive);
                    command.Parameters.AddWithValue("@LastSyncTime", (object)device.LastSyncTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FaceCapacity", (object)device.FaceCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RFIDCapacity", (object)device.RFIDCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FingerCapacity", (object)device.FingerCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MachineNo", device.MachineNo);
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);
                    command.Parameters.AddWithValue("@UpdatedAt", timestamp);
                    command.Parameters.AddWithValue("@Password", device.Password);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Update an existing staff device
        public void UpdateStaffDevice(Device device)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"
UPDATE StaffDevices
SET 
    Department = @Department,
    StaffAttendanceType = @StaffAttendanceType,
    Port = @Port,
    TimeZone = @TimeZone,
    Location = @Location,
    ConnectionType = @ConnectionType,
    IsActive = @IsActive,
    LastSyncTime = @LastSyncTime,
    FaceCapacity = @FaceCapacity,
    RFIDCapacity = @RFIDCapacity,
    FingerCapacity = @FingerCapacity,
    MachineNo = @MachineNo,
    Password = @Password,
    UpdatedAt = @UpdatedAt
WHERE StaffDeviceId = @StaffDeviceId", connection))
                {
                    command.Parameters.AddWithValue("@Department", (object)device.Department ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StaffAttendanceType", device.StaffAttendanceType ?? "");
                    command.Parameters.AddWithValue("@Port", device.Port);
                    command.Parameters.AddWithValue("@TimeZone", device.TimeZone);
                    command.Parameters.AddWithValue("@Location", device.Location);
                    command.Parameters.AddWithValue("@ConnectionType", device.ConnectionType);
                    command.Parameters.AddWithValue("@IsActive", device.IsActive);
                    command.Parameters.AddWithValue("@LastSyncTime", (object)device.LastSyncTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FaceCapacity", (object)device.FaceCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RFIDCapacity", (object)device.RFIDCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FingerCapacity", (object)device.FingerCapacity ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MachineNo", device.MachineNo);
                    command.Parameters.AddWithValue("@Password", device.Password);
                    command.Parameters.AddWithValue("@UpdatedAt", GetTimestamp());
                    command.Parameters.AddWithValue("@StaffDeviceId", device.StaffDeviceId);
                    command.ExecuteNonQuery();
                }
            }
        }
        // Delete a staff device
        public void DeleteStaffDevice(int staffDeviceId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("DELETE FROM StaffDevices WHERE StaffDeviceId = @StaffDeviceId", connection))
                {
                    command.Parameters.AddWithValue("@StaffDeviceId", staffDeviceId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Load all staff devices
        public List<Device> LoadStaffDevices()
        {
            var devices = new List<Device>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT StaffDeviceId, Department, StaffAttendanceType, DeviceSerial, DeviceIP, Port, MachineNo, TimeZone, Location, ConnectionType, IsActive, LastSyncTime, FaceCapacity, RFIDCapacity, FingerCapacity, Password, CreatedAt, UpdatedAt FROM StaffDevices", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        devices.Add(new Device
                        {
                            StaffDeviceId = reader.GetInt32(0),
                            Department = reader.IsDBNull(1) ? null : reader.GetString(1),
                            StaffAttendanceType = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DeviceSerial = Decrypt(reader.IsDBNull(3) ? null : reader.GetString(3)),
                            DeviceIP = Decrypt(reader.IsDBNull(4) ? null : reader.GetString(4)),
                            Port = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            MachineNo = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            TimeZone = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Location = reader.IsDBNull(8) ? null : reader.GetString(8),
                            ConnectionType = reader.IsDBNull(9) ? null : reader.GetString(9),
                            IsActive = !reader.IsDBNull(10) && Convert.ToBoolean(reader.GetValue(10)),
                            LastSyncTime = reader.IsDBNull(11) ? null : reader.GetString(11),
                            FaceCapacity = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                            RFIDCapacity = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                            FingerCapacity = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                            Password = reader.IsDBNull(15) ? null : reader.GetString(15),
                            CreatedAt = reader.IsDBNull(16) ? null : reader.GetString(16),
                            UpdatedAt = reader.IsDBNull(17) ? null : reader.GetString(17)
                        });
                    }
                }
            }
            return devices;
        }

        public int GetNextStaffDeviceId()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT IFNULL(MAX(StaffDeviceId), 0) + 1 FROM StaffDevices", connection))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool StaffDeviceExists(string deviceIP, int machineNo)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT COUNT(1) FROM StaffDevices WHERE DeviceIP = @ip AND MachineNo = @machineNo", connection))
                {
                    cmd.Parameters.AddWithValue("@ip", Encrypt(deviceIP));
                    cmd.Parameters.AddWithValue("@machineNo", machineNo);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }


        /// <summary>
        /// Gets the staff device ID by machine number for staff devices.
        /// </summary>
        /// <param name="machineNo">The machine number to search for</param>
        /// <returns>Staff device ID if found, null otherwise</returns>
        public int? GetStaffDeviceIdByMachineNo(int machineNo)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT StaffDeviceId FROM StaffDevices WHERE MachineNo = @machineNo LIMIT 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@machineNo", machineNo);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error getting staff device ID for machine number {machineNo}: {ex.Message}");
            }
            return null;
        }


        // Query function to get staff device details by StaffDeviceId
        public (string ip, int machineNo, int port) GetStaffDeviceDetails(int staffDeviceId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT DeviceIP, MachineNo, Port FROM StaffDevices WHERE StaffDeviceId = @staffDeviceId", connection))
                {
                    cmd.Parameters.AddWithValue("@staffDeviceId", staffDeviceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var ipEncrypted = reader["DeviceIP"] != DBNull.Value ? reader["DeviceIP"].ToString() : null;
                            string ip = string.IsNullOrEmpty(ipEncrypted) ? null : Decrypt(ipEncrypted);
                            int machineNo = reader["MachineNo"] != DBNull.Value ? Convert.ToInt32(reader["MachineNo"]) : 0;
                            int port = reader["Port"] != DBNull.Value ? Convert.ToInt32(reader["Port"]) : 0;
                            return (ip, machineNo, port);
                        }
                    }
                }
            }
            return (null, 0, 0);
        }

        public void DeleteStaffGLogsByStaffDeviceId(int staffDeviceId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("DELETE FROM StaffGLog WHERE StaffDeviceId = @StaffDeviceId", connection))
                    {
                        command.Parameters.AddWithValue("@StaffDeviceId", staffDeviceId);
                        int rowsAffected = command.ExecuteNonQuery();
                        LogHelper.Log($"✅ Deleted {rowsAffected} StaffGLog record(s) for StaffDeviceId {staffDeviceId}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error deleting StaffGLog records for StaffDeviceId {staffDeviceId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes all StaffGLog records from the database.
        /// </summary>
        public void DeleteAllStaffGLogs()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM StaffGLog";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                LogHelper.Log("✅ Successfully deleted all StaffGLog records from the database");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error deleting all StaffGLog records from the database: {ex.Message}");
            }
        }


        // Insert a staff GLog record
        public void InsertStaffGLog(int staffDeviceId, string enrollmentNumber, string deviceTime, string originalLine, bool sync, string deviceType, bool filter)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Check for duplicate record
                using (var checkCommand = new SQLiteCommand(@"
            SELECT COUNT(*) FROM StaffGLog 
            WHERE StaffDeviceId = @StaffDeviceId 
            AND EnrollmentNumber = @EnrollmentNumber 
            AND DeviceTime = @DeviceTime 
            AND OriginalLine = @OriginalLine", connection))
                {
                    checkCommand.Parameters.AddWithValue("@StaffDeviceId", staffDeviceId);
                    checkCommand.Parameters.AddWithValue("@EnrollmentNumber", enrollmentNumber);
                    checkCommand.Parameters.AddWithValue("@DeviceTime", deviceTime);
                    checkCommand.Parameters.AddWithValue("@OriginalLine", originalLine);

                    int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (existingCount > 0)
                    {
                        // Duplicate found, skip insertion
                        LogHelper.Log($"Duplicate StaffGLog record detected - skipping insert for enrollment {enrollmentNumber} at {deviceTime}");
                        return;
                    }
                }

                // Insert new record
                using (var command = new SQLiteCommand(connection))
                {
                    string timestamp = GetTimestamp();
                    command.CommandText = @"
                INSERT INTO StaffGLog (StaffDeviceId, EnrollmentNumber, DeviceTime, OriginalLine, Sync, DeviceType, Filter, CreatedOn, UpdatedOn)
                VALUES (@StaffDeviceId, @EnrollmentNumber, @DeviceTime, @OriginalLine, @Sync, @DeviceType, @Filter, @CreatedOn, @UpdatedOn)";
                    command.Parameters.AddWithValue("@StaffDeviceId", staffDeviceId);
                    command.Parameters.AddWithValue("@EnrollmentNumber", enrollmentNumber);
                    command.Parameters.AddWithValue("@DeviceTime", deviceTime);
                    command.Parameters.AddWithValue("@OriginalLine", originalLine);
                    command.Parameters.AddWithValue("@Sync", sync);
                    command.Parameters.AddWithValue("@DeviceType", deviceType);
                    command.Parameters.AddWithValue("@Filter", filter);
                    command.Parameters.AddWithValue("@CreatedOn", timestamp);
                    command.Parameters.AddWithValue("@UpdatedOn", timestamp);

                    command.ExecuteNonQuery();
                    LogHelper.Log($"Successfully inserted StaffGLog record for enrollment {enrollmentNumber} at {deviceTime}");
                }
            }
        }


        public void DebugStaffGLogFiltering()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Check attendance case
                    string attendanceCase = DecideAttendanceCase();
                    LogHelper.Log($"🔍 DEBUG: Attendance case = {attendanceCase}");

                    // Check device types
                    using (var cmd = new SQLiteCommand(@"
                SELECT sd.StaffDeviceId, sd.StaffAttendanceType, COUNT(sg.LogId) as RecordCount
                FROM StaffDevices sd
                LEFT JOIN StaffGLog sg ON sd.StaffDeviceId = sg.StaffDeviceId
                WHERE sg.Sync = 0
                GROUP BY sd.StaffDeviceId, sd.StaffAttendanceType", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            LogHelper.Log("🔍 DEBUG: Device types and record counts:");
                            while (reader.Read())
                            {
                                LogHelper.Log($"   Device {reader["StaffDeviceId"]}: Type='{reader["StaffAttendanceType"]}', Records={reader["RecordCount"]}");
                            }
                        }
                    }

                    // Check unsynced records by enrollment and date
                    using (var cmd = new SQLiteCommand(@"
                SELECT EnrollmentNumber, DATE(DeviceTime) as RecordDate, COUNT(*) as Count
                FROM StaffGLog sg
                WHERE Sync = 0
                GROUP BY EnrollmentNumber, DATE(DeviceTime)
                ORDER BY EnrollmentNumber, RecordDate", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            LogHelper.Log("🔍 DEBUG: Records by enrollment and date:");
                            while (reader.Read())
                            {
                                LogHelper.Log($"   Enrollment {reader["EnrollmentNumber"]}, Date {reader["RecordDate"]}: {reader["Count"]} records");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error in debug method: {ex.Message}");
            }
        }

        public int InsertOrUpdateStaffAttendance1Way(int enrollmentId, string punchTimeValue, string name, int debounceSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(punchTimeValue))
                return 0;

            DateTime punchDt;
            if (!DateTime.TryParse(punchTimeValue, out punchDt))
                return 0;

            string tsNow = GetTimestamp();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 1. Try to find an OPEN session (no checkout yet)
                int? openLogId = null;
                DateTime openCheckIn = DateTime.MinValue;

                using (var cmd = new SQLiteCommand(
                    @"SELECT LogID, CheckInTime 
              FROM StaffAttendanceLog
              WHERE EnrollmentID = @enr AND CheckOutTime IS NULL
              ORDER BY datetime(CheckInTime) DESC
              LIMIT 1", connection))
                {
                    cmd.Parameters.AddWithValue("@enr", enrollmentId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            openLogId = r.GetInt32(0);
                            DateTime.TryParse(r.GetString(1), out openCheckIn);
                        }
                    }
                }

                if (openLogId.HasValue)
                {
                    // Debounce / duplicate: ignore if punch inside debounce window
                    if (punchDt <= openCheckIn.AddSeconds(debounceSeconds))
                        return openLogId.Value;

                    // Guard: ensure not earlier than check-in
                    if (punchDt < openCheckIn)
                        return openLogId.Value; // ignore weird out-of-order

                    // Update checkout
                    using (var upd = new SQLiteCommand(
                        @"UPDATE StaffAttendanceLog
                  SET CheckOutTime = @co, UpdatedAt = @up
                  WHERE LogID = @id AND CheckOutTime IS NULL", connection))
                    {
                        upd.Parameters.AddWithValue("@co", punchDt.ToString("yyyy-MM-dd HH:mm:ss"));
                        upd.Parameters.AddWithValue("@up", tsNow);
                        upd.Parameters.AddWithValue("@id", openLogId.Value);
                        upd.ExecuteNonQuery();
                    }
                    return openLogId.Value;
                }

                // 2. No open session. Check most recent CLOSED row to prevent rapid duplicate new check-ins
                DateTime lastClosedCheckout = DateTime.MinValue;
                int? lastClosedId = null;
                using (var cmd = new SQLiteCommand(
                    @"SELECT LogID, CheckOutTime 
              FROM StaffAttendanceLog
              WHERE EnrollmentID = @enr AND CheckOutTime IS NOT NULL
              ORDER BY datetime(CheckOutTime) DESC
              LIMIT 1", connection))
                {
                    cmd.Parameters.AddWithValue("@enr", enrollmentId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read() && !r.IsDBNull(1))
                        {
                            lastClosedId = r.GetInt32(0);
                            DateTime.TryParse(r.GetString(1), out lastClosedCheckout);
                        }
                    }
                }

                // If new punch is within debounce of last checkout -> treat as duplicate, ignore
                if (lastClosedId.HasValue && punchDt <= lastClosedCheckout.AddSeconds(debounceSeconds))
                    return lastClosedId.Value;

                // 3. Insert new check-in row (include Name)
                using (var ins = new SQLiteCommand(
                    @"INSERT INTO StaffAttendanceLog
              (EnrollmentID, AttendanceId, Name, CheckInTime, CheckOutTime, CreatedAt, UpdatedAt)
              VALUES (@enr, NULL, @name, @ci, NULL, @cAt, @uAt);
              SELECT last_insert_rowid();", connection))
                {
                    ins.Parameters.AddWithValue("@enr", enrollmentId);
                    ins.Parameters.AddWithValue("@name", (object)name ?? DBNull.Value);  // Use the passed name
                    ins.Parameters.AddWithValue("@ci", punchDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    ins.Parameters.AddWithValue("@cAt", tsNow);
                    ins.Parameters.AddWithValue("@uAt", tsNow);
                    return Convert.ToInt32((long)ins.ExecuteScalar());
                }
            }
        }


        public int InsertStaffAttendanceLog(int enrollmentId, string checkInValue, string name)
        {
            if (string.IsNullOrWhiteSpace(checkInValue))
                return 0;

            DateTime checkInDt;
            if (!DateTime.TryParse(checkInValue, out checkInDt))
                return 0;

            string timestamp = GetTimestamp();
            int newLogId = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // OPTIONAL index (run once; harmless if already exists)
                using (var idx = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS IX_StaffAttendanceLog_Enrollment_CheckIn ON StaffAttendanceLog(EnrollmentID, CheckInTime);",
                    connection))
                {
                    idx.ExecuteNonQuery();
                }

                // 1. If exact duplicate (same enrollment + same check-in timestamp) -> return existing
                using (var dupCmd = new SQLiteCommand(
                    @"SELECT LogID FROM StaffAttendanceLog 
              WHERE EnrollmentID = @enrollmentId AND CheckInTime = @checkIn LIMIT 1", connection))
                {
                    dupCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                    dupCmd.Parameters.AddWithValue("@checkIn", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    var existing = dupCmd.ExecuteScalar();
                    if (existing != null && existing != DBNull.Value)
                        return Convert.ToInt32(existing);
                }

                // 2. Prevent multiple OPEN logs for the same calendar date:
                using (var openSameDateCmd = new SQLiteCommand(
                    @"SELECT LogID FROM StaffAttendanceLog
              WHERE EnrollmentID = @enrollmentId
                AND CheckOutTime IS NULL
                AND DATE(CheckInTime) = DATE(@checkInDate)
              ORDER BY CheckInTime DESC
              LIMIT 1", connection))
                {
                    openSameDateCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                    openSameDateCmd.Parameters.AddWithValue("@checkInDate", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    var openSameDate = openSameDateCmd.ExecuteScalar();
                    if (openSameDate != null && openSameDate != DBNull.Value)
                    {
                        // Decide policy: either ignore or update existing open log's CheckInTime.
                        // Here we IGNORE new duplicate-day check-in to avoid orphan pairs.
                        return Convert.ToInt32(openSameDate);
                    }
                }

                // 3. Insert new row (include Name)
                using (var insert = new SQLiteCommand(
                    @"INSERT INTO StaffAttendanceLog
              (EnrollmentID, AttendanceId, Name, CheckInTime, CheckOutTime, CreatedAt, UpdatedAt)
              VALUES (@enrollmentId, NULL, @name, @checkIn, NULL, @createdAt, @updatedAt);
              SELECT last_insert_rowid();", connection))
                {
                    insert.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                    insert.Parameters.AddWithValue("@name", (object)name ?? DBNull.Value);  // Use the passed name
                    insert.Parameters.AddWithValue("@checkIn", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    insert.Parameters.AddWithValue("@createdAt", timestamp);
                    insert.Parameters.AddWithValue("@updatedAt", timestamp);

                    newLogId = Convert.ToInt32((long)insert.ExecuteScalar());
                }
            }
            return newLogId;
        }


        // Update staff attendance log for check-out (pairs only with same-day or previous-day open check-in)
        public bool UpdateStaffAttendanceLogForCheckOut(int enrollmentId, string checkOutValue)
        {
            if (string.IsNullOrWhiteSpace(checkOutValue))
                return false;

            DateTime checkOutDt;
            if (!DateTime.TryParse(checkOutValue, out checkOutDt))
                return false;

            string timestamp = GetTimestamp();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Ensure index exists (safe if already created)
                using (var idx = new SQLiteCommand(
                    "CREATE INDEX IF NOT EXISTS IX_StaffAttendanceLog_Enrollment_Open ON StaffAttendanceLog(EnrollmentID, CheckOutTime, CheckInTime);",
                    connection))
                {
                    idx.ExecuteNonQuery();
                }

                // 1. Try same day open check-in (must be <= checkout)
                int? logId = null;
                DateTime chosenCheckIn = DateTime.MinValue;

                using (var sameDay = new SQLiteCommand(
                    @"SELECT LogID, CheckInTime FROM StaffAttendanceLog
                      WHERE EnrollmentID = @enrollmentId
                        AND CheckOutTime IS NULL
                        AND DATE(CheckInTime) = DATE(@checkOut)
                        AND datetime(CheckInTime) <= datetime(@checkOut)
                      ORDER BY datetime(CheckInTime) DESC
                      LIMIT 1", connection))
                {
                    sameDay.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                    sameDay.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    using (var r = sameDay.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            logId = r.GetInt32(0);
                            DateTime.TryParse(r.GetString(1), out chosenCheckIn);
                        }
                    }
                }

                // 2. If not found, try previous day (overnight session)
                if (!logId.HasValue)
                {
                    using (var prevDay = new SQLiteCommand(
                        @"SELECT LogID, CheckInTime FROM StaffAttendanceLog
                          WHERE EnrollmentID = @enrollmentId
                            AND CheckOutTime IS NULL
                            AND DATE(CheckInTime) = DATE(@checkOut, '-1 day')
                            AND datetime(CheckInTime) <= datetime(@checkOut)
                          ORDER BY datetime(CheckInTime) DESC
                          LIMIT 1", connection))
                    {
                        prevDay.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                        prevDay.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                        using (var r = prevDay.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                logId = r.GetInt32(0);
                                DateTime.TryParse(r.GetString(1), out chosenCheckIn);
                            }
                        }
                    }
                }

                // 3. Fallback: any open log whose CheckInTime <= checkout (prevent pairing with future check-in)
                if (!logId.HasValue)
                {
                    using (var anyOpen = new SQLiteCommand(
                        @"SELECT LogID, CheckInTime FROM StaffAttendanceLog
                          WHERE EnrollmentID = @enrollmentId
                            AND CheckOutTime IS NULL
                            AND datetime(CheckInTime) <= datetime(@checkOut)
                          ORDER BY datetime(CheckInTime) DESC
                          LIMIT 1", connection))
                    {
                        anyOpen.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                        anyOpen.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                        using (var r = anyOpen.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                logId = r.GetInt32(0);
                                DateTime.TryParse(r.GetString(1), out chosenCheckIn);
                            }
                        }
                    }
                }

                if (!logId.HasValue)
                {
                    // No suitable open check-in to pair -> ignore (or optionally create orphan record policy)
                    return false;
                }

                // Final guard: do not set a checkout earlier than its check-in
                if (chosenCheckIn != DateTime.MinValue && checkOutDt < chosenCheckIn)
                {
                    // Reject invalid pairing
                    return false;
                }

                using (var update = new SQLiteCommand(
                    @"UPDATE StaffAttendanceLog
                      SET CheckOutTime = @checkOut, UpdatedAt = @updatedAt
                      WHERE LogID = @logId AND CheckOutTime IS NULL", connection))
                {
                    update.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    update.Parameters.AddWithValue("@updatedAt", timestamp);
                    update.Parameters.AddWithValue("@logId", logId.Value);
                    return update.ExecuteNonQuery() > 0;
                }
            }
        }


        // Insert or update staff biometric log for a single staff/device
        public async Task InsertOrUpdateStaffLogAsync(string staffId, int staffDeviceId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                // Load staff biometric sources
                string rfid = null;
                bool hasFingerprint = false;
                int? enrollmentId = null;
                string picture = null;
                using (var cmd = new SQLiteCommand(
                    "SELECT RFID, Fingerprint, EnrollmentID, Picture FROM Staff WHERE StaffID = @sid", connection))
                {
                    cmd.Parameters.AddWithValue("@sid", staffId);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (await r.ReadAsync())
                        {
                            rfid = r["RFID"] != DBNull.Value ? r["RFID"].ToString() : null;
                            hasFingerprint = r["Fingerprint"] != DBNull.Value && Convert.ToBoolean(r["Fingerprint"]);
                            enrollmentId = r["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(r["EnrollmentID"]) : (int?)null;
                            picture = r["Picture"] != DBNull.Value ? r["Picture"].ToString() : null;
                        }
                    }
                }

                // Face file check (reuse FACE folder pattern with StaffID file name if applicable)
                var faceDir = Path.Combine(Directory.GetCurrentDirectory(), "FACE");
                var facePath = Path.Combine(faceDir, $"{staffId}.jpg");
                bool hasFace = !string.IsNullOrWhiteSpace(picture) && File.Exists(facePath);

                // Prepare desired payload
                var biometricData = new Dictionary<string, string>();
                var syncStatus = new Dictionary<string, bool>();
                if (!string.IsNullOrWhiteSpace(rfid))
                {
                    biometricData["RFID"] = rfid;
                    syncStatus["RFID"] = false;
                }
                if (hasFace)
                {
                    biometricData["Face"] = facePath;
                    syncStatus["Face"] = false;
                }
                if (hasFingerprint && enrollmentId.HasValue)
                {
                    biometricData["Fingerprint"] = enrollmentId.Value.ToString();
                    syncStatus["Fingerprint"] = false;
                }

                // If nothing to sync, return
                if (biometricData.Count == 0) return;

                // Check existing StaffLog
                bool exists = false;
                Dictionary<string, string> existingBio = null;
                Dictionary<string, bool> existingSync = null;
                using (var check = new SQLiteCommand(
                    "SELECT BiometricData, SyncStatus FROM StaffLog WHERE StaffID = @sid AND StaffDeviceId = @sdid", connection))
                {
                    check.Parameters.AddWithValue("@sid", staffId);
                    check.Parameters.AddWithValue("@sdid", staffDeviceId);
                    using (var r = await check.ExecuteReaderAsync())
                    {
                        if (await r.ReadAsync())
                        {
                            exists = true;
                            existingBio = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(r["BiometricData"])) ?? new Dictionary<string, string>();
                            existingSync = JsonConvert.DeserializeObject<Dictionary<string, bool>>(Convert.ToString(r["SyncStatus"])) ?? new Dictionary<string, bool>();
                        }
                    }
                }

                // Merge and detect changes
                bool needsUpdate = !exists;
                if (exists)
                {
                    foreach (var kv in biometricData)
                    {
                        if (!existingBio.ContainsKey(kv.Key) || existingBio[kv.Key] != kv.Value)
                        {
                            needsUpdate = true;
                            break;
                        }
                    }
                    // Also remove stale modalities if present
                    foreach (var k in existingBio.Keys.ToList())
                    {
                        if (!biometricData.ContainsKey(k))
                        {
                            needsUpdate = true;
                            break;
                        }
                    }
                }

                if (!needsUpdate) return;

                // Merge old modalities (retain previous ones not present in new payload)
                if (exists)
                {
                    foreach (var kv in existingBio)
                    {
                        if (!biometricData.ContainsKey(kv.Key))
                        {
                            biometricData[kv.Key] = kv.Value;
                            syncStatus[kv.Key] = existingSync.ContainsKey(kv.Key) ? existingSync[kv.Key] : false;
                        }
                    }
                }

                var bioJson = JsonConvert.SerializeObject(biometricData);
                var syncJson = JsonConvert.SerializeObject(syncStatus);

                if (exists)
                {
                    using (var upd = new SQLiteCommand(
                        @"UPDATE StaffLog
                  SET EnrollmentID = @enr,
                      BiometricData = @bio,
                      SyncStatus = @sync,
                      UpdatedAt = @ts
                  WHERE StaffID = @sid AND StaffDeviceId = @sdid", connection))
                    {
                        upd.Parameters.AddWithValue("@enr", enrollmentId ?? (object)DBNull.Value);
                        upd.Parameters.AddWithValue("@bio", bioJson);
                        upd.Parameters.AddWithValue("@sync", syncJson);
                        upd.Parameters.AddWithValue("@ts", timestamp);
                        upd.Parameters.AddWithValue("@sid", staffId);
                        upd.Parameters.AddWithValue("@sdid", staffDeviceId);
                        await upd.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    using (var ins = new SQLiteCommand(
                        @"INSERT INTO StaffLog (StaffID, StaffDeviceId, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt)
                  VALUES (@sid, @sdid, @enr, @bio, @sync, @ts, @ts)", connection))
                    {
                        ins.Parameters.AddWithValue("@sid", staffId);
                        ins.Parameters.AddWithValue("@sdid", staffDeviceId);
                        ins.Parameters.AddWithValue("@enr", enrollmentId ?? (object)DBNull.Value);
                        ins.Parameters.AddWithValue("@bio", bioJson);
                        ins.Parameters.AddWithValue("@sync", syncJson);
                        ins.Parameters.AddWithValue("@ts", timestamp);
                        await ins.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        // Utility: delete staff logs by staffId
        public int DeleteStaffLogs(string staffId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM StaffLog WHERE StaffID = @sid", connection))
                {
                    cmd.Parameters.AddWithValue("@sid", staffId);
                    return cmd.ExecuteNonQuery();
                }
            }
        }


        public async Task InsertLogsForAllStaffAsync(string staffId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                try
                {
                    // Get all active staff device IDs
                    var staffDeviceIds = new List<int>();
                    using (var cmd = new SQLiteCommand(
                        "SELECT StaffDeviceId FROM StaffDevices WHERE IsActive = 1;", connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            staffDeviceIds.Add(reader.GetInt32(0));
                        }
                    }

                    if (staffDeviceIds.Count == 0)
                    {
                        LogHelper.Log($"[Staff] No active staff devices found for StaffID {staffId}. Skipping.");
                        return;
                    }

                    // Process each staff device: upsert StaffLog per device
                    foreach (var staffDeviceId in staffDeviceIds)
                    {
                        try
                        {
                            await InsertOrUpdateStaffLogAsync(staffId, staffDeviceId);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"[Staff] Error processing StaffDeviceId {staffDeviceId} for StaffID {staffId}: {ex.Message}");
                        }
                    }

                    LogHelper.Log($"[Staff] Log processing complete for StaffID {staffId}. Success: 1, Failed: 0, Total: {staffDeviceIds.Count}");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[Staff] Error inserting logs for StaffID {staffId}: {ex.Message}");
                    LogHelper.Log($"[Staff] Log processing complete for StaffID {staffId}. Success: 0, Failed: 1, Total: 0");
                }
            }
        }
        // Staff: fetch unsynced StaffLog entries
        public async Task<List<StaffLogEntry>> GetUnsyncedStaffLogEntriesAsync()
        {
            var result = new List<StaffLogEntry>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SQLiteCommand(
                    "SELECT Id, StaffID, StaffDeviceId, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt " +
                    "FROM StaffLog WHERE SyncStatus LIKE '%false%'", connection))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        var sync = JsonConvert.DeserializeObject<Dictionary<string, bool>>(Convert.ToString(r["SyncStatus"])) ?? new Dictionary<string, bool>();
                        if (sync.Any(s => !s.Value))
                        {
                            result.Add(new StaffLogEntry
                            {
                                Id = Convert.ToInt32(r["Id"]),
                                StaffID = Convert.ToString(r["StaffID"]),
                                StaffDeviceId = Convert.ToInt32(r["StaffDeviceId"]),
                                EnrollmentID = r["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(r["EnrollmentID"]) : (int?)null,
                                BiometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(r["BiometricData"])) ?? new Dictionary<string, string>(),
                                SyncStatus = sync,
                                CreatedAt = Convert.ToString(r["CreatedAt"]),
                                UpdatedAt = Convert.ToString(r["UpdatedAt"])
                            });
                        }
                    }
                }
            }
            return result;
        }

        // Staff: update a single biometric sync flag in StaffLog
        public async Task UpdateStaffBiometricSyncStatusAsync(int staffLogId, string biometricType, bool isSynced)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var tx = connection.BeginTransaction())
                {
                    Dictionary<string, bool> sync = null;
                    using (var read = new SQLiteCommand("SELECT SyncStatus FROM StaffLog WHERE Id = @id", connection, tx))
                    {
                        read.Parameters.AddWithValue("@id", staffLogId);
                        var raw = await read.ExecuteScalarAsync();
                        if (raw == null || raw == DBNull.Value)
                            throw new InvalidOperationException($"StaffLog entry {staffLogId} not found");

                        sync = JsonConvert.DeserializeObject<Dictionary<string, bool>>(raw.ToString()) ?? new Dictionary<string, bool>();
                    }

                    sync[biometricType] = isSynced;

                    using (var upd = new SQLiteCommand("UPDATE StaffLog SET SyncStatus = @sync, UpdatedAt = @ts WHERE Id = @id", connection, tx))
                    {
                        upd.Parameters.AddWithValue("@sync", JsonConvert.SerializeObject(sync));
                        upd.Parameters.AddWithValue("@ts", GetTimestamp());
                        upd.Parameters.AddWithValue("@id", staffLogId);
                        await upd.ExecuteNonQueryAsync();
                    }

                    tx.Commit();
                }
            }
        }

        public async Task<List<LogBiometricChange>> VerifyAndUpdateStaffLogBiometricsAsync(string staffId)
        {
            var changes = new List<LogBiometricChange>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                // Ensure staff exists in StaffLog
                bool staffExistsInLog = false;
                using (var checkCmd = new SQLiteCommand("SELECT 1 FROM StaffLog WHERE StaffID = @sid LIMIT 1", connection))
                {
                    checkCmd.Parameters.AddWithValue("@sid", staffId);
                    staffExistsInLog = (await checkCmd.ExecuteScalarAsync()) != null;
                }
                if (!staffExistsInLog)
                {
                    LogHelper.Log($"[VerifyAndUpdateStaffLogBiometricsAsync] Staff {staffId} not found in StaffLog table. Skipping.");
                    return changes;
                }

                // Load staff biometrics
                string rfid = null, picture = null;
                bool hasFingerprint = false;
                int? enrollmentId = null;
                using (var cmd = new SQLiteCommand("SELECT RFID, Fingerprint, EnrollmentID, Picture FROM Staff WHERE StaffID = @sid", connection))
                {
                    cmd.Parameters.AddWithValue("@sid", staffId);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (await r.ReadAsync())
                        {
                            rfid = r["RFID"] != DBNull.Value ? r["RFID"].ToString() : null;
                            hasFingerprint = r["Fingerprint"] != DBNull.Value && Convert.ToBoolean(r["Fingerprint"]);
                            enrollmentId = r["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(r["EnrollmentID"]) : (int?)null;
                            picture = r["Picture"] != DBNull.Value ? r["Picture"].ToString() : null;
                        }
                    }
                }

                var faceDir = Path.Combine(Directory.GetCurrentDirectory(), "FACE");
                var facePath = Path.Combine(faceDir, $"{staffId}.jpg");
                bool hasFace = !string.IsNullOrEmpty(picture) && File.Exists(facePath);

                // Target staff devices present in StaffLog
                var deviceIds = new List<int>();
                using (var devCmd = new SQLiteCommand("SELECT DISTINCT StaffDeviceId FROM StaffLog WHERE StaffID = @sid", connection))
                {
                    devCmd.Parameters.AddWithValue("@sid", staffId);
                    using (var r = await devCmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            deviceIds.Add(r.GetInt32(0));
                    }
                }

                foreach (var deviceId in deviceIds)
                {
                    int logId = 0;
                    Dictionary<string, string> biometricData = null;
                    Dictionary<string, bool> syncStatus = null;
                    int? logEnrollmentId = null;
                    bool logExists = false;

                    using (var logCmd = new SQLiteCommand(
                        "SELECT Id, EnrollmentID, BiometricData, SyncStatus FROM StaffLog WHERE StaffID = @sid AND StaffDeviceId = @did",
                        connection))
                    {
                        logCmd.Parameters.AddWithValue("@sid", staffId);
                        logCmd.Parameters.AddWithValue("@did", deviceId);
                        using (var r = await logCmd.ExecuteReaderAsync())
                        {
                            if (await r.ReadAsync())
                            {
                                logExists = true;
                                logId = r.GetInt32(0);
                                logEnrollmentId = r["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(r["EnrollmentID"]) : (int?)null;
                                biometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(r["BiometricData"])) ?? new Dictionary<string, string>();
                                syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(Convert.ToString(r["SyncStatus"])) ?? new Dictionary<string, bool>();
                            }
                        }
                    }

                    if (!logExists) continue;

                    bool updated = false;

                    // RFID
                    if (biometricData.ContainsKey("RFID"))
                    {
                        if (string.IsNullOrEmpty(rfid))
                        {
                            biometricData.Remove("RFID");
                            syncStatus.Remove("RFID");
                            changes.Add(new LogBiometricChange { BiometricType = "RFID", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            updated = true;
                        }
                        else if (biometricData["RFID"] != rfid)
                        {
                            biometricData["RFID"] = rfid;
                            syncStatus["RFID"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "RFID", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            updated = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(rfid))
                    {
                        biometricData["RFID"] = rfid;
                        syncStatus["RFID"] = false;
                        changes.Add(new LogBiometricChange { BiometricType = "RFID", DeviceId = deviceId, EnrollmentId = logEnrollmentId ?? enrollmentId });
                        updated = true;
                    }

                    // Face (require picture AND file)
                    if (biometricData.ContainsKey("Face"))
                    {
                        if (string.IsNullOrEmpty(picture) || !hasFace)
                        {
                            biometricData.Remove("Face");
                            syncStatus.Remove("Face");
                            changes.Add(new LogBiometricChange { BiometricType = "Face", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            updated = true;
                        }
                        else if (biometricData["Face"] != facePath)
                        {
                            biometricData["Face"] = facePath;
                            syncStatus["Face"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "Face", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            updated = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(picture) && hasFace)
                    {
                        biometricData["Face"] = facePath;
                        syncStatus["Face"] = false;
                        changes.Add(new LogBiometricChange { BiometricType = "Face", DeviceId = deviceId, EnrollmentId = logEnrollmentId ?? enrollmentId });
                        updated = true;
                    }

                    // Fingerprint
                    if (biometricData.ContainsKey("Fingerprint"))
                    {
                        if (!hasFingerprint || !enrollmentId.HasValue)
                        {
                            biometricData.Remove("Fingerprint");
                            syncStatus.Remove("Fingerprint");
                            changes.Add(new LogBiometricChange { BiometricType = "Fingerprint", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            updated = true;
                        }
                        else if (biometricData["Fingerprint"] != enrollmentId.Value.ToString())
                        {
                            biometricData["Fingerprint"] = enrollmentId.Value.ToString();
                            syncStatus["Fingerprint"] = false;
                            changes.Add(new LogBiometricChange { BiometricType = "Fingerprint", DeviceId = deviceId, EnrollmentId = logEnrollmentId });
                            updated = true;
                        }
                    }
                    else if (hasFingerprint && enrollmentId.HasValue)
                    {
                        biometricData["Fingerprint"] = enrollmentId.Value.ToString();
                        syncStatus["Fingerprint"] = false;
                        changes.Add(new LogBiometricChange { BiometricType = "Fingerprint", DeviceId = deviceId, EnrollmentId = logEnrollmentId ?? enrollmentId });
                        updated = true;
                    }

                    // If nothing remains, delete the log
                    if (biometricData.Count == 0)
                    {
                        using (var deleteCmd = new SQLiteCommand("DELETE FROM StaffLog WHERE Id = @id", connection))
                        {
                            deleteCmd.Parameters.AddWithValue("@id", logId);
                            await deleteCmd.ExecuteNonQueryAsync();
                        }
                        continue;
                    }

                    if (updated)
                    {
                        using (var updateCmd = new SQLiteCommand(
                            "UPDATE StaffLog SET BiometricData = @bio, SyncStatus = @sync, UpdatedAt = @ts WHERE Id = @id",
                            connection))
                        {
                            updateCmd.Parameters.AddWithValue("@bio", JsonConvert.SerializeObject(biometricData));
                            updateCmd.Parameters.AddWithValue("@sync", JsonConvert.SerializeObject(syncStatus));
                            updateCmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                            updateCmd.Parameters.AddWithValue("@id", logId);
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

            return changes;
        }



        // Get active staff with biometrics but no logs
        public List<string> GetActiveStaffIdsWithBiometricsAndNoLogs()
        {
            var staffIds = new List<string>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                const string sql = @"
SELECT s.StaffID
FROM Staff s
WHERE LOWER(TRIM(IFNULL(s.Status, ''))) = 'active'
  AND s.EnrollmentID IS NOT NULL
  AND (
        TRIM(IFNULL(s.RFID, '')) <> ''
     OR s.Fingerprint = 1
     OR TRIM(IFNULL(s.Picture, '')) <> ''
  )
  AND NOT EXISTS (
        SELECT 1
        FROM StaffLog sl
        WHERE sl.StaffID = s.StaffID
  );";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        staffIds.Add(reader.GetString(0));
                }
            }

            return staffIds;
        }


        // Fetch all staff attendance logs with resolved staff names and attendee IDs (staff IDs) by joining with Staff table on EnrollmentID
        public List<dynamic> GetAllStaffAttendanceLogs()
        {
            var results = new List<dynamic>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Join StaffAttendanceLog with Staff on EnrollmentID to resolve AttendeeID (StaffID) and Name
                string sql = @"
            SELECT 
                sal.LogID                        AS LogID,
                sal.EnrollmentID                 AS EnrollmentID,
                s.StaffID                        AS AttendeeID,
                COALESCE(s.Name, sal.Name)       AS Name,
                sal.CheckInTime                  AS CheckInTime,
                sal.CheckOutTime                 AS CheckOutTime
            FROM StaffAttendanceLog sal
            LEFT JOIN Staff s 
              ON s.EnrollmentID = sal.EnrollmentID
            ORDER BY 
                COALESCE(datetime(sal.CheckInTime), datetime(sal.CheckOutTime)) DESC,
                sal.LogID DESC;";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new
                        {
                            LogID = reader.IsDBNull(reader.GetOrdinal("LogID")) ? 0 : reader.GetInt32(reader.GetOrdinal("LogID")),
                            EnrollmentID = reader.IsDBNull(reader.GetOrdinal("EnrollmentID")) ? 0 : reader.GetInt32(reader.GetOrdinal("EnrollmentID")),
                            AttendeeID = reader.IsDBNull(reader.GetOrdinal("AttendeeID")) ? string.Empty : reader.GetString(reader.GetOrdinal("AttendeeID")),
                            Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                            CheckInTime = reader.IsDBNull(reader.GetOrdinal("CheckInTime")) ? string.Empty : reader.GetString(reader.GetOrdinal("CheckInTime")),
                            CheckOutTime = reader.IsDBNull(reader.GetOrdinal("CheckOutTime")) ? string.Empty : reader.GetString(reader.GetOrdinal("CheckOutTime"))
                        });
                    }
                }
            }

            return results;
        }













































        /// <summary>
        /// Decides the attendance case based on staff devices configuration using SQL query.
        /// If there's one device with "Check_In" type and another with "Check_Out" type, it's 2-way attendance.
        /// If there's only one device or multiple devices with "Check_In/Check_Out" type, it's 1-way attendance.
        /// </summary>
        /// <returns>Returns "2way" if devices are configured for separate check-in/check-out, "1way" otherwise</returns>
        public string DecideAttendanceCase()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Query to count different attendance types
                    using (var command = new SQLiteCommand(@"
                SELECT 
                    SUM(CASE WHEN LOWER(StaffAttendanceType) = 'check_in' THEN 1 ELSE 0 END) as CheckInCount,
                    SUM(CASE WHEN LOWER(StaffAttendanceType) = 'check_out' THEN 1 ELSE 0 END) as CheckOutCount,
                    SUM(CASE WHEN LOWER(StaffAttendanceType) = 'check_in/check_out' THEN 1 ELSE 0 END) as MixedCount,
                    COUNT(*) as TotalDevices
                FROM StaffDevices", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int checkInCount = Convert.ToInt32(reader["CheckInCount"]);
                                int checkOutCount = Convert.ToInt32(reader["CheckOutCount"]);
                                int mixedCount = Convert.ToInt32(reader["MixedCount"]);
                                int totalDevices = Convert.ToInt32(reader["TotalDevices"]);

                                LogHelper.Log($"Staff device analysis - Total: {totalDevices}, Check_In: {checkInCount}, Check_Out: {checkOutCount}, Mixed: {mixedCount}");

                                // No devices found
                                if (totalDevices == 0)
                                {
                                    LogHelper.Log("No staff devices available for attendance case decision");
                                    return "1way"; // Default to 1way if no devices
                                }

                                // If we have at least one Check_In device AND at least one Check_Out device = 2-way attendance
                                if (checkInCount > 0 && checkOutCount > 0)
                                {
                                    LogHelper.Log("✅ 2-way attendance detected: Separate check-in and check-out devices found");
                                    return "2way";
                                }

                                // All other cases:
                                // - Single device (regardless of type)
                                // - Multiple devices but all same type (all Check_In, all Check_Out, or all Check_In/Check_Out)
                                // - Any combination that doesn't have both Check_In AND Check_Out devices
                                LogHelper.Log("✅ 1-way attendance detected: Single device type configuration or mixed devices only");
                                return "1way";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in DecideAttendanceCase: {ex.Message}");
                return "1way"; // Default to 1way on error
            }

            // Fallback (should not reach here)
            LogHelper.Log("Fallback: Defaulting to 1-way attendance");
            return "1way";
        }

        /// <summary>
        /// Filters and updates all staff GLog records.
        /// 2-way case:
        ///   - Uses all unprocessed (Filter=0) rows from BOTH check-in & check-out devices together.
        ///   - Pairs earliest check-in with earliest checkout AFTER it.
        ///   - Skips (ignores) any extra check-ins occurring before that paired checkout.
        ///   - Continues until no further checkout; keeps the earliest remaining check-in as an open session.
        /// 1-way case: original alternating logic.
        /// Call this ONCE after inserting all device logs for the cycle.
        /// </summary>
        public List<GLogRecord> FilterAndUpdateStaffGLogRecordsByDate()
        {
            var filteredRecords = new List<GLogRecord>();
            try
            {
                string attendanceCase = DecideAttendanceCase();
                LogHelper.Log($"[StaffPair] Starting pairing run. Case={attendanceCase}");

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var tx = connection.BeginTransaction())
                    {
                        // -------------- NEW: Re-open previously filtered rows for dates that received new logs --------------
                        // Scenario: We first processed only Check_In device -> open check-in was marked Filter=1 Sync=1.
                        // Later Check_Out logs arrive (Sync=0). We must reprocess the whole date & enrollment.
                        if (attendanceCase == "2way")
                        {
                            string reopenSql = @"
UPDATE StaffGLog
SET Sync = 0,
    Filter = 0
WHERE EXISTS (
    SELECT 1
    FROM StaffGLog sg2
    WHERE sg2.EnrollmentNumber = StaffGLog.EnrollmentNumber
      AND DATE(sg2.DeviceTime) = DATE(StaffGLog.DeviceTime)
      AND sg2.Sync = 0            -- new (still unpaired) rows just inserted (e.g. check-outs)
);";
                            using (var reopenCmd = new SQLiteCommand(reopenSql, connection, tx))
                            {
                                int reopened = reopenCmd.ExecuteNonQuery();
                                LogHelper.Log($"[StaffPair] Reopened {reopened} previously processed rows for re-evaluation (if any).");
                            }
                        }
                        // -------------------------------------------------------------------------------------------------------

                        // Map device types
                        var deviceTypeMap = new Dictionary<int, string>();
                        using (var mapCmd = new SQLiteCommand("SELECT StaffDeviceId, LOWER(IFNULL(StaffAttendanceType,'')) t FROM StaffDevices", connection, tx))
                        using (var r = mapCmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string raw = r.GetString(1);
                                string eff = raw == "check_in" ? "Check_In"
                                           : raw == "check_out" ? "Check_Out"
                                           : "Mixed";
                                deviceTypeMap[r.GetInt32(0)] = eff;
                            }
                        }

                        // NOTE: We now always pick rows with Sync=0 (after reopen step they all get Sync=0 for affected dates)
                        string selectSql = @"
                            SELECT LogId, StaffDeviceId, EnrollmentNumber, DeviceTime, OriginalLine,
                                   Sync, DeviceType, Filter, CreatedOn, UpdatedOn,
                                   DATE(DeviceTime) AS RecordDate
                            FROM StaffGLog
                            WHERE Sync = 0
                            ORDER BY EnrollmentNumber, DATE(DeviceTime), DeviceTime";
                        var all = new List<dynamic>();
                        using (var cmd = new SQLiteCommand(selectSql, connection, tx))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int devId = r.GetInt32(1);
                                string eff;
                                if (!deviceTypeMap.TryGetValue(devId, out eff))
                                    eff = "Unknown";
                                if (attendanceCase == "2way" && eff == "Mixed")
                                {
                                    var stored = (r["DeviceType"]?.ToString() ?? "").ToLower();
                                    if (stored == "check_in") eff = "Check_In";
                                    else if (stored == "check_out") eff = "Check_Out";
                                    else eff = "Unknown";
                                }
                                all.Add(new
                                {
                                    LogId = r.GetInt32(0),
                                    StaffDeviceId = devId,
                                    EnrollmentNumber = r["EnrollmentNumber"].ToString(),
                                    DeviceTime = DateTime.Parse(r["DeviceTime"].ToString()),
                                    DeviceTimeText = r["DeviceTime"].ToString(),
                                    OriginalLine = r["OriginalLine"].ToString(),
                                    Sync = Convert.ToBoolean(r["Sync"]),
                                    OriginalDeviceType = r["DeviceType"]?.ToString() ?? "unknown",
                                    EffectiveType = eff,
                                    Filter = r["Filter"] != DBNull.Value && Convert.ToBoolean(r["Filter"]),
                                    CreatedOn = r["CreatedOn"].ToString(),
                                    UpdatedOn = r["UpdatedOn"].ToString(),
                                    RecordDate = r["RecordDate"].ToString()
                                });
                            }
                        }

                        LogHelper.Log($"[StaffPair] Loaded {all.Count} rows for pairing after reopen phase.");

                        var grouped = all.GroupBy(g => new { g.EnrollmentNumber, g.RecordDate }).ToList();
                        var selectedIds = new HashSet<int>();
                        var openIds = new HashSet<int>();
                        var ignoredIds = new HashSet<int>();

                        // --- UPDATED inside FilterAndUpdateStaffGLogRecordsByDate() loop over grouped ---
                        foreach (var group in grouped)
                        {
                            LogHelper.Log($"[StaffPair] Group Enroll={group.Key.EnrollmentNumber} Date={group.Key.RecordDate} Total={group.Count()}");
                            if (attendanceCase == "2way")
                            {
                                PairTwoWay(group.ToList(), selectedIds, openIds, ignoredIds, filteredRecords);
                            }
                            else
                            {
                                // One-way (single / mixed device) pairing
                                PairOneWay(group.OrderBy(x => x.DeviceTime).ToList(), selectedIds, ignoredIds, filteredRecords);
                            }
                        }


                        string ts = GetTimestamp();
                        if (selectedIds.Count > 0)
                        {
                            string updateSelected = $"UPDATE StaffGLog SET Sync=1, Filter=1, UpdatedOn=@ts WHERE LogId IN ({string.Join(",", selectedIds)})";
                            using (var us = new SQLiteCommand(updateSelected, connection, tx))
                            {
                                us.Parameters.AddWithValue("@ts", ts);
                                int n = us.ExecuteNonQuery();
                                LogHelper.Log($"[StaffPair] Marked {n} paired/open rows Filter=1 Sync=1.");
                            }
                        }
                        var toIgnore = ignoredIds.Except(selectedIds).ToList();
                        if (toIgnore.Count > 0)
                        {
                            string updateIgnored = $"UPDATE StaffGLog SET Sync=1, Filter=0, UpdatedOn=@ts WHERE LogId IN ({string.Join(",", toIgnore)})";
                            using (var ui = new SQLiteCommand(updateIgnored, connection, tx))
                            {
                                ui.Parameters.AddWithValue("@ts", ts);
                                int n = ui.ExecuteNonQuery();
                                LogHelper.Log($"[StaffPair] Marked {n} ignored rows Filter=0 Sync=1.");
                            }
                        }

                        tx.Commit();
                        LogHelper.Log($"[StaffPair] Finished. Output filtered rows={filteredRecords.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[StaffPair][ERROR] {ex.Message}");
                throw;
            }

            return filteredRecords;
        }





    }
}
