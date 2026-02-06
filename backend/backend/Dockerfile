FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

COPY backend.csproj ./
RUN dotnet restore

COPY . ./

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]
