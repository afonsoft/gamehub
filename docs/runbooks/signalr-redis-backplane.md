# SignalR Redis backplane

O backplane é opcional e usa o pacote já referenciado
`Microsoft.AspNetCore.SignalR.StackExchangeRedis`. A configuração permanece
condicional para manter o modo de instância única sem Redis.

```json
{
  "RedisCache": {
    "IsEnabled": true,
    "ConnectionString": "<secret-managed-connection-string>"
  },
  "SignalR": {
    "Backplane": {
      "IsEnabled": true,
      "ChannelPrefix": "gamehub:production"
    }
  }
}
```

## Validação de duas instâncias

1. Execute duas instâncias da API com o mesmo Redis e o mesmo
   `SignalR:Backplane:ChannelPrefix`.
2. Conecte um cliente de teste a `/signalr-match` ou `/signalr-network` em cada
   instância.
3. Envie uma mensagem/grupo pela primeira instância e confirme a entrega na
   segunda.
4. Confirme que os logs não exibem connection strings e que o health check
   `multiplayer_presence_cache` permanece saudável.
5. Repita com prefixes diferentes e confirme que os ambientes não recebem
   eventos uns dos outros.

Sem backplane, `ICacheManager` continua compartilhando presença/TTL quando o
provider é distribuído, mas não replica mensagens ou grupos do SignalR.
