FROM nginx:latest as base

FROM mcr.microsoft.com/dotnet/sdk:7.0 as dot_net

ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true  
ENV ASPNETCORE_URLS=http://+:80  

# Change timezone to local time
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY . /app
WORKDIR /app/TimeTracker.Web

RUN apt update
RUN apt install -y apt-transport-https wget ca-certificates
RUN echo "{}" > appsettings.Local.json
RUN dotnet publish --configuration=Production -o "/var/www" -f net7.0 -r linux-x64 --self-contained true

FROM base
WORKDIR /usr/share/nginx/html
COPY ./devops/publish_native/web/nginx.conf /etc/nginx/nginx.conf 
COPY --from=dot_net /var/www/ /var/www/
RUN ls /usr/share/nginx/html
EXPOSE 80
