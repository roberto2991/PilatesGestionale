# Fase 1: Compilazione e pubblicazione
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia il file di progetto e ripristina le dipendenze
COPY ["PilatesGestionale.csproj", "./"]
RUN dotnet restore "PilatesGestionale.csproj"

# Copia tutti i file sorgenti e pubblica l'applicazione
COPY . .
RUN dotnet publish "PilatesGestionale.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Fase 2: Immagine di runtime finale
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copia l'output della compilazione dalla fase precedente
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PilatesStudio.dll"]