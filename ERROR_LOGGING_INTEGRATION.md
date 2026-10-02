# Device Error Logging Integration Guide

## Overview
When device operations fail (e.g., setting user face, creating user), capture error details and log them to the database.

## Database Schema Changes
Run: `migration_add_error_logging.sql`
- Adds `AttemptCount` column (int, default 0)
- Adds `FailureReason` column (nvarchar(max))
- To tables: `log`, `offlinememberlog`, `stafflog`

## Using DeviceErrorLogger

### When Device Operation Fails:

```csharp
// Receive device response
JObject deviceResponse = JsonConvert.DeserializeObject<JObject>(responseData);

if (!deviceResponse["result"].Value<bool>())
{
    // Parse error details
    string errorReason = DeviceErrorLogger.ParseDeviceError(deviceResponse);
    
    // Log failure
    DeviceErrorLogger.LogDeviceOperationFailure(
        _dbHelper,
        "log",  // or "offlinememberlog" or "stafflog"
        enrollmentId,
        deviceSN,
        "setuserinfo",  // operation type
        errorReason,
        attemptCount: 1
    );
    
    LogHelper.Log($"[DEVICE-ERROR] {errorReason}");
}
```

## Error Format Captured
```
Reason:12 | Message:face is double | ConflictID:23783
```

## Where to Add Integration

### 1. AttendenceServerFunctions.cs
- When `setuserinfo` fails
- When `setface` fails
- When `setfp` fails

### 2. StaffAttendenceServerFunctions.cs
- When staff user creation fails on device

### 3. OfflineEnrollmentSubscriptionService.cs
- When offline member sync to device fails

### 4. AttendanceLogSyncService.cs / StaffAttendanceLogSyncService.cs
- When updating user on device fails

## Example Implementation

Find code like:
```csharp
if (ok) success++;
```

And add:
```csharp
if (ok) 
{
    success++;
}
else
{
    string errorReason = DeviceErrorLogger.ParseDeviceError(responseObject);
    DeviceErrorLogger.LogDeviceOperationFailure(
        _dbHelper,
        "log",
        enrollId,
        deviceSN,
        "operation_name",
        errorReason
    );
}
```

## Database Query to View Errors

```sql
-- View all failed operations
SELECT 
    EnrollmentID,
    DeviceSN,
    AttemptCount,
    FailureReason,
    LogTime
FROM log
WHERE FailureReason IS NOT NULL
ORDER BY LogTime DESC
```
