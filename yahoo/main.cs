using System.Globalization;
using System.Net;
using yahoo;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");

var quotations = new List<string>();
var waitHandler = new AutoResetEvent(true);
var pathRes = $"{Environment.CurrentDirectory}/../../../result.txt";
if (File.Exists(pathRes))
    File.Delete(pathRes);

using (var reader = new StreamReader($"{Environment.CurrentDirectory}/../../../ticker.txt"))
{
    while (await reader.ReadLineAsync() is { } line)
    {
        quotations.Add(line);
    }
}

var tasks = new Task[quotations.Count];
var size = 0;
foreach (var action in quotations)
{
    var t1 = Task.Run(() =>
    {
        var response = "";
        while (true)
        {
            try
            {
                response = Yahoo.GetData(action);
                break;
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"404 (Not Found)  {action} may be delisted");
                    break;
                }

                Console.WriteLine($"{e.Message}  {action}");
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Thread.Sleep(300000);
                }

                Thread.Sleep(10000);
            }
        }

        return KeyValuePair.Create(action, response);
    });
    tasks[size++] = t1.ContinueWith(t =>
    {
        var dec = Yahoo.MeanYearly(t.Result.Value);
        if (dec is null) return;
        //Console.WriteLine($"{t.Result.Key} : {dec:f4}");
        //res.Add(dec.Value);
        SaveToFile(KeyValuePair.Create(t.Result.Key, dec.GetValueOrDefault()));
    });
    Thread.Sleep(11);
}


void SaveToFile(KeyValuePair<string, decimal> pair)
{
    waitHandler.WaitOne();
    File.AppendAllText(pathRes, $"{pair.Key} : {pair.Value:f4}\n");
    waitHandler.Set();
}

Task.WaitAll(tasks);