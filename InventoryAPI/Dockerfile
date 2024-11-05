#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

#FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
#WORKDIR /app
#EXPOSE 5000

#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#WORKDIR /src
#COPY ["InventoryAPI.csproj", "./"]
#RUN dotnet restore "InventoryAPI.csproj"
#COPY . .
#RUN dotnet build "InventoryAPI.csproj" -c Release -o /app/build

#FROM build AS publish
#RUN dotnet publish "InventoryAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "InventoryAPI.dll"]



# Use the official .NET 6 runtime as a parent image
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine

# Set the working directory in the container
WORKDIR /app

# Copy the build output from your local machine to the container
COPY ./bin/Release/net6.0/ /app/

EXPOSE 11184
EXPOSE 1184
EXPOSE 80
EXPOSE 443

ENV RABBITMQ_HOSTNAME=rabbitmq_hostname
ENV RABBITMQ_PORT=rabbitmq_port
ENV RABBITMQ_USERNAME=rabbitmq_username
ENV RABBITMQ_PASSWORD=rabbitmq_password
ENV MONGODB_CONNECTION_STRING=mongodb_connection_string

ENV ASPNETCORE_URLS=http://+:1184

# Set the command to run your app using the dotnet command
# Replace 'MyApp.dll' with the name of your application's DLL
ENTRYPOINT ["dotnet", "InventoryAPI.dll"]
