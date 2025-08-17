// =====================================================================
// Global Using Statements for RefactorMCP.Tests
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
// RefactorMCP Core Libraries
// =====================================================================
global using RefactorMCP.Core.Move;
global using RefactorMCP.Core.SyntaxRewriters;
global using RefactorMCP.Core.SyntaxWalkers;
global using RefactorMCP.Core.Tools;