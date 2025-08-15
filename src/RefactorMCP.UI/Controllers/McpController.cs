using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RefactorMCP.UI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class McpController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<McpController> _logger;

    public McpController(IServiceProvider serviceProvider, ILogger<McpController> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [HttpPost]
    [Route("tools")]
    public async Task<IActionResult> HandleToolCall([FromBody] McpToolCallRequest request)
    {
        try
        {
            _logger.LogInformation("MCP tool call: {ToolName}", request.ToolName);

            // Find the tool method
            var method = FindToolMethod(request.ToolName);
            if (method == null)
            {
                return BadRequest(new McpErrorResponse 
                { 
                    Error = "Tool not found",
                    Code = -32601 
                });
            }

            // Convert parameters
            var parameters = ConvertParameters(method, request.Parameters);
            
            // Invoke the tool
            var result = method.Invoke(null, parameters);
            
            // Handle async results
            string responseText;
            if (result is Task<string> taskStr)
            {
                responseText = await taskStr;
            }
            else if (result is Task task)
            {
                await task;
                responseText = "Operation completed successfully";
            }
            else
            {
                responseText = result?.ToString() ?? "No result";
            }

            return Ok(new McpToolCallResponse
            {
                Content = new List<McpContent>
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = responseText
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing MCP tool {ToolName}", request.ToolName);
            return BadRequest(new McpErrorResponse 
            { 
                Error = ex.Message,
                Code = -32000 
            });
        }
    }

    [HttpGet]
    [Route("tools")]
    public IActionResult ListTools()
    {
        var tools = GetAvailableTools();
        return Ok(new McpListToolsResponse
        {
            Tools = tools
        });
    }

    [HttpGet]
    [Route("server-info")]
    public IActionResult GetServerInfo()
    {
        return Ok(new McpServerInfo
        {
            Name = "RefactorMCP",
            Version = "1.0.0",
            ProtocolVersion = "2024-11-05",
            Capabilities = new McpCapabilities
            {
                Tools = new McpToolsCapability(),
                Resources = new McpResourcesCapability()
            }
        });
    }

    private MethodInfo? FindToolMethod(string toolName)
    {
        return Assembly.GetExecutingAssembly().GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Concat(new[] { Assembly.GetExecutingAssembly() })
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttributes(typeof(McpServerToolTypeAttribute), false).Any())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .FirstOrDefault(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any() &&
                                 string.Equals(m.Name, toolName, StringComparison.OrdinalIgnoreCase));
    }

    private object?[] ConvertParameters(MethodInfo method, Dictionary<string, JsonElement>? parameters)
    {
        var methodParams = method.GetParameters();
        var result = new object?[methodParams.Length];

        for (int i = 0; i < methodParams.Length; i++)
        {
            var param = methodParams[i];
            if (parameters?.TryGetValue(param.Name!, out var value) == true)
            {
                result[i] = JsonSerializer.Deserialize(value.GetRawText(), param.ParameterType);
            }
            else if (param.HasDefaultValue)
            {
                result[i] = param.DefaultValue;
            }
            else
            {
                throw new ArgumentException($"Missing required parameter: {param.Name}");
            }
        }

        return result;
    }

    private List<McpTool> GetAvailableTools()
    {
        return Assembly.GetExecutingAssembly().GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Concat(new[] { Assembly.GetExecutingAssembly() })
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttributes(typeof(McpServerToolTypeAttribute), false).Any())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any())
            .Select(m =>
            {
                var description = m.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description ?? m.Name;
                return new McpTool
                {
                    Name = ToKebabCase(m.Name),
                    Description = description,
                    InputSchema = GenerateInputSchema(m)
                };
            })
            .ToList();
    }

    private object GenerateInputSchema(MethodInfo method)
    {
        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var param in method.GetParameters())
        {
            properties[param.Name!] = new
            {
                type = GetJsonSchemaType(param.ParameterType),
                description = param.Name
            };

            if (!param.HasDefaultValue)
            {
                required.Add(param.Name!);
            }
        }

        return new
        {
            type = "object",
            properties,
            required
        };
    }

    private string GetJsonSchemaType(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long)) return "integer";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(string[])) return "array";
        return "string";
    }
    
    private static string ToKebabCase(string name)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0)
                sb.Append('-');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}

// MCP Request/Response Models

public class McpToolCallRequest
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, JsonElement>? Parameters { get; set; }
}

public class McpToolCallResponse
{
    public List<McpContent> Content { get; set; } = new();
}

public class McpContent
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class McpErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public int Code { get; set; }
}

public class McpListToolsResponse
{
    public List<McpTool> Tools { get; set; } = new();
}

public class McpTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? InputSchema { get; set; }
}

public class McpServerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ProtocolVersion { get; set; } = string.Empty;
    public McpCapabilities? Capabilities { get; set; }
}

public class McpCapabilities
{
    public McpToolsCapability? Tools { get; set; }
    public McpResourcesCapability? Resources { get; set; }
}

public class McpToolsCapability
{
    public bool ListChanged { get; set; } = false;
}

public class McpResourcesCapability
{
    public bool Subscribe { get; set; } = false;
    public bool ListChanged { get; set; } = false;
}

// These attributes are provided by ModelContextProtocol.Server package