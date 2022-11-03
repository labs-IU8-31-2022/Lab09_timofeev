using System.Net;

namespace yahoo;

class Yahoo
{
    public static string GetData(string quotation)
    {
        HttpClient request = new();
        request.BaseAddress = new Uri($"https://query1.finance.yahoo.com/v7/finance/download/{quotation}");
        var response = 
            request.GetAsync($"?period1={DateTimeOffset.Now.AddYears(-1).ToUnixTimeSeconds()}" +
                             $"&period2={DateTimeOffset.Now.ToUnixTimeSeconds()}" +
                             "&interval=1d&events=history&includeAdjustedClose=true");
        response.Result.EnsureSuccessStatusCode();
        /*if (response.Result.StatusCode != HttpStatusCode.OK)
        {
            HttpRequestException ex = new();
            ex.StatusCode = HttpStatusCode.NotFound;
            throw new HttpRequestException("TOO MANY REQUESTS");
        }*/
        return response.Result.Content.ReadAsStringAsync().Result;
    }

    public static decimal MeanYearly(string data)
    {
        var sum = data
            .Split('\n')
            .Select(line => line.Split(','))
            .Where(numbers => numbers[0] != "Date")
            .Sum(numbers => (Convert.ToDecimal(numbers[2].Replace('.', ',')) +
                             Convert.ToDecimal(numbers[3].Replace('.', ','))) / 2);
        return sum / (data.Split('\n').Length - 1);
    }
}