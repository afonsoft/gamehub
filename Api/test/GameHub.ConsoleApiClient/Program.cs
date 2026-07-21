using Abp.Application.Services.Dto;
using Abp.Json;
using Abp.Web.Models;
using Eaf.Middleware.Authorization.Users.Dto;
using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GameHub.ConsoleApiClient
{
    /*
     * This is a sample code to create an IdentityServer4 client and use ResourceOwnerPassword flow to call an API.
     * Enable IdentityServer from appsettings.json of Web.Host/Web.Mvc project first.
     */

    internal static class Program
    {
        private const string ServerUrlBase = "https://localhost:8001/"; // NOSONAR

        // If you have changed "Configuration.MultiTenancy.TenantIdResolveKey" in your web app, use the same value here.
        private const string TenantIdResolveKey = "Eaf.TenantId";

        private static void Main(string[] args)
        {
            RunDemoAsync().Wait();
            Console.ReadLine();
        }

        public static async Task RunDemoAsync()
        {
            var accessToken = await GetAccessTokenViaOwnerPasswordAsync();
            await GetUsersListAsync(accessToken);
        }

        private static async Task<string> GetAccessTokenViaOwnerPasswordAsync()
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(ServerUrlBase);
            if (disco.IsError)
            {
                throw new InvalidOperationException(disco.Error);
            }

            client.DefaultRequestHeaders.Add("Eaf.TenantId", "1");  //Set TenantId
            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = GetRequiredEnvironmentVariable("ConsoleApiClient_ClientSecret"),

                Scope = "default-api",

                UserName = "admin",
                Password = GetRequiredEnvironmentVariable("ConsoleApiClient_Password")
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Error: ");
                Console.WriteLine(tokenResponse.Error);
            }

            Console.WriteLine(tokenResponse.Json);

            return tokenResponse.AccessToken;
        }

        private static async Task GetUsersListAsync(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.SetBearerToken(accessToken);

                var response = await client.GetAsync($"{ServerUrlBase}api/services/app/user/getUsers");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var ajaxResponse = JsonConvert.DeserializeObject<AjaxResponse<PagedResultDto<UserListDto>>>(content);
                if (!ajaxResponse.Success)
                {
                    throw new InvalidOperationException(ajaxResponse.Error?.Message ?? "Remote service throws exception!");
                }

                Console.WriteLine();
                Console.WriteLine("Total user count: " + ajaxResponse.Result.TotalCount);
                Console.WriteLine();

                foreach (var user in ajaxResponse.Result.Items)
                {
                    Console.WriteLine($"### UserId: {user.Id}, UserName: {user.UserName}");
                    Console.WriteLine(user.ToJsonString(indented: true));
                }
            }
        }

        private static string GetRequiredEnvironmentVariable(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException($"Environment variable '{name}' is required.");
            return value;
        }
    }
}