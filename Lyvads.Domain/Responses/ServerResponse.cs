using Newtonsoft.Json;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lyvads.Domain.Responses;

public class ServerResponse<T>
{
    public ServerResponse(bool success = false)
    {
        IsSuccessful = success;
    }
    public bool IsSuccessful { get; set; }
    public ErrorResponse ErrorResponse { get; set; }
    [JsonProperty("responseCode")]
    public string ResponseCode { get; set; }
    [JsonProperty("responseMessage")]
    public string ResponseMessage { get; set; }
    public T Data { get; set; }
}


public class ErrorResponse
{
    [JsonProperty("responseCode")]
    public string? ResponseCode { get; set; }
    [JsonProperty("responseMessage")]
    public string? ResponseMessage { get; set; }
    [JsonProperty("responseDescription")]
    public string? ResponseDescription { get; set; }
}
