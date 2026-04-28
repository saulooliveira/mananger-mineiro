# AGENTS — Regras para Codex / Claude Code

## Regra geral
Implemente só o pedido. Poucas mudanças. Sem refatoração grande sem necessidade. Codigo o mais simples. Melhor padrao desenvolvimento.

## Obrigatório
Após qualquer alteração, atualizar `docs/CHANGELOG.md`.

Formato obrigatório:

```md
## YYYY-MM-DD
- autor: Codex | Claude Code | Humano
- arquivo: caminho/do/arquivo
- ação: criação | alteração | correção | remoção
- compilou: sim | não | N/A
- testado: sim | não | N/A
- observação: texto curto
```

## Design
Usar `paperclipai/paperclip` como referência visual:
- sidebar
- cards
- layout limpo
- painel central
- painel lateral
- cores neutras

Não copiar logo/marca/textos.

## Prioridade
1. Banco SQLite
2. API produtos
3. UI produtos
4. seleção múltiplo 4
5. PDF A4 QuestPDF
6. preview
7. impressão
8. quantidade_impressa

## Proibido no MVP
- login
- usuário
- cloud
- permissões
- template editor avançado
