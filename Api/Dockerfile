FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8001
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8001
ENV DOTNET_gcServer=true
ENV DOTNET_TieredCompilation=1
ENV DOTNET_ReadyToRun=true
ENV DOTNET_EventPipeEnable=true
ENV DOTNET_JitStackTraceDepth=1
ENV DOTNET_ThreadPool_UnfairSemaphoreSpinLimit=5000
ENV DOTNET_GCHeapCount=2
ENV DOTNET_GCHeapAffinitize=0
ENV DOTNET_GCLatencyMode=1
ENV DOTNET_gcConcurrent=true
ENV DOTNET_PROCESSOR_COUNT=2
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV ConnectionStrings__Default=""
ENV Database__Provider=SqlServer
ENV Hangfire__IsEnabled=false
ENV SqlServerCache__IsEnabled=false
ENV RedisCache__IsEnabled=false
ENV RedisCache__ConnectionString="redis:6379"
ENV RedisCache__DatabaseId=0
ENV App__ServerRootAddress=http://localhost:8001/
ENV App__ClientRootAddress=http://localhost:8000/
ENV App__CorsOrigins=*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Templates/Api/src/Eaf.ProjectName.Web.Host"
RUN dotnet restore "Eaf.ProjectName.Web.Host.csproj"
RUN dotnet publish "Eaf.ProjectName.Web.Host.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:PublishTrimmed=false \
    -p:PublishSingleFile=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN rm -f appsettings.Local.json appsettings.Production.json appsettings.Staging.json appsettings.Development.json

ENV TZ="America/Sao_Paulo"

RUN apt-get update && \
    apt-get install -yq --no-install-recommends \
        ca-certificates \
        curl \
        icu-devtools \
        libc6-dev \
        libgdiplus \
        tzdata \
        zlib1g-dev && \
    ln -fs /usr/share/zoneinfo/America/Sao_Paulo /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    rm -rf /var/lib/apt/lists/* && \
    chown -R app:app /app

USER app

ENTRYPOINT ["dotnet", "Eaf.ProjectName.Web.Host.dll"]
