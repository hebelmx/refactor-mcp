// =====================================================================
// Global Using Statements for ExxerFactor.Mcp.Tests
// =====================================================================

// =====================================================================
// Testing Frameworks
// =====================================================================
global using Xunit;
global using NSubstitute;

// =====================================================================
// Microsoft Roslyn & Code Analysis
// =====================================================================
global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.Editing;
global using Microsoft.CodeAnalysis.Formatting;
global using Microsoft.CodeAnalysis.Text;

// =====================================================================
// System & JSON
// =====================================================================
global using System.Text.Json;

// =====================================================================
// Protocol & Communication
// =====================================================================
global using ModelContextProtocol;

// =====================================================================
// ExxerFactor.Mcp Core Libraries
// =====================================================================
global using ExxerFactor.Mcp.Core.Move;
global using ExxerFactor.Mcp.Core.SyntaxRewriters;
global using ExxerFactor.Mcp.Core.SyntaxWalkers;
global using ExxerFactor.Mcp.Core.Tools;