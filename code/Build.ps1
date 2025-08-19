dotnet restore "F:\Dynamic\Refactor\refactor-mcp\code\src\ExxerFactor.Mcp.Web\Exxerfactor.Mcp.Web.csproj" --force-evaluate && dotnet build        

dotnet restore "F:\Dynamic\Refactor\refactor-mcp\code\src\ExxerFactor.Mcp.Web\Exxerfactor.Mcp.Web.csproj" --force-evaluate && dotnet build    --no-restore     


dotnet restore "F:\Dynamic\Refactor\refactor-mcp\code\src\ExxerFactor.Mcp.Web\Exxerfactor.Mcp.Web.csproj" --no-http-cache --ignore-failed-sources  && dotnet build "F:\Dynamic\Refactor\refactor-mcp\code\src\ExxerFactor.Mcp.Web\Exxerfactor.Mcp.Web.csproj" --no-restore  
dotnet restore --force-evaluate --ignore-failed-sources  && dotnet build --no-restore  &&  dotnet run --no-build -p:RunAnalyzers=false  

cd "F:\Dynamic\ExxerFactor\ExxerFactor-Mcp\code\src\ExxerFactor.Mcp.Core"
dotnet restore --force-evaluate && dotnet build --no-restore
cd "F:\Dynamic\ExxerFactor\ExxerFactor-Mcp\code\src\ExxerFactor.Mcp.Server"
dotnet restore --force-evaluate && dotnet build --no-restore
cd "F:\Dynamic\ExxerFactor\ExxerFactor-Mcp\code\src\ExxerFactor.Mcp.Web"


dotnet restore "F:\Dynamic\ExxerFactor\ExxerFactor-Mcp\code\src\ExxerFactor.Mcp.Web\ExxerFactor.Mcp.Web.csproj" --force-evaluate && dotnet build
 dotnet restore "F:\Dynamic\ExxerFactor\ExxerFactor-Mcp\code\src\ExxerFactor.Mcp.Web\ExxerFactor.Mcp.Web.csproj" --force-evaluate && dotnet build
