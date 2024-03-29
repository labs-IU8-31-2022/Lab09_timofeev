﻿namespace yahoo;

internal static class Yahoo
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
        return response.Result.Content.ReadAsStringAsync().Result;
    }

    public static decimal? MeanYearly(string data)
    {
        if (data == "")
        {
            return null;
        }

        var sum = data
            .Split('\n')
            .Select(line => line.Split(','))
            .Where(numbers => numbers[0] != "Date" && !numbers.Contains("null"))
            .Sum(numbers => (Convert.ToDecimal(numbers[2].Replace('.', ',')) +
                             Convert.ToDecimal(numbers[3].Replace('.', ','))) / 2);
        return sum / data
            .Split('\n')
            .Count(line => !line.Contains("null") && !line.Contains("Date")) ;
    }
}