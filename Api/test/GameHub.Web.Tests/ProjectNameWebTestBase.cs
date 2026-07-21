using Abp.AspNetCore.TestBase;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Json;
using Abp.MultiTenancy;
using Abp.Web.Models;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Eaf.Middleware.Web.Models.TokenAuth;
using Eaf.Middleware.Web.Serilog;
using GameHub.EntityFrameworkCore;
using GameHub.Web.Startup;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameHub.Web.Tests
{
    public abstract class ProjectNameWebTestBase : AbpAspNetCoreIntegratedTestBase<Startup>
    {
        protected static readonly string ContentRootFolder;

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base
                .CreateWebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                //.UseEafConfiguration(prefix: "ProjectName_")
                .UseEafKeyVault(opt => opt.Provider = KeyVault.EnumKeyVault.None)
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(WebHostModule).Assembly.FullName);
        }

        #region Get response

        protected async Task<T> GetResponseAsObjectAsync<T>(string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var strResponse = await GetResponseAsStringAsync(url, expectedStatusCode);
            return JsonSerializer.Deserialize<T>(strResponse, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        protected async Task<string> GetResponseAsStringAsync(string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var response = await GetResponseAsync(url, expectedStatusCode);
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<HttpResponseMessage> GetResponseAsync(string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var response = await Client.GetAsync(url);
            response.StatusCode.ShouldBe(expectedStatusCode);
            return response;
        }

        #endregion Get response

        #region Authenticate

        /// <summary>
        /// /api/TokenAuth/Authenticate
        /// TokenAuthController
        /// </summary>
        /// <param name="tenancyName"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected async Task AuthenticateAsync(string tenancyName, AuthenticateModel input)
        {
            if (tenancyName.IsNullOrWhiteSpace())
            {
                var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
                if (tenant != null)
                {
                    AbpSession.TenantId = tenant.Id;
                    Client.DefaultRequestHeaders.Add("Eaf.TenantId", tenant.Id.ToString());  //Set TenantId
                }
            }

            var response = await Client.PostAsync("/api/TokenAuth/Authenticate",
                new StringContent(input.ToJsonString(), Encoding.UTF8, "application/json"));
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<AjaxResponse<AuthenticateResultModel>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Result.AccessToken);

            AbpSession.UserId = result.Result.UserId;
        }

        #endregion Authenticate

        #region Login

        protected void LoginAsHostAdmin()
        {
            LoginAsHost(AbpUserBase.AdminUserName);
        }

        protected void LoginAsDefaultTenantAdmin()
        {
            LoginAsTenant(AbpTenantBase.DefaultTenantName, AbpUserBase.AdminUserName);
        }

        protected void LoginAsHost(string userName)
        {
            AbpSession.TenantId = null;

            var user =
                UsingDbContext(
                    context =>
                        context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new InvalidOperationException("There is no user: " + userName + " for host.");
            }

            AbpSession.UserId = user.Id;
        }

        protected void LoginAsTenant(string tenancyName, string userName)
        {
            var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
            if (tenant == null)
            {
                throw new InvalidOperationException("There is no tenant: " + tenancyName);
            }

            AbpSession.TenantId = tenant.Id;

            var user =
                UsingDbContext(
                    context =>
                        context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new InvalidOperationException("There is no user: " + userName + " for tenant: " + tenancyName);
            }

            AbpSession.UserId = user.Id;
        }

        #endregion Login

        #region UsingDbContext

        protected void UsingDbContext(Action<ProjectNameDbContext> action)
        {
            using (var context = IocManager.Resolve<ProjectNameDbContext>())
            {
                action(context);
                context.SaveChanges();
            }
        }

        protected T UsingDbContext<T>(Func<ProjectNameDbContext, T> func)
        {
            T result;

            using (var context = IocManager.Resolve<ProjectNameDbContext>())
            {
                result = func(context);
                context.SaveChanges();
            }

            return result;
        }

        protected async Task UsingDbContextAsync(Func<ProjectNameDbContext, Task> action)
        {
            using (var context = IocManager.Resolve<ProjectNameDbContext>())
            {
                await action(context);
                await context.SaveChangesAsync(true);
            }
        }

        protected async Task<T> UsingDbContextAsync<T>(Func<ProjectNameDbContext, Task<T>> func)
        {
            T result;

            using (var context = IocManager.Resolve<ProjectNameDbContext>())
            {
                result = await func(context);
                await context.SaveChangesAsync(true);
            }

            return result;
        }

        #endregion UsingDbContext

        #region ParseHtml

        protected IHtmlDocument ParseHtml(string htmlString)
        {
            return new HtmlParser().ParseDocument(htmlString);
        }

        #endregion ParseHtml
    }
}