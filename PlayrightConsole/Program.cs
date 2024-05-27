using Microsoft.Playwright;
using System.Net;
using System.Text;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.GotoAsync("https://playwright.dev");
var result = await page.ContentAsync();
// access cookies 
page.Context.CookiesAsync();

var http = new HttpCustom();

await http.RunServer();

class HttpCustom
{
    public async Task RunServer()
    {
        var prefix = "http://*:4200/";
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        try
        {
            listener.Start();
        }
        catch (HttpListenerException hlex)
        {
            return;
        }
        while (listener.IsListening)
        {
            var context = listener.GetContext();
            ProcessRequest(context);
        }
        listener.Close();
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        // Get the data from the HTTP stream
        var body = new StreamReader(context.Request.InputStream).ReadToEnd();

        byte[] b = Encoding.UTF8.GetBytes("ACK");
        context.Response.StatusCode = 200;
        context.Response.KeepAlive = false;
        context.Response.ContentLength64 = b.Length;

        var output = context.Response.OutputStream;
        output.Write(b, 0, b.Length);
        context.Response.Close();

    }
}

// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");



