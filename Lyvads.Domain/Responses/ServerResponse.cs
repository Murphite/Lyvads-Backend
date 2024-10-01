using Newtonsoft.Json;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lyvads.Domain.Responses;

public class ServerResponse<T>
{
    private bool _isSuccessful;
    private bool _isFailure;

    public bool IsSuccessful
    {
        get => _isSuccessful;
        set
        {
            _isSuccessful = value;
            _isFailure = !value;  // Automatically update IsFailure
        }
    }

    public bool IsFailure
    {
        get => _isFailure;
        set
        {
            _isFailure = value;
            _isSuccessful = !value;  // Automatically update IsSuccessful
        }
    }

    public ErrorResponse ErrorResponse { get; set; }

    [JsonProperty("responseCode")]
    public string ResponseCode { get; set; }

    [JsonProperty("responseMessage")]
    public string ResponseMessage { get; set; }

    public T Data { get; set; }

    public ServerResponse()
    {
        // Default constructor behavior
    }

    public ServerResponse(bool success = true)
    {
        IsSuccessful = success;
        IsFailure = !success;
    }
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
