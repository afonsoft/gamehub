# Eaf.KeyVault.AspNetCore

## Visão Geral

O módulo `Eaf.KeyVault.AspNetCore` fornece integração específica do Azure Key Vault com ASP.NET Core, simplificando a configuração para aplicações web EAF.

## Propósito

Facilitar a integração do Azure Key Vault com aplicações ASP.NET Core, fornecendo configurações pré-definidas e helpers específicos para o ambiente web.

## Instalação

```bash
Install-Package Azure.Identity
Install-Package Azure.Security.KeyVault.Secrets
Install-Package Microsoft.Extensions.Configuration.AzureKeyVault
```

## Configuração

### 1. Configuração em appsettings.json

```json
{
  "AzureKeyVault": {
    "IsEnabled": true,
    "KeyVaultUri": "https://seu-eaf-keyvault.vault.azure.net/",
    "ReloadInterval": "00:05:00"
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
                    var reloadInterval = keyVaultConfig.GetValue<TimeSpan?>("ReloadInterval");
                    
                    var builder = config.AddAzureKeyVault(new Uri(keyVaultUri), credential);
                    
                    if (reloadInterval.HasValue)
                    {
                        builder.AddAzureKeyVault(new Uri(keyVaultUri), credential, new KeyVaultSecretClientOptions
                        {
                            ReloadInterval = reloadInterval.Value
                        });
                    }
                }
            }
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

## Recursos Adicionais

- **Recarga Automática**: Configuração opcional para recarregar segredos automaticamente
- **Managed Identity**: Suporte nativo para Azure Managed Identity
- **Secrets Management**: Gerenciamento simplificado de segredos em ambiente web

## Troubleshooting

- **Recarga de Segredos**: Verifique o intervalo de recarga se segredos não estão sendo atualizados
- **Permissões**: Verifique se a Managed Identity tem permissões de "Get" e "List"
