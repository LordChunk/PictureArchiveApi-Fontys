FROM microsoft/dotnet:2.1-sdk
COPY ./publish /publish
WORKDIR /publish
EXPOSE 5000/tcp
ENTRYPOINT ["dotnet", "Logic.dll"]