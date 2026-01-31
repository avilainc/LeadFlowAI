FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["src/LeadFlowAI.WebAPI/LeadFlowAI.WebAPI.csproj", "src/LeadFlowAI.WebAPI/"]
COPY ["src/LeadFlowAI.Application/LeadFlowAI.Application.csproj", "src/LeadFlowAI.Application/"]
COPY ["src/LeadFlowAI.Domain/LeadFlowAI.Domain.csproj", "src/LeadFlowAI.Domain/"]
COPY ["src/LeadFlowAI.Infrastructure/LeadFlowAI.Infrastructure.csproj", "src/LeadFlowAI.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/LeadFlowAI.WebAPI/LeadFlowAI.WebAPI.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/LeadFlowAI.WebAPI"
RUN dotnet build "LeadFlowAI.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LeadFlowAI.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeadFlowAI.WebAPI.dll"]
