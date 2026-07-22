# EAF Angular UI Template

## Overview

The EAF (Enterprise Application Framework) Angular UI Template is a full-featured Single Page Application (SPA) built with Angular 18 and TypeScript 5.2. It serves as a starter template for enterprise applications, providing out-of-the-box functionality for authentication, authorization, multi-tenancy, administration, real-time communication, and theming.

## Technology Stack

- **Angular 18**: Modern frontend framework
- **TypeScript 5.2**: Type-safe JavaScript superset
- **PrimeNG 17**: UI component library
- **ngx-bootstrap 10.2**: Bootstrap components
- **SignalR 7.0.14**: Real-time communication
- **Moment.js 2.30.1**: Date/time manipulation
- **Metronic Admin Theme**: Professional admin template

## Features

- **Authentication**: Login, logout, external providers (Google, Microsoft, Auth0, OpenID Connect)
- **Authorization**: Permission-based access control
- **Multi-Tenancy**: Full multi-tenancy support with tenant management
- **Admin Panel**: Complete admin interface for users, roles, tenants, languages, settings
- **Real-Time Chat**: Built-in chat with SignalR
- **Notifications**: Real-time notifications system
- **Theme System**: Multiple built-in themes with customization
- **Audit Logs**: Comprehensive audit logging
- **Background Jobs**: Integration with Hangfire
- **Localization**: Multi-language support

## Documentation

For detailed documentation about the template, see the [docs folder](docs/).

### Available Documentation

- **[Getting Started](docs/GETTING_STARTED.md)** - Installation, configuration, and running the application
- **[Functionality](docs/FUNCTIONALITY.md)** - Detailed feature documentation
- **[Modules](docs/MODULES.md)** - Angular module structure and organization
- **[Implementations](docs/IMPLEMENTATIONS.md)** - Implementation patterns and best practices
- **[Components](docs/COMPONENTS.md)** - Component reference and usage
- **[Migration Guides](docs/)** - Migration guides for Angular version upgrades

## Quick Start

### Prerequisites

- Node.js v18 or later
- npm v9 or later
- Angular CLI v19 or later
- Backend API (EAF ASP.NET Core) must be running

### Installation

```bash
# Install dependencies
npm install

# Generate service proxies
npm run nswag

# Start development server
npm start
```

The application will be available at `http://localhost:4200`

### Configuration

Edit environment files in `src/environments/`:

**Development (environment.ts)**:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:21021',
  remoteServiceBaseUrl: 'http://localhost:21021',
  signalrUrl: 'http://localhost:21021/signalr'
};
```

**Production (environment.prod.ts)**:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api-domain.com',
  remoteServiceBaseUrl: 'https://your-api-domain.com',
  signalrUrl: 'https://your-api-domain.com/signalr'
};
```

## Project Structure

```
src/
├── account/                    # Account module (login, registration, password)
├── app/                        # Main application module
│   ├── admin/                  # Admin panel (lazy loaded)
│   ├── main/                   # Main business module (lazy loaded)
│   └── shared/                 # Shared app components
├── shared/                     # Shared across all modules
│   ├── animations/             # Route transition animations
│   ├── common/                 # Base classes, pipes, session
│   ├── helpers/                # URL, DOM, DataTable helpers
│   ├── service-proxies/        # NSwag-generated API client proxies
│   └── utils/                  # Directives, pipes, services
└── root.module.ts              # Application bootstrap module
```

## Development Scripts

```bash
# Start development server
npm start

# Build for production
npm run build

# Run unit tests
npm test

# Lint code
npm run lint

# Format code with Prettier
npm run prettier
npm run prettier-fix

# Generate service proxies
npm run nswag
```

## Docker Deployment

The template includes a `dockerfile` for containerized deployment using Node 20 and Nginx.

### Build

```bash
docker build -t eaf-angular-ui .
```

### Run with API URL via environment variables

```bash
docker run -d -p 80:80 \
  -e REMOTE_SERVICE_BASE_URL=http://localhost:8001 \
  -e APP_BASE_URL=http://localhost:8000 \
  -e ASPNETCORE_ENVIRONMENT=Local \
  eaf-angular-ui
```

### Configuration at runtime

The container uses `envsubst` to generate `assets/env.js` from `src/assets/env.template.js` at startup. The following variables are available:

- `REMOTE_SERVICE_BASE_URL` - URL base da API EAF (ex: `http://localhost:8001`)
- `APP_BASE_URL` - URL base da aplicação Angular (ex: `http://localhost:8000`)
- `ASPNETCORE_ENVIRONMENT` - Ambiente usado para carregar `appconfig.{env}.json` quando `REMOTE_SERVICE_BASE_URL` não é fornecido

Quando `REMOTE_SERVICE_BASE_URL` está definido, o `AppPreBootstrap` o utiliza diretamente e ignora o `appconfig.{env}.json`. Isso permite reutilizar a mesma imagem Docker em diferentes ambientes (dev, hom, prod) sem recompilar o frontend.

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## License

This template is part of the EAF (Enterprise Application Framework) project and follows the same license as the main project.

## Support

For issues and questions:

- Check the [documentation](docs/) folder
- Review the [migration guides](docs/) for Angular upgrade information
- Consult the main EAF documentation

## Contributing

When contributing to this template:

1. Follow the existing code style and patterns
2. Extend `AppComponentBase` for new components
3. Use the localization pipe for all user-facing text
4. Check permissions before showing sensitive UI
5. Add appropriate unit tests
6. Update documentation for new features
