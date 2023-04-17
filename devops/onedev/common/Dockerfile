FROM mcr.microsoft.com/dotnet/runtime:7.0

# Change timezone to local time
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY ./build/ /app
WORKDIR /app/

ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true  
ENV ASPNETCORE_URLS=http://+:80  
EXPOSE 80

COPY ./devops/publish_native/common/boot.sh /usr/local/bin/php_boot

CMD ["sh", "/usr/local/bin/php_boot"]


