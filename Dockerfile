FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG GITHUB_SHA
ARG VERSION

ADD backend /backend
ADD Botan.sln /

# restore nuget packages
RUN dotnet restore

# build the app
RUN \
    echo "version=$VERSION" && \
    dotnet publish -c Release -o /app /p:Version=${VERSION}


FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim

ARG GITHUB_SHA

LABEL org.opencontainers.image.authors="Yura Hunter <yurii.myslyvets@gmail.com>" \
      org.opencontainers.image.description="botan.in.ua - q&a platform for students" \
      org.opencontainers.image.source="https://github.com/yurii-hunter/botan" \
      org.opencontainers.image.title="Botan" \
      org.opencontainers.image.url="https://botan.in.ua" \
      org.opencontainers.image.revision="${GITHUB_SHA}"

WORKDIR /app

# copy artifacts from build stage
COPY --from=build /app /app

EXPOSE 80

ENTRYPOINT ["dotnet", "Botan.Web.dll"]
