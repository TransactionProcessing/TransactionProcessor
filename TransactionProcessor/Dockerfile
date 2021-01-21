FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
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

FROM build AS publish
RUN dotnet publish "TransactionProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransactionProcessor.dll"]