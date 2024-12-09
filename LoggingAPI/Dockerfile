#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine

# Set the working directory in the container
WORKDIR /app

# Copy the build output from your local machine to the container
COPY ./bin/Release/net6.0/ /app/

EXPOSE 11186
EXPOSE 1186
EXPOSE 80
EXPOSE 443

ENV RABBITMQ_HOSTNAME=rabbitmq_hostname
ENV RABBITMQ_PORT=rabbitmq_port
ENV RABBITMQ_USERNAME=rabbitmq_username
ENV RABBITMQ_PASSWORD=rabbitmq_password
ENV MONGODB_CONNECTION_STRING=mongodb_connection_string

ENV ASPNETCORE_URLS=http://+:1186

# Set the command to run your app using the dotnet command
ENTRYPOINT ["dotnet", "LoggingAPI.dll"]
