using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Helpers;



public class UnifonicSmsService
{
    private readonly HttpClient _httpClient;


    private readonly string _appSid = "9kDlshIZhkhAbbkAEH6GZ7i0eEb8a5"; // Your AppSid
    public UnifonicSmsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendSmsAsync(string recipient, string messageBody,string SenderId)
    {
        var requestUrl = "https://el.cloud.unifonic.com/rest/SMS/messages";

        var parameters = new Dictionary<string, string>
        {
            { "AppSid", _appSid },
            //{ "SenderID", "DevWePayBE" },
            { "Body", messageBody },
            { "Recipient", recipient },
            { "responseType", "JSON" },
            { "CorrelationID", "test-correlation-id" },
            { "baseEncode", "true" },
            { "statusCallback", "sent" },
            { "async", "false" },
            { "MessageType", "6" } // Awareness
        };

        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            // Successful response (200)
            return responseContent;
        }
        else
        {
            // Handle or log error response
            throw new Exception($"Error sending SMS: {response.StatusCode}, {responseContent}");
        }
    }
}
















//public class UnifonicSmsService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _appSid = "9kDlshIZhkhAbbkAEH6GZ7i0eEb8a5"; // Your AppSid

//    public UnifonicSmsService(HttpClient httpClient)
//    {
//        _httpClient = httpClient;
//    }

//    public async Task<string> SendSmsAsync(string to, string message)
//    {
//        var url = "https://el.cloud.unifonic.com/rest/SMS/messages";

//        var parameters = new Dictionary<string, string>
//        {
//            { "AppSid", _appSid },
//            { "Recipient", to },
//            { "Body", message }
//        };

//        var content = new FormUrlEncodedContent(parameters);

//        var response = await _httpClient.PostAsync(url, content);

//        var responseBody = await response.Content.ReadAsStringAsync();

//        // For debugging/logging:
//        Console.WriteLine($"Response Code: {response.StatusCode}");
//        Console.WriteLine($"Response Body: {responseBody}");

//        return responseBody;
//    }
//}


//public class UnifonicSmsService
//{
//    private readonly HttpClient _httpClient;

//    public UnifonicSmsService(HttpClient httpClient)
//    {
//        _httpClient = httpClient;
//    }

//    public async Task<string> SendSmsAsync(string toPhoneNumber, string message)
//    {
//        var request = new Dictionary<string, string>
//    {
//        { "AppSid", "9kDlshIZhkhAbbkAEH6GZ7i0eEb8a5" },
//        { "Recipient", toPhoneNumber },
//        { "Body", message },
//        { "SenderID", "DevWePayBE" }
//    };

//        var content = new FormUrlEncodedContent(request);
//        var response = await _httpClient.PostAsync("https://el.cloud.unifonic.com/rest/SMS/messages", content);

//        var result = await response.Content.ReadAsStringAsync();

//        // Logging actual HTTP response and status code
//        Console.WriteLine("Status Code: " + response.StatusCode);
//        Console.WriteLine("Response Body: " + result);

//        return result;
//    }



//}
