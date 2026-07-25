# GameHub CLI

O GameHub CLI permite que desenvolvedores e equipes de CI/CD enviem builds HTML5/WebGL diretamente para a plataforma, sem precisar acessar o portal web.

## Instalação

O CLI é um client leve baseado em HTTP. Você pode usá-lo via `curl` ou embutir em scripts de CI.

## Autenticação

Toda requisição CLI exige uma API Key vinculada ao perfil do desenvolvedor (`DeveloperProfile`) ou à equipe (`DeveloperTeam`).

A API Key pode ser obtida no portal do desenvolvedor em **Profile → API Keys** ou gerenciada pelo administrador para equipes.

## Contrato `gamehub.json`

Adicione um arquivo `gamehub.json` na raiz do repositório do jogo:

```json
{
  "apiKey": "gh_dev_xxxxxxxxxxxx",
  "gameSlug": "meu-jogo-legal",
  "version": "1.2.3",
  "packagePath": "dist.zip"
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `apiKey` | Sim | Chave da API do desenvolvedor ou equipe. |
| `gameSlug` | Sim | Slug público do jogo cadastrado na GameHub. |
| `version` | Não | Versão do build. Se omitido, o backend gera `1.0.N`. |
| `packagePath` | Não | Caminho relativo do pacote `.zip`. Padrão: `dist.zip`. |

## Upload de Build

Endpoint: `POST /api/services/app/GameBuild/UploadFromCli`

O corpo da requisição é um JSON com o conteúdo do `gamehub.json` e o pacote codificado em Base64:

```json
{
  "apiKey": "gh_dev_xxxxxxxxxxxx",
  "gameSlug": "meu-jogo-legal",
  "version": "1.2.3",
  "package": "<base64-do-dist.zip>"
}
```

### Exemplo com curl

```bash
#!/bin/bash
set -e

API_KEY="gh_dev_xxxxxxxxxxxx"
GAME_SLUG="meu-jogo-legal"
VERSION="1.2.3"
PACKAGE="dist.zip"

PACKAGE_B64=$(base64 -w 0 "$PACKAGE")

curl -X POST https://gamehub.afonsoft.dev/api/services/app/GameBuild/UploadFromCli \
  -H "Content-Type: application/json" \
  -d "{
    \"apiKey\": \"$API_KEY\",
    \"gameSlug\": \"$GAME_SLUG\",
    \"version\": \"$VERSION\",
    \"package\": \"$PACKAGE_B64\"
  }"
```

## Resposta

```json
{
  "result": {
    "buildId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "version": "1.2.3",
    "status": "Validated",
    "validationSummary": {
      "isValid": true,
      "packageSizeBytes": 123456,
      "hasIndexHtml": true,
      "indexHtmlPath": "index.html",
      "errors": [],
      "warnings": []
    }
  },
  "success": true
}
```

## Fluxo Recomendado

1. Compile o jogo para WebGL/HTML5.
2. Compacte o diretório de saída em `dist.zip`.
3. Envie via `UploadFromCli`.
4. Valide o `validationSummary`.
5. Acesse o portal para aprovar o build e submeter para revisão (ou use o Versions Tab para preview/inspector).

## Permissões e Roles

- Membros com papel `Developer` ou `Billing` em uma equipe podem fazer upload com a API Key da equipe.
- O upload vincula o build ao jogo identificado pelo `gameSlug` e ao perfil resolvido a partir da API Key.

## Limites

- Tamanho máximo do pacote: 100 MB.
- Apenas arquivos `.zip` são aceitos.
- Builds inválidos (sem `index.html`, com requests externos não declarados, etc.) são rejeitados automaticamente.
