FROM nginx:latest as base

COPY ./build/TimeTracker.Web/wwwroot /var/www/wwwroot
WORKDIR /var/www/wwwroot

COPY ./devops/onedev/web/nginx.conf /etc/nginx/nginx.conf 
EXPOSE 80
