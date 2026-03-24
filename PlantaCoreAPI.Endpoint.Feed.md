# Endpoint do Feed de Posts

## Requisição

```
GET /api/v1/post/feed?pagina=1&tamanho=10&ordenarPor=mais_recente|mais_antigo|mais_curtido|mais_comentado
```

### Parâmetros de Query
- **pagina** (opcional): número da página (ex: 1)
- **tamanho** (opcional): quantidade de posts por página (ex: 10)
- **ordenarPor** (opcional): filtro de ordenação. Valores aceitos:
  - `mais_recente` (padrão)
  - `mais_antigo`
  - `mais_curtido`
  - `mais_comentado`

### Exemplo de requisição
```
GET /api/v1/post/feed?pagina=2&tamanho=20&ordenarPor=mais_curtido
```

## Resposta esperada (JSON)
```json
{
  "sucesso": true,
  "dados": [
    {
      "id": "guid",
      "usuarioId": "guid",
      "nomeUsuario": "string",
      "conteudo": "string",
      "totalCurtidas": 0,
      "totalComentarios": 0,
      "curtiuUsuario": false,
      "comentadoPorMim": false,
      "dataCriacao": "2024-05-01T12:00:00Z"
      // ...outros campos do post
    }
    // ...outros posts
  ],
  "meta": {
    "pagina": 2,
    "tamanho": 20,
    "total": 100,
    "totalPaginas": 5
  }
}
```

## Observações
- O usuário deve estar autenticado (enviar token JWT no header Authorization).
- Se não passar `ordenarPor`, o padrão é `mais_recente`.
- Os parâmetros de paginação são opcionais.
