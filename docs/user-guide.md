# Guia do Desenvolvedor — GameHub

## Introdução

Este guia orienta desenvolvedores de jogos HTML5/WebGL a publicarem, gerenciarem e monetizarem títulos na plataforma GameHub.

## Conteúdo

1. [Primeiros passos](#primeiros-passos)
2. [Criando um jogo](#criando-um-jogo)
3. [Enviando builds](#enviando-builds)
4. [Integrando o SDK](#integrando-o-sdk)
5. [Publicação e moderação](#publicação-e-moderação)
6. [Analytics e earnings](#analytics-e-earnings)
7. [Multiplayer e chat](#multiplayer-e-chat)
8. [Perguntas frequentes](#perguntas-frequentes)

## Primeiros passos

### Criar uma conta

1. Acesse o portal público do GameHub.
2. Clique em **Register** e preencha e-mail, nome e senha.
3. Confirme o e-mail, se necessário.
4. Faça login para acessar o **Developer Portal**.

### Criar perfil de desenvolvedor

No Developer Portal, complete:

- Nome do estúdio ou desenvolvedor.
- Site e redes sociais (opcional).
- Informações fiscais/pagamento para repasse de receita.

## Criando um jogo

1. No Developer Portal, vá em **Games > New Game**.
2. Preencha:
   - **Title**: nome público do jogo.
   - **Slug**: identificador único usado na URL.
   - **Description**: resumo para o catálogo.
   - **Category**: gênero do jogo.
   - **Age Rating**: classificação indicativa.
   - **Orientation**: portrait, landscape ou ambos.
3. Salve como **Draft**.

### Assets obrigatórios

- Ícone (512x512 PNG).
- Capa/catalog (1280x720 ou 1920x1080).
- Screenshots (mínimo 3).
- Descrição em pelo menos um idioma suportado.

## Enviando builds

### Requisitos do pacote

- Formato: arquivo ZIP.
- Tamanho máximo: conforme `MaxBuildPackageSizeBytes` configurado na instância.
- Conteúdo obrigatório: `index.html` na raiz.
- Conteúdo proibido: executáveis (`.exe`, `.dll`, `.bat`, `.cmd`, `.ps1`), scripts de servidor, malware.

### Processo de upload

1. Na página do jogo, acesse a aba **Builds**.
2. Arraste ou selecione o arquivo ZIP.
3. Aguarde a validação automática:
   - Verificação de `index.html`.
   - Cálculo de hash SHA-256.
   - Verificação de tamanho e conteúdo.
4. O build receberá o status **Pending**.

### Versões

- Cada upload cria uma nova versão.
- A versão aprovada mais recente pode ser definida como **Live**.
- Builds rejeitados exibem motivo e permitem novo upload.

## Integrando o SDK

### Inicialização

```html
<script src="https://<gamehub-host>/assets/gameplay-bridge.js"></script>
<script>
  GameHubSDK.init({ gameId: 'seu-game-id' });
</script>
```

### Eventos obrigatórios

- `gameLoadingFinished`: quando o carregamento termina.
- `gameplayStart`: quando o jogador inicia uma partida.
- `gameplayStop`: quando a partida termina.
- `commercialBreakCompleted`: após anúncio padrão.
- `rewardedBreakCompleted`: após anúncio premiado.
- `gameErrorCaptured`: erros capturados pelo jogo.
- `measure`: eventos customizados de analytics.

### Exemplo

```javascript
GameHubSDK.gameLoadingFinished();
GameHubSDK.gameplayStart();
// durante o jogo
GameHubSDK.commercialBreakCompleted();
GameHubSDK.rewardedBreakCompleted({ success: true });
GameHubSDK.gameplayStop();
```

## Publicação e moderação

### Submeter para revisão

1. Certifique-se de que há um build **Approved**.
2. Na página do jogo, clique em **Submit for Review**.
3. Preencha notas para o moderador, se desejar.
4. O jogo passará para status **Under Review**.

### Estados de moderação

| Status      | Significado                                    |
|-------------|------------------------------------------------|
| Draft       | Em edição, não visível ao público.             |
| Under Review| Aguardando análise da equipe.                  |
| Approved    | Aprovado, mas ainda não publicado.             |
| Rejected    | Rejeitado; motivo disponível no painel.        |
| Published   | Visível no catálogo e jogável.                 |

### Atualizar jogo publicado

1. Envie um novo build.
2. Após aprovação, clique em **Publish** para torná-lo live.
3. O catálogo e os jogadores passam a usar a nova versão automaticamente.

## Analytics e earnings

### Dashboard

Acesse **Dashboard** para visualizar:

- Total de plays e jogadores únicos.
- Sessões finalizadas e duração média.
- Eventos de gameplay (start, loading, errors).
- Comerciais e rewarded breaks.

### Filtros

- Período (from/to).
- País.
- Tipo de dispositivo.
- Fonte de tráfego.
- Campanha UTM.

### Earnings

A aba **Earnings** mostra:

- Receita bruta estimada.
- Parcela do desenvolvedor.
- Parcela da plataforma.
- Breakdown por jogo e dia.

Importante: os valores são **estimados** e não representam pagamento confirmado.

### Exportar CSV

1. Aplique os filtros desejados.
2. Clique em **Export CSV**.
3. O arquivo será baixado com os dados do período.

## Multiplayer e chat

### Multiplayer

Use `GameHubSDK.multiplayer` para:

- Criar ou ingressar em salas.
- Enviar mensagens de jogo.
- Sincronizar estado entre jogadores.

### Chat

Use `GameHubSDK.chat` para:

- Enviar mensagens de texto em salas.
- Reportar mensagens inapropriadas.
- Receber moderação automática por palavras proibidas.

## Perguntas frequentes

**Preciso hospedar o jogo no GameHub?**
Sim. O build é hospedado na infraestrutura do GameHub e executado em iframe protegido.

**Posso usar WebGL?**
Sim. Builds HTML5/WebGL são suportados desde que rodem em um `index.html`.

**Como funciona o repasse de receita?**
A divisão depende do contrato de revenue share configurado para o jogo (non-exclusive ou exclusive).

**Como reportar um bug?**
Use o formulário de suporte no Developer Portal ou envie um e-mail para o time de plataforma.
