FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/LeadFlowAI.WebAPI/LeadFlowAI.WebAPI.csproj", "LeadFlowAI.WebAPI/"]
COPY ["src/LeadFlowAI.Application/LeadFlowAI.Application.csproj", "LeadFlowAI.Application/"]
COPY ["src/LeadFlowAI.Domain/LeadFlowAI.Domain.csproj", "LeadFlowAI.Domain/"]
COPY ["src/LeadFlowAI.Infrastructure/LeadFlowAI.Infrastructure.csproj", "LeadFlowAI.Infrastructure/"]

RUN dotnet restore "LeadFlowAI.WebAPI/LeadFlowAI.WebAPI.csproj"
COPY src/ .
WORKDIR "/src/LeadFlowAI.WebAPI"
RUN dotnet build "LeadFlowAI.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LeadFlowAI.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeadFlowAI.WebAPI.dll"]
