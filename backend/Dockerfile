FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG PROJECT
WORKDIR /src

COPY . .

RUN dotnet restore "$PROJECT"
RUN dotnet publish "$PROJECT" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    --no-self-contained

RUN project_dir="$(dirname "$PROJECT")"; \
    if [ -f "$project_dir/Keys/public_key.pem" ]; then \
        mkdir -p /app/publish/Keys; \
        cp "$project_dir/Keys/public_key.pem" /app/publish/Keys/public_key.pem; \
    fi

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

ARG DLL
ENV APP_DLL=$DLL
ENV ASPNETCORE_HTTP_PORTS=8080

WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "dotnet \"$APP_DLL\""]
