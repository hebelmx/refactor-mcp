using System;
using System.Collections.Generic;
using System.Text;

namespace ExxerFactor.Mcp.Core.Exceptions;

public class McpException : Exception
{
    public McpException() : base("Mcp Exception occurred")
    { }

    public McpException(string message) : base(message)
    { }

    public McpException(string message, Exception innerException) : base(message, innerException)
    { }
}