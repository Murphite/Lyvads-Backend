
namespace Lyvads.Application.Dtos;

public class Error
{
    public static readonly IEnumerable<Error> None = Array.Empty<Error>();

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Message { get; }
    public string Code { get; }
}
