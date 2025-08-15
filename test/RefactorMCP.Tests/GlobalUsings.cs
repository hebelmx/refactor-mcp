// Global using statements for RefactorMCP.Tests (Integration Tests)

// Testing frameworks
global using Xunit;
global using FluentAssertions;

// System namespaces commonly used in tests
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Diagnostics;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

// RefactorMCP
global using RefactorMCP.Core.Abstractions;
global using RefactorMCP.Core.Services;

// RefactorMCP ConsoleApp namespaces
global using RefactorMCP.ConsoleApp.Move;
global using RefactorMCP.ConsoleApp.SyntaxWalkers;
global using RefactorMCP.ConsoleApp.SyntaxRewriters;

// RefactorMCP Core namespaces
global using RefactorMCP.Core.SyntaxRewriters;
global using RefactorMCP.Core.SyntaxWalkers;

// Model Context Protocol
global using ModelContextProtocol;
global using ModelContextProtocol.Server;