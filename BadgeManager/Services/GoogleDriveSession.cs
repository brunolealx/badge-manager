using Google.Apis.Drive.v3;

namespace BadgeManager.Services
{
    public static class GoogleDriveSession
    {
        public static DriveService? DriveService { get; set; }
    }
}