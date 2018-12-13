// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE in the project root for license information.
using graph_tutorial.TokenStorage;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace graph_tutorial.Helpers
{
    public static class GraphHelper
    {
        // Load configuration settings from PrivateSettings.config
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];

        public static async Task<User> GetUserDetailsAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }));

            return await graphClient.Me.Request().GetAsync();
        }

        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var events = await graphClient.Me.Events.Request()
                .Select("subject,organizer,start,end, location")
                .OrderBy("createdDateTime DESC")
                .GetAsync();

            return events.CurrentPage;
        }

        public static async Task<IEnumerable<Notebook>> GetNotebookAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var notebooks = await graphClient.Me.Onenote.Notebooks.Request()
                //.Select("displayName, createdBy, lastModifiedBy, id")
                .GetAsync();

            return notebooks.AsEnumerable();
        }

        public static async Task CreateNotebookAsync(string nomeNotebook)
        {
            var graphClient = GetAuthenticatedClient();

            await graphClient.Me.Onenote.Notebooks.Request().AddAsync(new Notebook
            {
                DisplayName = nomeNotebook
            });
        }

        public static async Task<IEnumerable<OnenoteSection>> GetSectionAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var sections = await graphClient.Me.Onenote.Sections.Request().GetAsync();

            return sections.AsEnumerable();
        }
        
        public static async Task CreateSectionAsync(OnenoteSection section)
        {
            var graphClient = GetAuthenticatedClient();
            
            await graphClient.Me.Onenote.Notebooks[section.ParentNotebook.Id].Sections.Request()
            .AddAsync(new OnenoteSection {
                DisplayName = section.DisplayName
            });
        }

        public static async Task<IEnumerable<OnenotePage>> GetPagesAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var pages = await graphClient.Me.Onenote.Pages.Request().GetAsync();

            return pages.AsEnumerable();
        }

        public static async Task CreatePageAsync(OnenotePage page)
        {
            var graphClient = GetAuthenticatedClient();

            string htmlBody = $"<!DOCTYPE html><html><head><title>{page.Title}</title></head>";
            byte[] byteArray = Encoding.ASCII.GetBytes(htmlBody);

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                await graphClient.Me.Onenote.Sections[page.ParentSection.Id].Pages.Request()
                .AddAsync(stream, "text/html");
            }
        }

        private static GraphServiceClient GetAuthenticatedClient()
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        // Get the signed in user's id and create a token cache
                        string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                        SessionTokenStore tokenStore = new SessionTokenStore(signedInUserId,
                            new HttpContextWrapper(HttpContext.Current));

                        var idClient = new ConfidentialClientApplication(
                            appId, redirectUri, new ClientCredential(appSecret),
                            tokenStore.GetMsalCacheInstance(), null);

                        var accounts = await idClient.GetAccountsAsync();

                        // By calling this here, the token can be refreshed
                        // if it's expired right before the Graph call is made
                        var result = await idClient.AcquireTokenSilentAsync(
                            graphScopes.Split(' '), accounts.FirstOrDefault());

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
        }
    }
}