using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string RFID { get; set; }
        public string Picture { get; set; }
        public int? EnrollmentID { get; set; }
        public bool Fingerprint { get; set; }
        public string PackageExpiry { get; set; }
        public bool is_staff { get; set; } 
        public Dictionary<int, bool> SyncStatus { get; set; } = new Dictionary<int, bool>();
    }

    public class Package
    {
        public int PackageId { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string PricingTierJson { get; set; } // raw JSON, e.g. {"monthly":5000,"quarterly":14000}
        public decimal? RegistrationFee { get; set; }
        public string RegistrationFeeBasis { get; set; }
        public string GroupMode { get; set; }
        public bool Active { get; set; }
    }

    public class Trainer
    {
        public string TrainerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}".Trim();
    }

    public class OfflineMember
    {
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string SubscriptionJson { get; set; }
        public Dictionary<int, bool> SyncStatus { get; set; } = new Dictionary<int, bool>();
        public string Picture { get; set; }
        public string RFID { get; set; }
        public bool Fingerprint { get; set; }
        public int EnrollmentID { get; set; }
        public bool ServerSync { get; set; }
        // Per-device deletion progress after a successful server sync: {deviceId: true} once
        // that device's deleteuser call has succeeded. Only meaningful once ServerSync = true.
        public Dictionary<int, bool> DeviceCleanupStatus { get; set; } = new Dictionary<int, bool>();
        public string CreatedAt { get; set; }
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

    // OfflineMemberLog model — mirrors LogEntry but keyed by ClientId instead of MemberID
    public class OfflineLogEntry
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public int DeviceID { get; set; }
        public int EnrollmentID { get; set; }
        public Dictionary<string, string> BiometricData { get; set; }
        public Dictionary<string, bool> SyncStatus { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }

    // A pending membership-renewal awaiting upload to /api/sync/enrollments.
    public class OfflineSubscription
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string ClientId { get; set; }
        public string SubscriptionJson { get; set; }
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
        public int? CheckInDeviceId { get; set; }
        public int? CheckOutDeviceId { get; set; }
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

    public partial class DatabaseHelper
    {

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["SmartAttendance"]?.ConnectionString;
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6");

        /// <summary>
        /// Initializes the SQL Server database by creating the file (if needed) and the necessary tables.
        /// Tables:
        ///  - Settings: to store key-value pairs (e.g., APIKey, TimeZone, VersionTimestamp)
        ///  - Members: to store member details (MemberID, Name, Status, RFID, Picture, MachineID)
        ///  - AttendanceLogs: to store attendance records (LogID, MemberID, CheckInTime, CheckOutTime, Synced)
        /// </summary>
        private void EnsureDatabaseExists()
        {
            // Build a connection string pointing to master so we can create the DB if missing
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using (var masterConn = new SqlConnection(builder.ConnectionString))
            {
                masterConn.Open();
                using (var cmd = new SqlCommand(
                    $"IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{dbName}') CREATE DATABASE [{dbName}];",
                    masterConn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Explicitly enforce AUTO_CLOSE OFF every startup — regardless of whether the
                // database was just created or already existed. A frequently-accessed database
                // like this one (constant device/dashboard/sync connections) should never be
                // allowed to fully close and reopen between uses: that churn resets the query
                // plan cache and adds avoidable overhead on every reconnect. This can't be left
                // to rely on `model`'s default inheriting correctly, since the live database was
                // found to have AUTO_CLOSE ON even though `model` itself has it OFF.
                using (var cmd = new SqlCommand(
                    $"ALTER DATABASE [{dbName}] SET AUTO_CLOSE OFF;",
                    masterConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InitializeDatabase()
        {
            // Ensure the database itself exists before trying to open it
            EnsureDatabaseExists();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // All DDL statements to run with corrected data types
                var createSettingsSql = @"
IF OBJECT_ID('dbo.Settings','U') IS NULL
BEGIN
    CREATE TABLE dbo.Settings (
        [Key] NVARCHAR(255) PRIMARY KEY,
        Value NVARCHAR(MAX),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createMembersSql = @"
IF OBJECT_ID('dbo.Members','U') IS NULL
BEGIN
    CREATE TABLE dbo.Members (
        MemberID INT IDENTITY(1,1) PRIMARY KEY,
        ClientId NVARCHAR(36),
        Name NVARCHAR(255),
        Status NVARCHAR(50),
        RFID NVARCHAR(100),
        Picture NVARCHAR(500),
        Fingerprint BIT,
        EnrollmentID INT,
        PackageExpiry NVARCHAR(100),
        SyncStatus NVARCHAR(50),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createStaffSql = @"
IF OBJECT_ID('dbo.Staff','U') IS NULL
BEGIN
    CREATE TABLE dbo.Staff (
        StaffID NVARCHAR(100) PRIMARY KEY,
        Name NVARCHAR(255),
        Status NVARCHAR(50),
        RFID NVARCHAR(100),
        Picture NVARCHAR(500),
        Fingerprint BIT,
        EnrollmentID INT,
        SyncStatus NVARCHAR(50),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createAttendanceLogsSql = @"
IF OBJECT_ID('dbo.AttendanceLogs','U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceLogs (
        LogID INT IDENTITY(1,1) PRIMARY KEY,
        MemberID INT,
        AttendanceId NVARCHAR(100),
        CheckInTime DATETIME2,
        CheckOutTime DATETIME2,
        SyncStatus NVARCHAR(50),
        DeviceInfo NVARCHAR(100),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2,
        CONSTRAINT FK_AttendanceLogs_Members FOREIGN KEY(MemberID) REFERENCES dbo.Members(MemberID)
    );
END";

                var createLocalUsersSql = @"
IF OBJECT_ID('dbo.LocalUsers','U') IS NULL
BEGIN
    CREATE TABLE dbo.LocalUsers (
        LocalUserID INT IDENTITY(1,1) PRIMARY KEY,
        UserName NVARCHAR(100) UNIQUE NOT NULL,
        Role NVARCHAR(20) CHECK(Role IN ('admin','user','developer')) NOT NULL,
        Password NVARCHAR(255) NOT NULL,
        BackupCode NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createDevicesSql = @"
IF OBJECT_ID('dbo.Devices','U') IS NULL
BEGIN
    CREATE TABLE dbo.Devices (
        DeviceId INT IDENTITY(1,1) PRIMARY KEY,
        Department NVARCHAR(100),
        AttendanceType BIT,
        DeviceSerial NVARCHAR(500),
        DeviceIP NVARCHAR(500) UNIQUE NOT NULL,
        Port INT,
        MachineNo INT UNIQUE,
        TimeZone NVARCHAR(100),
        Location NVARCHAR(100),
        ConnectionType NVARCHAR(20),
        IsActive BIT,
        LastSyncTime DATETIME2,
        Password NVARCHAR(255) NOT NULL,
        FaceCapacity INT,
        RFIDCapacity INT,
        FingerCapacity INT,
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createFingerprintsSql = @"
IF OBJECT_ID('dbo.Fingerprints','U') IS NULL
BEGIN
    CREATE TABLE dbo.Fingerprints (
        EnrollNumber INT PRIMARY KEY,
        FingerNumber INT NOT NULL,
        Privilege INT DEFAULT 0,
        TemplateData VARBINARY(MAX),
        TemplateString NVARCHAR(MAX),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createGLogSql = @"
IF OBJECT_ID('dbo.GLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.GLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId INT NOT NULL,
        EnrollmentNumber NVARCHAR(100) NOT NULL,
        DeviceTime DATETIME2 NOT NULL,
        OriginalLine NVARCHAR(1000) NOT NULL,
        Sync BIT DEFAULT 0,
        DeviceType NVARCHAR(50),
        Filter BIT DEFAULT 0,
        CreatedOn DATETIME2,
        UpdatedOn DATETIME2,
        CONSTRAINT FK_GLog_Devices FOREIGN KEY (DeviceId) REFERENCES dbo.Devices(DeviceId)
    );
END";

                var createSyncLogSql = @"
IF OBJECT_ID('dbo.Log','U') IS NULL
BEGIN
    CREATE TABLE dbo.Log (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MemberID INT NOT NULL,
        DeviceID INT NOT NULL,
        EnrollmentID INT,
        BiometricData NVARCHAR(MAX),
        SyncStatus NVARCHAR(50),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2,
        CONSTRAINT FK_Log_Members FOREIGN KEY (MemberID) REFERENCES dbo.Members(MemberID),
        CONSTRAINT FK_Log_Devices FOREIGN KEY (DeviceID) REFERENCES dbo.Devices(DeviceId)
    );
END";

                var createStaffDevicesSql = @"
IF OBJECT_ID('dbo.StaffDevices','U') IS NULL
BEGIN
    CREATE TABLE dbo.StaffDevices (
        StaffDeviceId INT IDENTITY(1,1) PRIMARY KEY,
        Department NVARCHAR(100),
        StaffAttendanceType NVARCHAR(100),
        DeviceSerial NVARCHAR(500),
        DeviceIP NVARCHAR(500) UNIQUE NOT NULL,
        Port INT,
        MachineNo INT UNIQUE,
        TimeZone NVARCHAR(100),
        Location NVARCHAR(100),
        ConnectionType NVARCHAR(20),
        IsActive BIT,
        LastSyncTime DATETIME2,
        Password NVARCHAR(255) NOT NULL,
        FaceCapacity INT,
        RFIDCapacity INT,
        FingerCapacity INT,
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createStaffLogSql = @"
IF OBJECT_ID('dbo.StaffLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.StaffLog (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StaffID NVARCHAR(100) NOT NULL,
        StaffDeviceId INT NOT NULL,
        EnrollmentID INT,
        BiometricData NVARCHAR(MAX),
        SyncStatus NVARCHAR(50),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2,
        CONSTRAINT FK_StaffLog_Staff FOREIGN KEY (StaffID) REFERENCES dbo.Staff(StaffID),
        CONSTRAINT FK_StaffLog_StaffDevices FOREIGN KEY (StaffDeviceId) REFERENCES dbo.StaffDevices(StaffDeviceId)
    );
END";

                var createStaffGLogSql = @"
IF OBJECT_ID('dbo.StaffGLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.StaffGLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        StaffDeviceId INT NOT NULL,
        EnrollmentNumber NVARCHAR(100) NOT NULL,
        DeviceTime DATETIME2 NOT NULL,
        OriginalLine NVARCHAR(1000) NOT NULL,
        Sync BIT DEFAULT 0,
        DeviceType NVARCHAR(50),
        Filter BIT DEFAULT 0,
        CreatedOn DATETIME2,
        UpdatedOn DATETIME2,
        CONSTRAINT FK_StaffGLog_StaffDevices FOREIGN KEY (StaffDeviceId) REFERENCES dbo.StaffDevices(StaffDeviceId)
    );
END";

                var createStaffAttendanceLogSql = @"
IF OBJECT_ID('dbo.StaffAttendanceLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.StaffAttendanceLog (
        LogID INT IDENTITY(1,1) PRIMARY KEY,
        EnrollmentID INT NOT NULL,
        AttendanceId NVARCHAR(100),
        CheckInTime DATETIME2,
        CheckOutTime DATETIME2,
        SyncStatus NVARCHAR(500),
        DeviceInfo NVARCHAR(100),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";


                var createGateAccessedSql = @"
IF OBJECT_ID('dbo.GateAccessed','U') IS NULL
BEGIN
    CREATE TABLE dbo.GateAccessed (
        GateId INT IDENTITY(1,1) PRIMARY KEY,
        DeviceIP NVARCHAR(50) NOT NULL,
        MemberId INT NULL,
        IsVisitor BIT NOT NULL DEFAULT 0,
        VisitorName NVARCHAR(255),
        VisitorPhone NVARCHAR(50),
        Reason NVARCHAR(500),
        AccessedTime DATETIME2,
        Username NVARCHAR(100),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2,
        CONSTRAINT FK_GateAccessed_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberID)
    );
END";

                var createPendingJobsSql = @"
IF OBJECT_ID('dbo.PendingJobs','U') IS NULL
BEGIN
    CREATE TABLE dbo.PendingJobs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Action NVARCHAR(100) NOT NULL,
        MemberId INT NOT NULL,
        EnrollmentId INT NOT NULL,
        DeviceId INT,
        Ip NVARCHAR(50) NOT NULL,
        Port INT NOT NULL,
        MachineNo INT NOT NULL,
        Password NVARCHAR(255) NOT NULL,
        BackupNumber INT NOT NULL,
        CreatedAt DATETIME2,
        CONSTRAINT FK_PendingJobs_Devices FOREIGN KEY (DeviceId) REFERENCES dbo.Devices(DeviceId)
    );
END";

                var createPackagesSql = @"
IF OBJECT_ID('dbo.Packages','U') IS NULL
BEGIN
    CREATE TABLE dbo.Packages (
        PackageId INT PRIMARY KEY,
        Title NVARCHAR(200),
        Code NVARCHAR(100),
        Description NVARCHAR(MAX),
        PricingTier NVARCHAR(MAX),
        RegistrationFee DECIMAL(18,2) NULL,
        RegistrationFeeBasis NVARCHAR(50),
        GroupMode NVARCHAR(50),
        Active BIT,
        RemoteUpdatedAt DATETIME2,
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createTrainersSql = @"
IF OBJECT_ID('dbo.Trainers','U') IS NULL
BEGIN
    CREATE TABLE dbo.Trainers (
        TrainerId NVARCHAR(100) PRIMARY KEY,
        FirstName NVARCHAR(150),
        LastName NVARCHAR(150),
        PersonalInformation NVARCHAR(MAX),
        RemoteUpdatedAt DATETIME2,
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createOfflineMembersSql = @"
IF OBJECT_ID('dbo.OfflineMembers','U') IS NULL
BEGIN
    CREATE TABLE dbo.OfflineMembers (
        ClientId NVARCHAR(36) PRIMARY KEY,
        Name NVARCHAR(255),
        FirstName NVARCHAR(150),
        LastName NVARCHAR(150),
        Gender NVARCHAR(20),
        Phone NVARCHAR(50),
        Subscription NVARCHAR(MAX),
        SyncStatus NVARCHAR(50),
        Picture NVARCHAR(500),
        RFID NVARCHAR(100),
        Fingerprint BIT,
        EnrollmentID INT UNIQUE,
        ServerSync BIT,
        DeviceCleanupStatus NVARCHAR(500),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2
    );
END";

                var createOfflineMemberLogSql = @"
IF OBJECT_ID('dbo.OfflineMemberLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.OfflineMemberLog (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ClientId NVARCHAR(36) NOT NULL,
        DeviceID INT NOT NULL,
        EnrollmentID INT,
        BiometricData NVARCHAR(MAX),
        SyncStatus NVARCHAR(50),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2,
        CONSTRAINT FK_OfflineMemberLog_OfflineMembers FOREIGN KEY (ClientId) REFERENCES dbo.OfflineMembers(ClientId),
        CONSTRAINT FK_OfflineMemberLog_Devices FOREIGN KEY (DeviceID) REFERENCES dbo.Devices(DeviceId)
    );
END";

                var createOfflineSubscriptionsSql = @"
IF OBJECT_ID('dbo.OfflineSubscriptions','U') IS NULL
BEGIN
    CREATE TABLE dbo.OfflineSubscriptions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        ClientId NVARCHAR(36),
        Subscription NVARCHAR(MAX),
        CreatedAt DATETIME2,
        UpdatedAt DATETIME2,
        CONSTRAINT FK_OfflineSubscriptions_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberID)
    );
END";

                var createIndexesSql = @"
-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GLog_DeviceId' AND object_id = OBJECT_ID('dbo.GLog'))
    CREATE INDEX IX_GLog_DeviceId ON dbo.GLog(DeviceId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GLog_DeviceTime' AND object_id = OBJECT_ID('dbo.GLog'))
    CREATE INDEX IX_GLog_DeviceTime ON dbo.GLog(DeviceTime);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AttendanceLogs_MemberID' AND object_id = OBJECT_ID('dbo.AttendanceLogs'))
    CREATE INDEX IX_AttendanceLogs_MemberID ON dbo.AttendanceLogs(MemberID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AttendanceLogs_CheckInTime' AND object_id = OBJECT_ID('dbo.AttendanceLogs'))
    CREATE INDEX IX_AttendanceLogs_CheckInTime ON dbo.AttendanceLogs(CheckInTime);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Log_MemberID' AND object_id = OBJECT_ID('dbo.Log'))
    CREATE INDEX IX_Log_MemberID ON dbo.Log(MemberID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Log_DeviceID' AND object_id = OBJECT_ID('dbo.Log'))
    CREATE INDEX IX_Log_DeviceID ON dbo.Log(DeviceID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StaffLog_StaffID' AND object_id = OBJECT_ID('dbo.StaffLog'))
    CREATE INDEX IX_StaffLog_StaffID ON dbo.StaffLog(StaffID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StaffLog_StaffDeviceId' AND object_id = OBJECT_ID('dbo.StaffLog'))
    CREATE INDEX IX_StaffLog_StaffDeviceId ON dbo.StaffLog(StaffDeviceId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StaffGLog_StaffDeviceId' AND object_id = OBJECT_ID('dbo.StaffGLog'))
    CREATE INDEX IX_StaffGLog_StaffDeviceId ON dbo.StaffGLog(StaffDeviceId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StaffGLog_DeviceTime' AND object_id = OBJECT_ID('dbo.StaffGLog'))
    CREATE INDEX IX_StaffGLog_DeviceTime ON dbo.StaffGLog(DeviceTime);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GateAccessed_MemberId' AND object_id = OBJECT_ID('dbo.GateAccessed'))
    CREATE INDEX IX_GateAccessed_MemberId ON dbo.GateAccessed(MemberId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GateAccessed_AccessedTime' AND object_id = OBJECT_ID('dbo.GateAccessed'))
    CREATE INDEX IX_GateAccessed_AccessedTime ON dbo.GateAccessed(AccessedTime);

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.GateAccessed') AND name = 'MemberId' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.GateAccessed DROP CONSTRAINT FK_GateAccessed_Members;
    ALTER TABLE dbo.GateAccessed ALTER COLUMN MemberId INT NULL;
    ALTER TABLE dbo.GateAccessed ADD CONSTRAINT FK_GateAccessed_Members FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberID);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PendingJobs_DeviceId' AND object_id = OBJECT_ID('dbo.PendingJobs'))
    CREATE INDEX IX_PendingJobs_DeviceId ON dbo.PendingJobs(DeviceId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PendingJobs_CreatedAt' AND object_id = OBJECT_ID('dbo.PendingJobs'))
    CREATE INDEX IX_PendingJobs_CreatedAt ON dbo.PendingJobs(CreatedAt);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AttendanceLogs') AND name = 'DeviceInfo')
    ALTER TABLE dbo.AttendanceLogs ADD DeviceInfo NVARCHAR(100);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.StaffAttendanceLog') AND name = 'DeviceInfo')
    ALTER TABLE dbo.StaffAttendanceLog ADD DeviceInfo NVARCHAR(100);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OfflineMemberLog_ClientId' AND object_id = OBJECT_ID('dbo.OfflineMemberLog'))
    CREATE INDEX IX_OfflineMemberLog_ClientId ON dbo.OfflineMemberLog(ClientId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OfflineMemberLog_DeviceID' AND object_id = OBJECT_ID('dbo.OfflineMemberLog'))
    CREATE INDEX IX_OfflineMemberLog_DeviceID ON dbo.OfflineMemberLog(DeviceID);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Members') AND name = 'ClientId')
BEGIN
    ALTER TABLE dbo.Members ADD ClientId NVARCHAR(36) NULL;
    EXEC('UPDATE dbo.Members SET ClientId = LOWER(CONVERT(NVARCHAR(36), NEWID())) WHERE ClientId IS NULL');
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OfflineMembers') AND name = 'DeviceCleanupStatus')
    ALTER TABLE dbo.OfflineMembers ADD DeviceCleanupStatus NVARCHAR(500);

-- Migration: Add error logging columns
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Log') AND name = 'AttemptCount')
    ALTER TABLE dbo.Log ADD AttemptCount INT DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Log') AND name = 'FailureReason')
    ALTER TABLE dbo.Log ADD FailureReason NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.StaffLog') AND name = 'AttemptCount')
    ALTER TABLE dbo.StaffLog ADD AttemptCount INT DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.StaffLog') AND name = 'FailureReason')
    ALTER TABLE dbo.StaffLog ADD FailureReason NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OfflineMemberLog') AND name = 'AttemptCount')
    ALTER TABLE dbo.OfflineMemberLog ADD AttemptCount INT DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OfflineMemberLog') AND name = 'FailureReason')
    ALTER TABLE dbo.OfflineMemberLog ADD FailureReason NVARCHAR(MAX) NULL;";

                var ddlCommands = new[]
                {
                createSettingsSql,
                createMembersSql,
                createStaffSql,
                createDevicesSql,
                createFingerprintsSql,
                createGLogSql,
                createAttendanceLogsSql,
                createLocalUsersSql,
                createSyncLogSql,
                createPackagesSql,
                createTrainersSql,
                createOfflineMembersSql,
                createOfflineMemberLogSql,
                createOfflineSubscriptionsSql,
                createStaffDevicesSql,
                createStaffLogSql,
                createStaffGLogSql,
                createStaffAttendanceLogSql,
                createGateAccessedSql,
                createPendingJobsSql,
                createIndexesSql
            };

                // Execute all DDL statements inside a transaction and handle errors
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var sql in ddlCommands)
                        {
                            using (var command = new SqlCommand(sql, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { /* ignore rollback errors */ }
                        // Log or rethrow as appropriate for your app
                        throw new InvalidOperationException("Failed to initialize database schema.", ex);
                    }
                }

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

        public string EncryptSetting(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = EncryptionKey;
                aesAlg.IV = new byte[16];
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public string DecryptSetting(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return null;
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = EncryptionKey;
                    aesAlg.IV = new byte[16];
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch { return null; }
        }

        public void SaveEncryptedSetting(string key, string value)
        {
            SaveSetting(key, EncryptSetting(value));
        }

        public string GetDecryptedSetting(string key)
        {
            return DecryptSetting(GetSetting(key));
        }

       

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

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
INSERT INTO LocalUsers (UserName, Role, Password, BackupCode, CreatedAt, UpdatedAt)
VALUES (@UserName, @Role, @Password, @BackupCode, @CreatedAt, @UpdatedAt);", connection, transaction))
                        {
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
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error adding user: {ex.Message}");
                        return false;
                    }
                }
            }
        }


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

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
INSERT INTO LocalUsers (UserName, Role, Password, BackupCode, CreatedAt, UpdatedAt)
VALUES (@UserName, @Role, @Password, @BackupCode, @CreatedAt, @UpdatedAt);
SELECT SCOPE_IDENTITY();", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserName", userName);
                            command.Parameters.AddWithValue("@Role", role);
                            command.Parameters.AddWithValue("@Password", hashedPassword);
                            command.Parameters.AddWithValue("@BackupCode", hashedBackupCode);
                            command.Parameters.AddWithValue("@CreatedAt", timestamp);
                            command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                            var result = command.ExecuteScalar();
                            int lastId = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                            transaction.Commit();
                            Console.WriteLine($"User '{userName}' added successfully.");
                            return lastId;
                        }
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error adding user: {ex.Message}");
                        return null;
                    }
                }
            }
        }

        //Login user with username and password or backup code
        public (bool Success, string Role, int UserId) LoginUser(string userName, string passwordOrBackup)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
SELECT LocalUserID, Password, BackupCode, Role 
FROM LocalUsers 
WHERE UserName = @UserName;", connection))
                    {
                        command.Parameters.AddWithValue("@UserName", userName ?? string.Empty);

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Console.WriteLine("❌ User not found.");
                                return (false, null, -1);
                            }

                            int userId = Convert.ToInt32(reader["LocalUserID"]);
                            string storedPasswordBase64 = reader["Password"]?.ToString() ?? string.Empty;
                            string storedBackupCodeBase64 = reader["BackupCode"]?.ToString() ?? string.Empty;
                            string userRole = reader["Role"]?.ToString();

                            byte[] inputHashedBytes = HashString(passwordOrBackup);
                            string inputHashedBase64 = Convert.ToBase64String(inputHashedBytes);

                            if (storedPasswordBase64 == inputHashedBase64 || storedBackupCodeBase64 == inputHashedBase64)
                            {
                                Console.WriteLine("✅ Login successful.");
                                return (true, userRole, userId);
                            }

                            Console.WriteLine("❌ Invalid credentials.");
                            return (false, null, -1);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return (false, null, -1);
            }
        }

        public bool VerifyLoginUser(string userName, string passwordOrBackup)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
SELECT Password, BackupCode FROM LocalUsers WHERE UserName = @UserName;", connection))
                    {
                        command.Parameters.AddWithValue("@UserName", userName ?? string.Empty);

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Console.WriteLine("❌ User not found.");
                                return false;
                            }

                            string storedPasswordBase64 = reader["Password"]?.ToString() ?? string.Empty;
                            string storedBackupCodeBase64 = reader["BackupCode"]?.ToString() ?? string.Empty;

                            byte[] inputHashedBytes = HashString(passwordOrBackup);
                            string inputHashedBase64 = Convert.ToBase64String(inputHashedBytes);

                            if (storedPasswordBase64 == inputHashedBase64 || storedBackupCodeBase64 == inputHashedBase64)
                            {
                                Console.WriteLine("✅ Login successful.");
                                return true;
                            }

                            Console.WriteLine("❌ Invalid credentials.");
                            return false;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return false;
            }
        }

        //Check if a user exists in the LocalUsers table
        public bool AdminUserExists()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM LocalUsers WHERE Role = 'admin'", connection))
                    {
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking admin user existence: {ex.Message}");
                return false;
            }
        }

        //Load all users
        public List<LocalUser> LoadUsers()
        {
            var users = new List<LocalUser>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT * FROM LocalUsers", connection))
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
                                CreatedAt = reader["CreatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedAt"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedAt"]).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
            return users;
        }

        //Delete a user from the LocalUsers table
        public void DeleteUserById(int userId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("DELETE FROM LocalUsers WHERE LocalUserID = @id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", userId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error deleting user: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        //Update user information in the LocalUsers table
        public void UpdateUserInfo(int userId, string newUsername, string newRole)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("UPDATE LocalUsers SET UserName = @username, Role = @role, UpdatedAt = @updated WHERE LocalUserID = @id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@username", newUsername);
                            command.Parameters.AddWithValue("@role", newRole);
                            command.Parameters.AddWithValue("@updated", GetTimestamp());
                            command.Parameters.AddWithValue("@id", userId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating user info: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        //Update user password in the LocalUsers table
        public void UpdateUserPassword(int userId, string newPassword)
        {
            string hashedPassword = Convert.ToBase64String(HashString(newPassword));

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("UPDATE LocalUsers SET Password = @password, UpdatedAt = @updated WHERE LocalUserID = @id", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@password", hashedPassword);
                            command.Parameters.AddWithValue("@updated", GetTimestamp());
                            command.Parameters.AddWithValue("@id", userId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating user password: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Saves or updates a setting (as a key-value pair) in the Settings table.
        /// </summary>
        public void SaveSetting(string key, string value)
        {
            string timestamp = GetTimestamp();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
MERGE Settings AS target
USING (SELECT @key AS [Key], @value AS Value, @createdAt AS CreatedAt, @updatedAt AS UpdatedAt) AS source
ON target.[Key] = source.[Key]
WHEN MATCHED THEN
    UPDATE SET Value = source.Value, UpdatedAt = source.UpdatedAt
WHEN NOT MATCHED THEN
    INSERT ([Key], Value, CreatedAt, UpdatedAt) VALUES (source.[Key], source.Value, source.CreatedAt, source.UpdatedAt);", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@key", key);
                            command.Parameters.AddWithValue("@value", value);
                            command.Parameters.AddWithValue("@createdAt", timestamp);
                            command.Parameters.AddWithValue("@updatedAt", timestamp);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error saving setting: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the value of a setting from the Settings table.
        /// Returns null if the key does not exist.
        /// </summary>
        public string GetSetting(string key)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT Value FROM Settings WHERE [Key] = @key;", connection))
                    {
                        command.Parameters.AddWithValue("@key", key);
                        var result = command.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting setting: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns SQL Server's current process memory usage in MB (physical memory actually
        /// in use, not the buffer pool target). Returns null if the query fails — e.g. the
        /// login lacks VIEW SERVER STATE permission, or SQL Server is briefly unreachable —
        /// so callers can show "N/A" instead of a misleading number.
        /// Intentionally cheap: a single scalar read from a DMV, no table scans.
        /// </summary>
        public int? GetSqlServerMemoryUsageMb()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        "SELECT physical_memory_in_use_kb / 1024 FROM sys.dm_os_process_memory;", connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (SqlException ex)
            {
                LogHelper.Log($"[DatabaseHelper] Error getting SQL Server memory usage: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves or updates a JSON array in the Settings table for the given key.
        /// The entire array is replaced with the provided list of values.
        /// </summary>
        public void SaveJsonArraySetting(string key, List<string> values)
        {
            string jsonValue = JsonConvert.SerializeObject(values ?? new List<string>());
            SaveSetting(key, jsonValue);
        }

        public Member GetMember(int memberId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        "SELECT MemberID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, PackageExpiry, SyncStatus, ClientId " +
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
                                    SyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>(),
                                    ClientId = reader.IsDBNull(9) ? null : reader.GetString(9)
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting member: {ex.Message}");
            }
            return null;
        }

        // Updated GetMembers method
        public List<Member> GetMembers()
        {
            var members = new List<Member>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        "SELECT MemberID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, PackageExpiry, SyncStatus, ClientId FROM Members", connection))
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
                                SyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>(),
                                ClientId = reader.IsDBNull(9) ? null : reader.GetString(9)
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting members: {ex.Message}");
            }
            return members;
        }

        

        /// <summary>
        /// Inserts or updates a member record in the local DB using data received from the cloud.
        /// </summary>
        public void UpsertMember(JObject member)
        {
            int memberId = (int)member["id"];
            string name = member["name"]?.ToString() ?? string.Empty;
            string status = (string)member["status"];
            string rfid = member["rfid"]?.ToString() ?? string.Empty;
            string picture = member["picture"]?.ToString() ?? string.Empty;
            bool fingerprint = member["fingerprint"]?.ToObject<bool>() ?? false;
            int? enrollmentId = member["enrollment_id"]?.ToObject<int?>();
            string packageExpiry = member["package_expiry"]?.ToString();
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var existingMember = GetMember(memberId);
            var devices = LoadDevices();
            var syncStatus = new Dictionary<int, bool>();

            bool isBiometricChange = false;
            if (existingMember != null)
            {
                // Check if any biometric data changed
                bool rfidChanged = rfid != (existingMember.RFID ?? string.Empty);
                bool fingerprintChanged = fingerprint != existingMember.Fingerprint;
                bool enrollmentIdChanged = enrollmentId != existingMember.EnrollmentID;
                bool pictureChanged = picture != (existingMember.Picture ?? string.Empty);
                bool statusChanged = status != existingMember.Status;

                isBiometricChange = rfidChanged || fingerprintChanged || enrollmentIdChanged || pictureChanged || statusChanged;
            }
            else
            {
                // New member -> treat as biometric change
                isBiometricChange = true;
            }

            // Only initialize/reset sync status if there's a biometric change or it's a new member
            if (isBiometricChange)
            {
                foreach (var device in devices)
                {
                    // Reset to false (not synced) only if biometric data changed
                    syncStatus[device.DeviceId] = false;
                }
            }
            else
            {
                // No biometric change -> preserve existing sync status
                if (existingMember?.SyncStatus != null)
                {
                    syncStatus = new Dictionary<int, bool>(existingMember.SyncStatus);
                }
                else
                {
                    // Fallback: no existing sync status, initialize as not synced
                    foreach (var device in devices)
                    {
                        syncStatus[device.DeviceId] = false;
                    }
                }
            }

            string syncStatusJson = JsonConvert.SerializeObject(syncStatus);

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Check if record exists
                        bool exists;
                        using (var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Members WHERE MemberID = @id", conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@id", memberId);
                            exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        if (exists)
                        {
                            using (var cmd = new SqlCommand(@"
UPDATE Members
SET Name = @name,
    Status = @status,
    RFID = @rfid,
    Picture = @picture,
    Fingerprint = @fingerprint,
    EnrollmentID = @enrollment_id,
    PackageExpiry = @package_expiry,
    SyncStatus = @syncStatus,
    UpdatedAt = @updatedAt
WHERE MemberID = @id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", memberId);
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.Parameters.AddWithValue("@status", status ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@rfid", string.IsNullOrEmpty(rfid) ? (object)DBNull.Value : rfid);
                                cmd.Parameters.AddWithValue("@picture", string.IsNullOrEmpty(picture) ? (object)DBNull.Value : picture);
                                cmd.Parameters.AddWithValue("@fingerprint", fingerprint);
                                cmd.Parameters.AddWithValue("@enrollment_id", enrollmentId ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@package_expiry", string.IsNullOrEmpty(packageExpiry) ? (object)DBNull.Value : packageExpiry);
                                cmd.Parameters.AddWithValue("@syncStatus", syncStatusJson);
                                cmd.Parameters.AddWithValue("@updatedAt", timestamp);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert with explicit identity value: enable IDENTITY_INSERT for this session
                            using (var setOn = new SqlCommand("SET IDENTITY_INSERT dbo.Members ON;", conn, transaction))
                            {
                                setOn.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(@"
INSERT INTO Members (MemberID, ClientId, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, PackageExpiry, SyncStatus, CreatedAt, UpdatedAt)
VALUES (@id, @clientId, @name, @status, @rfid, @picture, @fingerprint, @enrollment_id, @package_expiry, @syncStatus, @createdAt, @updatedAt);", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", memberId);
                                cmd.Parameters.AddWithValue("@clientId", Guid.NewGuid().ToString());
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.Parameters.AddWithValue("@status", status ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@rfid", string.IsNullOrEmpty(rfid) ? (object)DBNull.Value : rfid);
                                cmd.Parameters.AddWithValue("@picture", string.IsNullOrEmpty(picture) ? (object)DBNull.Value : picture);
                                cmd.Parameters.AddWithValue("@fingerprint", fingerprint);
                                cmd.Parameters.AddWithValue("@enrollment_id", enrollmentId ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@package_expiry", string.IsNullOrEmpty(packageExpiry) ? (object)DBNull.Value : packageExpiry);
                                cmd.Parameters.AddWithValue("@syncStatus", syncStatusJson);
                                cmd.Parameters.AddWithValue("@createdAt", timestamp);
                                cmd.Parameters.AddWithValue("@updatedAt", timestamp);

                                cmd.ExecuteNonQuery();
                            }

                            using (var setOff = new SqlCommand("SET IDENTITY_INSERT dbo.Members OFF;", conn, transaction))
                            {
                                setOff.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { /* ignore */ }
                        LogHelper.Log($"Error upserting member {memberId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public void UpsertPackage(JObject package)
        {
            int packageId = (int)package["id"];
            string title = package["title"]?.ToString();
            string code = package["code"]?.ToString();
            string description = package["description"]?.ToString();
            string pricingTierJson = package["pricing_tier"] != null ? package["pricing_tier"].ToString(Formatting.None) : null;
            decimal? registrationFee = package["registration_fee"]?.ToObject<decimal?>();
            string registrationFeeBasis = package["registration_fee_basis"]?.ToString();
            string groupMode = package["group_mode"]?.ToString();
            bool active = package["active"]?.ToObject<bool>() ?? false;
            DateTime? remoteUpdatedAt = package["updated_at"]?.ToObject<DateTime?>();
            string timestamp = GetTimestamp();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        bool exists;
                        using (var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Packages WHERE PackageId = @id", conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@id", packageId);
                            exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        if (exists)
                        {
                            using (var cmd = new SqlCommand(@"
UPDATE Packages
SET Title = @title,
    Code = @code,
    Description = @description,
    PricingTier = @pricingTier,
    RegistrationFee = @registrationFee,
    RegistrationFeeBasis = @registrationFeeBasis,
    GroupMode = @groupMode,
    Active = @active,
    RemoteUpdatedAt = @remoteUpdatedAt,
    UpdatedAt = @updatedAt
WHERE PackageId = @id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", packageId);
                                cmd.Parameters.AddWithValue("@title", (object)title ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@code", (object)code ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@description", (object)description ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@pricingTier", (object)pricingTierJson ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@registrationFee", (object)registrationFee ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@registrationFeeBasis", (object)registrationFeeBasis ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@groupMode", (object)groupMode ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@active", active);
                                cmd.Parameters.AddWithValue("@remoteUpdatedAt", (object)remoteUpdatedAt ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (var cmd = new SqlCommand(@"
INSERT INTO Packages (PackageId, Title, Code, Description, PricingTier, RegistrationFee, RegistrationFeeBasis, GroupMode, Active, RemoteUpdatedAt, CreatedAt, UpdatedAt)
VALUES (@id, @title, @code, @description, @pricingTier, @registrationFee, @registrationFeeBasis, @groupMode, @active, @remoteUpdatedAt, @createdAt, @updatedAt);", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", packageId);
                                cmd.Parameters.AddWithValue("@title", (object)title ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@code", (object)code ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@description", (object)description ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@pricingTier", (object)pricingTierJson ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@registrationFee", (object)registrationFee ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@registrationFeeBasis", (object)registrationFeeBasis ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@groupMode", (object)groupMode ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@active", active);
                                cmd.Parameters.AddWithValue("@remoteUpdatedAt", (object)remoteUpdatedAt ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@createdAt", timestamp);
                                cmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { /* ignore */ }
                        LogHelper.Log($"Error upserting package {packageId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public void UpsertTrainer(JObject trainer)
        {
            string trainerId = trainer["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(trainerId)) return;

            string firstName = trainer["first_name"]?.ToString();
            string lastName = trainer["last_name"]?.ToString();
            string personalInfoJson = trainer["personal_information"] != null ? trainer["personal_information"].ToString(Formatting.None) : null;
            DateTime? remoteUpdatedAt = trainer["updated_at"]?.ToObject<DateTime?>();
            string timestamp = GetTimestamp();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        bool exists;
                        using (var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Trainers WHERE TrainerId = @id", conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@id", trainerId);
                            exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        if (exists)
                        {
                            using (var cmd = new SqlCommand(@"
UPDATE Trainers
SET FirstName = @firstName,
    LastName = @lastName,
    PersonalInformation = @personalInfo,
    RemoteUpdatedAt = @remoteUpdatedAt,
    UpdatedAt = @updatedAt
WHERE TrainerId = @id", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", trainerId);
                                cmd.Parameters.AddWithValue("@firstName", (object)firstName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@lastName", (object)lastName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@personalInfo", (object)personalInfoJson ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@remoteUpdatedAt", (object)remoteUpdatedAt ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (var cmd = new SqlCommand(@"
INSERT INTO Trainers (TrainerId, FirstName, LastName, PersonalInformation, RemoteUpdatedAt, CreatedAt, UpdatedAt)
VALUES (@id, @firstName, @lastName, @personalInfo, @remoteUpdatedAt, @createdAt, @updatedAt);", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", trainerId);
                                cmd.Parameters.AddWithValue("@firstName", (object)firstName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@lastName", (object)lastName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@personalInfo", (object)personalInfoJson ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@remoteUpdatedAt", (object)remoteUpdatedAt ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@createdAt", timestamp);
                                cmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { /* ignore */ }
                        LogHelper.Log($"Error upserting trainer {trainerId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Returns active packages for populating enrollment/subscription pickers.
        /// </summary>
        public List<Package> GetPackages(bool activeOnly = true)
        {
            var packages = new List<Package>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT PackageId, Title, Code, Description, PricingTier, RegistrationFee, RegistrationFeeBasis, GroupMode, Active
                    FROM Packages" + (activeOnly ? " WHERE Active = 1" : "") + @"
                    ORDER BY Title";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        packages.Add(new Package
                        {
                            PackageId = reader.GetInt32(0),
                            Title = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Code = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            PricingTierJson = reader.IsDBNull(4) ? null : reader.GetString(4),
                            RegistrationFee = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                            RegistrationFeeBasis = reader.IsDBNull(6) ? null : reader.GetString(6),
                            GroupMode = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Active = !reader.IsDBNull(8) && reader.GetBoolean(8)
                        });
                    }
                }
            }
            return packages;
        }

        public List<Trainer> GetTrainers()
        {
            var trainers = new List<Trainer>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "SELECT TrainerId, FirstName, LastName FROM Trainers ORDER BY FirstName, LastName", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trainers.Add(new Trainer
                        {
                            TrainerId = reader.GetString(0),
                            FirstName = reader.IsDBNull(1) ? null : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return trainers;
        }

        /// <summary>
        /// Creates a locally-enrolled ("offline") member with a subscription snapshot.
        /// Assigns a ClientId (GUID) and a decrementing EnrollmentID starting at 999998,
        /// keeping a private numbering block clear of real server-assigned EnrollmentIDs.
        /// </summary>
        public OfflineMember InsertOfflineMember(
            string firstName,
            string lastName,
            string gender,
            string phone,
            JObject subscription,
            string picture = null,
            string rfid = null,
            bool fingerprint = false)
        {
            string clientId = Guid.NewGuid().ToString();
            string name = $"{firstName} {lastName}".Trim();
            string subscriptionJson = subscription != null ? subscription.ToString(Formatting.None) : null;
            string timestamp = GetTimestamp();

            var devices = LoadDevices();
            var syncStatus = new Dictionary<int, bool>();
            foreach (var device in devices)
                syncStatus[device.DeviceId] = false;
            string syncStatusJson = JsonConvert.SerializeObject(syncStatus);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int nextEnrollmentId;
                        using (var cmd = new SqlCommand(
                            "SELECT MIN(EnrollmentID) FROM OfflineMembers WITH (UPDLOCK, HOLDLOCK)", connection, transaction))
                        {
                            var result = cmd.ExecuteScalar();
                            nextEnrollmentId = (result == null || result == DBNull.Value) ? 999998 : Convert.ToInt32(result) - 1;
                        }

                        using (var cmd = new SqlCommand(@"
INSERT INTO OfflineMembers (ClientId, Name, FirstName, LastName, Gender, Phone, Subscription, SyncStatus, Picture, RFID, Fingerprint, EnrollmentID, ServerSync, CreatedAt, UpdatedAt)
VALUES (@clientId, @name, @firstName, @lastName, @gender, @phone, @subscription, @syncStatus, @picture, @rfid, @fingerprint, @enrollmentId, @serverSync, @createdAt, @updatedAt);", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@firstName", (object)firstName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@lastName", (object)lastName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@gender", (object)gender ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@phone", (object)phone ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@subscription", (object)subscriptionJson ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@syncStatus", syncStatusJson);
                            cmd.Parameters.AddWithValue("@picture", string.IsNullOrEmpty(picture) ? (object)DBNull.Value : picture);
                            cmd.Parameters.AddWithValue("@rfid", string.IsNullOrEmpty(rfid) ? (object)DBNull.Value : rfid);
                            cmd.Parameters.AddWithValue("@fingerprint", fingerprint);
                            cmd.Parameters.AddWithValue("@enrollmentId", nextEnrollmentId);
                            cmd.Parameters.AddWithValue("@serverSync", false);
                            cmd.Parameters.AddWithValue("@createdAt", timestamp);
                            cmd.Parameters.AddWithValue("@updatedAt", timestamp);
                            cmd.ExecuteNonQuery();
                        }

                        // One OfflineMemberLog row per (member) device — same shape/purpose as dbo.Log
                        // for real Members. Staff devices are intentionally excluded.
                        var logBiometricData = new Dictionary<string, string>();
                        var logSyncStatus = new Dictionary<string, bool>();
                        if (!string.IsNullOrWhiteSpace(rfid))
                        {
                            logBiometricData["RFID"] = rfid;
                            logSyncStatus["RFID"] = false;
                        }
                        if (fingerprint)
                        {
                            logBiometricData["Fingerprint"] = nextEnrollmentId.ToString();
                            logSyncStatus["Fingerprint"] = false;
                        }
                        string logBiometricJson = JsonConvert.SerializeObject(logBiometricData);
                        string logSyncJson = JsonConvert.SerializeObject(logSyncStatus);

                        foreach (var device in devices)
                        {
                            using (var logCmd = new SqlCommand(@"
INSERT INTO OfflineMemberLog (ClientId, DeviceID, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt)
VALUES (@clientId, @deviceId, @enrollmentId, @biometricData, @syncStatus, @createdAt, @updatedAt);", connection, transaction))
                            {
                                logCmd.Parameters.AddWithValue("@clientId", clientId);
                                logCmd.Parameters.AddWithValue("@deviceId", device.DeviceId);
                                logCmd.Parameters.AddWithValue("@enrollmentId", nextEnrollmentId);
                                logCmd.Parameters.AddWithValue("@biometricData", logBiometricJson);
                                logCmd.Parameters.AddWithValue("@syncStatus", logSyncJson);
                                logCmd.Parameters.AddWithValue("@createdAt", timestamp);
                                logCmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                logCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return new OfflineMember
                        {
                            ClientId = clientId,
                            Name = name,
                            FirstName = firstName,
                            LastName = lastName,
                            Gender = gender,
                            Phone = phone,
                            SubscriptionJson = subscriptionJson,
                            SyncStatus = syncStatus,
                            Picture = picture,
                            RFID = rfid,
                            Fingerprint = fingerprint,
                            EnrollmentID = nextEnrollmentId,
                            ServerSync = false
                        };
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { /* ignore */ }
                        LogHelper.Log($"Error inserting offline member: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the Picture path for an offline member after the enrollment-numbered
        /// face image file has been written to disk (the file is named after EnrollmentID,
        /// which is only known once the row has been inserted).
        /// </summary>
        public void UpdateOfflineMemberPicture(string clientId, string picturePath)
        {
            string timestamp = GetTimestamp();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "UPDATE OfflineMembers SET Picture = @picture, UpdatedAt = @updatedAt WHERE ClientId = @clientId", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            cmd.Parameters.AddWithValue("@picture", (object)picturePath ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@updatedAt", timestamp);
                            cmd.ExecuteNonQuery();
                        }

                        // Patch "Face" into every device's OfflineMemberLog row for this member —
                        // the photo is only known after InsertOfflineMember already created these rows.
                        var rows = new List<(int Id, string BiometricData, string SyncStatus)>();
                        using (var selectCmd = new SqlCommand(
                            "SELECT Id, BiometricData, SyncStatus FROM OfflineMemberLog WHERE ClientId = @clientId", connection, transaction))
                        {
                            selectCmd.Parameters.AddWithValue("@clientId", clientId);
                            using (var reader = selectCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    rows.Add((
                                        reader.GetInt32(0),
                                        reader.IsDBNull(1) ? null : reader.GetString(1),
                                        reader.IsDBNull(2) ? null : reader.GetString(2)));
                                }
                            }
                        }

                        foreach (var row in rows)
                        {
                            var biometricData = !string.IsNullOrWhiteSpace(row.BiometricData)
                                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(row.BiometricData) ?? new Dictionary<string, string>()
                                : new Dictionary<string, string>();
                            var syncStatus = !string.IsNullOrWhiteSpace(row.SyncStatus)
                                ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(row.SyncStatus) ?? new Dictionary<string, bool>()
                                : new Dictionary<string, bool>();

                            biometricData["Face"] = picturePath;
                            syncStatus["Face"] = false;

                            using (var updateLogCmd = new SqlCommand(
                                "UPDATE OfflineMemberLog SET BiometricData = @biometricData, SyncStatus = @syncStatus, UpdatedAt = @updatedAt WHERE Id = @id",
                                connection, transaction))
                            {
                                updateLogCmd.Parameters.AddWithValue("@id", row.Id);
                                updateLogCmd.Parameters.AddWithValue("@biometricData", JsonConvert.SerializeObject(biometricData));
                                updateLogCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                                updateLogCmd.Parameters.AddWithValue("@updatedAt", timestamp);
                                updateLogCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { /* ignore */ }
                        LogHelper.Log($"Error updating offline member picture for {clientId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a member record from the local SQL Server DB based on MemberID.
        /// </summary>
        public void DeleteMemberById(int memberId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand("DELETE FROM Members WHERE MemberID = @id", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", memberId);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        Console.WriteLine($"🗑 Member {memberId} deleted from local DB.");
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error deleting member: {ex.Message}");
                        throw;
                    }
                }
            }
        }


        public (int LogId, string AttendanceId) InsertAttendanceLog(int memberId, string checkInValue, int? deviceId = null)
        {
            string syncStatusJson = JsonConvert.SerializeObject(new { checkIn = false, checkOut = false });
            string deviceInfoJson = JsonConvert.SerializeObject(new { checkIn = deviceId, checkOut = (int?)null });
            string timestamp = GetTimestamp();
            int newLogId = 0;
            string attendanceId = Guid.NewGuid().ToString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check if the provided check-in time is later than the member's last check-out time
                        if (DateTime.TryParse(checkInValue, out DateTime checkInDateTime))
                        {
                            DateTime lastCheckOutTime = GetLastCheckOutTime(memberId);
                            if (checkInDateTime <= lastCheckOutTime)
                            {
                                LogHelper.Log($"❌ Check-in time {checkInValue} is not later than the member's last check-out time {lastCheckOutTime}.");
                                transaction.Rollback();
                                return (0, null);
                            }
                        }

                        using (var command = new SqlCommand(
                            @"INSERT INTO AttendanceLogs
                      (MemberID, AttendanceId, CheckInTime, CheckOutTime, SyncStatus, DeviceInfo, CreatedAt, UpdatedAt)
                      VALUES (@memberId, @attendanceId, @checkIn, NULL, @syncStatus, @deviceInfo, @createdAt, @updatedAt);
                      SELECT SCOPE_IDENTITY();", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@memberId", memberId);
                            command.Parameters.AddWithValue("@attendanceId", attendanceId);
                            command.Parameters.AddWithValue("@checkIn", (object)checkInValue ?? DBNull.Value);
                            command.Parameters.AddWithValue("@syncStatus", syncStatusJson);
                            command.Parameters.AddWithValue("@deviceInfo", deviceInfoJson);
                            command.Parameters.AddWithValue("@createdAt", timestamp);
                            command.Parameters.AddWithValue("@updatedAt", timestamp);

                            var result = command.ExecuteScalar();
                            newLogId = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error inserting attendance log: {ex.Message}");
                        return (0, null);
                    }
                }
            }
            // Return both so the caller doesn't have to query the DB again
            return (newLogId, attendanceId);
        }

        public void UpdateAttendanceLogForCheckOut(int memberId, string checkOutValue, int? deviceId = null)
        {
            string timestamp = GetTimestamp();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // First, validate the check-out time format
                        DateTime checkOutDateTime;
                        if (!DateTime.TryParse(checkOutValue, out checkOutDateTime))
                        {
                            transaction.Rollback();
                            return; // Invalid check-out time format
                        }

                        // Check if there's any check-in record for this member on the check-out date
                        bool hasCheckInForDate = HasAttendanceLogForDate(memberId, checkOutDateTime.Date, "check-in");

                        int logId = 0;
                        string existingJson = null;
                        string existingDeviceJson = null;
                        DateTime checkInTime = DateTime.MinValue;

                        if (hasCheckInForDate)
                        {
                            // Get latest open (no checkout) log for this member on the same date
                            using (var get = new SqlCommand(
                                @"SELECT LogID, CheckInTime, SyncStatus, DeviceInfo
                  FROM AttendanceLogs
                  WHERE MemberID = @memberId AND CheckOutTime IS NULL
                  AND CONVERT(DATE, CheckInTime) = @checkOutDate
                  ORDER BY CheckInTime DESC", connection, transaction))
                            {
                                get.Parameters.AddWithValue("@memberId", memberId);
                                get.Parameters.AddWithValue("@checkOutDate", checkOutDateTime.Date);
                                using (var reader = get.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        logId = reader.GetInt32(0);
                                        checkInTime = reader.GetDateTime(1);

                                        // Check that the check-out time is greater than the check-in time
                                        if (checkOutDateTime <= checkInTime)
                                        {
                                            LogHelper.Log($"❌ Check-out time {checkOutValue} is not greater than the check-in time {checkInTime} for same day.");
                                            transaction.Rollback();
                                            return; // Prevent invalid check-out
                                        }
                                        existingJson = reader.IsDBNull(2) ? null : reader.GetString(2);
                                        existingDeviceJson = reader.IsDBNull(3) ? null : reader.GetString(3);
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        return; // No open attendance for this date
                                    }
                                }
                            }
                        }
                        else
                        {
                            // No check-in for current date, look for previous day only
                            DateTime previousDay = checkOutDateTime.Date.AddDays(-1);

                            using (var get = new SqlCommand(
                                @"SELECT LogID, CheckInTime, SyncStatus, DeviceInfo
                  FROM AttendanceLogs
                  WHERE MemberID = @memberId AND CheckOutTime IS NULL
                  AND CONVERT(DATE, CheckInTime) = @previousDay
                  ORDER BY CheckInTime DESC", connection, transaction))
                            {
                                get.Parameters.AddWithValue("@memberId", memberId);
                                get.Parameters.AddWithValue("@previousDay", previousDay);
                                using (var reader = get.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        logId = reader.GetInt32(0);
                                        checkInTime = reader.GetDateTime(1);

                                        // Check that the check-out time is greater than the check-in time
                                        if (checkOutDateTime <= checkInTime)
                                        {
                                            LogHelper.Log($"❌ Check-out time {checkOutValue} is not greater than the check-in time {checkInTime} for previous day.");
                                            transaction.Rollback();
                                            return; // Prevent invalid check-out
                                        }
                                        existingJson = reader.IsDBNull(2) ? null : reader.GetString(2);
                                        existingDeviceJson = reader.IsDBNull(3) ? null : reader.GetString(3);
                                    }
                                    else
                                    {
                                        transaction.Rollback();
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

                        var deviceDict = !string.IsNullOrWhiteSpace(existingDeviceJson)
                            ? JsonConvert.DeserializeObject<Dictionary<string, int?>>(existingDeviceJson) ?? new Dictionary<string, int?>()
                            : new Dictionary<string, int?>();
                        if (!deviceDict.ContainsKey("checkIn")) deviceDict["checkIn"] = null;
                        deviceDict["checkOut"] = deviceId;

                        using (var update = new SqlCommand(
                            @"UPDATE AttendanceLogs
              SET CheckOutTime = @checkOut, SyncStatus = @syncStatus, DeviceInfo = @deviceInfo, UpdatedAt = @updatedAt
              WHERE LogID = @logId", connection, transaction))
                        {
                            update.Parameters.AddWithValue("@checkOut", (object)checkOutValue ?? DBNull.Value);
                            update.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncDict));
                            update.Parameters.AddWithValue("@deviceInfo", JsonConvert.SerializeObject(deviceDict));
                            update.Parameters.AddWithValue("@updatedAt", timestamp);
                            update.Parameters.AddWithValue("@logId", logId);
                            update.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating attendance log for check-out: {ex.Message}");
                    }
                }
            }
        }

        private DateTime GetLastCheckOutTime(int memberId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(
                            "SELECT MAX(CheckOutTime) FROM AttendanceLogs WHERE MemberID = @memberId", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@memberId", memberId);
                            object result = command.ExecuteScalar();
                            transaction.Commit();
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
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error getting last check-out time: {ex.Message}");
                        return DateTime.MinValue;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the sync status of an attendance log based on provided parameters.
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

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string existingJson = null;
                        int targetLogId = 0;
                        bool foundRecord = false;

                        // For check-in: require both attendanceId and checkInTime to match
                        if (hasCheckIn && !string.IsNullOrWhiteSpace(attendanceId) && checkInTime.HasValue)
                        {
                            using (var select = new SqlCommand(
                                "SELECT TOP 1 LogID, SyncStatus FROM AttendanceLogs WHERE AttendanceId = @aid AND MemberID = @mid AND CheckInTime = @checkInTime", connection, transaction))
                            {
                                select.Parameters.AddWithValue("@aid", attendanceId);
                                select.Parameters.AddWithValue("@mid", memberId);
                                select.Parameters.AddWithValue("@checkInTime", checkInTime.Value);
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
                            using (var select = new SqlCommand(
                                "SELECT TOP 1 LogID, SyncStatus FROM AttendanceLogs WHERE AttendanceId = @aid AND MemberID = @mid AND CheckOutTime = @checkOutTime", connection, transaction))
                            {
                                select.Parameters.AddWithValue("@aid", attendanceId);
                                select.Parameters.AddWithValue("@mid", memberId);
                                select.Parameters.AddWithValue("@checkOutTime", checkOutTime.Value);
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
                            using (var selectLatest = new SqlCommand(
                                @"SELECT TOP 1 LogID, SyncStatus 
                          FROM AttendanceLogs 
                          WHERE MemberID = @mid 
                          ORDER BY CheckInTime DESC", connection, transaction))
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
                            transaction.Rollback();
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
                                transaction.Rollback();
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
                                transaction.Rollback();
                                return false;
                            }
                        }

                        // Only update if there was an actual change
                        if (valueChanged)
                        {
                            using (var update = new SqlCommand(
                                "UPDATE AttendanceLogs SET SyncStatus = @json, UpdatedAt = @ts WHERE LogID = @lid", connection, transaction))
                            {
                                update.Parameters.AddWithValue("@json", JsonConvert.SerializeObject(flags));
                                update.Parameters.AddWithValue("@ts", GetTimestamp());
                                update.Parameters.AddWithValue("@lid", targetLogId);
                                int rowsUpdated = update.ExecuteNonQuery();

                                if (rowsUpdated > 0)
                                {
                                    LogHelper.Log($"Successfully updated {(hasCheckIn ? "check-in" : "check-out")} sync status for log {targetLogId}");
                                    transaction.Commit();
                                    return true;
                                }
                                else
                                {
                                    LogHelper.Log($"Failed to update {(hasCheckIn ? "check-in" : "check-out")} sync status for log {targetLogId}");
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }

                        transaction.Rollback();
                        return false;
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating attendance log sync status: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public string GetDeviceAttendanceAction(int deviceMachineNo)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT TOP 1 AttendanceType FROM Devices WHERE MachineNo = @m", connection))
                    {
                        cmd.Parameters.AddWithValue("@m", deviceMachineNo);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        return Convert.ToBoolean(result) ? "check-in" : "check-out";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting device attendance action: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Returns the decrypted DeviceIP for a given (plain-text) serial number.
        /// Searches both devices and staff_devices tables.
        /// Returns null if not found.
        /// </summary>
        public string GetDeviceIpBySn(string sn)
        {
            if (string.IsNullOrEmpty(sn)) return null;

            try
            {
                string encryptedSn = Encrypt(sn);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT DeviceIP FROM devices        WHERE DeviceSerial = @SN
                        UNION
                        SELECT DeviceIP FROM staff_devices  WHERE DeviceSerial = @SN";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@SN", encryptedSn);
                        var result = cmd.ExecuteScalar();

                        if (result == null || result == DBNull.Value) return null;
                        return Decrypt(result.ToString());
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting device ip by sn: {ex.Message}");
                return null;
            }
        }

        public int? GetMemberIdByEnrollment(int enrollmentId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT TOP 1 MemberID FROM Members WHERE EnrollmentID = @enroll", connection))
                    {
                        cmd.Parameters.AddWithValue("@enroll", enrollmentId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting member ID by enrollment: {ex.Message}");
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
            const int timeTolerance = 5;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    int? memberId = null;
                    using (var cmd = new SqlCommand("SELECT TOP 1 MemberID FROM Members WHERE EnrollmentID = @enrollId", connection))
                    {
                        cmd.Parameters.AddWithValue("@enrollId", enrollNumber);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value) memberId = Convert.ToInt32(result);
                    }

                    if (!memberId.HasValue)
                    {
                        LogHelper.Log($"No member found for enrollment ID: {enrollNumber}");
                        return false;
                    }

                    DateTime startTime = deviceTime.AddSeconds(-timeTolerance);
                    DateTime endTime = deviceTime.AddSeconds(timeTolerance);

                    using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM AttendanceLogs 
                WHERE MemberID = @memberId AND 
                ((CheckInTime BETWEEN @startTime AND @endTime) OR 
                 (CheckOutTime IS NOT NULL AND CheckOutTime BETWEEN @startTime AND @endTime))", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId.Value);
                        cmd.Parameters.AddWithValue("@startTime", startTime);
                        cmd.Parameters.AddWithValue("@endTime", endTime);
                        var countObj = await cmd.ExecuteScalarAsync();
                        long count = countObj == null || countObj == DBNull.Value ? 0L : Convert.ToInt64(countObj);
                        return count > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking record existence in attendance log: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Retrieves the ID of the most recent open attendance log for a member.
        /// An open log is one where CheckOutTime is NULL.
        /// </summary>
        /// <param name="memberId">The member ID to search for</param>
        /// <returns>The LogID of the open attendance log, or 0 if no open log exists</returns>
        public int GetOpenAttendanceLogId(int memberId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        @"SELECT TOP 1 LogID FROM AttendanceLogs 
                  WHERE MemberID = @memberId AND CheckOutTime IS NULL 
                  ORDER BY CheckInTime DESC", connection))
                    {
                        command.Parameters.AddWithValue("@memberId", memberId);
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                        return 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting open attendance log ID: {ex.Message}");
                return 0;
            }
        }

        public DateTime? GetOpenAttendanceLogCheckInTime(int memberId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
                SELECT TOP 1 CheckInTime
                FROM AttendanceLogs
                WHERE MemberID = @memberId
                  AND CheckOutTime IS NULL
                ORDER BY CheckInTime DESC;", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        DateTime parsed;
                        if (DateTime.TryParse(Convert.ToString(result), out parsed)) return parsed;
                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting open attendance log check-in time: {ex.Message}");
                return null;
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
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;

                    // We use a 1-second window (ABS(DATEDIFF)) to account for millisecond 
                    // precision differences between C# and SQL's DATETIME2.
                    if (checkIn.HasValue)
                    {
                        sql = @"SELECT TOP 1 AttendanceId FROM AttendanceLogs 
                        WHERE MemberID = @mid 
                        AND ABS(DATEDIFF(SECOND, CheckInTime, @checkIn)) <= 1 
                        ORDER BY ABS(DATEDIFF(MILLISECOND, CheckInTime, @checkIn)) ASC";
                    }
                    else if (checkOut.HasValue)
                    {
                        sql = @"SELECT TOP 1 AttendanceId FROM AttendanceLogs 
                        WHERE MemberID = @mid 
                        AND ABS(DATEDIFF(SECOND, CheckOutTime, @checkOut)) <= 1 
                        ORDER BY ABS(DATEDIFF(MILLISECOND, CheckOutTime, @checkOut)) ASC";
                    }
                    else
                    {
                        sql = @"SELECT TOP 1 AttendanceId FROM AttendanceLogs 
                        WHERE MemberID = @mid 
                        AND AttendanceId IS NOT NULL 
                        AND LTRIM(RTRIM(AttendanceId)) <> '' 
                        ORDER BY CheckInTime DESC";
                    }

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@mid", memberId);
                        if (checkIn.HasValue) cmd.Parameters.AddWithValue("@checkIn", checkIn.Value);
                        if (checkOut.HasValue) cmd.Parameters.AddWithValue("@checkOut", checkOut.Value);

                        var result = cmd.ExecuteScalar();
                        return result == null || result == DBNull.Value ? null : result.ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                LogHelper.Log($"SQL Error getting attendance ID for member {memberId}: {ex.Message}");
                return null;
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string query = @"
                SELECT TOP 1 CheckInTime
                FROM AttendanceLogs
                WHERE MemberID = @memberId
                  AND CheckOutTime = @checkOutTime";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@memberId", memberId);
                        command.Parameters.AddWithValue("@checkOutTime", checkOutTime);
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
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting paired check-in time: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error in GetPairedCheckInTime(memberId={memberId}, checkOut={checkOutTime}): {ex.Message}");
            }

            return null;
        }


        public bool UpdateAttendanceLogBulkMembers(
            int memberId,
            DateTime checkInTime,
            DateTime? checkOutTime,
            string attendanceId,
            bool setCheckInSynced,
            bool setCheckOutSynced)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Find the log row by member + exact check-in time
                            int logId = 0;
                            string existingSyncStatusJson = null;

                            using (var cmd = new SqlCommand(
                                @"SELECT TOP 1 LogID, SyncStatus 
                          FROM AttendanceLogs
                          WHERE MemberID = @memberId
                            AND CheckInTime = @checkIn", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@memberId", memberId);
                                cmd.Parameters.AddWithValue("@checkIn", checkInTime);

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
                                transaction.Rollback();
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

                            using (var update = new SqlCommand(
                                @"UPDATE AttendanceLogs
                          SET AttendanceId = @attendanceId,
                              SyncStatus   = @syncStatus,
                              UpdatedAt    = @updatedAt
                          WHERE LogID = @logId", connection, transaction))
                            {
                                update.Parameters.AddWithValue("@attendanceId", (object)attendanceId ?? DBNull.Value);
                                update.Parameters.AddWithValue("@syncStatus", newSyncJson);
                                update.Parameters.AddWithValue("@updatedAt", timestamp);
                                update.Parameters.AddWithValue("@logId", logId);

                                update.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error updating attendance log from cloud: {ex.Message}");
                            return false;
                        }
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

        public bool UpdateStaffAttendanceLogBulk(
    int enrollmentId,
    DateTime checkInTime,
    DateTime? checkOutTime,
    string attendanceId,
    bool setCheckInSynced,
    bool setCheckOutSynced)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Find the log row by enrollment + exact check-in time
                            int logId = 0;
                            string existingSyncStatusJson = null;

                            using (var cmd = new SqlCommand(
                                @"SELECT TOP 1 LogID, SyncStatus 
                          FROM StaffAttendanceLog
                          WHERE EnrollmentID = @enrollmentId
                            AND CheckInTime = @checkIn", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                                cmd.Parameters.AddWithValue("@checkIn", checkInTime);

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
                                LogHelper.Log($"⚠️ No local StaffAttendanceLog found for Enrollment={enrollmentId}, CheckIn={checkInTime:yyyy-MM-dd HH:mm:ss}");
                                transaction.Rollback();
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

                            using (var update = new SqlCommand(
                                @"UPDATE StaffAttendanceLog
                          SET AttendanceId = @attendanceId,
                              SyncStatus   = @syncStatus,
                              UpdatedAt    = @updatedAt
                          WHERE LogID = @logId", connection, transaction))
                            {
                                update.Parameters.AddWithValue("@attendanceId", (object)attendanceId ?? DBNull.Value);
                                update.Parameters.AddWithValue("@syncStatus", newSyncJson);
                                update.Parameters.AddWithValue("@updatedAt", timestamp);
                                update.Parameters.AddWithValue("@logId", logId);

                                update.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            LogHelper.Log($"Error updating staff attendance log from cloud: {ex.Message}");
                            return false;
                        }
                    }
                }

                LogHelper.Log($"✅ Updated StaffAttendanceLog from cloud for Enrollment={enrollmentId}, CheckIn={checkInTime:yyyy-MM-dd HH:mm:ss}, AttendanceId={attendanceId}");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error updating StaffAttendanceLog from cloud for Enrollment={enrollmentId}: {ex.Message}");
                return false;
            }
        }


        public async Task<List<UnsyncedAttendanceLog>> CheckUnsyncedStaffAttendanceLogsAsync()
        {
            var list = new List<UnsyncedAttendanceLog>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    string query = @"
                        SELECT LogID, EnrollmentID, AttendanceId, CheckInTime, CheckOutTime, SyncStatus, DeviceInfo
                        FROM StaffAttendanceLog
                        WHERE SyncStatus LIKE '%""checkIn"":false%'
                           OR SyncStatus LIKE '%""checkOut"":false%'";

                    using (var cmd = new SqlCommand(query, connection))
                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            string syncStatusJson = reader.IsDBNull(5) ? null : reader.GetString(5);
                            var syncDict = !string.IsNullOrWhiteSpace(syncStatusJson)
                                ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(syncStatusJson)
                                : new Dictionary<string, bool>();

                            bool needsCheckIn = syncDict.ContainsKey("checkIn") && !syncDict["checkIn"];
                            bool needsCheckOut = syncDict.ContainsKey("checkOut") && !syncDict["checkOut"];

                            string deviceInfoJson = reader.IsDBNull(6) ? null : reader.GetString(6);
                            Dictionary<string, int?> deviceInfo;
                            try
                            {
                                deviceInfo = !string.IsNullOrWhiteSpace(deviceInfoJson)
                                    ? JsonConvert.DeserializeObject<Dictionary<string, int?>>(deviceInfoJson) ?? new Dictionary<string, int?>()
                                    : new Dictionary<string, int?>();
                            }
                            catch
                            {
                                deviceInfo = new Dictionary<string, int?>();
                            }

                            if (needsCheckIn || needsCheckOut)
                            {
                                list.Add(new UnsyncedAttendanceLog
                                {
                                    LogId = reader.GetInt32(0),
                                    EnrollmentId = reader.GetInt32(1),
                                    AttendanceId = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    CheckInTime = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                                    CheckOutTime = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                                    NeedsCheckInSync = needsCheckIn,
                                    NeedsCheckOutSync = needsCheckOut,
                                    CheckInDeviceId = deviceInfo.ContainsKey("checkIn") ? deviceInfo["checkIn"] : null,
                                    CheckOutDeviceId = deviceInfo.ContainsKey("checkOut") ? deviceInfo["checkOut"] : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error checking unsynced staff logs: {ex.Message}");
            }
            return list;
        }



        public DateTime? GetLastCheckOutTimeForMember(int memberId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
                SELECT TOP 1 CheckOutTime
                FROM AttendanceLogs
                WHERE MemberID = @memberId
                  AND CheckOutTime IS NOT NULL
                ORDER BY CheckOutTime DESC;", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        DateTime parsed;
                        if (DateTime.TryParse(Convert.ToString(result), out parsed)) return parsed;
                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting last check-out time for member: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB][GetLastCheckOutTimeForMember][ERROR] {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes completed and synced attendance logs for both members and staff that are older than the specified number of days.
        /// </summary>
        /// <summary>
        /// Deletes attendance logs that are fully synced (check-in and check-out) 
        /// and older than the specified retention days.
        /// Covers both AttendanceLogs (Members) and StaffAttendanceLog (Staff).
        /// </summary>
        public int DeleteOldSyncedAttendanceLogs(int daysOld)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete from Member records
                            string memberSql = @"
                                DELETE FROM AttendanceLogs 
                                WHERE CheckInTime IS NOT NULL 
                                  AND CheckOutTime IS NOT NULL 
                                  AND SyncStatus LIKE '%""checkIn"":true%' 
                                  AND SyncStatus LIKE '%""checkOut"":true%'
                                  AND CheckInTime < DATEADD(DAY, -@days, GETDATE())";

                            // Delete from Staff records
                            string staffSql = @"
                                DELETE FROM StaffAttendanceLog 
                                WHERE CheckInTime IS NOT NULL 
                                  AND CheckOutTime IS NOT NULL 
                                  AND SyncStatus LIKE '%""checkIn"":true%' 
                                  AND SyncStatus LIKE '%""checkOut"":true%'
                                  AND CheckInTime < DATEADD(DAY, -@days, GETDATE())";

                            int totalDeleted = 0;

                            using (var cmd = new SqlCommand(memberSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@days", daysOld);
                                totalDeleted += cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(staffSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@days", daysOld);
                                totalDeleted += cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return totalDeleted;
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            LogHelper.Log($"❌ SQL Error during attendance log cleanup: {ex.Message}");
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Critical error in DeleteOldSyncedAttendanceLogs: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Identifies members whose package expired more than the specified months threshold.
        /// </summary>
        public List<Member> GetExpiredMembersForCleanup(int monthsThreshold = 2)
        {
            var expiredMembers = new List<Member>();
            DateTime thresholdDate = DateTime.Today.AddMonths(-monthsThreshold);

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT MemberID, EnrollmentID, Name, PackageExpiry 
                        FROM Members 
                        WHERE PackageExpiry IS NOT NULL AND PackageExpiry <> ''", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string expiryStr = reader["PackageExpiry"].ToString();
                                if (DateTime.TryParse(expiryStr, out DateTime expiryDate))
                                {
                                    if (expiryDate < thresholdDate)
                                    {
                                        expiredMembers.Add(new Member
                                        {
                                            MemberID = Convert.ToInt32(reader["MemberID"]),
                                            EnrollmentID = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : (int?)null,
                                            Name = reader["Name"].ToString(),
                                            PackageExpiry = expiryStr
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Cleanup] Error fetching expired members: {ex.Message}");
            }

            return expiredMembers;
        }

        /// <summary>
        /// Deletes all database records (Log, AttendanceLogs, and Members) for a specific member ID.
        /// </summary>
        public bool DeleteExpiredMemberFromDb(int memberId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Delete from Log table
                            using (var cmd = new SqlCommand("DELETE FROM Log WHERE MemberID = @mid", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@mid", memberId);
                                cmd.ExecuteNonQuery();
                            }

                            // 2. Delete from AttendanceLogs table
                            using (var cmd = new SqlCommand("DELETE FROM AttendanceLogs WHERE MemberID = @mid", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@mid", memberId);
                                cmd.ExecuteNonQuery();
                            }

                            // 3. Delete from Members table
                            using (var cmd = new SqlCommand("DELETE FROM Members WHERE MemberID = @mid", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@mid", memberId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogHelper.Log($"[DbCleanup] Failed for member {memberId}: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DbCleanup] Critical error for member {memberId}: {ex.Message}");
                return false;
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql;
                    if (action == "check-in")
                    {
                        sql = @"SELECT COUNT(*) FROM AttendanceLogs 
                        WHERE MemberID = @memberId 
                        AND CONVERT(DATE, CheckInTime) = @date
                        AND CheckInTime IS NOT NULL";
                    }
                    else if (action == "check-out")
                    {
                        sql = @"SELECT COUNT(*) FROM AttendanceLogs 
                        WHERE MemberID = @memberId 
                        AND CONVERT(DATE, CheckInTime) = @date
                        AND CheckOutTime IS NOT NULL";
                    }
                    else
                    {
                        sql = @"SELECT COUNT(*) FROM AttendanceLogs 
                        WHERE MemberID = @memberId 
                        AND CONVERT(DATE, CheckInTime) = @date";
                    }

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@memberId", memberId);
                        command.Parameters.AddWithValue("@date", date.Date);
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking attendance log for date: {ex.Message}");
                return false;
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

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string sql = @"
                SELECT COUNT(*) FROM AttendanceLogs
                WHERE MemberID = @mid
                  AND CheckInTime IS NOT NULL
                  AND (CheckOutTime IS NULL OR LTRIM(RTRIM(CheckOutTime)) = '')
                  AND (
                        CONVERT(DATE, CheckInTime) = CONVERT(DATE, @dt)
                     OR CONVERT(DATE, CheckInTime) = CONVERT(DATE, DATEADD(DAY, -1, @dt))
                      );";

                    int openCount;
                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@mid", memberId);
                        cmd.Parameters.AddWithValue("@dt", deviceTime);
                        openCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (action == "check-in") return openCount == 0;
                    if (action == "check-out") return openCount > 0;
                    return false;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error processing attendance: {ex.Message}");
                return false;
            }
        }

        public async Task ForceFaceSyncStatusFalseAsync(int memberId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get all log entries for the member with Face biometric
                    var logsToUpdate = new List<(int logId, string syncStatusJson)>();

                    using (var cmd = new SqlCommand("SELECT Id, SyncStatus FROM Log WHERE MemberID = @memberId", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int logId = reader.GetInt32(0);
                                string syncStatusJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                                logsToUpdate.Add((logId, syncStatusJson));
                            }
                        }
                    } // DataReader is closed here

                    // Now update the logs after the reader is closed
                    if (logsToUpdate.Count > 0)
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var (logId, syncStatusJson) in logsToUpdate)
                                {
                                    var syncStatus = string.IsNullOrEmpty(syncStatusJson)
                                        ? new Dictionary<string, bool>()
                                        : JsonConvert.DeserializeObject<Dictionary<string, bool>>(syncStatusJson)
                                            ?? new Dictionary<string, bool>();

                                    // Set Face sync to false
                                    syncStatus["Face"] = false;

                                    using (var updateCmd = new SqlCommand("UPDATE Log SET SyncStatus = @syncStatus WHERE Id = @logId", connection, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                                        updateCmd.Parameters.AddWithValue("@logId", logId);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                                transaction.Commit();
                                LogHelper.Log($"✅ Set Face sync status to false for member {memberId} in {logsToUpdate.Count} log entries.");
                            }
                            catch (SqlException ex)
                            {
                                transaction.Rollback();
                                LogHelper.Log($"❌ Error updating Face sync status: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        LogHelper.Log($"ℹ️ No log entries found for member {memberId}.");
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
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    // Joined with Members to retrieve the EnrollmentID
                    string query = @"
                SELECT TOP 2000
                    al.AttendanceId,
                    al.MemberID,
                    al.CheckInTime,
                    al.CheckOutTime,
                    al.SyncStatus,
                    m.EnrollmentID,
                    al.DeviceInfo
                FROM
                    AttendanceLogs al
                INNER JOIN Members m ON al.MemberID = m.MemberID
                WHERE
                    al.SyncStatus LIKE '%false%'
                ORDER BY al.CheckInTime ASC";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string attendanceId = reader.IsDBNull(0) ? null : reader.GetString(0);
                            int memberId = reader.GetInt32(1);

                            DateTime checkInTime;
                            if (!DateTime.TryParse(reader.GetValue(2).ToString(), out checkInTime))
                                continue;

                            DateTime? checkOutTime = null;
                            if (!reader.IsDBNull(3)) checkOutTime = reader.GetDateTime(3);

                            string syncStatusJson = reader.IsDBNull(4) ? "{}" : reader.GetString(4);

                            // Retrieve EnrollmentID from the join results
                            int enrollmentId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                            string deviceInfoJson = reader.IsDBNull(6) ? null : reader.GetString(6);

                            Dictionary<string, bool> syncStatus;
                            try
                            {
                                syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(syncStatusJson) ?? new Dictionary<string, bool>();
                            }
                            catch
                            {
                                syncStatus = new Dictionary<string, bool>();
                            }

                            Dictionary<string, int?> deviceInfo;
                            try
                            {
                                deviceInfo = !string.IsNullOrWhiteSpace(deviceInfoJson)
                                    ? JsonConvert.DeserializeObject<Dictionary<string, int?>>(deviceInfoJson) ?? new Dictionary<string, int?>()
                                    : new Dictionary<string, int?>();
                            }
                            catch
                            {
                                deviceInfo = new Dictionary<string, int?>();
                            }

                            bool needsCheckInSync = syncStatus.ContainsKey("checkIn") && !syncStatus["checkIn"];
                            bool needsCheckOutSync = syncStatus.ContainsKey("checkOut") && !syncStatus["checkOut"] && checkOutTime.HasValue;

                            if (needsCheckInSync || needsCheckOutSync)
                            {
                                unsyncedLogs.Add(new UnsyncedAttendanceLog
                                {
                                    AttendanceId = attendanceId,
                                    MemberId = memberId,
                                    EnrollmentId = enrollmentId,
                                    CheckInTime = checkInTime,
                                    CheckOutTime = checkOutTime ?? DateTime.MinValue,
                                    NeedsCheckInSync = needsCheckInSync,
                                    NeedsCheckOutSync = needsCheckOutSync,
                                    CheckInDeviceId = deviceInfo.ContainsKey("checkIn") ? deviceInfo["checkIn"] : null,
                                    CheckOutDeviceId = deviceInfo.ContainsKey("checkOut") ? deviceInfo["checkOut"] : null
                                });
                            }
                        }
                    }
                }

                unsyncedLogs = unsyncedLogs
                    .OrderBy(u => u.CheckInTime)
                    .ThenBy(u => u.AttendanceId)
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
                    SELECT TOP 10000
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
                                CheckInTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss"),
                                CheckOutTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"),
                                CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting all attendance logs: {ex.Message}");
            }
            return logs;
        }


        //Abdul Hadi Code
        public void SaveDevice(Device device)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string timestamp = GetTimestamp();

                        // Encrypt sensitive fields
                        string encryptedSerial = Encrypt(device.DeviceSerial);
                        string encryptedIP = Encrypt(device.DeviceIP);

                        using (var command = new SqlCommand(@"
INSERT INTO Devices 
(Department, AttendanceType, DeviceSerial, DeviceIP, Port, TimeZone, Location, ConnectionType, IsActive, LastSyncTime, FaceCapacity, RFIDCapacity, FingerCapacity, MachineNo, CreatedAt, UpdatedAt, Password) 
VALUES 
(@Department, @AttendanceType, @DeviceSerial, @DeviceIP, @Port, @TimeZone, @Location, @ConnectionType, @IsActive, @LastSyncTime, @FaceCapacity, @RFIDCapacity, @FingerCapacity, @MachineNo, @CreatedAt, @UpdatedAt, @Password)", connection, transaction))
                        {
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error saving device: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        //Abdul Hadi Code
        public void UpdateDevice(Device device)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string timestamp = GetTimestamp();
                            // Encrypt sensitive fields
                            string encryptedSerial = Encrypt(device.DeviceSerial);
                            string encryptedIP = Encrypt(device.DeviceIP);

                            using (var command = new SqlCommand(@"
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
WHERE DeviceId = @DeviceId", connection, transaction))
                            {
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
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error updating device: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
                SELECT DeviceIP, MachineNo
                FROM Devices
                WHERE DeviceIP = @DeviceIP OR MachineNo = @MachineNo;", connection))
                    {
                        command.Parameters.AddWithValue("@DeviceIP", Encrypt(deviceIP));
                        command.Parameters.AddWithValue("@MachineNo", machineNo);

                        bool ipExists = false, machineExists = false;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var encryptedIp = reader["DeviceIP"] != DBNull.Value ? reader["DeviceIP"].ToString() : null;
                                if (!string.IsNullOrEmpty(encryptedIp) && Decrypt(encryptedIp) == deviceIP) ipExists = true;
                                if (reader["MachineNo"] != DBNull.Value && Convert.ToInt32(reader["MachineNo"]) == machineNo) machineExists = true;
                            }
                        }

                        if (ipExists && machineExists) return "Both Device IP and Machine No exist.";
                        if (ipExists) return "Device IP exists.";
                        if (machineExists) return "Machine No exists.";
                        return "Neither IP nor Machine No exists.";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking device exists: {ex.Message}");
                return "Error occurred.";
            }
        }

        //Abdul Hadi Code
        // Check if a device with the same IP or Machine No already exists, excluding the current deviceId
        // Updated version - Port uniqueness check removed
        public string IDCheckDeviceExists(string deviceIP, int machineNo, int? deviceId = null)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
                SELECT DeviceId, DeviceIP, MachineNo
                FROM Devices
                WHERE (DeviceIP = @DeviceIP OR MachineNo = @MachineNo)
                AND (@DeviceId IS NULL OR DeviceId != @DeviceId);", connection))
                    {
                        command.Parameters.AddWithValue("@DeviceIP", Encrypt(deviceIP));
                        command.Parameters.AddWithValue("@MachineNo", machineNo);
                        command.Parameters.AddWithValue("@DeviceId", deviceId.HasValue ? (object)deviceId.Value : DBNull.Value);

                        bool ipExists = false, machineExists = false;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var existingIPEncrypted = reader["DeviceIP"] != DBNull.Value ? reader["DeviceIP"].ToString() : null;
                                if (!string.IsNullOrEmpty(existingIPEncrypted) && Decrypt(existingIPEncrypted) == deviceIP) ipExists = true;
                                if (reader["MachineNo"] != DBNull.Value && Convert.ToInt32(reader["MachineNo"]) == machineNo) machineExists = true;
                            }
                        }

                        if (ipExists && machineExists) return "Device IP and Machine No exist.";
                        if (ipExists) return "Device IP exists.";
                        if (machineExists) return "Machine No exists.";
                        return "";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking device exists by ID: {ex.Message}");
                return "Error occurred.";
            }
        }

        public void DeleteDevice(int deviceId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("DELETE FROM Devices WHERE DeviceId = @DeviceId", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@DeviceId", deviceId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error deleting device: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        //Abdul Hadi Code
        public List<Device> LoadDevices()
        {
            var devices = new List<Device>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT * FROM Devices", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            devices.Add(new Device
                            {
                                DeviceId = Convert.ToInt32(reader["DeviceId"]),
                                Department = reader["Department"] as string,
                                AttendanceType = reader["AttendanceType"] != DBNull.Value && Convert.ToBoolean(reader["AttendanceType"]),
                                DeviceSerial = reader["DeviceSerial"] == DBNull.Value ? null : Decrypt(reader["DeviceSerial"].ToString()),
                                DeviceIP = reader["DeviceIP"] == DBNull.Value ? null : Decrypt(reader["DeviceIP"].ToString()),
                                Port = reader["Port"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Port"]),
                                TimeZone = reader["TimeZone"] as string,
                                Location = reader["Location"] as string,
                                ConnectionType = reader["ConnectionType"] as string,
                                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                                LastSyncTime = reader["LastSyncTime"] as string,
                                FaceCapacity = reader["FaceCapacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FaceCapacity"]),
                                RFIDCapacity = reader["RFIDCapacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RFIDCapacity"]),
                                FingerCapacity = reader["FingerCapacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FingerCapacity"]),
                                MachineNo = reader["MachineNo"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MachineNo"]),
                                CreatedAt = reader["CreatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedAt"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedAt"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                Password = reader["Password"] == DBNull.Value ? null : reader["Password"].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error loading devices: {ex.Message}");
            }
            return devices;
        }


        public async Task InsertLogsForMemberAcrossAllDevicesAsync(int memberId)
        {
            string faceDir = Path.Combine(Directory.GetCurrentDirectory(), "FACE");
            string facePath = Path.Combine(faceDir, $"{memberId}.jpg");
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get member biometric data
                string rfid = null;
                bool hasFingerprint = false;
                int? enrollmentId = null;
                bool hasFace = false;

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT RFID, Fingerprint, EnrollmentID FROM Members WHERE MemberID = @memberId",
                            connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error getting member biometric data: {ex.Message}");
                        return;
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
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT DeviceId FROM Devices WHERE IsActive = 1;",
                            connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error getting active devices: {ex.Message}");
                        return;
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

                            using (var checkCmd = new SqlCommand(
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
                                    using (var updateCmd = new SqlCommand(
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
                                    using (var insertCmd = new SqlCommand(
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

        // Get active members with biometrics but no logs or mismatched logs
        public List<int> GetActiveMemberIdsWithBiometricsAndNoLogs()
        {
            var memberIds = new List<int>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string sql = @"
SELECT DISTINCT m.MemberID
FROM Members m
LEFT JOIN Log l ON m.MemberID = l.MemberID
WHERE LOWER(LTRIM(RTRIM(ISNULL(m.Status, '')))) = 'active'
  AND m.EnrollmentID IS NOT NULL
  AND (
        LTRIM(RTRIM(ISNULL(m.RFID, ''))) <> ''
     OR m.Fingerprint = 1
     OR LTRIM(RTRIM(ISNULL(m.Picture, ''))) <> ''
  )
  AND (
        l.MemberID IS NULL 
     OR (m.RFID IS NOT NULL AND l.BiometricData NOT LIKE '%' + m.RFID + '%')
     OR (m.Picture IS NOT NULL AND l.BiometricData NOT LIKE '%Face%')
  );";

                    using (var cmd = new SqlCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            memberIds.Add(reader.GetInt32(0));
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting active member IDs with biometrics: {ex.Message}");
            }
            return memberIds;
        }


        public async Task InsertLogsForAllMembersAsync(int memberId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await InsertLogsForMemberAcrossAllDevicesAsync(memberId);
                        LogHelper.Log($"Log processing complete for MemberID {memberId}. Success: 1, Failed: 0, Total: 1");
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error inserting logs for MemberID {memberId}: {ex.Message}");
                        LogHelper.Log($"Log processing complete for MemberID {memberId}. Success: 0, Failed: 1, Total: 1");
                    }
                }
            }
        }

        public async Task UpdateBiometricSyncStatus(int logId, string biometricType, bool isSynced, string failureReason = null, int attemptCount = 0)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Get current sync status from Log table
                        Dictionary<string, bool> syncStatus = null;
                        using (var cmd = new SqlCommand(
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

                        // 3. Update the Log table with sync status and error details (if failed)
                        string updateQuery = "UPDATE Log SET SyncStatus = @syncStatus";
                        if (!isSynced)
                        {
                            updateQuery += ", FailureReason = @failureReason";
                            if (attemptCount > 0)
                                updateQuery += ", AttemptCount = ISNULL(AttemptCount, 0) + @attemptCount";
                        }
                        updateQuery += " WHERE Id = @logId";

                        using (var updateCmd = new SqlCommand(updateQuery, connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                            updateCmd.Parameters.AddWithValue("@logId", logId);

                            if (!isSynced)
                            {
                                updateCmd.Parameters.AddWithValue("@failureReason", failureReason ?? "Sync failed");
                                if (attemptCount > 0)
                                    updateCmd.Parameters.AddWithValue("@attemptCount", attemptCount);
                            }

                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        if (isSynced)
                            LogHelper.Log($"Updated {biometricType} sync status for log {logId} to {isSynced}");
                        else
                            LogHelper.Log($"Updated {biometricType} sync status for log {logId} to FAILED: {failureReason}");
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
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                bool logExists = false;
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var checkCmd = new SqlCommand(
                            "SELECT TOP 1 1 FROM Log WHERE MemberID = @memberId AND DeviceID = @deviceId",
                            connection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@memberId", memberId);
                            checkCmd.Parameters.AddWithValue("@deviceId", deviceId);
                            logExists = (await checkCmd.ExecuteScalarAsync()) != null;
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error checking log existence: {ex.Message}");
                        return false;
                    }
                }

                if (!logExists) return false;

                bool allSynced = true;
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT BiometricData, SyncStatus FROM Log WHERE MemberID = @memberId AND DeviceID = @deviceId",
                            connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error checking sync status: {ex.Message}");
                        return false;
                    }
                }

                if (!allSynced) return false;

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<int, bool> memberSyncStatus = new Dictionary<int, bool>();

                        using (var cmd = new SqlCommand(
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

                        using (var updateCmd = new SqlCommand(
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var deviceIds = new List<int>();
                    using (var deviceCmd = new SqlCommand(
                        onlyActiveDevices ? "SELECT DeviceId FROM Devices WHERE IsActive = 1" : "SELECT DeviceId FROM Devices", connection))
                    using (var reader = deviceCmd.ExecuteReader())
                    {
                        while (reader.Read()) deviceIds.Add(reader.GetInt32(0));
                    }

                    if (deviceIds.Count == 0) return 0;

                    int fullySyncedCount = 0;
                    using (var memberCmd = new SqlCommand("SELECT MemberID, SyncStatus FROM Members", connection))
                    using (var reader = memberCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string syncStatusJson = reader.IsDBNull(1) ? "{}" : reader.GetString(1);
                            Dictionary<int, bool> syncDict;
                            try
                            {
                                syncDict = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>();
                            }
                            catch
                            {
                                syncDict = new Dictionary<int, bool>();
                            }

                            bool allDevicesTrue = true;
                            foreach (var deviceId in deviceIds)
                            {
                                bool value;
                                if (!syncDict.TryGetValue(deviceId, out value) || !value)
                                {
                                    allDevicesTrue = false;
                                    break;
                                }
                            }

                            if (allDevicesTrue) fullySyncedCount++;
                        }
                    }

                    return fullySyncedCount;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting fully synced members count: {ex.Message}");
                return 0;
            }
        }


        // Get unsynced log entries
        public async Task<List<LogEntry>> GetUnsyncedLogEntriesAsync()
        {
            var unsyncedEntries = new List<LogEntry>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
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
                                BiometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(reader["BiometricData"])),
                                SyncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(Convert.ToString(reader["SyncStatus"])),
                                CreatedAt = Convert.ToString(reader["CreatedAt"]),
                                UpdatedAt = Convert.ToString(reader["UpdatedAt"])
                            };
                            if (entry.SyncStatus.Any(s => !s.Value)) unsyncedEntries.Add(entry);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting unsynced log entries: {ex.Message}");
            }
            return unsyncedEntries;
        }

        // Get unsynced offline-member log entries (mirrors GetUnsyncedLogEntriesAsync)
        public async Task<List<OfflineLogEntry>> GetUnsyncedOfflineLogEntriesAsync()
        {
            var unsyncedEntries = new List<OfflineLogEntry>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT Id, ClientId, DeviceID, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt " +
                        "FROM OfflineMemberLog WHERE SyncStatus LIKE '%false%'", connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var entry = new OfflineLogEntry
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ClientId = Convert.ToString(reader["ClientId"]),
                                DeviceID = Convert.ToInt32(reader["DeviceID"]),
                                EnrollmentID = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : 0,
                                BiometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(reader["BiometricData"])),
                                SyncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(Convert.ToString(reader["SyncStatus"])),
                                CreatedAt = Convert.ToString(reader["CreatedAt"]),
                                UpdatedAt = Convert.ToString(reader["UpdatedAt"])
                            };
                            if (entry.SyncStatus.Any(s => !s.Value)) unsyncedEntries.Add(entry);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting unsynced offline log entries: {ex.Message}");
            }
            return unsyncedEntries;
        }

        // Mirrors UpdateBiometricSyncStatus, targeting OfflineMemberLog
        public async Task UpdateOfflineBiometricSyncStatus(int logId, string biometricType, bool isSynced, string failureReason = null, int attemptCount = 0)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<string, bool> syncStatus = null;
                        using (var cmd = new SqlCommand(
                            "SELECT SyncStatus FROM OfflineMemberLog WHERE Id = @logId",
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
                                throw new Exception($"OfflineMemberLog entry with ID {logId} not found");
                            }
                        }

                        if (syncStatus.ContainsKey(biometricType))
                            syncStatus[biometricType] = isSynced;
                        else
                            syncStatus.Add(biometricType, isSynced);

                        string updateQuery = "UPDATE OfflineMemberLog SET SyncStatus = @syncStatus";
                        if (!isSynced)
                        {
                            updateQuery += ", FailureReason = @failureReason";
                            if (attemptCount > 0)
                                updateQuery += ", AttemptCount = ISNULL(AttemptCount, 0) + @attemptCount";
                        }
                        updateQuery += " WHERE Id = @logId";

                        using (var updateCmd = new SqlCommand(updateQuery, connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                            updateCmd.Parameters.AddWithValue("@logId", logId);

                            if (!isSynced)
                            {
                                updateCmd.Parameters.AddWithValue("@failureReason", failureReason ?? "Sync failed");
                                if (attemptCount > 0)
                                    updateCmd.Parameters.AddWithValue("@attemptCount", attemptCount);
                            }

                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        if (isSynced)
                            LogHelper.Log($"Updated {biometricType} sync status for offline log {logId} to {isSynced}");
                        else
                            LogHelper.Log($"Updated {biometricType} sync status for offline log {logId} to FAILED: {failureReason}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error updating sync status for offline log {logId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Mirrors CheckAndUpdateMemberDeviceSyncStatus, targeting OfflineMemberLog/OfflineMembers
        public async Task<bool> CheckAndUpdateOfflineMemberDeviceSyncStatus(string clientId, int deviceId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                bool logExists = false;
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var checkCmd = new SqlCommand(
                            "SELECT TOP 1 1 FROM OfflineMemberLog WHERE ClientId = @clientId AND DeviceID = @deviceId",
                            connection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@clientId", clientId);
                            checkCmd.Parameters.AddWithValue("@deviceId", deviceId);
                            logExists = (await checkCmd.ExecuteScalarAsync()) != null;
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error checking offline log existence: {ex.Message}");
                        return false;
                    }
                }

                if (!logExists) return false;

                bool allSynced = true;
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT BiometricData, SyncStatus FROM OfflineMemberLog WHERE ClientId = @clientId AND DeviceID = @deviceId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error checking offline sync status: {ex.Message}");
                        return false;
                    }
                }

                if (!allSynced) return false;

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<int, bool> memberSyncStatus = new Dictionary<int, bool>();

                        using (var cmd = new SqlCommand(
                            "SELECT SyncStatus FROM OfflineMembers WHERE ClientId = @clientId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                memberSyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(result.ToString())
                                                  ?? new Dictionary<int, bool>();
                            }
                        }

                        bool alreadyTrue;
                        if (memberSyncStatus.TryGetValue(deviceId, out alreadyTrue) && alreadyTrue)
                        {
                            transaction.Commit();
                            return false; // no change performed
                        }

                        memberSyncStatus[deviceId] = true;

                        using (var updateCmd = new SqlCommand(
                            "UPDATE OfflineMembers SET SyncStatus = @syncStatus WHERE ClientId = @clientId",
                            connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(memberSyncStatus));
                            updateCmd.Parameters.AddWithValue("@clientId", clientId);
                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        LogHelper.Log($"Updated offline member {clientId} sync status for device {deviceId}");

                        transaction.Commit();
                        return true; // changed
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error updating offline member sync status: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        // Returns offline members for the biometric-sync worker (ClientId/Name/EnrollmentID)
        public List<OfflineMember> GetOfflineMembers()
        {
            var members = new List<OfflineMember>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "SELECT ClientId, Name, FirstName, LastName, Gender, Phone, Subscription, Picture, RFID, Fingerprint, EnrollmentID, ServerSync, CreatedAt, DeviceCleanupStatus " +
                    "FROM OfflineMembers ORDER BY CreatedAt DESC", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cleanupJson = reader.IsDBNull(13) ? null : reader.GetString(13);
                        Dictionary<int, bool> cleanupStatus;
                        try
                        {
                            cleanupStatus = !string.IsNullOrWhiteSpace(cleanupJson)
                                ? JsonConvert.DeserializeObject<Dictionary<int, bool>>(cleanupJson) ?? new Dictionary<int, bool>()
                                : new Dictionary<int, bool>();
                        }
                        catch
                        {
                            cleanupStatus = new Dictionary<int, bool>();
                        }

                        members.Add(new OfflineMember
                        {
                            ClientId = reader.GetString(0),
                            Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                            FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            LastName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Gender = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Phone = reader.IsDBNull(5) ? null : reader.GetString(5),
                            SubscriptionJson = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Picture = reader.IsDBNull(7) ? null : reader.GetString(7),
                            RFID = reader.IsDBNull(8) ? null : reader.GetString(8),
                            Fingerprint = !reader.IsDBNull(9) && reader.GetBoolean(9),
                            EnrollmentID = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                            ServerSync = !reader.IsDBNull(11) && reader.GetBoolean(11),
                            CreatedAt = reader.IsDBNull(12) ? null : Convert.ToString(reader.GetValue(12)),
                            DeviceCleanupStatus = cleanupStatus
                        });
                    }
                }
            }
            return members;
        }

        // OfflineMemberLog rows for one offline member (which devices they were pushed to).
        public List<OfflineLogEntry> GetOfflineMemberLogs(string clientId)
        {
            var logs = new List<OfflineLogEntry>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "SELECT Id, ClientId, DeviceID, EnrollmentID FROM OfflineMemberLog WHERE ClientId = @clientId", connection))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new OfflineLogEntry
                            {
                                Id = reader.GetInt32(0),
                                ClientId = reader.GetString(1),
                                DeviceID = reader.GetInt32(2),
                                EnrollmentID = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            return logs;
        }

        /// <summary>
        /// Flips an offline member to "accepted by server, pending device cleanup": sets
        /// ServerSync = 1 and seeds DeviceCleanupStatus with one {deviceId: false} entry per
        /// device it was actually pushed to. Called once, right after a successful
        /// /api/sync/enrollments upload — never re-uploads after this.
        /// </summary>
        public void MarkOfflineMemberServerSynced(string clientId, IEnumerable<int> deviceIds)
        {
            var cleanupStatus = new Dictionary<int, bool>();
            foreach (var deviceId in deviceIds.Distinct())
                cleanupStatus[deviceId] = false;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "UPDATE OfflineMembers SET ServerSync = 1, DeviceCleanupStatus = @status, UpdatedAt = @updatedAt WHERE ClientId = @clientId", connection))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    command.Parameters.AddWithValue("@status", JsonConvert.SerializeObject(cleanupStatus));
                    command.Parameters.AddWithValue("@updatedAt", GetTimestamp());
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Marks one device as successfully cleaned up for an already-server-synced offline
        /// member. Read-modify-write on DeviceCleanupStatus so progress survives across timer
        /// ticks — a device that already succeeded is never retried.
        /// </summary>
        public void MarkOfflineMemberDeviceCleaned(string clientId, int deviceId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string existingJson = null;
                        using (var cmd = new SqlCommand(
                            "SELECT DeviceCleanupStatus FROM OfflineMembers WHERE ClientId = @clientId", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            var result = cmd.ExecuteScalar();
                            existingJson = result != null && result != DBNull.Value ? result.ToString() : null;
                        }

                        var status = !string.IsNullOrWhiteSpace(existingJson)
                            ? JsonConvert.DeserializeObject<Dictionary<int, bool>>(existingJson) ?? new Dictionary<int, bool>()
                            : new Dictionary<int, bool>();
                        status[deviceId] = true;

                        using (var cmd = new SqlCommand(
                            "UPDATE OfflineMembers SET DeviceCleanupStatus = @status, UpdatedAt = @updatedAt WHERE ClientId = @clientId", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@clientId", clientId);
                            cmd.Parameters.AddWithValue("@status", JsonConvert.SerializeObject(status));
                            cmd.Parameters.AddWithValue("@updatedAt", GetTimestamp());
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { /* ignore */ }
                        LogHelper.Log($"Error marking device {deviceId} cleaned for offline member {clientId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Deletes every OfflineMemberLog row for a member (call before DeleteOfflineMember —
        // OfflineMemberLog.ClientId is FK'd to OfflineMembers.ClientId).
        public void DeleteOfflineMemberLogs(string clientId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("DELETE FROM OfflineMemberLog WHERE ClientId = @clientId", connection))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Deletes the OfflineMembers row itself. Call after DeleteOfflineMemberLogs.
        public void DeleteOfflineMember(string clientId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("DELETE FROM OfflineMembers WHERE ClientId = @clientId", connection))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    command.ExecuteNonQuery();
                }
            }
        }


        public bool AddOrUpdateFingerprint(FingerprintData fingerprintdata)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SqlCommand(@"
MERGE Fingerprints AS target
USING (SELECT @EnrollNumber AS EnrollNumber, @FingerNumber AS FingerNumber, @Privilege AS Privilege, @TemplateData AS TemplateData, @TemplateString AS TemplateString, @CreatedAt AS CreatedAt, @UpdatedAt AS UpdatedAt) AS source
ON target.EnrollNumber = source.EnrollNumber
WHEN MATCHED THEN
    UPDATE SET FingerNumber = source.FingerNumber, Privilege = source.Privilege, TemplateData = source.TemplateData, TemplateString = source.TemplateString, UpdatedAt = source.UpdatedAt
WHEN NOT MATCHED THEN
    INSERT (EnrollNumber, FingerNumber, Privilege, TemplateData, TemplateString, CreatedAt, UpdatedAt) VALUES (source.EnrollNumber, source.FingerNumber, source.Privilege, source.TemplateData, source.TemplateString, source.CreatedAt, source.UpdatedAt);", connection, transaction))
                            {
                                var now = GetTimestamp();

                                command.Parameters.AddWithValue("@EnrollNumber", fingerprintdata.EnrollNumber);
                                command.Parameters.AddWithValue("@FingerNumber", fingerprintdata.FingerNumber);
                                command.Parameters.AddWithValue("@Privilege", fingerprintdata.Privilege);
                                command.Parameters.AddWithValue("@TemplateData", fingerprintdata.TemplateData ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@TemplateString", fingerprintdata.TemplateString ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@CreatedAt", fingerprintdata.CreatedAt ?? now);
                                command.Parameters.AddWithValue("@UpdatedAt", now);

                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            return true;
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error adding or updating fingerprint: {ex.Message}");
                            return false;
                        }
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT * FROM Fingerprints", connection))
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
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting all fingerprints: {ex.Message}");
            }
            return fingerprints;
        }

        public List<byte[]> GetAllBinaryTemplates()
        {
            var templates = new List<byte[]>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT TemplateData FROM Fingerprints WHERE TemplateData IS NOT NULL", connection))
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
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting all binary templates: {ex.Message}");
            }
            return templates;
        }


        public List<string> GetAllStringTemplates()
        {
            var templates = new List<string>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT TemplateString FROM Fingerprints WHERE TemplateString IS NOT NULL", connection))
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
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting all string templates: {ex.Message}");
            }
            return templates;
        }

        public bool DeleteFingerprint(int enrollNumber)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SqlCommand(
                                "DELETE FROM Fingerprints WHERE EnrollNumber = @EnrollNumber", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EnrollNumber", enrollNumber);
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            return true;
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error deleting fingerprint: {ex.Message}");
                            return false;
                        }
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SqlCommand("DELETE FROM Fingerprints", connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            return true;
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error deleting all fingerprints: {ex.Message}");
                            return false;
                        }
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
            var members = new List<Member>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
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

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                var member = new Member
                                {
                                    MemberID = Convert.ToInt32(reader["MemberID"]),
                                    Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : null,
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                    RFID = reader["RFID"] != DBNull.Value ? reader["RFID"].ToString() : null,
                                    Picture = reader["Picture"] != DBNull.Value ? reader["Picture"].ToString() : null,
                                    Fingerprint = Convert.ToBoolean(reader["Fingerprint"]),
                                    EnrollmentID = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : (int?)null,
                                    PackageExpiry = reader["PackageExpiry"] != DBNull.Value ? reader["PackageExpiry"].ToString() : null
                                };
                                members.Add(member);
                                LogHelper.Log($"Found member with missing fingerprint - MemberID: {member.MemberID}, Name: {member.Name}, EnrollmentID: {member.EnrollmentID}, RFID: {member.RFID}");
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log($"Error processing member record: {ex.Message}");
                            }
                        }
                    }
                }
                LogHelper.Log($"Completed missing fingerprints check. Found {members.Count} members.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in GetMembersWithMissingFingerprintsAsync: {ex.Message}");
                throw;
            }
            return members;
        }

        public async Task<List<dynamic>> GetMembersFingerprintStatusAsync()
        {
            var results = new List<dynamic>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT m.MemberID, m.Name, m.EnrollmentID, 
                               CASE WHEN f.EnrollNumber IS NULL THEN 'Missing' ELSE 'Available' END AS FingerStatus
                        FROM Members m
                        LEFT JOIN Fingerprints f ON m.EnrollmentID = f.EnrollNumber
                        WHERE m.Fingerprint = 1 AND m.EnrollmentID IS NOT NULL";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                MemberID = Convert.ToInt32(reader["MemberID"]),
                                Name = reader["Name"]?.ToString(),
                                EnrollmentID = reader["EnrollmentID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["EnrollmentID"]) : null,
                                Status = reader["FingerStatus"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in GetMembersFingerprintStatusAsync: {ex.Message}");
            }
            return results;
        }


        public (bool shouldDisable, List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)> deviceInfo)
CheckMemberAccessStatus(int memberId)
        {
            var deviceInfo = new List<(int, string, int, string, int, int)>();
            bool shouldDisable = false;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
                SELECT m.Status, m.PackageExpiry, m.EnrollmentID,
                       d.DeviceIP, d.Port, d.MachineNo
                FROM Members m
                JOIN Log l ON m.MemberID = l.MemberID
                JOIN Devices d ON l.DeviceID = d.DeviceId
                WHERE m.MemberID = @memberId", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string status = reader["Status"]?.ToString() ?? "active";
                                string packageExpiry = reader["PackageExpiry"]?.ToString();
                                int enrollmentNumber = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : 0;

                                bool isExpired = false;
                                if (!string.IsNullOrEmpty(packageExpiry) &&
                                    DateTime.TryParse(packageExpiry, out DateTime expiryDate))
                                {
                                    isExpired = expiryDate < DateTime.Today;
                                }

                                if (status.Equals("disabled", StringComparison.OrdinalIgnoreCase))
                                {
                                    shouldDisable = false;
                                }
                                else
                                {
                                    bool isInactive = status.Equals("inactive", StringComparison.OrdinalIgnoreCase);
                                    bool isOverdue = status.Equals("overdue", StringComparison.OrdinalIgnoreCase);
                                    shouldDisable = isInactive || isOverdue || isExpired;
                                }

                                do
                                {
                                    int machineNo = reader["MachineNo"] != DBNull.Value ? Convert.ToInt32(reader["MachineNo"]) : 0;
                                    string ip = reader["DeviceIP"] != DBNull.Value ? Decrypt(reader["DeviceIP"].ToString()) : "0.0.0.0";
                                    int port = reader["Port"] != DBNull.Value ? Convert.ToInt32(reader["Port"]) : 0;

                                    deviceInfo.Add((
                                        machineNo,
                                        ip,
                                        port,
                                        "0",
                                        enrollmentNumber,
                                        0
                                    ));
                                } while (reader.Read());
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking member access status: {ex.Message}");
            }
            return (shouldDisable, deviceInfo);
        }


        public Dictionary<int, (bool shouldDisable, List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)> deviceInfo)>
    GetMembersAccessStatus()
        {
            var results = new Dictionary<int, (bool, List<(int, string, int, string, int, int)>)>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
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
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting members access status: {ex.Message}");
            }
            return results;
        }


        public List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)>
    GetDetails(int memberId)
        {
            var deviceDetails = new List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
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

                            int totalDevices = 0;
                            while (reader.Read())
                            {
                                int enrollmentNumber = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : 0;
                                int machineNo = reader["MachineNo"] != DBNull.Value ? Convert.ToInt32(reader["MachineNo"]) : 0;
                                string ip = reader["DeviceIP"] != DBNull.Value ? Decrypt(reader["DeviceIP"].ToString()) : "0.0.0.0";
                                int port = reader["Port"] != DBNull.Value ? Convert.ToInt32(reader["Port"]) : 0;

                                deviceDetails.Add((machineNo, ip, port, "0", enrollmentNumber, 0));
                                totalDevices++;

                                LogHelper.Log($"🔧 Device {totalDevices}: Machine No: {machineNo}, IP:Port: {ip}:{port}, Enrollment: {enrollmentNumber}");
                            }
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
            var deviceDetails = new List<(int, string, int, string, int, int)>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var checkCmd = new SqlCommand(@"
                        SELECT TOP 1 1 
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

                    using (var cmd = new SqlCommand(@"
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

                            int totalDevices = 0;
                            while (reader.Read())
                            {
                                int enrollmentNumber = reader["EnrollmentID"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentID"]) : 0;
                                int machineNo = reader["MachineNo"] != DBNull.Value ? Convert.ToInt32(reader["MachineNo"]) : 0;
                                string ip = reader["DeviceIP"] != DBNull.Value ? Decrypt(reader["DeviceIP"].ToString()) : "0.0.0.0";
                                int port = reader["Port"] != DBNull.Value ? Convert.ToInt32(reader["Port"]) : 0;

                                deviceDetails.Add((machineNo, ip, port, "0", enrollmentNumber, 0));
                                totalDevices++;
                                LogHelper.Log($"🔧 Device {totalDevices}: Machine No: {machineNo}, IP:Port: {ip}:{port}, Enrollment: {enrollmentNumber}");
                            }
                            LogHelper.Log($"📊 Summary: Total Active Devices: {totalDevices}");
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
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
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting member device sync status: {ex.Message}");
                return (false, false, "Error");
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            LogHelper.Log($"ℹ️ Updating status for member {memberId} to '{newStatus}'");

                            using (var cmd = new SqlCommand(@"
                        UPDATE Members 
                        SET Status = @status,
                            UpdatedAt = @updatedAt
                        WHERE MemberID = @memberId", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@status", newStatus);
                                cmd.Parameters.AddWithValue("@updatedAt", GetTimestamp());
                                cmd.Parameters.AddWithValue("@memberId", memberId);

                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    LogHelper.Log($"✅ Successfully updated status for member {memberId}");
                                    transaction.Commit();
                                    return true;
                                }
                                else
                                {
                                    LogHelper.Log($"⚠️ No member found with ID {memberId}");
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error updating member status: {ex.Message}");
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

        /// <summary>
        /// Local, immediate side of an offline renewal: marks the member active and clears
        /// PackageExpiry (the real expiry only becomes known once the server processes the
        /// queued OfflineSubscriptions row and returns package_expiry).
        /// </summary>
        public void RenewMemberLocally(int memberId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE Members SET Status = @status, PackageExpiry = NULL, UpdatedAt = @updatedAt WHERE MemberID = @memberId", connection))
                {
                    cmd.Parameters.AddWithValue("@status", "active");
                    cmd.Parameters.AddWithValue("@updatedAt", GetTimestamp());
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateMemberPackageExpiry(int memberId, string packageExpiry)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE Members SET PackageExpiry = @packageExpiry, UpdatedAt = @updatedAt WHERE MemberID = @memberId", connection))
                {
                    cmd.Parameters.AddWithValue("@packageExpiry", (object)packageExpiry ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@updatedAt", GetTimestamp());
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int InsertOfflineSubscription(int memberId, string clientId, JObject subscription)
        {
            string timestamp = GetTimestamp();
            string subscriptionJson = subscription != null ? subscription.ToString(Formatting.None) : null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(@"
INSERT INTO OfflineSubscriptions (MemberId, ClientId, Subscription, CreatedAt, UpdatedAt)
VALUES (@memberId, @clientId, @subscription, @createdAt, @updatedAt);
SELECT SCOPE_IDENTITY();", connection))
                {
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    cmd.Parameters.AddWithValue("@clientId", (object)clientId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@subscription", (object)subscriptionJson ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@createdAt", timestamp);
                    cmd.Parameters.AddWithValue("@updatedAt", timestamp);

                    var result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public List<OfflineSubscription> GetPendingOfflineSubscriptions()
        {
            var list = new List<OfflineSubscription>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(
                    "SELECT Id, MemberId, ClientId, Subscription FROM OfflineSubscriptions ORDER BY CreatedAt", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new OfflineSubscription
                        {
                            Id = reader.GetInt32(0),
                            MemberId = reader.GetInt32(1),
                            ClientId = reader.IsDBNull(2) ? null : reader.GetString(2),
                            SubscriptionJson = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }
            return list;
        }

        public void DeleteOfflineSubscription(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand("DELETE FROM OfflineSubscriptions WHERE Id = @id", connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool DeleteMemberLogs(int memberId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            LogHelper.Log($"ℹ️ Preparing to delete logs for member {memberId}");

                            using (var cmd = new SqlCommand(@"
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
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error deleting member logs: {ex.Message}");
                            return false;
                        }
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Devices", connection))
                    {
                        int deviceCount = Convert.ToInt32(cmd.ExecuteScalar());
                        return deviceCount == 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking if first device: {ex.Message}");
                return false;
            }
        }



        // Check if there are any unsynced logs
        public async Task<bool> CheckForUnsyncedLogs()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand(@"
                        SELECT CASE WHEN EXISTS(
                            SELECT TOP 1 1 FROM Log 
                            WHERE SyncStatus LIKE '%false%' 
                        ) THEN 1 ELSE 0 END", connection))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        return Convert.ToBoolean(result);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking for unsynced logs: {ex.Message}");
                return false;
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

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Check if the member exists in the Log table
                bool memberExistsInLog = false;
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var checkCmd = new SqlCommand("SELECT TOP 1 1 FROM Log WHERE MemberID = @memberId", connection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@memberId", memberId);
                            memberExistsInLog = (await checkCmd.ExecuteScalarAsync()) != null;
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error checking member existence in log: {ex.Message}");
                        return changes;
                    }
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

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand("SELECT RFID, Fingerprint, EnrollmentID, Picture, SyncStatus FROM Members WHERE MemberID = @memberId", connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error getting member biometric data: {ex.Message}");
                        return changes;
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

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var logCmd = new SqlCommand("SELECT Id, EnrollmentID, BiometricData, SyncStatus FROM Log WHERE MemberID = @memberId AND DeviceID = @deviceId", connection, transaction))
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
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error checking log existence: {ex.Message}");
                            continue;
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
                            using (var transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    using (var insertCmd = new SqlCommand(
                                        "INSERT INTO Log (MemberID, DeviceID, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt) VALUES (@memberId, @deviceId, @enrollmentId, @biometricData, @syncStatus, @createdAt, @updatedAt)", connection, transaction))
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
                                        transaction.Commit();
                                        LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Inserted new log for member {memberId} on device {deviceId} with biometrics: {string.Join(", ", biometricData.Keys)}");
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    transaction.Rollback();
                                    Console.WriteLine($"Error inserting new log: {ex.Message}");
                                }
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
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                using (var deleteCmd = new SqlCommand("DELETE FROM Log WHERE Id = @logId", connection, transaction))
                                {
                                    deleteCmd.Parameters.AddWithValue("@logId", logId);
                                    await deleteCmd.ExecuteNonQueryAsync();
                                    transaction.Commit();
                                }
                                LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Deleted log for member {memberId} on device {deviceId} because no biometric data remains.");
                            }
                            catch (SqlException ex)
                            {
                                transaction.Rollback();
                                Console.WriteLine($"Error deleting log: {ex.Message}");
                            }
                        }
                        continue;
                    }

                    if (updated)
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                using (var updateCmd = new SqlCommand("UPDATE Log SET BiometricData = @biometricData, SyncStatus = @syncStatus, UpdatedAt = @updatedAt WHERE Id = @logId", connection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@biometricData", JsonConvert.SerializeObject(biometricData));
                                    updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncStatus));
                                    updateCmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                                    updateCmd.Parameters.AddWithValue("@logId", logId);
                                    await updateCmd.ExecuteNonQueryAsync();
                                    transaction.Commit();
                                }
                                LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Updated log for member {memberId} on device {deviceId}. Remaining biometrics: {string.Join(", ", biometricData.Keys)}");
                            }
                            catch (SqlException ex)
                            {
                                transaction.Rollback();
                                Console.WriteLine($"Error updating log: {ex.Message}");
                            }
                        }
                    }
                }
            }

            //LogHelper.Log($"[VerifyAndUpdateLogBiometricsAsync] Completed for member {memberId}. Total changes: {changes.Count}");
            return changes;
        }

        // Query function to get device details by DeviceId
        public (string ip, int machineNo, int port) GetDeviceDetails(int deviceId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT DeviceIP, MachineNo, Port FROM Devices WHERE DeviceId = @deviceId", connection))
                    {
                        cmd.Parameters.AddWithValue("@deviceId", deviceId);
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
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting device details: {ex.Message}");
            }
            return (null, 0, 0);
        }


        public (bool exists, bool synced) CheckAttendanceDuplicateAndSynced(int memberId, DateTime deviceTime, string action)
        {
            try
            {
                string timeStr = deviceTime.ToString("yyyy-MM-dd HH:mm:ss");
                string column = action == "check-in" ? "CheckInTime" : "CheckOutTime";
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand($@"
                    SELECT TOP 1 SyncStatus 
                    FROM AttendanceLogs
                    WHERE MemberID=@mid AND {column}=@ts;", conn))
                    {
                        cmd.Parameters.AddWithValue("@mid", memberId);
                        cmd.Parameters.AddWithValue("@ts", timeStr);
                        var raw = cmd.ExecuteScalar();
                        if (raw == null || raw == DBNull.Value) return (false, false);
                        try
                        {
                            var dict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(raw.ToString()) ?? new Dictionary<string, bool>();
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
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking attendance duplicate and synced: {ex.Message}");
                return (false, false);
            }
        }


        // Helper to delete logs for a device
        public void DeleteLogsByDeviceId(int deviceId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 1. Delete logs for the device
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("DELETE FROM Log WHERE DeviceID = @DeviceId", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@DeviceId", deviceId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error deleting logs by device ID: {ex.Message}");
                        throw;
                    }
                }

                // 2. Update SyncStatus in Members table for all members
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var selectCmd = new SqlCommand("SELECT MemberID, SyncStatus FROM Members", connection, transaction))
                        using (var reader = selectCmd.ExecuteReader())
                        {
                            var updates = new List<(int memberId, string newSyncStatus)>();

                            while (reader.Read())
                            {
                                int memberId = reader.GetInt32(0);
                                string syncStatusJson = reader.IsDBNull(1) ? "{}" : reader.GetString(1);

                                var syncStatusDict = JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>();

                                if (syncStatusDict.ContainsKey(deviceId))
                                {
                                    syncStatusDict.Remove(deviceId);
                                    string newSyncStatusJson = JsonConvert.SerializeObject(syncStatusDict);
                                    updates.Add((memberId, newSyncStatusJson));
                                }
                            }

                            reader.Close();

                            // Apply updates
                            foreach (var update in updates)
                            {
                                using (var updateCmd = new SqlCommand("UPDATE Members SET SyncStatus = @SyncStatus WHERE MemberID = @MemberID", connection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@SyncStatus", update.newSyncStatus);
                                    updateCmd.Parameters.AddWithValue("@MemberID", update.memberId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating sync status after deleting logs: {ex.Message}");
                        throw;
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = new SqlCommand("SELECT TOP 1 AttendanceType FROM Devices WHERE DeviceId = @DeviceId", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                                var result = cmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    bool attendanceType = Convert.ToBoolean(result);
                                    deviceType = attendanceType ? "check-in" : "check-out";
                                }
                            }
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error getting device type: {ex.Message}");
                        }
                    }
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check for existing record to prevent duplicates
                            using (var checkCommand = new SqlCommand(@"
                        SELECT COUNT(*) FROM GLog 
                        WHERE DeviceId = @DeviceId 
                        AND EnrollmentNumber = @EnrollmentNumber 
                        AND DeviceTime = @DeviceTime 
                        AND OriginalLine = @OriginalLine", connection, transaction))
                            {
                                checkCommand.Parameters.AddWithValue("@DeviceId", deviceId);
                                checkCommand.Parameters.AddWithValue("@EnrollmentNumber", enrollmentNumber);
                                checkCommand.Parameters.AddWithValue("@DeviceTime", deviceTimeText);
                                checkCommand.Parameters.AddWithValue("@OriginalLine", originalLine);

                                int existingCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                                if (existingCount > 0)
                                {
                                    LogHelper.Log($"Duplicate GLog record detected - skipping insert for enrollment {enrollmentNumber} at {deviceTimeText}");
                                    transaction.Commit();
                                    return;
                                }
                            }

                            using (var command = new SqlCommand(@"
                        INSERT INTO GLog (DeviceId, EnrollmentNumber, DeviceTime, OriginalLine, Sync, DeviceType, Filter, CreatedOn, UpdatedOn)
                        VALUES (@DeviceId, @EnrollmentNumber, @DeviceTime, @OriginalLine, @Sync, @DeviceType, @Filter, @CreatedOn, @UpdatedOn)", connection, transaction))
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
                                transaction.Commit();
                                LogHelper.Log($"Successfully inserted GLog record for enrollment {enrollmentNumber} at {deviceTimeText}");
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error inserting GLog record: {ex.Message}");
                            throw;
                        }
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

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
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
                      AND CONVERT(DATE, g2.DeviceTime) = CONVERT(DATE, GLog.DeviceTime)
                      AND g2.Sync = 0
                );";

                            using (var reopenCmd = new SqlCommand(reopenSql, connection, transaction))
                            {
                                int reopened = reopenCmd.ExecuteNonQuery();
                                LogHelper.Log($"[GLogPair] Reopened {reopened} previously processed rows for re-evaluation.");
                            }

                            // Get device type mapping for all devices
                            var deviceTypeMap = new Dictionary<int, string>();
                            using (var mapCmd = new SqlCommand("SELECT DeviceId, AttendanceType FROM Devices", connection, transaction))
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
                               CONVERT(DATE, DeviceTime) AS RecordDate
                        FROM GLog
                        WHERE Sync = 0
                        ORDER BY EnrollmentNumber, CONVERT(DATE, DeviceTime), DeviceTime";

                            var allRecords = new List<dynamic>();
                            using (var cmd = new SqlCommand(selectSql, connection, transaction))
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

                            transaction.Commit();
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
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error in FilterAndUpdateGLogRecordsByDateTwoWay: {ex.Message}");
                            throw;
                        }
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

        private void UpdateGLogSyncFlagsInBatches(SqlConnection connection, List<int> ids, bool filterValue, string timestamp, int batchSize)
        {
            if (ids == null || ids.Count == 0)
                return;

            for (int i = 0; i < ids.Count; i += batchSize)
            {
                var batch = ids.Skip(i).Take(batchSize).ToList();
                var idParameters = batch.Select((_, index) => $"@id{index}").ToList();
                string updateSql = $"UPDATE GLog SET Sync=1, Filter=@filter, UpdatedOn=@ts WHERE LogId IN ({string.Join(", ", idParameters)})";

                using (var transaction = connection.BeginTransaction())
                using (var cmd = new SqlCommand(updateSql, connection, transaction))
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT TOP 1 Name FROM Members WHERE MemberID = @memberId", connection))
                    {
                        cmd.Parameters.AddWithValue("@memberId", memberId);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting member name: {ex.Message}");
                return null;
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = "DELETE FROM GLog";
                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error deleting all GLogs: {ex.Message}");
                        }
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        "SELECT TOP 10000 GateId, DeviceIP, MemberId, IsVisitor, VisitorName, VisitorPhone, Reason, AccessedTime, Username, CreatedAt, UpdatedAt " +
                        "FROM GateAccessed ORDER BY AccessedTime DESC", connection))
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
                                AccessedTime = reader.IsDBNull(7) ? DateTime.MinValue : Convert.ToDateTime(reader.GetValue(7)),
                                Username = reader.IsDBNull(8) ? null : reader.GetString(8)
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting all gate accesseds: {ex.Message}");
            }
            return gateAccesseds;
        }
        public void InsertGateAccessed(GateAccessed gateAccessed)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string timestamp = GetTimestamp();
                        string accessedTime = gateAccessed.AccessedTime != DateTime.MinValue
                            ? gateAccessed.AccessedTime.ToString("yyyy-MM-dd HH:mm:ss")
                            : timestamp;

                        using (var command = new SqlCommand(@"
            INSERT INTO GateAccessed 
            (DeviceIP, MemberId, IsVisitor, VisitorName, VisitorPhone, Reason, AccessedTime, Username, CreatedAt, UpdatedAt) 
            VALUES 
            (@DeviceIP, @MemberId, @IsVisitor, @VisitorName, @VisitorPhone, @Reason, @AccessedTime, @Username, @CreatedAt, @UpdatedAt)", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@DeviceIP", gateAccessed.DeviceIP);
                            command.Parameters.AddWithValue("@MemberId", gateAccessed.MemberId > 0 ? (object)gateAccessed.MemberId : DBNull.Value);
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting gate accessed: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Creates a backup of the SQL Server DB
        public string CreateDatabaseSnapshot()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                string databaseName = builder.InitialCatalog;

                // Use a directory that the SQL Server Service is more likely to have access to.
                // Note: You may need to manually grant 'Full Control' to 'NT Service\MSSQLSERVER' 
                // on this folder if it still fails.
                string backupFolder = @"C:\SmartAttendanceBackups";

                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                string fileName = $"{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string backupPath = Path.Combine(backupFolder, fileName);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // WITH FORMAT ensures we create a new backup file
                    string query = $"BACKUP DATABASE [{databaseName}] TO DISK = @path WITH FORMAT, NAME = 'Full Backup of {databaseName}';";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@path", backupPath);
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }

                LogHelper.Log($"Database backup created successfully at: {backupPath}");
                return backupPath;
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
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(
                            @"INSERT INTO PendingJobs (Action, MemberId, EnrollmentId, DeviceId, Ip, Port, MachineNo, Password, BackupNumber, CreatedAt)
              VALUES (@action, @memberId, @enrollmentId, @deviceId, @ip, @port, @machineNo, @password, @backupNumber, @createdAt)", connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting pending job: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public List<PendingJob> GetAllPendingJobs()
        {
            var jobs = new List<PendingJob>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
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
                                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            };

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
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting all pending jobs: {ex.Message}");
            }
            return jobs;
        }


        public void DeletePendingJob(PendingJob job)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(
                            @"DELETE FROM PendingJobs 
              WHERE Action = @action 
                AND MemberId = @memberId 
                AND EnrollmentId = @enrollmentId 
                AND Ip = @ip 
                AND Port = @port 
                AND MachineNo = @machineNo
                AND (DeviceId = @deviceId OR (DeviceId IS NULL AND @deviceId IS NULL))", connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error deleting pending job: {ex.Message}");
                        throw;
                    }
                }
            }
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
                SELECT 
                    SUM(CASE WHEN LOWER(ISNULL(StaffAttendanceType, '')) = 'check_in' THEN 1 ELSE 0 END) as CheckInCount,
                    SUM(CASE WHEN LOWER(ISNULL(StaffAttendanceType, '')) = 'check_out' THEN 1 ELSE 0 END) as CheckOutCount,
                    SUM(CASE WHEN LOWER(ISNULL(StaffAttendanceType, '')) = 'check_in/check_out' THEN 1 ELSE 0 END) as MixedCount,
                    COUNT(*) as TotalDevices
                FROM StaffDevices", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Use DBNull check or null-coalescing with Convert to handle empty tables
                            int checkInCount = reader["CheckInCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CheckInCount"]);
                            int checkOutCount = reader["CheckOutCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CheckOutCount"]);
                            int mixedCount = reader["MixedCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MixedCount"]);
                            int totalDevices = reader["TotalDevices"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalDevices"]);

                            LogHelper.Log($"Staff device analysis - Total: {totalDevices}, Check_In: {checkInCount}, Check_Out: {checkOutCount}, Mixed: {mixedCount}");

                            if (totalDevices == 0)
                            {
                                LogHelper.Log("No staff devices available for attendance case decision");
                                return "1way";
                            }

                            if (checkInCount > 0 && checkOutCount > 0)
                            {
                                LogHelper.Log("✅ 2-way attendance detected: Separate check-in and check-out devices found");
                                return "2way";
                            }

                            LogHelper.Log("✅ 1-way attendance detected: Single device type configuration or mixed devices only");
                            return "1way";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                LogHelper.Log($"SQL Error deciding attendance case: {ex.Message}");
                return "1way";
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in DecideAttendanceCase: {ex.Message}");
                return "1way";
            }

            LogHelper.Log("Fallback: Defaulting to 1-way attendance");
            return "1way";
        }



        /// <summary>
        /// Determine whether the provided IP belongs to a staff device or a member device.
        /// Returns a tuple: (exists, "staff"|"member"|null, deviceId)
        /// </summary>
        public (bool exists, string type, int deviceId) GetDeviceCategoryByIp(string deviceIp)
        {
            if (string.IsNullOrWhiteSpace(deviceIp))
                return (false, null, 0);

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Staff devices stored in StaffDevices with encrypted DeviceIP
                    using (var cmd = new SqlCommand("SELECT TOP 1 StaffDeviceId FROM StaffDevices WHERE DeviceIP = @ip", connection))
                    {
                        cmd.Parameters.AddWithValue("@ip", Encrypt(deviceIp));
                        var res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                            return (true, "staff", Convert.ToInt32(res));
                    }

                    // Member (attendance) devices stored in Devices with encrypted DeviceIP
                    using (var cmd2 = new SqlCommand("SELECT TOP 1 DeviceId FROM Devices WHERE DeviceIP = @ip", connection))
                    {
                        cmd2.Parameters.AddWithValue("@ip", Encrypt(deviceIp));
                        var res2 = cmd2.ExecuteScalar();
                        if (res2 != null && res2 != DBNull.Value)
                            return (true, "member", Convert.ToInt32(res2));
                    }

                    return (false, null, 0);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetDeviceCategoryByIp][ERROR] IP={deviceIp} : {ex.Message}");
                return (false, null, 0);
            }
        }


        ////////////////////////////////////////////STAFF HELPERS////////////////////////////////////////////

        /// <summary>
        /// Ensures all biometric modalities for the given staff+device are synced, and if so
        /// marks Staff.SyncStatus[staffDeviceId] = true.
        /// Returns true when a DB change was made (flag was set true), false otherwise.
        /// </summary>
        public async Task<bool> CheckAndUpdateStaffDeviceSyncStatus(string staffId, int staffDeviceId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Ensure a StaffLog exists for this staff + device
                bool logExists = false;
                using (var checkCmd = new SqlCommand(
                    "SELECT TOP 1 1 FROM StaffLog WHERE StaffID = @staffId AND StaffDeviceId = @sdid",
                    connection))
                {
                    checkCmd.Parameters.AddWithValue("@staffId", staffId ?? string.Empty);
                    checkCmd.Parameters.AddWithValue("@sdid", staffDeviceId);
                    logExists = (await checkCmd.ExecuteScalarAsync()) != null;
                }

                if (!logExists) return false;

                // Ensure all biometric modalities in the StaffLog are already synced
                bool allSynced = true;
                using (var cmd = new SqlCommand(
                    "SELECT BiometricData, SyncStatus FROM StaffLog WHERE StaffID = @staffId AND StaffDeviceId = @sdid",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@staffId", staffId ?? string.Empty);
                    cmd.Parameters.AddWithValue("@sdid", staffDeviceId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var bioJson = Convert.ToString(reader["BiometricData"]) ?? "{}";
                            var syncJson = Convert.ToString(reader["SyncStatus"]) ?? "{}";

                            var biometricData = JsonConvert.DeserializeObject<Dictionary<string, string>>(bioJson) ?? new Dictionary<string, string>();
                            var syncStatus = JsonConvert.DeserializeObject<Dictionary<string, bool>>(syncJson) ?? new Dictionary<string, bool>();

                            allSynced = biometricData.Keys.All(key =>
                                syncStatus.ContainsKey(key) && syncStatus[key]);
                        }
                    }
                }

                if (!allSynced) return false;

                // Mark Staff.SyncStatus[staffDeviceId] = true (if not already true)
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<int, bool> staffSyncStatus = new Dictionary<int, bool>();

                        using (var cmd = new SqlCommand(
                            "SELECT SyncStatus FROM Staff WHERE StaffID = @staffId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@staffId", staffId ?? string.Empty);
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                staffSyncStatus = JsonConvert.DeserializeObject<Dictionary<int, bool>>(result.ToString())
                                                  ?? new Dictionary<int, bool>();
                            }
                        }

                        // Skip DB write if already true
                        bool alreadyTrue;
                        if (staffSyncStatus.TryGetValue(staffDeviceId, out alreadyTrue) && alreadyTrue)
                        {
                            transaction.Commit();
                            return false; // no change performed
                        }

                        staffSyncStatus[staffDeviceId] = true;

                        using (var updateCmd = new SqlCommand(
                            "UPDATE Staff SET SyncStatus = @syncStatus WHERE StaffID = @staffId",
                            connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(staffSyncStatus));
                            updateCmd.Parameters.AddWithValue("@staffId", staffId ?? string.Empty);
                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        LogHelper.Log($"Updated staff {staffId} sync status for device {staffDeviceId}");

                        transaction.Commit();
                        return true; // changed
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error updating staff sync status: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the AttendanceId for a staff (by StaffID).
        /// Resolves EnrollmentID from Staff table, then looks up StaffAttendanceLog by EnrollmentID.
        /// If checkIn provided matches CheckInTime it will be used, likewise checkOut for CheckOutTime.
        /// Otherwise returns the latest non-empty AttendanceId for that enrollment.
        /// </summary>
        public string GetAttendanceIdForStaff(string staffId, DateTime? checkIn = null, DateTime? checkOut = null)
        {
            if (string.IsNullOrWhiteSpace(staffId)) return null;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    int? enrollmentId = null;
                    using (var cmd = new SqlCommand("SELECT TOP 1 EnrollmentID FROM Staff WHERE StaffID = @sid", conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", staffId);
                        var r = cmd.ExecuteScalar();
                        if (r == null || r == DBNull.Value) return null;
                        enrollmentId = Convert.ToInt32(r);
                    }

                    if (!enrollmentId.HasValue) return null;

                    string sql;
                    if (checkIn.HasValue)
                    {
                        sql = "SELECT TOP 1 AttendanceId FROM StaffAttendanceLog WHERE EnrollmentID = @enr AND CheckInTime = @checkIn";
                    }
                    else if (checkOut.HasValue)
                    {
                        sql = "SELECT TOP 1 AttendanceId FROM StaffAttendanceLog WHERE EnrollmentID = @enr AND CheckOutTime = @checkOut";
                    }
                    else
                    {
                        sql = "SELECT TOP 1 AttendanceId FROM StaffAttendanceLog WHERE EnrollmentID = @enr AND AttendanceId IS NOT NULL AND LTRIM(RTRIM(AttendanceId)) <> '' ORDER BY CheckInTime DESC";
                    }

                    using (var cmd2 = new SqlCommand(sql, conn))
                    {
                        cmd2.Parameters.AddWithValue("@enr", enrollmentId.Value);
                        if (checkIn.HasValue) cmd2.Parameters.AddWithValue("@checkIn", checkIn.Value);
                        if (checkOut.HasValue) cmd2.Parameters.AddWithValue("@checkOut", checkOut.Value);

                        var result = cmd2.ExecuteScalar();
                        return result == null || result == DBNull.Value ? null : result.ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                LogHelper.Log($"[GetAttendanceIdForStaff][ERROR] StaffID={staffId}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetAttendanceIdForStaff][ERROR] StaffID={staffId}: {ex.Message}");
                return null;
            }
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

                using (var connection = new SqlConnection(connectionString))
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
      AND CONVERT(DATE, sg2.DeviceTime) = CONVERT(DATE, StaffGLog.DeviceTime)
      AND sg2.Sync = 0            -- new (still unpaired) rows just inserted (e.g. check-outs)
);";
                            using (var reopenCmd = new SqlCommand(reopenSql, connection, tx))
                            {
                                int reopened = reopenCmd.ExecuteNonQuery();
                                LogHelper.Log($"[StaffPair] Reopened {reopened} previously processed rows for re-evaluation (if any).");
                            }
                        }
                        // -------------------------------------------------------------------------------------------------------

                        // Map device types
                        var deviceTypeMap = new Dictionary<int, string>();
                        using (var mapCmd = new SqlCommand("SELECT StaffDeviceId, LOWER(ISNULL(StaffAttendanceType,'')) t FROM StaffDevices", connection, tx))
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
                                   CONVERT(DATE, DeviceTime) AS RecordDate
                            FROM StaffGLog
                            WHERE Sync = 0
                            ORDER BY EnrollmentNumber, CONVERT(DATE, DeviceTime), DeviceTime";
                        var all = new List<dynamic>();
                        using (var cmd = new SqlCommand(selectSql, connection, tx))
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
                            using (var us = new SqlCommand(updateSelected, connection, tx))
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
                            using (var ui = new SqlCommand(updateIgnored, connection, tx))
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

        public string GetStaffDeviceAttendanceAction(int staffMachineNo)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT TOP 1 StaffAttendanceType FROM StaffDevices WHERE MachineNo = @m", connection))
                    {
                        cmd.Parameters.AddWithValue("@m", staffMachineNo);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;

                        var raw = Convert.ToString(result)?.Trim();
                        if (string.IsNullOrEmpty(raw)) return null;

                        var norm = raw.ToLowerInvariant();

                        // Common normalization patterns
                        if (norm.Contains("check_in") || norm.Contains("check-in") || norm.Contains("check in"))
                        {
                            // If it also contains check_out, return combined
                            if (norm.Contains("check_out") || norm.Contains("check-out") || norm.Contains("check out") || norm.Contains("/"))
                                return "check-in/check-out";

                            return "check-in";
                        }

                        if (norm.Contains("check_out") || norm.Contains("check-out") || norm.Contains("check out"))
                        {
                            return "check-out";
                        }

                        // fallback: return the raw normalized value to preserve info
                        return norm;
                    }
                }
            }
            catch (SqlException ex)
            {
                LogHelper.Log($"[GetStaffDeviceAttendanceAction][ERROR] MachineNo={staffMachineNo}: {ex.Message}");
                return null;
            }
        }

        public string GetStaffIdByEnrollment(int enrollmentId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT TOP 1 StaffID FROM Staff WHERE EnrollmentID = @enroll", connection))
                    {
                        cmd.Parameters.AddWithValue("@enroll", enrollmentId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        return Convert.ToString(result);
                    }
                }
            }
            catch (SqlException ex)
            {
                LogHelper.Log($"[GetStaffIdByEnrollment][ERROR] Enroll={enrollmentId}: {ex.Message}");
                return null;
            }
        }

        public string GetEffectiveStaffActionForDevice(int staffMachineNo, int enrollmentId, DateTime deviceTime)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = new SqlCommand("SELECT TOP 1 StaffAttendanceType FROM StaffDevices WHERE MachineNo = @m", connection))
                    {
                        cmd.Parameters.AddWithValue("@m", staffMachineNo);
                        var raw = cmd.ExecuteScalar();
                        if (raw == null || raw == DBNull.Value) return null;

                        string type = Convert.ToString(raw)?.Trim();
                        if (string.IsNullOrEmpty(type)) return null;

                        string norm = type.ToLowerInvariant();

                        // If explicitly single-mode, return that regardless of session state
                        if (norm == "check_in" || norm == "check-in") return "check-in";
                        if (norm == "check_out" || norm == "check-out") return "check-out";

                        // Mixed / ambiguous / "both" / "check-in/check-out"
                        // Logic: Resolve based on current local DB state within a 24-hour window
                        using (var q = new SqlCommand(@"
                            SELECT TOP 1 LogID
                            FROM StaffAttendanceLog
                            WHERE EnrollmentID = @enr 
                              AND CheckOutTime IS NULL 
                              AND CheckInTime >= DATEADD(HOUR, -24, @now)
                            ORDER BY CheckInTime DESC", connection))
                        {
                            q.Parameters.AddWithValue("@enr", enrollmentId);
                            q.Parameters.AddWithValue("@now", deviceTime); // Use the current device event time as the reference
                            var result = q.ExecuteScalar();

                            if (result != null && result != DBNull.Value)
                            {
                                // Found an open session within the last 24 hours -> This new event MUST be a Check-Out
                                LogHelper.Log($"[GetEffectiveStaffAction] Open session found (LogID={result}) for Enroll={enrollmentId} within 24h. Resolving as check-out.");
                                return "check-out";
                            }

                            // No open session within 24 hours -> This new event is a Check-In
                            return "check-in";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetEffectiveStaffAction][ERROR] Machine={staffMachineNo}, Enroll={enrollmentId}: {ex.Message}");
                return "check-in"; // Fallback
            }
        }

        public DateTime? GetPairedCheckInTimeForStaff(string staffId, DateTime checkOutTime)
        {
            if (string.IsNullOrWhiteSpace(staffId)) return null;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Resolve enrollment id for the staff
                    int? enrollmentId = null;
                    using (var ecmd = new SqlCommand("SELECT TOP 1 EnrollmentID FROM Staff WHERE StaffID = @sid", connection))
                    {
                        ecmd.Parameters.AddWithValue("@sid", staffId);
                        var er = ecmd.ExecuteScalar();
                        if (er == null || er == DBNull.Value) return null;
                        enrollmentId = Convert.ToInt32(er);
                    }

                    if (!enrollmentId.HasValue) return null;

                    const string query = @"
                SELECT TOP 1 CheckInTime
                FROM StaffAttendanceLog
                WHERE EnrollmentID = @enr
                  AND CheckOutTime = @checkOut
                ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@enr", enrollmentId.Value);
                        command.Parameters.AddWithValue("@checkOut", checkOutTime.ToString("yyyy-MM-dd HH:mm:ss"));

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
            catch (SqlException ex)
            {
                LogHelper.Log($"[GetPairedCheckInTimeForStaff][ERROR] StaffID={staffId}: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetPairedCheckInTimeForStaff][ERROR] StaffID={staffId}: {ex.Message}");
            }

            return null;
        }

        public int InsertStaffCheckInByStaffId(string staffId, string checkInValue, int? deviceId = null)
        {
            if (string.IsNullOrWhiteSpace(staffId) || string.IsNullOrWhiteSpace(checkInValue))
                return 0;

            try
            {
                int? enrollmentId = null;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = new SqlCommand("SELECT TOP 1 EnrollmentID FROM Staff WHERE StaffID = @sid", connection))
                    {
                        cmd.Parameters.AddWithValue("@sid", staffId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return 0;
                        enrollmentId = Convert.ToInt32(result);
                    }

                    if (!enrollmentId.HasValue) return 0;

                    // Reuse existing InsertStaffAttendanceLog which handles duplicates and insert logic
                    int newId = InsertStaffAttendanceLog(enrollmentId.Value, checkInValue, deviceId);
                    if (newId > 0)
                        LogHelper.Log($"[InsertStaffCheckInByStaffId] Inserted check-in LogID={newId} StaffID={staffId} Enroll={enrollmentId} Time={checkInValue}");
                    else
                        LogHelper.Log($"[InsertStaffCheckInByStaffId] No insertion performed (possible duplicate) StaffID={staffId} Enroll={enrollmentId} Time={checkInValue}");

                    return newId;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[InsertStaffCheckInByStaffId][ERROR] StaffID={staffId}: {ex.Message}");
                return 0;
            }
        }

        public bool InsertStaffCheckOutByStaffId(string staffId, string checkOutValue, int? deviceId = null)
        {
            if (string.IsNullOrWhiteSpace(staffId) || string.IsNullOrWhiteSpace(checkOutValue))
                return false;

            try
            {
                int? enrollmentId = null;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = new SqlCommand("SELECT TOP 1 EnrollmentID FROM Staff WHERE StaffID = @sid", connection))
                    {
                        cmd.Parameters.AddWithValue("@sid", staffId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return false;
                        enrollmentId = Convert.ToInt32(result);
                    }
                }

                if (!enrollmentId.HasValue) return false;

                // Reuse existing UpdateStaffAttendanceLogForCheckOut which implements pairing and validation
                bool success = UpdateStaffAttendanceLogForCheckOut(enrollmentId.Value, checkOutValue, deviceId);
                LogHelper.Log($"[InsertStaffCheckOutByStaffId] StaffID={staffId} Enroll={enrollmentId} Time={checkOutValue} Success={success}");
                return success;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[InsertStaffCheckOutByStaffId][ERROR] StaffID={staffId}: {ex.Message}");
                return false;
            }
        }

        public Staff GetStaff(string staffId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        "SELECT TOP 1 StaffID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, SyncStatus FROM Staff WHERE StaffID = @StaffID", connection))
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
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting staff: {ex.Message}");
            }
            return null;
        }


        public List<Staff> GetStaffs()
        {
            var staffs = new List<Staff>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
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
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting staffs: {ex.Message}");
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

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"
MERGE Staff AS target
USING (SELECT @id AS StaffID, @name AS Name, @status AS Status, @rfid AS RFID, @picture AS Picture, @fingerprint AS Fingerprint, @enrollment_id AS EnrollmentID, @syncStatus AS SyncStatus, @createdAt AS CreatedAt, @updatedAt AS UpdatedAt) AS source
ON target.StaffID = source.StaffID
WHEN MATCHED THEN
    UPDATE SET Name = source.Name, Status = source.Status, RFID = source.RFID, Picture = source.Picture, Fingerprint = source.Fingerprint, EnrollmentID = source.EnrollmentID, SyncStatus = source.SyncStatus, UpdatedAt = source.UpdatedAt
WHEN NOT MATCHED THEN
    INSERT (StaffID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, SyncStatus, CreatedAt, UpdatedAt) VALUES (source.StaffID, source.Name, source.Status, source.RFID, source.Picture, source.Fingerprint, source.EnrollmentID, source.SyncStatus, source.CreatedAt, source.UpdatedAt);", conn, transaction))
                        {
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
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"Error upserting staff {staffId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Delete staff by ID
        public void DeleteStaffById(string staffId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand("DELETE FROM Staff WHERE StaffID = @id", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", staffId);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error deleting staff: {ex.Message}");
                        throw;
                    }
                }
            }
            LogHelper.Log($"🗑 Staff {staffId} deleted from local DB.");
        }

        // Insert a new staff device
        public void SaveStaffDevice(Device device)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string timestamp = GetTimestamp();

                        string encryptedSerial = Encrypt(device.DeviceSerial);
                        string encryptedIP = Encrypt(device.DeviceIP);

                        using (var command = new SqlCommand(@"
INSERT INTO StaffDevices 
(Department, StaffAttendanceType, DeviceSerial, DeviceIP, Port, TimeZone, Location, ConnectionType, IsActive, LastSyncTime, FaceCapacity, RFIDCapacity, FingerCapacity, MachineNo, CreatedAt, UpdatedAt, Password) 
VALUES 
(@Department, @StaffAttendanceType, @DeviceSerial, @DeviceIP, @Port, @TimeZone, @Location, @ConnectionType, @IsActive, @LastSyncTime, @FaceCapacity, @RFIDCapacity, @FingerCapacity, @MachineNo, @CreatedAt, @UpdatedAt, @Password)", connection, transaction))
                        {
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error saving staff device: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Update an existing staff device
        public void UpdateStaffDevice(Device device)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
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
WHERE StaffDeviceId = @StaffDeviceId", connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating staff device: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Delete a staff device
        public void DeleteStaffDevice(int staffDeviceId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("DELETE FROM StaffDevices WHERE StaffDeviceId = @StaffDeviceId", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@StaffDeviceId", staffDeviceId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error deleting staff device: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Load all staff devices
        public List<Device> LoadStaffDevices()
        {
            var devices = new List<Device>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
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
                                DeviceSerial = reader.IsDBNull(3) ? null : Decrypt(reader.GetString(3)),
                                DeviceIP = reader.IsDBNull(4) ? null : Decrypt(reader.GetString(4)),
                                Port = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                MachineNo = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                TimeZone = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Location = reader.IsDBNull(8) ? null : reader.GetString(8),
                                ConnectionType = reader.IsDBNull(9) ? null : reader.GetString(9),
                                IsActive = !reader.IsDBNull(10) && reader.GetBoolean(10),
                                LastSyncTime = reader.IsDBNull(11) ? null : reader.GetString(11),
                                FaceCapacity = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                                RFIDCapacity = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                                FingerCapacity = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                                Password = reader.IsDBNull(15) ? null : reader.GetString(15),
                                CreatedAt = reader.IsDBNull(16) ? null : reader.GetDateTime(16).ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = reader.IsDBNull(17) ? null : reader.GetDateTime(17).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error loading staff devices: {ex.Message}");
            }
            return devices;
        }

        public bool StaffDeviceExists(string deviceIP, int machineNo)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM StaffDevices WHERE DeviceIP = @ip AND MachineNo = @machineNo", connection))
                    {
                        cmd.Parameters.AddWithValue("@ip", Encrypt(deviceIP));
                        cmd.Parameters.AddWithValue("@machineNo", machineNo);
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error checking staff device exists: {ex.Message}");
                return false;
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT TOP 1 StaffDeviceId FROM StaffDevices WHERE MachineNo = @machineNo", connection))
                    {
                        cmd.Parameters.AddWithValue("@machineNo", machineNo);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            return Convert.ToInt32(result);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting staff device ID by machine number: {ex.Message}");
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT DeviceIP, MachineNo, Port FROM StaffDevices WHERE StaffDeviceId = @staffDeviceId", connection))
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
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting staff device details: {ex.Message}");
            }
            return (null, 0, 0);
        }

        public void DeleteStaffGLogsByStaffDeviceId(int staffDeviceId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SqlCommand("DELETE FROM StaffGLog WHERE StaffDeviceId = @StaffDeviceId", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@StaffDeviceId", staffDeviceId);
                                int rowsAffected = command.ExecuteNonQuery();
                                transaction.Commit();
                                LogHelper.Log($"✅ Deleted {rowsAffected} StaffGLog record(s) for StaffDeviceId {staffDeviceId}");
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error deleting StaffGLog records: {ex.Message}");
                        }
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
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = "DELETE FROM StaffGLog";
                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error deleting all StaffGLogs: {ex.Message}");
                        }
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
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check for duplicate record
                        using (var checkCommand = new SqlCommand(@"
            SELECT COUNT(*) FROM StaffGLog 
            WHERE StaffDeviceId = @StaffDeviceId 
            AND EnrollmentNumber = @EnrollmentNumber 
            AND DeviceTime = @DeviceTime 
            AND OriginalLine = @OriginalLine", connection, transaction))
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
                                transaction.Commit();
                                return;
                            }
                        }

                        // Insert new record
                        using (var command = new SqlCommand(@"
                INSERT INTO StaffGLog (StaffDeviceId, EnrollmentNumber, DeviceTime, OriginalLine, Sync, DeviceType, Filter, CreatedOn, UpdatedOn)
                VALUES (@StaffDeviceId, @EnrollmentNumber, @DeviceTime, @OriginalLine, @Sync, @DeviceType, @Filter, @CreatedOn, @UpdatedOn)", connection, transaction))
                        {
                            string timestamp = GetTimestamp();
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
                            transaction.Commit();
                            LogHelper.Log($"Successfully inserted StaffGLog record for enrollment {enrollmentNumber} at {deviceTime}");
                        }
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting StaffGLog record: {ex.Message}");
                        throw;
                    }
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

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // OPTIONAL index (run once; harmless if already exists)
                        using (var idx = new SqlCommand(
                            @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.StaffAttendanceLog') AND name = 'IX_StaffAttendanceLog_Enrollment_CheckIn')
                    CREATE INDEX IX_StaffAttendanceLog_Enrollment_CheckIn ON dbo.StaffAttendanceLog(EnrollmentID, CheckInTime);",
                            connection, transaction))
                        {
                            idx.ExecuteNonQuery();
                        }

                        // 1. If exact duplicate (same enrollment + same check-in timestamp) -> return existing
                        using (var dupCmd = new SqlCommand(
                            @"SELECT TOP 1 LogID FROM StaffAttendanceLog 
                      WHERE EnrollmentID = @enrollmentId AND CheckInTime = @checkIn", connection, transaction))
                        {
                            dupCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                            dupCmd.Parameters.AddWithValue("@checkIn", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            var existing = dupCmd.ExecuteScalar();
                            if (existing != null && existing != DBNull.Value)
                            {
                                transaction.Commit();
                                return Convert.ToInt32(existing);
                            }
                        }

                        // 2. Prevent multiple OPEN logs for the same calendar date:
                        using (var openSameDateCmd = new SqlCommand(
                            @"SELECT TOP 1 LogID FROM StaffAttendanceLog
                      WHERE EnrollmentID = @enrollmentId
                        AND CheckOutTime IS NULL
                        AND CONVERT(DATE, CheckInTime) = CONVERT(DATE, @checkInDate)
                      ORDER BY CheckInTime DESC", connection, transaction))
                        {
                            openSameDateCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                            openSameDateCmd.Parameters.AddWithValue("@checkInDate", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            var openSameDate = openSameDateCmd.ExecuteScalar();
                            if (openSameDate != null && openSameDate != DBNull.Value)
                            {
                                // Decide policy: either ignore or update existing open log's CheckInTime.
                                // Here we IGNORE new duplicate-day check-in to avoid orphan pairs.
                                transaction.Commit();
                                return Convert.ToInt32(openSameDate);
                            }
                        }

                        // 3. Insert new row (include Name)
                        using (var insert = new SqlCommand(
                            @"INSERT INTO StaffAttendanceLog
                      (EnrollmentID, AttendanceId, Name, CheckInTime, CheckOutTime, CreatedAt, UpdatedAt)
                      VALUES (@enrollmentId, NULL, @name, @checkIn, NULL, @createdAt, @updatedAt);
                      SELECT SCOPE_IDENTITY();", connection, transaction))
                        {
                            insert.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                            insert.Parameters.AddWithValue("@name", (object)name ?? DBNull.Value);  // Use the passed name
                            insert.Parameters.AddWithValue("@checkIn", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            insert.Parameters.AddWithValue("@createdAt", timestamp);
                            insert.Parameters.AddWithValue("@updatedAt", timestamp);

                            var result = insert.ExecuteScalar();
                            newLogId = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting staff attendance log: {ex.Message}");
                        return 0;
                    }
                }
            }
            return newLogId;
        }

        // Insert or update staff biometric log for a single staff/device
        public async Task InsertOrUpdateStaffLogAsync(string staffId, int staffDeviceId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Load staff biometric sources
                        string rfid = null;
                        bool hasFingerprint = false;
                        int? enrollmentId = null;
                        string picture = null;
                        using (var cmd = new SqlCommand(
                            "SELECT RFID, Fingerprint, EnrollmentID, Picture FROM Staff WHERE StaffID = @sid", connection, transaction))
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
                        if (biometricData.Count == 0)
                        {
                            transaction.Commit();
                            return;
                        }

                        // Check existing StaffLog
                        bool exists = false;
                        Dictionary<string, string> existingBio = null;
                        Dictionary<string, bool> existingSync = null;
                        using (var check = new SqlCommand(
                            "SELECT BiometricData, SyncStatus FROM StaffLog WHERE StaffID = @sid AND StaffDeviceId = @sdid", connection, transaction))
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

                        if (!needsUpdate)
                        {
                            transaction.Commit();
                            return;
                        }

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
                            using (var upd = new SqlCommand(
                                @"UPDATE StaffLog
                          SET EnrollmentID = @enr,
                              BiometricData = @bio,
                              SyncStatus = @sync,
                              UpdatedAt = @ts
                          WHERE StaffID = @sid AND StaffDeviceId = @sdid", connection, transaction))
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
                            using (var ins = new SqlCommand(
                                @"INSERT INTO StaffLog (StaffID, StaffDeviceId, EnrollmentID, BiometricData, SyncStatus, CreatedAt, UpdatedAt)
                          VALUES (@sid, @sdid, @enr, @bio, @sync, @ts, @ts)", connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting or updating staff log: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public async Task InsertLogsForAllStaffAsync(string staffId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get all active staff device IDs
                        var staffDeviceIds = new List<int>();
                        using (var cmd = new SqlCommand(
                            "SELECT StaffDeviceId FROM StaffDevices WHERE IsActive = 1;", connection, transaction))
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
                            transaction.Commit();
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

                        transaction.Commit();
                        LogHelper.Log($"[Staff] Log processing complete for StaffID {staffId}. Success: 1, Failed: 0, Total: {staffDeviceIds.Count}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogHelper.Log($"[Staff] Error inserting logs for StaffID {staffId}: {ex.Message}");
                        LogHelper.Log($"[Staff] Log processing complete for StaffID {staffId}. Success: 0, Failed: 1, Total: 0");
                    }
                }
            }
        }

        // Staff: fetch unsynced StaffLog entries
        public async Task<List<StaffLogEntry>> GetUnsyncedStaffLogEntriesAsync()
        {
            var result = new List<StaffLogEntry>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand(
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
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting unsynced staff log entries: {ex.Message}");
            }
            return result;
        }

        // Staff: update a single biometric sync flag in StaffLog
        public async Task UpdateStaffBiometricSyncStatusAsync(int staffLogId, string biometricType, bool isSynced, string failureReason = null, int attemptCount = 0)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<string, bool> sync = null;
                        using (var read = new SqlCommand("SELECT SyncStatus FROM StaffLog WHERE Id = @id", connection, transaction))
                        {
                            read.Parameters.AddWithValue("@id", staffLogId);
                            var raw = await read.ExecuteScalarAsync();
                            if (raw == null || raw == DBNull.Value)
                                throw new InvalidOperationException($"StaffLog entry {staffLogId} not found");

                            sync = JsonConvert.DeserializeObject<Dictionary<string, bool>>(raw.ToString()) ?? new Dictionary<string, bool>();
                        }

                        sync[biometricType] = isSynced;

                        string updateQuery = "UPDATE StaffLog SET SyncStatus = @sync, UpdatedAt = @ts";
                        if (!isSynced)
                        {
                            updateQuery += ", FailureReason = @failureReason";
                            if (attemptCount > 0)
                                updateQuery += ", AttemptCount = ISNULL(AttemptCount, 0) + @attemptCount";
                        }
                        updateQuery += " WHERE Id = @id";

                        using (var upd = new SqlCommand(updateQuery, connection, transaction))
                        {
                            upd.Parameters.AddWithValue("@sync", JsonConvert.SerializeObject(sync));
                            upd.Parameters.AddWithValue("@ts", GetTimestamp());
                            upd.Parameters.AddWithValue("@id", staffLogId);

                            if (!isSynced)
                            {
                                upd.Parameters.AddWithValue("@failureReason", failureReason ?? "Sync failed");
                                if (attemptCount > 0)
                                    upd.Parameters.AddWithValue("@attemptCount", attemptCount);
                            }

                            await upd.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating staff biometric sync status: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public async Task<List<LogBiometricChange>> VerifyAndUpdateStaffLogBiometricsAsync(string staffId)
        {
            var changes = new List<LogBiometricChange>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Ensure staff exists in StaffLog
                bool staffExistsInLog = false;
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var checkCmd = new SqlCommand("SELECT TOP 1 1 FROM StaffLog WHERE StaffID = @sid", connection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@sid", staffId);
                            staffExistsInLog = (await checkCmd.ExecuteScalarAsync()) != null;
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error checking staff existence in log: {ex.Message}");
                        return changes;
                    }
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
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand("SELECT RFID, Fingerprint, EnrollmentID, Picture FROM Staff WHERE StaffID = @sid", connection, transaction))
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
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error loading staff biometrics: {ex.Message}");
                        return changes;
                    }
                }

                var faceDir = Path.Combine(Directory.GetCurrentDirectory(), "FACE");
                var facePath = Path.Combine(faceDir, $"{staffId}.jpg");
                bool hasFace = !string.IsNullOrEmpty(picture) && File.Exists(facePath);

                // Target staff devices present in StaffLog
                var deviceIds = new List<int>();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var devCmd = new SqlCommand("SELECT DISTINCT StaffDeviceId FROM StaffLog WHERE StaffID = @sid", connection, transaction))
                        {
                            devCmd.Parameters.AddWithValue("@sid", staffId);
                            using (var r = await devCmd.ExecuteReaderAsync())
                            {
                                while (await r.ReadAsync())
                                    deviceIds.Add(r.GetInt32(0));
                            }
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error getting staff device IDs: {ex.Message}");
                        return changes;
                    }
                }

                foreach (var deviceId in deviceIds)
                {
                    int logId = 0;
                    Dictionary<string, string> biometricData = null;
                    Dictionary<string, bool> syncStatus = null;
                    int? logEnrollmentId = null;
                    bool logExists = false;

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var logCmd = new SqlCommand(
                                "SELECT Id, EnrollmentID, BiometricData, SyncStatus FROM StaffLog WHERE StaffID = @sid AND StaffDeviceId = @did",
                                connection, transaction))
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
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Error checking log existence: {ex.Message}");
                            continue;
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
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                using (var deleteCmd = new SqlCommand("DELETE FROM StaffLog WHERE Id = @id", connection, transaction))
                                {
                                    deleteCmd.Parameters.AddWithValue("@id", logId);
                                    await deleteCmd.ExecuteNonQueryAsync();
                                    transaction.Commit();
                                }
                            }
                            catch (SqlException ex)
                            {
                                transaction.Rollback();
                                Console.WriteLine($"Error deleting log: {ex.Message}");
                            }
                        }
                        continue;
                    }

                    if (updated)
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                using (var updateCmd = new SqlCommand(
                                    "UPDATE StaffLog SET BiometricData = @bio, SyncStatus = @sync, UpdatedAt = @ts WHERE Id = @id",
                                    connection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@bio", JsonConvert.SerializeObject(biometricData));
                                    updateCmd.Parameters.AddWithValue("@sync", JsonConvert.SerializeObject(syncStatus));
                                    updateCmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                                    updateCmd.Parameters.AddWithValue("@id", logId);
                                    await updateCmd.ExecuteNonQueryAsync();
                                    transaction.Commit();
                                }
                            }
                            catch (SqlException ex)
                            {
                                transaction.Rollback();
                                Console.WriteLine($"Error updating log: {ex.Message}");
                            }
                        }
                    }
                }
            }

            return changes;
        }

        // Get active staff with biometrics but no logs or mismatched logs
        public List<string> GetActiveStaffIdsWithBiometricsAndNoLogs()
        {
            var staffIds = new List<string>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    const string sql = @"
SELECT DISTINCT s.StaffID
FROM Staff s
LEFT JOIN StaffLog sl ON s.StaffID = sl.StaffID
WHERE LOWER(LTRIM(RTRIM(ISNULL(s.Status, '')))) = 'active'
  AND s.EnrollmentID IS NOT NULL
  AND (
        LTRIM(RTRIM(ISNULL(s.RFID, ''))) <> ''
     OR s.Fingerprint = 1
     OR LTRIM(RTRIM(ISNULL(s.Picture, ''))) <> ''
  )
  AND (
        sl.StaffID IS NULL
     OR (s.RFID IS NOT NULL AND sl.BiometricData NOT LIKE '%' + s.RFID + '%')
     OR (s.Picture IS NOT NULL AND sl.BiometricData NOT LIKE '%Face%')
  );";

                    using (var cmd = new SqlCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            staffIds.Add(reader.GetString(0));
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting active staff IDs with biometrics: {ex.Message}");
            }
            return staffIds;
        }


        public int InsertStaffAttendanceLog(int enrollmentId, string checkInValue, int? deviceId = null)
        {
            if (string.IsNullOrWhiteSpace(checkInValue))
                return 0;

            DateTime checkInDt;
            if (!DateTime.TryParse(checkInValue, out checkInDt))
                return 0;

            string timestamp = GetTimestamp();
            int newLogId = 0;
            // Generate a random GUID for the AttendanceId column
            string randomAttendanceId = Guid.NewGuid().ToString();

            // initial sync JSON for staff rows (same shape as AttendanceLogs)
            string initialSyncJson = JsonConvert.SerializeObject(new Dictionary<string, bool> { { "checkIn", false }, { "checkOut", false } });
            string initialDeviceJson = JsonConvert.SerializeObject(new Dictionary<string, int?> { { "checkIn", deviceId }, { "checkOut", null } });

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // OPTIONAL index (run once; harmless if already exists)
                        using (var idx = new SqlCommand(
                            @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.StaffAttendanceLog') AND name = 'IX_StaffAttendanceLog_Enrollment_CheckIn')
            CREATE INDEX IX_StaffAttendanceLog_Enrollment_CheckIn ON dbo.StaffAttendanceLog(EnrollmentID, CheckInTime);",
                            connection, transaction))
                        {
                            idx.ExecuteNonQuery();
                        }

                        // 1. If exact duplicate (same enrollment + same check-in timestamp) -> return existing
                        using (var dupCmd = new SqlCommand(
                            @"SELECT TOP 1 LogID FROM StaffAttendanceLog 
              WHERE EnrollmentID = @enrollmentId AND CheckInTime = @checkIn", connection, transaction))
                        {
                            dupCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                            dupCmd.Parameters.AddWithValue("@checkIn", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            var existing = dupCmd.ExecuteScalar();
                            if (existing != null && existing != DBNull.Value)
                            {
                                transaction.Commit();
                                return Convert.ToInt32(existing);
                            }
                        }

                        // 2. Prevent multiple OPEN logs for the same calendar date:
                        using (var openSameDateCmd = new SqlCommand(
                            @"SELECT TOP 1 LogID FROM StaffAttendanceLog
              WHERE EnrollmentID = @enrollmentId
                AND CheckOutTime IS NULL
                AND CONVERT(DATE, CheckInTime) = CONVERT(DATE, @checkInDate)
              ORDER BY CheckInTime DESC", connection, transaction))
                        {
                            openSameDateCmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                            openSameDateCmd.Parameters.AddWithValue("@checkInDate", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            var openSameDate = openSameDateCmd.ExecuteScalar();
                            if (openSameDate != null && openSameDate != DBNull.Value)
                            {
                                // Ignore new duplicate-day check-in to avoid orphan pairs.
                                transaction.Commit();
                                return Convert.ToInt32(openSameDate);
                            }
                        }

                        // 3. Insert new row (Include generated randomAttendanceId)
                        using (var insert = new SqlCommand(
                            @"INSERT INTO StaffAttendanceLog
              (EnrollmentID, AttendanceId, CheckInTime, CheckOutTime, SyncStatus, DeviceInfo, CreatedAt, UpdatedAt)
              VALUES (@enrollmentId, @attendanceId, @checkIn, NULL, @syncStatus, @deviceInfo, @createdAt, @updatedAt);
              SELECT SCOPE_IDENTITY();", connection, transaction))
                        {
                            insert.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                            insert.Parameters.AddWithValue("@attendanceId", randomAttendanceId);
                            insert.Parameters.AddWithValue("@checkIn", checkInDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            insert.Parameters.AddWithValue("@syncStatus", initialSyncJson);
                            insert.Parameters.AddWithValue("@deviceInfo", initialDeviceJson);
                            insert.Parameters.AddWithValue("@createdAt", timestamp);
                            insert.Parameters.AddWithValue("@updatedAt", timestamp);

                            var result = insert.ExecuteScalar();
                            newLogId = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting staff attendance log: {ex.Message}");
                        return 0;
                    }
                }
            }
            return newLogId;
        }



        public bool UpdateStaffAttendanceLogForCheckOut(int enrollmentId, string checkOutValue, int? deviceId = null)
        {
            if (string.IsNullOrWhiteSpace(checkOutValue))
                return false;

            DateTime checkOutDt;
            if (!DateTime.TryParse(checkOutValue, out checkOutDt))
                return false;

            string timestamp = GetTimestamp();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Ensure index exists (safe if already created)
                        using (var idx = new SqlCommand(
                            @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.StaffAttendanceLog') AND name = 'IX_StaffAttendanceLog_Enrollment_Open')
            CREATE INDEX IX_StaffAttendanceLog_Enrollment_Open ON dbo.StaffAttendanceLog(EnrollmentID, CheckOutTime, CheckInTime);",
                            connection, transaction))
                        {
                            idx.ExecuteNonQuery();
                        }

                        // 1. Try same day open check-in (must be <= checkout)
                        int? logId = null;
                        DateTime chosenCheckIn = DateTime.MinValue;
                        string existingSyncJson = null;
                        string existingDeviceJson = null;

                        using (var sameDay = new SqlCommand(
                            @"SELECT TOP 1 LogID, CheckInTime, SyncStatus, DeviceInfo FROM StaffAttendanceLog
                      WHERE EnrollmentID = @enrollmentId
                        AND CheckOutTime IS NULL
                        AND CONVERT(DATE, CheckInTime) = CONVERT(DATE, @checkOut)
                        AND CheckInTime <= @checkOut
                      ORDER BY CheckInTime DESC", connection, transaction))
                        {
                            sameDay.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                            sameDay.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            using (var r = sameDay.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    logId = r.GetInt32(0);
                                    DateTime.TryParse(r.GetValue(1).ToString(), out chosenCheckIn);
                                    existingSyncJson = r.IsDBNull(2) ? null : r.GetString(2);
                                    existingDeviceJson = r.IsDBNull(3) ? null : r.GetString(3);
                                }
                            }
                        }

                        // 2. If not found, try previous day (overnight session)
                        if (!logId.HasValue)
                        {
                            using (var prevDay = new SqlCommand(
                                @"SELECT TOP 1 LogID, CheckInTime, SyncStatus, DeviceInfo FROM StaffAttendanceLog
                          WHERE EnrollmentID = @enrollmentId
                            AND CheckOutTime IS NULL
                            AND CONVERT(DATE, CheckInTime) = CONVERT(DATE, DATEADD(DAY, -1, @checkOut))
                            AND CheckInTime <= @checkOut
                          ORDER BY CheckInTime DESC", connection, transaction))
                            {
                                prevDay.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                                prevDay.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                                using (var r = prevDay.ExecuteReader())
                                {
                                    if (r.Read())
                                    {
                                        logId = r.GetInt32(0);
                                        DateTime.TryParse(r.GetValue(1).ToString(), out chosenCheckIn);
                                        existingSyncJson = r.IsDBNull(2) ? null : r.GetString(2);
                                        existingDeviceJson = r.IsDBNull(3) ? null : r.GetString(3);
                                    }
                                }
                            }
                        }

                        // 3. Fallback: any open log whose CheckInTime <= checkout (prevent pairing with future check-in)
                        if (!logId.HasValue)
                        {
                            using (var anyOpen = new SqlCommand(
                                @"SELECT TOP 1 LogID, CheckInTime, SyncStatus, DeviceInfo FROM StaffAttendanceLog
                          WHERE EnrollmentID = @enrollmentId
                            AND CheckOutTime IS NULL
                            AND CheckInTime <= @checkOut
                          ORDER BY CheckInTime DESC", connection, transaction))
                            {
                                anyOpen.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                                anyOpen.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                                using (var r = anyOpen.ExecuteReader())
                                {
                                    if (r.Read())
                                    {
                                        logId = r.GetInt32(0);
                                        DateTime.TryParse(r.GetValue(1).ToString(), out chosenCheckIn);
                                        existingSyncJson = r.IsDBNull(2) ? null : r.GetString(2);
                                        existingDeviceJson = r.IsDBNull(3) ? null : r.GetString(3);
                                    }
                                }
                            }
                        }

                        if (!logId.HasValue)
                        {
                            // No suitable open check-in to pair -> ignore
                            transaction.Commit();
                            return false;
                        }

                        // Final guard: do not set a checkout earlier than its check-in
                        if (chosenCheckIn != DateTime.MinValue && checkOutDt < chosenCheckIn)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        // Parse / update SyncStatus JSON: ensure keys exist and mark checkOut as needing sync (false)
                        Dictionary<string, bool> syncDict;
                        try
                        {
                            syncDict = !string.IsNullOrWhiteSpace(existingSyncJson)
                                ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(existingSyncJson) ?? new Dictionary<string, bool>()
                                : new Dictionary<string, bool>();
                        }
                        catch
                        {
                            syncDict = new Dictionary<string, bool>();
                        }

                        if (!syncDict.ContainsKey("checkIn")) syncDict["checkIn"] = false;
                        syncDict["checkOut"] = false;

                        Dictionary<string, int?> deviceDict;
                        try
                        {
                            deviceDict = !string.IsNullOrWhiteSpace(existingDeviceJson)
                                ? JsonConvert.DeserializeObject<Dictionary<string, int?>>(existingDeviceJson) ?? new Dictionary<string, int?>()
                                : new Dictionary<string, int?>();
                        }
                        catch
                        {
                            deviceDict = new Dictionary<string, int?>();
                        }
                        if (!deviceDict.ContainsKey("checkIn")) deviceDict["checkIn"] = null;
                        deviceDict["checkOut"] = deviceId;

                        using (var update = new SqlCommand(
                            @"UPDATE StaffAttendanceLog
                      SET CheckOutTime = @checkOut, SyncStatus = @syncStatus, DeviceInfo = @deviceInfo, UpdatedAt = @updatedAt
                      WHERE LogID = @logId AND CheckOutTime IS NULL", connection, transaction))
                        {
                            update.Parameters.AddWithValue("@checkOut", checkOutDt.ToString("yyyy-MM-dd HH:mm:ss"));
                            update.Parameters.AddWithValue("@syncStatus", JsonConvert.SerializeObject(syncDict));
                            update.Parameters.AddWithValue("@deviceInfo", JsonConvert.SerializeObject(deviceDict));
                            update.Parameters.AddWithValue("@updatedAt", timestamp);
                            update.Parameters.AddWithValue("@logId", logId.Value);
                            var rowsAffected = update.ExecuteNonQuery();
                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating staff attendance log for check-out: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public List<dynamic> GetAllStaffAttendanceLogs()
        {
            var results = new List<dynamic>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
        SELECT TOP 10000
            sal.LogID                        AS LogID,
            sal.EnrollmentID                 AS EnrollmentID,
            s.StaffID                        AS AttendeeID,
            COALESCE(s.Name, '')             AS Name,
            sal.AttendanceId                 AS AttendanceId,
            sal.SyncStatus                   AS SyncStatus,
            sal.CheckInTime                  AS CheckInTime,
            sal.CheckOutTime                 AS CheckOutTime
        FROM StaffAttendanceLog sal
        LEFT JOIN Staff s 
          ON s.EnrollmentID = sal.EnrollmentID
        ORDER BY 
            COALESCE(sal.CheckInTime, sal.CheckOutTime) DESC,
            sal.LogID DESC;";

                    using (var cmd = new SqlCommand(sql, connection))
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
                                AttendanceId = reader.IsDBNull(reader.GetOrdinal("AttendanceId")) ? string.Empty : reader.GetString(reader.GetOrdinal("AttendanceId")),
                                SyncStatus = reader.IsDBNull(reader.GetOrdinal("SyncStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("SyncStatus")),
                                CheckInTime = reader.IsDBNull(reader.GetOrdinal("CheckInTime")) ? string.Empty : reader.GetDateTime(reader.GetOrdinal("CheckInTime")).ToString("yyyy-MM-dd HH:mm:ss"),
                                CheckOutTime = reader.IsDBNull(reader.GetOrdinal("CheckOutTime")) ? string.Empty : reader.GetDateTime(reader.GetOrdinal("CheckOutTime")).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error getting all staff attendance logs: {ex.Message}");
            }
            return results;
        }


        

        public bool UpdateStaffAttendanceLogSyncStatus(
            int enrollmentId,
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

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string existingJson = null;
                        int targetLogId = 0;
                        bool foundRecord = false;

                        // For check-in: require both attendanceId and checkInTime to match
                        if (hasCheckIn && !string.IsNullOrWhiteSpace(attendanceId) && checkInTime.HasValue)
                        {
                            using (var select = new SqlCommand(
                                "SELECT TOP 1 LogID, SyncStatus FROM StaffAttendanceLog WHERE AttendanceId = @aid AND EnrollmentID = @enr AND CheckInTime = @checkInTime", connection, transaction))
                            {
                                select.Parameters.AddWithValue("@aid", attendanceId);
                                select.Parameters.AddWithValue("@enr", enrollmentId);
                                select.Parameters.AddWithValue("@checkInTime", checkInTime.Value);
                                using (var reader = select.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        targetLogId = reader.GetInt32(0);
                                        existingJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                                        foundRecord = true;
                                    }
                                }
                            }
                        }
                        // For check-out: require both attendanceId and checkOutTime to match
                        else if (hasCheckOut && !string.IsNullOrWhiteSpace(attendanceId) && checkOutTime.HasValue)
                        {
                            using (var select = new SqlCommand(
                                "SELECT TOP 1 LogID, SyncStatus FROM StaffAttendanceLog WHERE AttendanceId = @aid AND EnrollmentID = @enr AND CheckOutTime = @checkOutTime", connection, transaction))
                            {
                                select.Parameters.AddWithValue("@aid", attendanceId);
                                select.Parameters.AddWithValue("@enr", enrollmentId);
                                select.Parameters.AddWithValue("@checkOutTime", checkOutTime.Value);
                                using (var reader = select.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        targetLogId = reader.GetInt32(0);
                                        existingJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                                        foundRecord = true;
                                    }
                                }
                            }
                        }
                        // For check-in without attendanceId, fallback to latest log for enrollment
                        else if (hasCheckIn && string.IsNullOrWhiteSpace(attendanceId))
                        {
                            using (var selectLatest = new SqlCommand(
                                @"SELECT TOP 1 LogID, SyncStatus 
                  FROM StaffAttendanceLog 
                  WHERE EnrollmentID = @enr 
                  ORDER BY CheckInTime DESC", connection, transaction))
                            {
                                selectLatest.Parameters.AddWithValue("@enr", enrollmentId);
                                using (var reader = selectLatest.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        targetLogId = reader.GetInt32(0);
                                        existingJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                                        foundRecord = true;
                                    }
                                }
                            }
                        }

                        if (!foundRecord)
                        {
                            transaction.Rollback();
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
                                transaction.Rollback();
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
                                transaction.Rollback();
                                return false;
                            }
                        }

                        if (valueChanged)
                        {
                            using (var update = new SqlCommand(
                                "UPDATE StaffAttendanceLog SET SyncStatus = @json, UpdatedAt = @ts WHERE LogID = @lid", connection, transaction))
                            {
                                update.Parameters.AddWithValue("@json", JsonConvert.SerializeObject(flags));
                                update.Parameters.AddWithValue("@ts", GetTimestamp());
                                update.Parameters.AddWithValue("@lid", targetLogId);
                                int rowsUpdated = update.ExecuteNonQuery();

                                if (rowsUpdated > 0)
                                {
                                    transaction.Commit();
                                    return true;
                                }
                                else
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }

                        transaction.Rollback();
                        return false;
                    }
                    catch (SqlException)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

public bool UpdateStaffAttendanceLogFromCloud(
    int enrollmentId,
    DateTime checkInTime,
    DateTime? checkOutTime,
    string attendanceId,
    bool setCheckInSynced,
    bool setCheckOutSynced)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Find the log row by enrollment + exact check-in time
                            int logId = 0;
                            string existingSyncStatusJson = null;

                            using (var cmd = new SqlCommand(
                                @"SELECT TOP 1 LogID, SyncStatus 
                  FROM StaffAttendanceLog
                  WHERE EnrollmentID = @enrollmentId
                    AND CheckInTime = @checkIn", connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                                cmd.Parameters.AddWithValue("@checkIn", checkInTime);

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
                                transaction.Rollback();
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

                            using (var update = new SqlCommand(
                                @"UPDATE StaffAttendanceLog
                  SET AttendanceId = @attendanceId,
                      SyncStatus   = @syncStatus,
                      UpdatedAt    = @updatedAt
                  WHERE LogID = @logId", connection, transaction))
                            {
                                update.Parameters.AddWithValue("@attendanceId", (object)attendanceId ?? DBNull.Value);
                                update.Parameters.AddWithValue("@syncStatus", newSyncJson);
                                update.Parameters.AddWithValue("@updatedAt", timestamp);
                                update.Parameters.AddWithValue("@logId", logId);

                                update.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (SqlException)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }











        /// <summary>
        /// Get latest log entry for a member to display attempt count and failure reason
        /// </summary>
        public dynamic GetLatestLogForMember(int enrollmentId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(
                        @"SELECT TOP 1 AttemptCount, FailureReason
                          FROM Log
                          WHERE EnrollmentID = @enrollId
                          AND FailureReason IS NOT NULL
                          ORDER BY CreatedAt DESC",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@enrollId", enrollmentId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new
                                {
                                    AttemptCount = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                    FailureReason = reader.IsDBNull(1) ? null : reader.GetString(1)
                                };
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Get latest staff log entry to display attempt count and failure reason
        /// </summary>
        public dynamic GetLatestStaffLogForEnrollment(int enrollmentId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(
                        @"SELECT TOP 1 AttemptCount, FailureReason
                          FROM StaffLog
                          WHERE EnrollmentID = @enrollId
                          AND FailureReason IS NOT NULL
                          ORDER BY CreatedAt DESC",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@enrollId", enrollmentId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new
                                {
                                    AttemptCount = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                    FailureReason = reader.IsDBNull(1) ? null : reader.GetString(1)
                                };
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Get latest offline member log entry to display attempt count and failure reason
        /// </summary>
        public dynamic GetLatestOfflineLogForMember(string clientId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(
                        @"SELECT TOP 1 AttemptCount, FailureReason
                          FROM OfflineMemberLog
                          WHERE ClientId = @clientId
                          AND FailureReason IS NOT NULL
                          ORDER BY CreatedAt DESC",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@clientId", clientId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new
                                {
                                    AttemptCount = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                    FailureReason = reader.IsDBNull(1) ? null : reader.GetString(1)
                                };
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        // Returns the current FailureReason stored for a specific log row by its Id.
        // tableName must be one of: "Log", "StaffLog", "OfflineMemberLog"
        public string GetLogFailureReasonById(int logId, string tableName)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(
                        $"SELECT FailureReason FROM {tableName} WHERE Id = @id",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("@id", logId);
                        var result = cmd.ExecuteScalar();
                        return result == DBNull.Value ? null : result as string;
                    }
                }
            }
            catch { return null; }
        }
    }
}
