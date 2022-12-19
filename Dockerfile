FROM mcr.microsoft.com/dotnet/sdk:7.0.101-alpine3.16-amd64 AS build-env
WORKDIR /app
EXPOSE 80/tcp

# copy sln file
COPY *.sln ./

# Copy the main source project files
COPY src/*/*.csproj ./
RUN for file in *.csproj; do mkdir -p src/"${file%.*}"/ && mv "$file" src/"${file%.*}"/; done

# Copy the test project files
# COPY test/*/*.csproj ./
# RUN for file in *.csproj; do mkdir -p test/"${file%.*}"/ && mv "$file" test/"${file%.*}"/; done

# restore nuget packages
RUN dotnet restore

# copy everything else and build app
COPY ./src/ ./src

# Build and publish a release
RUN dotnet publish ./src/Zlib.Torznab.Presentation.API/ -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0.1-alpine3.16-amd64
RUN apk add mysql-client=10.6.10-r0 --no-cache && rm -rf /var/cache/apk/*
WORKDIR /app
COPY --from=build-env /app/out .
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Zlib.Torznab.Presentation.API.dll"]
