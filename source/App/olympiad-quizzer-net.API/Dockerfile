FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY source/Core/                            source/Core/
COPY source/Infrastructure/                  source/Infrastructure/
COPY source/App/olympiad-quizzer-net.API/    source/App/olympiad-quizzer-net.API/

RUN dotnet publish source/App/olympiad-quizzer-net.API/olympiad-quizzer-net.API.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "olympiad-quizzer-net.API.dll"]
