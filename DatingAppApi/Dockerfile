
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app
EXPOSE 8080

#copy csproj and restore as disntict layers
COPY *.csproj ./
RUN dotnet restore 

#copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

#build run image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "DatingAppApi.dll"]