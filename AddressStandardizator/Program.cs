using Microsoft.Extensions.DependencyInjection;
using Dadata;
using Dadata.Model;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors(); // добавляем Cors
var app = builder.Build();

//httpClientFactory
var services = new ServiceCollection();
services.AddHttpClient();
var serviceProvider = services.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
var httpClient = httpClientFactory?.CreateClient();
// задаём api dadata
var token = "927777ed1d86fc93ea4f121c89e662d7174b3d21";
var secret = "caac29a684135a2279bda141485fc222258a2b8e";
var api = new CleanClientAsync(token, secret);

app.UseHttpLogging(); //включаем базовое логгирование
app.UseCors(builder => builder.AllowAnyOrigin()); //задаём политику для Cors 
app.UseHttpsRedirection();

app.Run(async (context) =>
{
    
    context.Response.ContentType = "text/html; charset=utf-8";

    // если обращение идет по адресу "/postuser", получаем данные формы
    if (context.Request.Path == "/postaddress")
    {
        var form = context.Request.Form;
        string address = form["address"];
        var fixedAddress = await api.Clean<Address>(address);//используем dadata, чтобы стандартизировать адрес
        var settings = new JsonSerializerSettings { };
        string addressJson = JsonConvert.SerializeObject(fixedAddress,settings);
        //await context.Response.WriteAsJsonAsync($"<div><p>{fixedAddress}</p></div>"); //не вся инфа
        await context.Response.WriteAsync($"<div><p>{addressJson}</p></div>");

    }
    else
    {
        await context.Response.SendFileAsync("html/index.html");
    }
    //string content = await httpClient.GetStringAsync("https://localhost:7144/postaddress");
    //Console.WriteLine(content);

});

app.Run();