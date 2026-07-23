# Eaf.KeyVault

## Visão Geral

O módulo `Eaf.KeyVault` fornece integração com Azure Key Vault para gerenciamento seguro de segredos e configurações sensíveis em aplicações EAF.

## Propósito

Gerenciar configurações sensíveis e segredos da aplicação de forma segura, utilizando o Azure Key Vault. Isso evita a necessidade de armazenar informações como connection strings, chaves de API, ou outros segredos diretamente em arquivos de configuração no repositório de código ou nos servidores de aplicação.

## Instalação

```bash
Install-Package Azure.Identity
Install-Package Azure.Security.KeyVault.Secrets
```

## Configuração

### 1. Configuração em appsettings.json

```json
{
  "AzureKeyVault": {
    "IsEnabled": true,
    "KeyVaultUri": "https://seu-eaf-keyvault.vault.azure.net/",
    "TenantId": "seu-tenant-id-azure-ad",
    "ClientId": "seu-client-id-do-service-principal",
    "ClientSecret": "seu-client-secret"
  }
}
```

### 2. Configuração em Program.cs

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            var builtConfig = config.Build();
            var keyVaultConfig = builtConfig.GetSection("AzureKeyVault");

            if (keyVaultConfig.GetValue<bool>("IsEnabled"))
            {
                var keyVaultUri = keyVaultConfig["KeyVaultUri"];
                if (!string.IsNullOrEmpty(keyVaultUri))
                {
                    var credential = new DefaultAzureCredential();
                    config.AddAzureKeyVault(new Uri(keyVaultUri), credential);
                }
            }
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

## Acesso aos Segredos

```csharp
public class MySecureService : ITransientDependency
{
    private readonly IConfiguration _configuration;

    public MySecureService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task CallExternalApi()
    {
        var apiKey = _configuration["ThirdPartyApiServiceKey"];
        // Use apiKey para autenticar na API externa
    }
}
```

## Troubleshooting

- **Permissões Insuficientes**: Verifique as roles e policies no portal Azure
- **URI do Key Vault Incorreto**: Verifique o valor de `KeyVaultUri`
- **Conectividade de Rede**: Verifique se há conectividade com o Key Vault
