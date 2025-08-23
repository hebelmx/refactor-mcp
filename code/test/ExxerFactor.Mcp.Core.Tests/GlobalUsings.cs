// =====================================================================
// Global Using Statements for ExxerFactor.Mcp.Core.Tests
// =====================================================================

// =====================================================================
// Testing Frameworks
// =====================================================================
global using Xunit;
global using Shouldly;
global using NSubstitute;

// =====================================================================
// System Namespaces
// =====================================================================
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;

// =====================================================================
// Microsoft Extensions & Configuration
// =====================================================================
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

// =====================================================================
// ExxerFactor.Mcp Core Libraries
// =====================================================================
global using ExxerFactor.Mcp.Core.Move;
global using ExxerFactor.Mcp.Core.SyntaxRewriters;
global using ExxerFactor.Mcp.Core.SyntaxWalkers;
global using ExxerFactor.Mcp.Core.Tools;

global using System.ComponentModel;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;

global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.CSharp;
global using ModelContextProtocol;
global using ModelContextProtocol.Server;
global using Microsoft.CodeAnalysis.Text;
global using Microsoft.CodeAnalysis.Host.Mef;
global using Microsoft.Build.Locator;
global using Microsoft.CodeAnalysis.MSBuild;
global using Microsoft.CodeAnalysis.Formatting;
global using ModelContextProtocol.Protocol;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.CodeAnalysis.Editing;
global using Microsoft.CodeAnalysis.FindSymbols;
global using Microsoft.CodeAnalysis.Rename;