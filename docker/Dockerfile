FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY ./src/Yami.Api/bin/Debug/net10.0/publish .
ENTRYPOINT ["dotnet", "Yami.Api.dll"]