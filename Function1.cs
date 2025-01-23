using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace RedirectFunctionApp
{
    public class RedirectFunction
    {
        private readonly ILogger _logger;

        public RedirectFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RedirectFunction>();
        }

        [Function("RedirectFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Processing redirect request.");

            // Extract platform from sec-ch-ua-platform header
            string platform = null;
            if (req.Headers.TryGetValues("sec-ch-ua-platform", out var platformValues))
            {
                platform = platformValues.FirstOrDefault()?.Trim('"').ToLower(); // Remove quotes and normalize
                _logger.LogInformation($"Extracted Platform: \"{platform}\"");
            }

            // Android detection
            if (platform == "android")
            {
                _logger.LogInformation("Platform is Android. Redirecting to Google Play Store.");
                return RedirectResponse(req, "https://play.google.com/store/apps/details?id=com.mobiledemand.xscale");
            }

            // Windows detection
            if (platform == "windows")
            {
                _logger.LogInformation("Platform is Windows. Showing custom HTML response with redirect.");
                return await CustomWindowsResponse(req);
            }

            // Fallback for other platforms
            _logger.LogWarning("Unsupported platform detected. Redirecting to Apple App Store as fallback.");
            return RedirectResponse(req, "https://apps.apple.com/us/app/xscale/id6477849456");
        }

        private HttpResponseData RedirectResponse(HttpRequestData req, string url)
        {
            var response = req.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Add("Location", url);
            _logger.LogInformation($"Redirecting to: {url}");
            return response;
        }

        private async Task<HttpResponseData> CustomWindowsResponse(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Custom HTML content
            string htmlContent = @"
<html>
<head>
    <title>Windows OS detected...redirecting...</title>
    <link rel=themeData href='4d_redirect_files/themedata.thmx'>
    <link rel=colorSchemeMapping href='4d_redirect_files/colorschememapping.xml'>
    <meta http-equiv='refresh' content='7;url=https://www.4dmobilesoft.com' />
    <style>
        body {
            font-family: Arial, sans-serif;
        }
        h1 {
            font-family: Arial, sans-serif;
            text-align: center;
        }
        img {
            display: block;
            margin: 0 auto;
            width: 50%; /* Scale the image to 50% of its original width */
            height: auto; /* Maintain aspect ratio */
        }
    </style>
</head>
<body lang='EN-US' link='#467886' vlink='#96607D' style='tab-interval:.5in; word-wrap:break-word'>
    <div class='WordSection1'>
        <br>
        <h1>Microsoft Windows OS detected. Try xDIM for Windows!</h1>
        <p><img src='https://4dmobilesoft.com/wp-content/uploads/2023/10/4D-Mobile-Main-Banner-5icons.jpg'></p>
        <p class='MsoNormal' align='center' style='text-align:center'><o:p>&nbsp;</o:p></p>
        <p class='MsoNormal' align='center' style='text-align:center'>You will be
            redirected to <a href='http://www.4dmobilesoft.com'>www.4dmobilesoft.com</a>
            shortly.</p>
        <p class='MsoNormal' align='center' style='text-align:center'>Click <a
                href='https://www.4dmobilesoft.com/'>here</a> if not redirected.</p>
    </div>
</body>
</html>";

            await response.WriteStringAsync(htmlContent, System.Text.Encoding.UTF8);
            response.Headers.Add("Content-Type", "text/html");
            _logger.LogInformation("Custom HTML response sent for Windows platform.");
            return response;
        }
    }
}
