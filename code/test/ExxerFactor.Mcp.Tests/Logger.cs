namespace ExxerFactor.Mcp.Tests;

public class Logger
{
    // Target location for moved instance methods
    public void Log(string message)
    {
        Console.WriteLine($"[LOG] {message}");
    }
}