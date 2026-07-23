# Angular UI Template

## Visão Geral

O template Angular UI fornece uma interface de usuário completa usando Angular 18, TypeScript 5.2, e integração com o framework EAF.

## Tecnologias

- **Angular 18**: Framework frontend moderno
- **TypeScript 5.2**: Linguagem de tipagem estática
- **PrimeNG 17**: Biblioteca de componentes UI
- **SignalR**: Comunicação em tempo real
- **RxJS 7**: Programação reativa

## Estrutura do Projeto

```
Templates/Angular/
├── src/
│   ├── app/
│   │   ├── components/     # Componentes customizados
│   │   ├── services/       # Serviços HTTP e SignalR
│   │   ├── guards/         # Guards de rota e permissão
│   │   ├── interceptors/   # Interceptores HTTP
│   │   └── shared/         # Módulos compartilhados
│   ├── assets/
│   │   └── lib/
│   │       └── eaf-web-resources/  # Recursos do framework EAF
│   └── environments/       # Configurações de ambiente
```

## Configuração

### 1. Instalação de Dependências

```bash
npm install
```

### 2. Configuração de Ambiente

Edite `src/environments/environment.ts` para desenvolvimento e `src/environments/environment.prod.ts` para produção:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:21021',
  remoteServiceBaseUrl: 'http://localhost:21021',
  signalrUrl: 'http://localhost:21021/signalr'
};
```

### 3. Executar o Projeto

```bash
npm start
```

A aplicação estará disponível em `http://localhost:4200`

## Padrões EAF UI

### Integração com Framework EAF

O template Angular inclui integração customizada com o framework EAF.js localizado em `src/assets/lib/eaf-web-resources/`.

- **Mapeamentos @eaf/**: O projeto usa mapeamentos de módulos customizados
- **Integração SignalR**: Comunicação em tempo real com @microsoft/signalr ^7.0.14
- **jQuery Legacy**: O template ainda usa jQuery para algumas funcionalidades legadas

### Padrões Modernos Angular

- **Componentes Standalone**: Use componentes standalone (padrão no Angular 18+)
- **Signals**: Use signals para estado reativo
- **ChangeDetection OnPush**: Configure OnPush para performance
- **Controle de Fluxo Nativo**: Use @if, @for, @switch em vez de *ngIf, *ngFor, *ngSwitch
- **Inject()**: Use inject() em vez de injeção no construtor

## Componentes

### Lista de Componentes

O template inclui componentes pré-configurados:

- **AppComponent**: Componente principal
- **LayoutComponent**: Layout da aplicação
- **UserMenuComponent**: Menu de usuário
- **NotificationComponent**: Notificações
- **ModalComponent**: Modais reutilizáveis

### Serviços

- **AppService**: Serviço principal da aplicação
- **AuthService**: Serviço de autenticação
- **SignalRService**: Serviço de comunicação SignalR
- **HttpInterceptor**: Interceptor HTTP

## Acessibilidade

O template segue padrões de acessibilidade WCAG AA:

- Use atributos ARIA apropriados
- Suporte a navegação por teclado
- Contraste de cores adequado
- Deve passar em todos os checks AXE

## Migração de jQuery

O template ainda usa jQuery em algumas áreas. Considere migrar gradualmente para Angular puro:

1. Identificar funcionalidades que usam jQuery
2. Reimplementar usando Angular/TypeScript
3. Remover dependências jQuery após migração completa

## Troubleshooting

- **Erros de Build**: Verifique se todas as dependências estão instaladas
- **Integração EAF**: Teste todas as importações de módulos EAF após upgrades do Angular
- **SignalR**: Verifique se a URL do SignalR está correta
- **Performance**: Use OnPush e signals para melhorar performance
