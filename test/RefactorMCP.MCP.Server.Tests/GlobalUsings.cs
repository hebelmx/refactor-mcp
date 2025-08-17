// Global using statements for RefactorMCP.MCP.Server.Tests

// Testing frameworks
global using Xunit;
global using FluentAssertions;
global using NSubstitute;

// System namespaces commonly used in tests
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Text.Json;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

// RefactorMCP Core and MCP Server
global using RefactorMCP.Core.Move;
global using RefactorMCP.Core.SyntaxRewriters;
global using RefactorMCP.Core.SyntaxWalkers;
global using RefactorMCP.Core.Tools;