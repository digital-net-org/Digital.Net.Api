using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Endpoints.Dto;

namespace Digital.Net.Tests.Core.Sdk;

public static class SitemapApi
{
    public const string BaseUrl = "/cms/sitemaps";

    public static async Task<HttpResponseMessage> GetSitemapData(this HttpClient client) =>
        await client.GetAsync($"{BaseUrl}/data");
}
