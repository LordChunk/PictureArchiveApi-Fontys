FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["PictureArchiveApi/Logic.csproj", "PictureArchiveApi/"]
COPY ["Model/Models.csproj", "Model/"]
COPY ["DAL/DAL.csproj", "DAL/"]
RUN dotnet restore "PictureArchiveApi/Logic.csproj"
COPY . .
WORKDIR "/src/PictureArchiveApi"
RUN dotnet build "Logic.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Logic.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Logic.dll"]