FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TinyLink.Jobs.Hits.Accumulator/TinyLink.Jobs.Hits.Accumulator.csproj", "TinyLink.Jobs.Hits.Accumulator/"]
COPY ["TinyLink.Hits.TableStorage/TinyLink.Hits.TableStorage.csproj", "TinyLink.Hits.TableStorage/"]
COPY ["TinyLink.Hits.Abstractions/TinyLink.Hits.Abstractions.csproj", "TinyLink.Hits.Abstractions/"]
RUN dotnet restore "TinyLink.Jobs.Hits.Accumulator/TinyLink.Jobs.Hits.Accumulator.csproj"
COPY . .
WORKDIR "/src/TinyLink.Jobs.Hits.Accumulator"
RUN dotnet build "TinyLink.Jobs.Hits.Accumulator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TinyLink.Jobs.Hits.Accumulator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TinyLink.Jobs.Hits.Accumulator.dll"]