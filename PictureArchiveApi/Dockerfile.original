## Build Command
#docker build --rm -f "PictureArchiveApi\Dockerfile" -t picturearchiveapi:latest PictureArchiveApi
#FROM gcr.io/google-appengine/aspnetcore:2.1
#
## Copy csproj and restore as distinct layers
#COPY *.csproj ./
#RUN dotnet restore
#
## Copy everything else and build
#COPY . ./
#RUN dotnet publish -c Release -o out
#
#COPY . /app
#WORKDIR /app
#ENTRYPOINT ["dotnet", "Logic.dll"]

FROM microsoft/dotnet:2.1-sdk
COPY . /app
#Restore Logic Layer
WORKDIR /app/src/Logic
RUN ["dotnet", "restore"]


#Restore DAL Layer
WORKDIR /app/src/DAL
RUN ["dotnet", "restore"]

#Restore Models Layer
WORKDIR /app//Models
RUN ["dotnet", "restore"]

## copy csproj and restore as distinct layers
#COPY *.csproj ./
#RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/Logic.dll"]