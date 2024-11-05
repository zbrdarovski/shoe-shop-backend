FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine

# Set the working directory in the container
WORKDIR /app

# Copy the build output from your local machine to the container
COPY ./bin/Release/net6.0/ /app/

EXPOSE 11187
EXPOSE 1187
EXPOSE 80
EXPOSE 443

ENV RABBITMQ_HOSTNAME=rabbitmq_hostname
ENV RABBITMQ_PORT=rabbitmq_port
ENV RABBITMQ_USERNAME=rabbitmq_username
ENV RABBITMQ_PASSWORD=rabbitmq_password
ENV MONGODB_CONNECTION_STRING=mongodb_connection_string

ENV ASPNETCORE_URLS=http://+:1187

# Set the command to run your app using the dotnet command
ENTRYPOINT ["dotnet", "StatsBrdarovski.dll"]
