---
name: gamehub-dotnet-rules
paths:
  - "**/*.cs"
  - "**/*.csproj"
description: Regras para código .NET no GameHub.
---

# GameHub — .NET Rules

- Preferir `record` ou classes imutáveis para value objects quando possível.
- Sempre usar `async`/`await` com `CancellationToken` em APIs públicas.
- EF Core: usar `AsNoTracking` em queries de leitura; `SaveChangesAsync`.
- Não usar `SELECT *`; projetar para DTOs.
- Validar entradas nos application services; lançar `UserFriendlyException` para regras de negócio.
- Adicionar `/// <summary>` para classes e métodos públicos.
- Nunca usar `throw ex;` — use `throw;` ou `throw new BusinessException("...", ex)`.
