FROM ubuntu:focal
WORKDIR /app
USER root

RUN cat /etc/os-release

RUN apt update --fix-missing
RUN apt install -y apt-transport-https wget ca-certificates iputils-ping telnet curl gnupg2

# Configure time
ARG DEBIAN_FRONTEND=noninteractive
ENV TZ = "UTC"
RUN apt-get install -y tzdata lsb-release

# Fix error: The configured user limit (128) on the number of inotify instances has been reached
#RUN echo "fs.inotify.max_user_watches=524288" >> /etc/sysctl.conf
#RUN echo "fs.inotify.max_user_instances=524288" >> /etc/sysctl.conf
#RUN sysctl -p

# Install .Net Core 5.0
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN curl https://packages.microsoft.com/config/ubuntu/20.10/prod.list | tee /etc/apt/sources.list.d/msprod.list
RUN apt update
RUN apt install -y dotnet-sdk-6.0

# Install Redis repositories
RUN curl -fsSL https://packages.redis.io/gpg | gpg --dearmor -o /usr/share/keyrings/redis-archive-keyring.gpg
RUN echo "deb [signed-by=/usr/share/keyrings/redis-archive-keyring.gpg] https://packages.redis.io/deb $(lsb_release -cs) main" | tee /etc/apt/sources.list.d/redis.list

# Update
RUN apt-get install -y net-tools sudo netcat \
    default-jre \
    postgresql postgresql-client redis

# Create dirs
RUN mkdir /tmp/test

ENV ASPNETCORE_URLS = "http://+:80" 
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true  
ENV DOTNET_CLI_HOME=/tmp/DOTNET_CLI_HOME
ENV HOME=/tmp

# Prepare project files
COPY . /app

ENTRYPOINT ["tail", "-f", "/dev/null"]
