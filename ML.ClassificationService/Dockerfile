#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ML.ClassificationService/ML.ClassificationService.csproj", "ML.ClassificationService/"]
COPY ["ML.BL/ML.BL.csproj", "ML.BL/"]
COPY ["ML.BL.Mongo/ML.BL.Mongo.csproj", "ML.BL.Mongo/"]
COPY ["ML.Domain.Entities/ML.Domain.Entities.csproj", "ML.Domain.Entities/"]
COPY ["ML.Infrastructure/ML.Infrastructure.csproj", "ML.Infrastructure/"]
COPY ["ML.Utils/ML.Utils.csproj", "ML.Utils/"]
COPY ["ML.Domain/ML.Domain.csproj", "ML.Domain/"]
COPY ["ML.Infrastructure.DependecyResolution/ML.Infrastructure.DependecyResolution.csproj", "ML.Infrastructure.DependecyResolution/"]
COPY ["ML.Core/ML.Core.csproj", "ML.Core/"]
COPY ["ML.ImageClassification.Train/ML.ImageClassification.Training.csproj", "ML.ImageClassification.Train/"]
RUN dotnet restore "ML.ClassificationService/ML.ClassificationService.csproj"
COPY . .
WORKDIR "/src/ML.ClassificationService"
RUN dotnet build "ML.ClassificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ML.ClassificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ML.ClassificationService.dll"]