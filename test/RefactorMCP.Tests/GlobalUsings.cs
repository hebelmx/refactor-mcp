// Global using statements for RefactorMCP.Tests (Integration Tests)

// Testing frameworks
global using Xunit;
global using FluentAssertions;

// System namespaces commonly used in tests
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Diagnostics;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

// RefactorMCP Core namespaces
global using RefactorMCP.Core;
global using RefactorMCP.Core.Abstractions;
global using RefactorMCP.Core.Extensions;
global using RefactorMCP.Core.Logging;
global using RefactorMCP.Core.Move;
global using RefactorMCP.Core.Services;
global using RefactorMCP.Core.SyntaxRewriters;
global using RefactorMCP.Core.SyntaxWalkers;
global using RefactorMCP.Core.Tools;

// Roslyn namespaces commonly used in tests
global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;

// Model Context Protocol
global using ModelContextProtocol;
global using ModelContextProtocol.Server;