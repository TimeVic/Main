FROM mcr.microsoft.com/dotnet/sdk:7.0

# Change timezone to local time
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY . /app
WORKDIR /app/

ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true  
ENV ASPNETCORE_URLS=http://+:80  
EXPOSE 80

RUN echo "{}" > appsettings.Local.json
RUN echo "{}" > TimeTracker.Tests.Integration.Api/appsettings.Local.json
RUN echo "{}" > TimeTracker.Tests.Integration.Business/appsettings.Local.json
RUN echo "{}" > TimeTracker.Api/appsettings.Local.json
RUN echo "{}" > TimeTracker.Migrations/appsettings.Local.json

RUN ["dotnet", "restore", "--verbosity=q"]
RUN ["dotnet", "build", "--verbosity=q"]

COPY ./devops/publish_native/common/boot.sh /usr/local/bin/php_boot

CMD ["sh", "/usr/local/bin/php_boot"]
