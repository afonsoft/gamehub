


# Environment Setup for EAF Deployment

This document outlines the steps required to set up the environment for deploying the Enterprise Application Framework (EAF).

## 1. Prerequisites

*   **Operating System**: Windows Server, Linux (Ubuntu, CentOS), or other compatible OS.
*   **.NET Runtime**: Ensure the .NET Runtime (version 9.0 is recommended) is installed.
*   **Database Server**: A database server (e.g., SQL Server, PostgreSQL) must be installed and configured.
*   **Web Server**: A web server (e.g., IIS, Apache, Nginx) must be installed and configured.

## 2. Installing the .NET Runtime

You can download the .NET Runtime from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

## 3. Configuring the Database

1.  **Install a Database Server**: Install a database server (e.g., SQL Server, PostgreSQL).
2.  **Create a Database**: Create a database for your EAF application.
3.  **Configure the Connection String**: Update the connection string in the `appsettings.json` file of your web project (e.g., `Eaf.Web.Mvc`).

## 4. Installing a Web Server

*   **IIS (Windows Server)**:
    1.  Install the IIS role.
    2.  Configure a website for your EAF application.
    3.  Set the application pool to use the .NET CLR.

*   **Apache (Linux)**:
    1.  Install Apache.
    2.  Configure a virtual host for your EAF application.
    3.  Use `mod_wsgi` to host the application.

*   **Nginx (Linux)**:
    1.  Install Nginx.
    2.  Configure a server block for your EAF application.
    3.  Use a reverse proxy to forward requests to the application.

## 5. Configuring the Web Server

1.  **Create a Website or Virtual Host**: Create a website or virtual host for your EAF application.
2.  **Set the Document Root**: Set the document root to the `wwwroot` directory of your web project.
3.  **Configure HTTPS**: Configure HTTPS to encrypt all communication between the client and the server.

## 6. Deploying the Application

1.  **Build the Application**: Build the application in Release mode.
2.  **Copy the Files**: Copy the files from the `publish` directory to the document root of your web server.
3.  **Configure Environment Variables**: Configure the environment variables for your application.

## 7. Next Steps

*   Test the deployment.
*   Monitor the application for errors.
*   Configure logging and alerting.



