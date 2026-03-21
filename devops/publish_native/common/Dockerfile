FROM mcr.microsoft.com/dotnet/sdk:10.0

ARG ENVIRONMENT=Production

# Change timezone to local time
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY . /app
WORKDIR /app/

ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
ENV DOTNET_USE_POLLING_FILE_WATCHER=true  
ENV ASPNETCORE_URLS=http://+:80  
EXPOSE 80

RUN apt update
RUN apt install -y apt-transport-https wget ca-certificates curl

RUN echo "{}" > appsettings.Local.json
RUN echo "{}" > TimeTracker.Tests.Integration.Api/appsettings.Local.json
RUN echo "{}" > TimeTracker.Tests.Integration.Business/appsettings.Local.json
RUN echo "{}" > TimeTracker.Api/appsettings.Local.json
RUN echo "{}" > TimeTracker.Migrations/appsettings.Local.json
RUN echo "{}" > TimeTracker.WorkerServices/appsettings.Local.json

RUN ["dotnet", "restore", "--verbosity=q"]

RUN ["dotnet", "build", "--verbosity=q", "TimeTracker.Api/TimeTracker.Api.csproj", "--no-restore"]
RUN ["dotnet", "build", "--verbosity=q", "TimeTracker.Migrations/TimeTracker.Migrations.csproj", "--no-restore"]
RUN ["dotnet", "build", "--verbosity=q", "TimeTracker.WorkerServices/TimeTracker.WorkerServices.csproj", "--no-restore"]

COPY ./devops/publish_native/common/boot.sh /usr/local/bin/php_boot

CMD ["sh", "/usr/local/bin/php_boot"]
