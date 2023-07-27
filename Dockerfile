FROM mcr.microsoft.com/dotnet/sdk:7.0 as BUILD
# Set the working directory
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY src/semantic-kernel-sidecar/*.csproj /app/
# Restore dependencies
RUN dotnet restore
# Copy everything else and build
COPY src/semantic-kernel-sidecar /app/
# Build
RUN dotnet publish -c Release -o sksidecar



# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as RUNTIME
# Set the working directory
WORKDIR /app
# Copy the build artifacts from the BUILD stage
COPY --from=BUILD /app/sksidecar .
# Set the entrypoint
ENTRYPOINT ["dotnet", "semantic-kernel-sidecar.dll"]
# Set the port
EXPOSE 8080
# Set the healthcheck
HEALTHCHECK --interval=5s --timeout=3s --start-period=5s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1
# Set the environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
# Set the user
USER 1001
# Set the command
CMD ["dotnet", "semantic-kernel-sidecar.dll"]
