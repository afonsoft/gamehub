# Eaf.Middleware.Ldap

## Visão Geral

O módulo `Eaf.Middleware.Ldap` fornece integração com LDAP/Active Directory para autenticação em aplicações EAF.

## Propósito

Permitir a autenticação de usuários contra um servidor LDAP, comum em muitas organizações para gerenciamento centralizado de identidades.

## Instalação

```bash
Install-Package Novell.Directory.Ldap.NETStandard
```

## Configuração

### 1. Configuração em appsettings.json

```json
{
  "Ldap": {
    "IsEnabled": true,
    "Server": "ldap.suaempresa.com",
    "Port": 389,
    "UseSsl": false,
    "BindDn": "cn=serviceuser,ou=services,dc=suaempresa,dc=com",
    "BindPassword": "senhaDaContaDeServico",
    "UserSearchBase": "ou=users,ou=accounts,dc=suaempresa,dc=com",
    "UserSearchFilter": "(&(objectClass=person)(sAMAccountName={0}))",
    "DomainName": "SUAEMPRESA",
    "AttributeMapping": {
      "UserName": "sAMAccountName",
      "Email": "mail",
      "Name": "givenName",
      "Surname": "sn",
      "PhoneNumber": "telephoneNumber"
    }
  }
}
```

### 2. Configuração em Startup.cs

```csharp
services.Configure<LdapOptions>(Configuration.GetSection("Ldap"));
services.AddScoped<ILdapAuthenticationService, LdapAuthenticationService>();
```

## Uso

```csharp
public class LdapAuthenticationService : ILdapAuthenticationService
{
    private readonly LdapOptions _options;

    public LdapAuthenticationService(IOptions<LdapOptions> options)
    {
        _options = options.Value;
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        using var connection = new LdapConnection();
        connection.Connect(_options.Server, _options.Port);
        
        if (_options.UseSsl)
        {
            connection.SecureSocketLayer = true;
        }

        try
        {
            connection.Bind(_options.BindDn, _options.BindPassword);
            
            var searchFilter = _options.UserSearchFilter.Replace("{0}", username);
            var searchResult = connection.Search(
                _options.UserSearchBase,
                LdapConnection.SCOPE_SUB,
                searchFilter,
                new string[] { "dn" }
            );

            if (searchResult.Count > 0)
            {
                var userDn = searchResult.Next().Dn;
                connection.Bind(userDn, password);
                return true;
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            connection.Disconnect();
        }
        return false;
    }
}
```

## Troubleshooting

- **Conectividade**: Verifique se o servidor EAF pode alcançar o servidor LDAP
- **Credenciais de Bind**: Verifique BindDn e BindPassword
- **User Search Base/Filter**: Verifique se o filtro está correto para seu diretório LDAP
- **SSL/TLS**: Requer que o servidor EAF confie no certificado do servidor LDAP
