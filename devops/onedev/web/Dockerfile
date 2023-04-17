FROM nginx:latest as base

COPY ./build/ /usr/share/nginx/html
WORKDIR /usr/share/nginx/html

#RUN dotnet publish --configuration=Production -o "/var/www" -f net7.0 -r linux-x64 --self-contained true

COPY ./devops/publish_native/web/nginx.conf /etc/nginx/nginx.conf 
EXPOSE 80
