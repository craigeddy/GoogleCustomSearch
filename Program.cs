using Google.Apis.CustomSearchAPI.v1.Data;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World! from the Google Search experiment");

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();
var Configuration = builder.Build();

// add the following to your user secrets:
var customSearchEngineID = Configuration["customSearchEngineID"];
var apiKey = Configuration["apiKey"];

var service = new Google.Apis.CustomSearchAPI.v1.CustomSearchAPIService(
    new Google.Apis.Services.BaseClientService.Initializer
    {
        ApiKey = apiKey,
        ApplicationName = "CustomSearchAPI Sample",
    });

// https://googleapis.dev/dotnet/Google.Apis.CustomSearchAPI.v1/latest/api/Google.Apis.CustomSearchAPI.v1.CseResource.ListRequest.html

var request = service.Cse.List();
request.Q = "\"Craig Eddy\""; // search query
request.SiteSearch = "clustrmaps.com flickr.com facebook.com twitter.com instagram.com linkedin.com beenverified.com mylife.com whitepages.com truthfinder.com spokeo.com cocofinder.com myheritage.com mylife.com usapeoplesearch.com";
request.SiteSearchFilter = Google.Apis.CustomSearchAPI.v1.CseResource.ListRequest.SiteSearchFilterEnum.E;
request.Fields = "queries,items(displayLink,title,snippet,link,pagemap/metatags,pagemap/article,pagemap/cse_thumbnail)";
request.Start = 1;
request.Gl = "us"; // Geolocation of end user.
                   // The gl parameter value is a two-letter country code.
                   // The gl parameter boosts search results whose country of origin matches the parameter value. See the Country Codes page.

// Custom search engine ID
request.Cx = customSearchEngineID;

var items = new List<Result>();

request.Start = 0;

var results = await request.ExecuteAsync();
var nextIndex = results.Queries.NextPage.FirstOrDefault()?.StartIndex ?? 0;
items.AddRange(results.Items);

while (items.Count < 20 && nextIndex > 0)
{
    request.Start = nextIndex;
    results = await request.ExecuteAsync();
    items.AddRange(results.Items);

    // check to see if there's another page
    nextIndex = results.Queries.NextPage.FirstOrDefault()?.StartIndex ?? 0;
}

items.ForEach(item =>
{
    var meta = item?.Pagemap["metatags"];
    var type = "Web";
    if(meta != null)
    {
        var array = (item?.Pagemap["metatags"] as Newtonsoft.Json.Linq.JArray);
        if (array != null && array.Count > 0 && array[0]["og:type"]?.ToString() == "article")
        {
            type = "News";
        }
    }

    Console.WriteLine($"Type: {type}, Title: {item.Title}, Url: {item.Link}, Date: {CalculatePostDate(item)}");
});

Console.ReadLine();


DateTime CalculatePostDate(Result item)
{
    if (item.Pagemap?.ContainsKey("article") == false) return DateTime.Now;

    var article = item?.Pagemap["article"] as Newtonsoft.Json.Linq.JArray;

    if (article != null && article.Count > 0)
    {
        var datePublished = article[0]["datemodified"]?.ToString();
        if (!string.IsNullOrWhiteSpace(datePublished))
        {
            return DateTime.Parse(datePublished);
        }

        datePublished = article[0]["datepublished"]?.ToString();
        if (!string.IsNullOrWhiteSpace(datePublished))
        {
            return DateTime.Parse(datePublished);
        }
    }

    return DateTime.Now;
}




