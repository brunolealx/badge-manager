using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Windows;


namespace BadgeManager.Services
{
    public class GoogleDriveAuthService
    {
        public async Task<DriveService?> LoginAsync()
        {
            try
            {
                string[] scopes = { DriveService.Scope.Drive };

                using var stream = new FileStream(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Credentials", "credentials.json"),
                    FileMode.Open,
                    FileAccess.Read);

                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("BadgeManager.TokenStore", true));

                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Badge Manager"
                });
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro Google OAuth");
                return null;
            }
        }
    }
}