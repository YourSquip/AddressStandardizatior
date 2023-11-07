using Microsoft.Extensions.DependencyInjection;
using Dadata;
using Dadata.Model;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors(); // ��������� Cors
var app = builder.Build();

//�������� httpClient ����� httpClientFactory
var services = new ServiceCollection();
services.AddHttpClient();
var serviceProvider = services.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
var httpClient = httpClientFactory?.CreateClient();

// ����� ������ ��� api dadata
var token = "927777ed1d86fc93ea4f121c89e662d7174b3d21";
var secret = "caac29a684135a2279bda141485fc222258a2b8e";
var api = new CleanClientAsync(token, secret);

app.UseHttpLogging(); //�������� ������� ������� ������� http
app.UseCors(builder => builder.AllowAnyOrigin()); //����� �������� ��� Cors 
app.UseHttpsRedirection(); //���������� �������� �� https

app.Run(async (context) =>
{
    
    context.Response.ContentType = "text/html; charset=utf-8";

    if (context.Request.Path == "/getaddress")
    {
        
        string getSource = "https://localhost:7210";//���� ��� url ��������, � ������� ����� ������� ����� ������ ������
        if(httpClient != null)
        {
            string address = await httpClient.GetStringAsync(getSource);
            if (address != null && address != "")
            {
                var fixedAddress = await api.Clean<Address>(address);//���������� dadata, ����� ����������������� �����

                //������������ ����� � json
                var settings = new JsonSerializerSettings { };
                string addressJson = JsonConvert.SerializeObject(fixedAddress, settings);

                await context.Response.WriteAsync($"<div><p>{addressJson}</p></div>"); //������� json
            }
            else
            {
                app.Logger.LogError("source page is empty");
            }
        }
        else
        {
            app.Logger.LogError("httpClient is null");
        }
        
       


    }
    else
    {
        await context.Response.SendFileAsync("html/index.html");
    }

});

app.Run();