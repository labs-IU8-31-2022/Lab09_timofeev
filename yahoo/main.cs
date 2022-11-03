using System.Net;
using yahoo;


var quotations = new List<string>();


using (var reader = new StreamReader($"{Environment.CurrentDirectory}/../../../ticker.txt"))
{
    /*while (await reader.ReadLineAsync() is { } line)
    {
        quotations.Add(line);
        //Console.WriteLine(line);
    }*/
    quotations.Add("ALTM");
    /*int i = 0;
    while (await reader.ReadLineAsync() is { } line)
    {
        if (i > 480) quotations.Add(line);
        ++i;
        //Console.WriteLine(line);
    }*/
}

CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
CancellationToken token = cancelTokenSource.Token;

var tasks = new Task<KeyValuePair<string, decimal>>[quotations.Count];
var size = 0;
var res = new List<decimal>();
foreach (var action in quotations)
{
    var t1 = Task.Run(() =>
    {
        string response;
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
                    Console.WriteLine($"{e.Message}  {action}");
                    cancelTokenSource.Cancel();
                }
                Console.WriteLine($"{e.Message}  {action}");
                Thread.Sleep(10000);
            }
        }

        return KeyValuePair.Create(action,response);
    }, token);
    tasks[size++] = t1.ContinueWith(t =>
    {
        var dec = Yahoo.MeanYearly(t.Result.Value);
        Console.WriteLine($"{t.Result.Key} : {dec:f4}");
        res.Add(dec);
        return KeyValuePair.Create(t.Result.Key, dec);
    });
    Thread.Sleep(11);
}


//var temp = Yahoo.GetData("AAPL");
//var sum = Yahoo.MeanYearly(temp);
//Console.WriteLine(temp);  
//Console.WriteLine(sum);


Task.WaitAll(tasks);
Console.WriteLine(res.Count);