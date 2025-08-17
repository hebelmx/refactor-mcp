// =====================================================================
// Global Using Statements for RefactorMCP.Web.Tests
// =====================================================================

// =====================================================================
// Testing Frameworks
// =====================================================================
global using Xunit;
global using FluentAssertions;
global using NSubstitute;
global using Bunit;

// =====================================================================
// Microsoft ASP.NET Core Testing
// =====================================================================
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;

// =====================================================================
// System Namespaces
// =====================================================================
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading.Tasks;
global using System.Text.Json;

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