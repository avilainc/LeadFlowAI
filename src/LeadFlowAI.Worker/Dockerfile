FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["src/LeadFlowAI.Worker/LeadFlowAI.Worker.csproj", "src/LeadFlowAI.Worker/"]
COPY ["src/LeadFlowAI.Application/LeadFlowAI.Application.csproj", "src/LeadFlowAI.Application/"]
COPY ["src/LeadFlowAI.Domain/LeadFlowAI.Domain.csproj", "src/LeadFlowAI.Domain/"]
COPY ["src/LeadFlowAI.Infrastructure/LeadFlowAI.Infrastructure.csproj", "src/LeadFlowAI.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/LeadFlowAI.Worker/LeadFlowAI.Worker.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/LeadFlowAI.Worker"
RUN dotnet build "LeadFlowAI.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LeadFlowAI.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeadFlowAI.Worker.dll"]
