using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorMCP.Core.Exceptions;

public class McpException : Exception
{
    public McpException() : base("MCP Exception occurred")
    { }

    public McpException(string message) : base(message)
    { }

    public McpException(string message, Exception innerException) : base(message, innerException)
    { }
}