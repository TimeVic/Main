FROM ubuntu:focal AS build
WORKDIR /app
USER root

COPY . /app

# Configure time
ENV TZ = "UTC"
RUN 'ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone'

# Fix error: The configured user limit (128) on the number of inotify instances has been reached
RUN echo "fs.inotify.max_user_watches=524288" >> /etc/sysctl.conf
RUN echo "fs.inotify.max_user_instances=524288" >> /etc/sysctl.conf
RUN sysctl -p

# Install .Net Core 5.0
RUN 'wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb'
RUN 'dpkg -i packages-microsoft-prod.deb'

# Update
RUN apt-get update
RUN apt-get install -y apt-transport-https wget ca-certificates apt-transport-https debconf-utils \
    net-tools sudo netcat \
    default-jre \
    dotnet-sdk-5.0 \
    postgresql postgresql-client

# Assign permissions
RUN chmod -R 700 $KAFKA_HOME
RUN chmod -R 700 ./devops/common/kafka/boot.sh
RUN chmod -R 700 ./devops/common/zookeeper/boot.sh

# Create dirs
RUN 'mkdir /tmp/test'

ENV HOME = '/tmp'
ENV DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
ENV ASPNETCORE_ENVIRONMENT = "Development"
ENV DOTNET_USE_POLLING_FILE_WATCHER = true  
ENV ASPNETCORE_URLS = "http://+:80" 
RUN echo "{}" > appsettings.Local.json
RUN echo "{}" > TimeTracker.Api/appsettings.Local.json
RUN echo "{}" > TimeTracker.Migrations/appsettings.Local.json
RUN dotnet restore

COPY ./devops/test/test/boot.sh /usr/local/bin/php_boot
CMD ["sh", "/usr/local/bin/php_boot"]
