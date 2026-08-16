FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY source/Core/                               source/Core/
COPY source/Infrastructure/                     source/Infrastructure/
COPY source/App/olympiad-quizzer-net.App.API/   source/App/olympiad-quizzer-net.App.API/

RUN dotnet publish source/App/olympiad-quizzer-net.App.API/olympiad-quizzer-net.App.API.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY data/ data/
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "olympiad-quizzer-net.App.API.dll"]
