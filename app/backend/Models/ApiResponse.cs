namespace Backend.Models;

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

public class SuccessResponse
{
    public bool Success { get; set; } = true;
}

public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}
