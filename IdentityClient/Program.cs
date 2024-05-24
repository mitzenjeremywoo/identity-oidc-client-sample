using System.Diagnostics;
using System.Net;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;

namespace OidcClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new OidcClientOptions
            {
                Authority = "https://your-identity-provider", // Replace with your identity provider URL
                ClientId = "your-client-id", // Replace with your client ID
                ClientSecret = "your-client-secret", // Replace with your client secret
                RedirectUri = "http://localhost:7890/", // Must match the redirect URI registered with the identity provider
                Scope = "openid profile email", // Add any additional scopes you need
                Browser = new SystemBrowser(4200), // Port for the local redirect URI
                
                //ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                //Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode
            };

            var oidcClient = new OidcClient(options);

            try
            {
                var loginRequest = new LoginRequest();
                var loginResult = await oidcClient.LoginAsync(loginRequest);

                if (loginResult.IsError)
                {
                    Console.WriteLine($"An error occurred: {loginResult.Error}");
                }
                else
                {
                    Console.WriteLine("Access Token: " + loginResult.AccessToken);
                    Console.WriteLine("Identity Token: " + loginResult.IdentityToken);
                    Console.WriteLine("Refresh Token: " + loginResult.RefreshToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
            }
        }
    }

    public class SystemBrowser : IBrowser
    {
        private readonly int _port;

        public SystemBrowser(int port)
        {
            _port = port;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, System.Threading.CancellationToken cancellationToken = default)
        {
            string redirectUri = $"http://localhost:{_port}/";
            var listener = new HttpListener();
            listener.Prefixes.Add(redirectUri);
            listener.Start();

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = options.StartUrl,
                    UseShellExecute = true
                });

                var context = await listener.GetContextAsync();

                var response = context.Response;
                string responseString = "<html><head><meta http-equiv='refresh' content='10;url=https://www.example.com'></head><body>Please return to the app.</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                await responseOutput.WriteAsync(buffer, 0, buffer.Length);
                responseOutput.Close();

                return new BrowserResult
                {
                    Response = context.Request.Url.ToString(),
                    ResultType = BrowserResultType.Success
                };
            }
            catch (Exception ex)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = ex.Message
                };
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}