
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SubwaySurfers3D.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerInfoController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        [HttpGet]
        public async Task<IActionResult> GetServerInfo()
        {
            string instanceId = "LOCAL-DEV";
            string privateIp = "Not running on EC2";
            string serverName = Environment.MachineName;

            try
            {
                // Get IMDSv2 token
                using var tokenRequest = new HttpRequestMessage(
                    HttpMethod.Put,
                    "http://169.254.169.254/latest/api/token"
                );

                tokenRequest.Headers.Add(
                    "X-aws-ec2-metadata-token-ttl-seconds",
                    "21600"
                );

                using var tokenResponse =
                    await httpClient.SendAsync(tokenRequest);

                tokenResponse.EnsureSuccessStatusCode();

                string token =
                    await tokenResponse.Content.ReadAsStringAsync();

                // Get EC2 Instance ID
                using var idRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    "http://169.254.169.254/latest/meta-data/instance-id"
                );

                idRequest.Headers.Add(
                    "X-aws-ec2-metadata-token",
                    token
                );

                using var idResponse =
                    await httpClient.SendAsync(idRequest);

                idResponse.EnsureSuccessStatusCode();

                instanceId =
                    await idResponse.Content.ReadAsStringAsync();

                // Get EC2 Private IP
                using var ipRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    "http://169.254.169.254/latest/meta-data/local-ipv4"
                );

                ipRequest.Headers.Add(
                    "X-aws-ec2-metadata-token",
                    token
                );

                using var ipResponse =
                    await httpClient.SendAsync(ipRequest);

                ipResponse.EnsureSuccessStatusCode();

                privateIp =
                    await ipResponse.Content.ReadAsStringAsync();

                serverName = $"SubwayGame-{instanceId}";
            }
            catch
            {
                // Local development or metadata unavailable
            }

            return Ok(new
            {
                instanceId,
                privateIp,
                serverName,
                status = "Running",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
