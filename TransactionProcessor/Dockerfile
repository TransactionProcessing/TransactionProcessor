FROM stuartferguson/txnprocbase:latest AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TransactionProcessor/NuGet.Config", "."]
COPY ["TransactionProcessor/TransactionProcessor.csproj", "TransactionProcessor/"]
COPY ["TransactionProcessor/NuGet.Config", "TransactionProcessor/"]
COPY ["TransactionProcessor.DataTransferObjects/TransactionProcessor.DataTransferObjects.csproj", "TransactionProcessor.DataTransferObjects/"]
COPY ["TransactionProcessor.BusinessLogic/TransactionProcessor.BusinessLogic.csproj", "TransactionProcessor.BusinessLogic/"]
COPY ["TransactionProcessor.Models/TransactionProcessor.Models.csproj", "TransactionProcessor.Models/"]
RUN dotnet restore "TransactionProcessor/TransactionProcessor.csproj"
COPY . .
WORKDIR "/src/TransactionProcessor"
RUN dotnet build "TransactionProcessor.csproj" -c Release -o /app/build

# Sort out certificate stuff here
RUN openssl x509 -inform DER -in /src/TransactionProcessor/aspnetapp-root-cert.cer -out /src/TransactionProcessor/aspnetapp-root-cert.crt
RUN cp /src/TransactionProcessor/aspnetapp-root-cert.crt /usr/local/share/ca-certificates/
RUN update-ca-certificates

FROM build AS publish
RUN dotnet publish "TransactionProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransactionProcessor.dll"]