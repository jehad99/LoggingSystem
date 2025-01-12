Distributed Logging System - Backend

This backend service is a logging system built with .NET Core, providing a robust API for log management. It includes features such as batch log processing and integration with AWS S3 for log storage.

Steps to Run the Project

1. Clone the Repository

git clone <repository_url>
cd <repository_folder>

2. Ensure Prerequisites are Installed

.NET SDK (version 8.0 or compatible with the project)

Docker

SQL Server (or Dockerized SQL Server)

3. Configure Environment Variables

Set the following environment variables in a .env file or directly in your Docker Compose setup:

ConnectionStrings__DefaultConnection: Connection string for the SQL Server database

DOTNET_URLS: URL for the backend server (e.g., http://0.0.0.0:8080)

4. Build and Run with Docker Compose

Run the following command to start all services:

=> docker-compose up --build

5. Access the API

Backend API: http://localhost:8080

Swagger UI (Development Mode): http://localhost:8080/swagger

