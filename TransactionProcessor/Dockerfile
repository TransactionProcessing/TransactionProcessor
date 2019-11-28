FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
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