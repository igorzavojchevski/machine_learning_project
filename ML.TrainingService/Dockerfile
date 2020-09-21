#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ML.TrainingService/ML.TrainingService.csproj", "ML.TrainingService/"]
COPY ["ML.Infrastructure.DependecyResolution/ML.Infrastructure.DependecyResolution.csproj", "ML.Infrastructure.DependecyResolution/"]
COPY ["ML.BL.Mongo/ML.BL.Mongo.csproj", "ML.BL.Mongo/"]
COPY ["ML.Domain.Entities/ML.Domain.Entities.csproj", "ML.Domain.Entities/"]
COPY ["ML.Infrastructure/ML.Infrastructure.csproj", "ML.Infrastructure/"]
COPY ["ML.BL/ML.BL.csproj", "ML.BL/"]
COPY ["ML.Utils/ML.Utils.csproj", "ML.Utils/"]
COPY ["ML.Domain/ML.Domain.csproj", "ML.Domain/"]
COPY ["ML.Core/ML.Core.csproj", "ML.Core/"]
COPY ["ML.ImageClassification.Train/ML.ImageClassification.Training.csproj", "ML.ImageClassification.Train/"]
RUN dotnet restore "ML.TrainingService/ML.TrainingService.csproj"
COPY . .
WORKDIR "/src/ML.TrainingService"
RUN dotnet build "ML.TrainingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ML.TrainingService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ML.TrainingService.dll"]