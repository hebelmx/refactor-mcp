// =====================================================================
// Global Using Statements for ExxerFactor.Mcp.Server.Tests
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
global using System.Text.Json;

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