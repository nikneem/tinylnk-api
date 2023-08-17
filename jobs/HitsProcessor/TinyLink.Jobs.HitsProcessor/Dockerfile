FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TinyLink.Jobs.HitsProcessor/TinyLink.Jobs.HitsProcessor.csproj", "TinyLink.Jobs.HitsProcessor/"]
RUN dotnet restore "TinyLink.Jobs.HitsProcessor/TinyLink.Jobs.HitsProcessor.csproj"
COPY . .
WORKDIR "/src/TinyLink.Jobs.HitsProcessor"
RUN dotnet build "TinyLink.Jobs.HitsProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TinyLink.Jobs.HitsProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TinyLink.Jobs.HitsProcessor.dll"]