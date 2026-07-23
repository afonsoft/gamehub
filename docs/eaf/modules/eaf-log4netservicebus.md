# Eaf.Log4NetServiceBus

## Visão Geral

O módulo `Eaf.Log4NetServiceBus` fornece um appender customizado para Log4Net que envia logs para filas ou tópicos do Azure Service Bus.

## Propósito

Enviar logs da aplicação para uma fila ou tópico do Azure Service Bus. Isso é útil para centralizar logs de múltiplas instâncias da aplicação, processamento assíncrono de logs, ou para integrar com sistemas de monitoramento e alerta.

## Instalação

```bash
Install-Package log4net
Install-Package Azure.Messaging.ServiceBus
```

## Configuração

### 1. Configuração em log4net.config

```xml
<log4net>
  <appender name="AzureServiceBusAppender" type="Eaf.Log4NetServiceBus.ServiceBusAppender, Eaf.Log4NetServiceBus">
    <connectionString value="Endpoint=sb://seu-eaf-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SUA_CHAVE_AQUI" />
    <entityName value="eaf-logs-queue" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %-5level %logger - %message%newline%exception" />
    </layout>
    <filter type="log4net.Filter.LevelRangeFilter">
      <levelMin value="INFO" />
      <levelMax value="FATAL" />
    </filter>
  </appender>

  <root>
    <level value="DEBUG" />
    <appender-ref ref="AzureServiceBusAppender" />
  </root>
</log4net>
```

### 2. Carregamento da Configuração

```csharp
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
Castle.Core.Logging.Log4NetFactory.UseLog4Net(logRepository.Name);
```

## Uso

```csharp
public class MyServiceUsingLog4Net : ITransientDependency
{
    public Castle.Core.Logging.ILogger Logger { get; set; } = NullLogger.Instance;

    public void DoWork()
    {
        Logger.Info("Esta mensagem será enviada para o Azure Service Bus via Log4Net.");
    }
}
```

## Casos de Uso

- **Centralização de Logs**: Agrega logs de múltiplas instâncias de aplicação
- **Processamento Assíncrono**: Permite envio rápido de logs para uma fila
- **Log-based Eventing**: Logs específicos podem disparar eventos ou workflows

## Troubleshooting

- **Conectividade**: Verifique a connectionString e se a fila/tópico existe
- **Permissões**: A política de acesso precisa ter permissão de "Send"
- **Tamanho da Mensagem**: Verifique se a mensagem não excede os limites do Service Bus
