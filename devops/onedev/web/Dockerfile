FROM nginx:latest as base

COPY ./build/TimeTracker.Web /usr/share/nginx/html
WORKDIR /usr/share/nginx/html

COPY ./devops/onedev/web/nginx.conf /etc/nginx/nginx.conf 
EXPOSE 80
