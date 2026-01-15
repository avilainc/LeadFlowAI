FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/LeadFlowAI.Worker/LeadFlowAI.Worker.csproj", "LeadFlowAI.Worker/"]
COPY ["src/LeadFlowAI.Application/LeadFlowAI.Application.csproj", "LeadFlowAI.Application/"]
COPY ["src/LeadFlowAI.Domain/LeadFlowAI.Domain.csproj", "LeadFlowAI.Domain/"]
COPY ["src/LeadFlowAI.Infrastructure/LeadFlowAI.Infrastructure.csproj", "LeadFlowAI.Infrastructure/"]

RUN dotnet restore "LeadFlowAI.Worker/LeadFlowAI.Worker.csproj"
COPY src/ .
WORKDIR "/src/LeadFlowAI.Worker"
RUN dotnet build "LeadFlowAI.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LeadFlowAI.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeadFlowAI.Worker.dll"]
