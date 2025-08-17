// =====================================================================
// Global Using Statements for RefactorMCP.Core.Tests
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
// RefactorMCP Core Libraries
// =====================================================================
global using RefactorMCP.Core.Move;
global using RefactorMCP.Core.SyntaxRewriters;
global using RefactorMCP.Core.SyntaxWalkers;
global using RefactorMCP.Core.Tools;