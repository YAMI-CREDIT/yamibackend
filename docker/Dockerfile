FROM mcr.microsoft.com/dotnet/aspnet:10.0

EXPOSE 8080

WORKDIR /app

COPY publish/ .

RUN useradd yamibackend -u 5017 -s /bin/sh && \
    chown -R yamibackend:yamibackend /app

USER 5017

ENTRYPOINT ["dotnet", "Yami.Api.dll"]