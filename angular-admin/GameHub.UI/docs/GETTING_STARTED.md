# Getting Started - EAF Angular UI Template

## Prerequisites

Before running the EAF Angular UI template, ensure you have the following installed:

- **Node.js**: v18 or later (LTS recommended)
- **npm**: v9 or later (comes with Node.js)
- **Angular CLI**: v18 or later (can be installed via npm)
- **Backend API**: The EAF ASP.NET Core API must be running

## Installation

### 1. Clone or Copy the Template

Copy the `GameHub.UI` folder to your desired location.

### 2. Install Dependencies

Navigate to the project directory and install npm dependencies:

```bash
cd GameHub.UI
npm install
```

This will install all required packages including Angular 18, PrimeNG, SignalR, and other dependencies.

## Configuration

### 1. Environment Configuration

Edit the environment files in `src/environments/`:

**Development (`environment.ts`)**:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:21021',
  remoteServiceBaseUrl: 'http://localhost:21021',
  signalrUrl: 'http://localhost:21021/signalr'
};
```

**Production (`environment.prod.ts`)**:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api-domain.com',
  remoteServiceBaseUrl: 'https://your-api-domain.com',
  signalrUrl: 'https://your-api-domain.com/signalr'
};
```

### 2. Update App Constants

Edit `src/shared/AppConsts.ts` to configure application-specific settings:

```typescript
export class AppConsts {
  static readonly remoteServiceBaseUrl = environment.remoteServiceBaseUrl;
  static readonly appBaseUrl = environment.appBaseUrl || '';
  static readonly recaptchaSiteKey = 'YOUR_RECAPTCHA_SITE_KEY';
  static readonly googleAnalytics = 'YOUR_GA_ID';
  static readonly googleTagManager = 'YOUR_GTM_ID';
}
```

### 3. Generate Service Proxies

The template uses NSwag to generate API client proxies. Run:

```bash
npm run nswag
```

This will generate TypeScript service proxies from the backend API Swagger definition.

## Running the Application

### Development Mode

Start the development server with hot module replacement:

```bash
npm start
```

Or use the Angular CLI directly:

```bash
ng serve
```

The application will be available at `http://localhost:4200`

### Production Build

Build the application for production:

```bash
npm run build
```

This creates an optimized build in the `dist/` directory.

### Production Server

To serve the production build, you can use any static file server:

**Using http-server**:

```bash
npm install -g http-server
http-server dist/GameHub.UI
```

**Using Nginx**:

```nginx
server {
    listen 80;
    server_name your-domain.com;
    root /path/to/dist/GameHub.UI;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

## Docker Deployment

The template includes a Dockerfile for containerized deployment.

### Build Docker Image

```bash
docker build -t eaf-angular-ui .
```

### Run Docker Container

```bash
docker run -p 80:80 eaf-angular-ui
```

### Docker Compose

Create a `docker-compose.yml`:

```yaml
version: '3.8'
services:
  angular-ui:
    build: .
    ports:
      - "80:80"
    environment:
      - API_URL=http://your-api-server:21021
```

Run with:

```bash
docker-compose up
```

## Development Scripts

Available npm scripts in `package.json`:

```bash
# Start development server
npm start

# Build for production
npm run build

# Run unit tests
npm test

# Run e2e tests
npm run e2e

# Lint code
npm run lint

# Format code with Prettier
npm run prettier
npm run prettier-fix

# Generate service proxies
npm run nswag

# Build with i18n
npm run build-i18n

# Extract i18n messages
npm run extract-i18n
```

## Troubleshooting

### Common Issues

**1. Module Not Found Errors**

If you encounter module not found errors:

```bash
rm -rf node_modules package-lock.json
npm install
```

**2. CORS Errors**

Ensure the backend API allows CORS from your frontend URL. Configure CORS in the backend `Startup.cs`:

```csharp
app.UseCors(options =>
{
    options.WithOrigins("http://localhost:4200")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
});
```

**3. SignalR Connection Issues**

Verify the SignalR URL in environment configuration matches the backend SignalR endpoint.

**4. Proxy Generation Fails**

Ensure the backend API is running and accessible at the configured URL before running `npm run nswag`.

**5. Build Errors**

Clear the Angular cache:

```bash
rm -rf .angular
npm start
```

## Browser Support

The EAF Angular UI template supports the following browsers:

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Next Steps

After successfully running the application:

1. Read the [Functionality Documentation](FUNCTIONALITY.md) for detailed feature information
2. Review the [Modules Documentation](MODULES.md) for module structure
3. Check the [Components Documentation](COMPONENTS.md) for component usage
4. See the [Implementations Documentation](IMPLEMENTATIONS.md) for implementation patterns
