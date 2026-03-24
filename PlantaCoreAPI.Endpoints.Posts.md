# Endpoints de Listagem de Posts com Filtros

## Feed do Usuário Autenticado
```
GET /api/v1/post/feed?pagina=1&tamanho=10&ordenarPor=mais_recente|mais_antigo|mais_curtido|mais_comentado
```
- **pagina**: número da página (opcional, padrão 1)
- **tamanho**: quantidade de itens por página (opcional, padrão 10)
- **ordenarPor**: 
  - `mais_recente` (padrão)
  - `mais_antigo`
  - `mais_curtido`
  - `mais_comentado`

---

## Posts de um Usuário
```
GET /api/v1/post/usuario/{usuarioId}?pagina=1&tamanho=10&ordenarPor=mais_recente|mais_antigo|mais_curtido|mais_comentado
```
- **usuarioId**: ID do usuário
- **pagina**, **tamanho**, **ordenarPor**: igual ao feed

---

## Explorar Posts
```
GET /api/v1/post/explorar?pagina=1&tamanho=10&ordenarPor=mais_recente|mais_antigo|mais_curtido|mais_comentado
```
- **pagina**, **tamanho**, **ordenarPor**: igual ao feed

---

## Posts de uma Comunidade
```
GET /api/v1/post/comunidade/{comunidadeId}?pagina=1&tamanho=10&ordenarPor=mais_recente|mais_antigo|mais_curtido|mais_comentado
```
- **comunidadeId**: ID da comunidade
- **pagina**, **tamanho**, **ordenarPor**: igual ao feed

---

### Valores aceitos em `ordenarPor`:
- `mais_recente` (padrão)
- `mais_antigo`
- `mais_curtido`
- `mais_comentado`

Se não passar o parâmetro, o padrão é `mais_recente`.
Os parâmetros de paginação são opcionais.
