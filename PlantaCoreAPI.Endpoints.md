# PlantaCoreAPI — Documentação de Endpoints (OpenAPI)

> **Baseado no código real dos controllers, DTOs e OpenAPI.**

---

## [AUTENTICAÇÃO]

### POST /api/v1/Autenticacao/registrar
- **Body:** JSON (RegistroDTOEntrada)
  - `nome` (string, obrigatório)
  - `email` (string, obrigatório)
  - `senha` (string, obrigatório)
  - `confirmacaoSenha` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Autenticacao/login
- **Body:** JSON (LoginDTOEntrada)
  - `email` (string, obrigatório)
  - `senha` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### POST /api/v1/Autenticacao/refresh-token
- **Body:** JSON (RefreshTokenDTOEntrada)
  - `tokenRefresh` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### POST /api/v1/Autenticacao/logout
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### POST /api/v1/Autenticacao/confirmar-email
- **Body:** JSON (ConfirmarEmailDTOEntrada)
  - `usuarioId` (uuid, obrigatório)
  - `token` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Autenticacao/resetar-senha
- **Body:** JSON (ResetarSenhaDTOEntrada)
  - `email` (string, obrigatório)
- **Respostas:**
  - 200 OK

### POST /api/v1/Autenticacao/nova-senha
- **Body:** JSON (NovaSenhaDTOEntrada)
  - `usuarioId` (uuid, obrigatório)
  - `token` (string, obrigatório)
  - `novaSenha` (string, obrigatório)
  - `confirmacaoSenha` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Autenticacao/trocar-senha
- **Autenticação:** Bearer
- **Body:** JSON (TrocarSenhaDTOEntrada)
  - `senhaAtual` (string, obrigatório)
  - `novaSenha` (string, obrigatório)
  - `confirmacaoSenha` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Autenticacao/reenviar-confirmacao
- **Body:** JSON (ResetarSenhaDTOEntrada)
  - `email` (string, obrigatório)
- **Respostas:**
  - 200 OK

---

## [ARMAZENAMENTO]

### GET /api/v1/armazenamento/fotos/listar
- **Respostas:**
  - 200 OK

### POST /api/v1/armazenamento/foto/upload
- **Body:** multipart/form-data
  - `foto` (arquivo, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/armazenamento/foto-perfil/upload
- **Body:** multipart/form-data
  - `foto` (arquivo, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/armazenamento/foto-planta/upload
- **Body:** multipart/form-data
  - `foto` (arquivo, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/armazenamento/foto
- **Parâmetros:**
  - Query: `nomeArquivo` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/armazenamento/fotos/todas
- **Parâmetros:**
  - Header: `X-Admin-Key` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 403 Forbidden
  - 401 Unauthorized

---

## [COMUNIDADE]

### POST /api/v1/Comunidade
- **Autenticação:** Bearer
- **Body:** JSON (CriarComunidadeDTOEntrada)
  - `nome` (string, opcional)
  - `descricao` (string, opcional)
  - `privada` (boolean, obrigatório)
- **Respostas:**
  - 201 Created
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Comunidade/{comunidadeId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Body:** JSON (AtualizarComunidadeDTOEntrada)
  - `nome` (string, opcional)
  - `descricao` (string, opcional)
  - `privada` (boolean, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Comunidade/{comunidadeId}
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### DELETE /api/v1/Comunidade/{comunidadeId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Comunidade
- **Parâmetros:**
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK

### GET /api/v1/Comunidade/buscar
- **Parâmetros:**
  - Query: `termo` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Comunidade/minhas
- **Autenticação:** Bearer
- **Parâmetros:**
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### GET /api/v1/Comunidade/usuario/{usuarioId}
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK

### POST /api/v1/Comunidade/{comunidadeId}/entrar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Comunidade/{comunidadeId}/sair
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Comunidade/{comunidadeId}/posts
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Comunidade/{comunidadeId}/expulsar/{usuarioId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
  - Path: `usuarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Comunidade/{comunidadeId}/transferir-admin
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Body:** JSON (TransferirAdminDTOEntrada)
  - `novoAdminId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Comunidade/recomendadas
- **Parâmetros:**
  - Query: `quantidade` (int, opcional)
- **Respostas:**
  - 200 OK

### GET /api/v1/Comunidade/{comunidadeId}/membros
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK

### GET /api/v1/Comunidade/{comunidadeId}/admins
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK

### GET /api/v1/Comunidade/{comunidadeId}/sou-membro
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### POST /api/v1/Comunidade/{comunidadeId}/solicitar-entrada
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Comunidade/{comunidadeId}/solicitacoes
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### PUT /api/v1/Comunidade/{comunidadeId}/solicitacoes/{usuarioId}/aprovar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
  - Path: `usuarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

---

## [EVENTO]

### GET /api/v1/Evento
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK

### GET /api/v1/Evento/{id}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `id` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### POST /api/v1/Evento
- **Autenticação:** Bearer
- **Body:** JSON (CriarEventoDTO)
  - `titulo` (string, obrigatório)
  - `descricao` (string, obrigatório)
  - `localizacao` (string, obrigatório)
  - `dataInicio` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### PUT /api/v1/Evento/{id}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `id` (uuid, obrigatório)
- **Body:** JSON (AtualizarEventoDTO)
  - `titulo` (string, obrigatório)
  - `descricao` (string, obrigatório)
  - `localizacao` (string, obrigatório)
  - `dataInicio` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### PUT /api/v1/Evento/marcar-participacao
- **Autenticação:** Bearer
- **Query:**
  - `eventoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### PUT /api/v1/Evento/desmarcar-participacao
- **Autenticação:** Bearer
- **Query:**
  - `eventoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### DELETE /api/v1/Evento/{eventoId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `eventoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Evento/{eventoId}/participantes
- **Parâmetros:**
  - Path: `eventoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK

---

## [LEMBRETE CUIDADO]

### POST /api/v1/lembretes-cuidado/gerar-para-todas-plantas
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

---

## [NOTIFICAÇÃO]

### GET /api/v1/Notificacao
- **Autenticação:** Bearer
- **Parâmetros:**
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Notificacao/nao-lidas
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Notificacao/{notificacaoId}/marcar-como-lida
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `notificacaoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Notificacao/marcar-todas-como-lidas
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Notificacao/{notificacaoId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `notificacaoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Notificacao
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Notificacao/configuracoes
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### PUT /api/v1/Notificacao/configuracoes
- **Autenticação:** Bearer
- **Body:** JSON (ConfiguracoesNotificacaoDTOEntrada)
  - `receberCurtidas` (boolean, opcional)
  - `receberComentarios` (boolean, opcional)
  - `receberNovoSeguidor` (boolean, opcional)
  - `receberSolicitacaoSeguir` (boolean, opcional)
  - `receberSolicitacaoAceita` (boolean, opcional)
  - `receberEvento` (boolean, opcional)
  - `receberPlantaCuidado` (boolean, opcional)
  - `receberPlantaIdentificada` (boolean, opcional)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

---

## [PLANTA]

### POST /api/v1/Planta/identificar
- **Autenticação:** Bearer
- **Body:** multipart/form-data
  - `Foto` (arquivo, obrigatório)
  - `Comentario` (string, opcional)
  - `CriarPostagem` (boolean, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Planta/buscar
- **Autenticação:** Bearer
- **Body:** JSON (BuscaPlantaDTOEntrada)
  - `nomePlanta` (string, opcional)
  - `pagina` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized
  - 404 Not Found

### POST /api/v1/Planta/buscar/adicionar
- **Autenticação:** Bearer
- **Body:** JSON (AdicionarPlantaTrefleDTO)
  - `plantaTrefleId` (int, obrigatório)
  - `nomeCientifico` (string, obrigatório)
  - `urlImagem` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Planta/minhas-plantas
- **Autenticação:** Bearer
- **Parâmetros:**
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Planta/minhas-plantas/buscar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Query: `termo` (string, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Planta/{plantaId}
- **Parâmetros:**
  - Path: `plantaId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### DELETE /api/v1/Planta/{plantaId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `plantaId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Planta/{plantaId}/gerar-lembrete-cuidado
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `plantaId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### GET /api/v1/Planta/{plantaId}/posts
- **Parâmetros:**
  - Path: `plantaId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK

---

## [POST]

### POST /api/v1/Post
- **Autenticação:** Bearer
- **Body:** JSON (CriarPostDTOEntrada)
  - `conteudo` (string, obrigatorio)
  - `plantaId` (uuid, opcional)
  - `comunidadeId` (uuid, opcional)
- **Respostas:**
  - 201 Created
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Post/{postId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Body:** JSON (AtualizarPostDTOEntrada)
  - `conteudo` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Post/{postId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Post/{postId}
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### GET /api/v1/Post/feed
- **Autenticação:** Bearer
- **Parâmetros:**
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
  - Query: `cursor` (string, opcional)
  - Query: `dataInicio` (date, opcional)
  - Query: `dataFim` (date, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Post/{postId}/curtir
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Post/{postId}/curtida
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Post/comentario
- **Autenticação:** Bearer
- **Body:** JSON (CriarComentarioDTOEntrada)
  - `postId` (uuid, opcional)
  - `conteudo` (string, opcional)
- **Respostas:**
  - 201 Created
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Post/comentario/{comentarioId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comentarioId` (uuid, obrigatório)
- **Body:** JSON (AtualizarComentarioDTOEntrada)
  - `conteudo` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Post/comentario/{comentarioId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comentarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Post/{postId}/comentarios
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
  - Query: `ordenar` (string, opcional)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### GET /api/v1/Post/usuario/{usuarioId}
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Post/explorar
- **Parâmetros:**
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Post/usuario/{usuarioId}/curtidos
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Post/comentario/{comentarioId}/curtir
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comentarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Post/comentario/{comentarioId}/curtida
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comentarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Post/{postId}/comentario/{comentarioId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
  - Path: `comentarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Post/buscar/hashtag
- **Parâmetros:**
  - Query: `hashtag` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Post/buscar/categoria
- **Parâmetros:**
  - Query: `categoria` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Post/buscar/palavra-chave
- **Parâmetros:**
  - Query: `palavraChave` (string, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Post/trending
- **Parâmetros:**
  - Query: `quantidade` (int, opcional)
- **Respostas:**
  - 200 OK

### POST /api/v1/Post/{postId}/salvar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Post/{postId}/salvar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Post/{postId}/compartilhar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Post/{postId}/visualizar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `postId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Post/comentario/{comentarioId}/responder
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `comentarioId` (uuid, obrigatório)
- **Body:** string (conteudo)
- **Respostas:**
  - 201 Created
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Post/comunidade/{comunidadeId}
- **Parâmetros:**
  - Path: `comunidadeId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Post/buscar
- **Parâmetros:**
  - Query: `hashtag` (string, opcional)
  - Query: `categoria` (string, opcional)
  - Query: `palavraChave` (string, opcional)
  - Query: `usuarioId` (uuid, opcional)
  - Query: `comunidadeId` (uuid, opcional)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

---

## [USUÁRIO]

### GET /api/v1/Usuario/perfil
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 401 Unauthorized
  - 404 Not Found

### GET /api/v1/Usuario/perfil-publico/{usuarioId}
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### PUT /api/v1/Usuario/nome
- **Autenticação:** Bearer
- **Body:** JSON (AtualizarNomeDTOEntrada)
  - `novoNome` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Usuario/biografia
- **Autenticação:** Bearer
- **Body:** JSON (AtualizarBiografiaDTOEntrada)
  - `biografia` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### PUT /api/v1/Usuario/privacidade
- **Autenticação:** Bearer
- **Body:** JSON (AlterarPrivacidadePerfilDTOEntrada)
  - `privado` (boolean, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Usuario/foto-perfil
- **Autenticação:** Bearer
- **Body:** multipart/form-data
  - `foto` (arquivo, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Usuario/conta
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Usuario/reativar/solicitar
- **Body:** JSON (SolicitarReativacaoDTOEntrada)
  - `email` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Usuario/reativar/confirmar
- **Body:** JSON (ReativarComTokenDTOEntrada)
  - `email` (string, opcional)
  - `token` (string, opcional)
  - `novaSenha` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Usuario/reativar/verificar-token
- **Body:** JSON (VerificarTokenReativacaoDTOEntrada)
  - `email` (string, opcional)
  - `token` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Usuario/reativar/resetar-senha
- **Body:** JSON (ResetarSenhaSemTokenDTOEntrada)
  - `email` (string, opcional)
  - `novaSenha` (string, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### POST /api/v1/Usuario/seguir/{usuarioIdParaSeguir}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `usuarioIdParaSeguir` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### DELETE /api/v1/Usuario/seguir/{usuarioIdParaDeseguir}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `usuarioIdParaDeseguir` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Usuario/{usuarioId}/seguidores
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### GET /api/v1/Usuario/{usuarioId}/seguindo
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 404 Not Found

### GET /api/v1/Usuario/{usuarioId}/seguidores/lista
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### GET /api/v1/Usuario/{usuarioId}/seguindo/lista
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### POST /api/v1/Usuario/solicitacao-seguir/{alvoId}
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `alvoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Usuario/solicitacoes-seguir
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### POST /api/v1/Usuario/solicitacoes-seguir/{solicitacaoId}/aceitar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `solicitacaoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### POST /api/v1/Usuario/solicitacoes-seguir/{solicitacaoId}/rejeitar
- **Autenticação:** Bearer
- **Parâmetros:**
  - Path: `solicitacaoId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 400 Bad Request
  - 401 Unauthorized

### GET /api/v1/Usuario/{usuarioId}/plantas
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Usuario/{usuarioId}/posts
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK
  - 400 Bad Request

### GET /api/v1/Usuario/{usuarioId}/relacao
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### GET /api/v1/Usuario/sugestoes
- **Autenticação:** Bearer
- **Parâmetros:**
  - Query: `quantidade` (int, opcional)
- **Respostas:**
  - 200 OK

### GET /api/v1/Usuario/posts-salvos
- **Autenticação:** Bearer
- **Respostas:**
  - 200 OK
  - 401 Unauthorized

### GET /api/v1/Usuario/buscar
- **Parâmetros:**
  - Query: `nome` (string, opcional)
- **Respostas:**
  - 200 OK

### GET /api/v1/Usuario/{usuarioId}/comunidades
- **Parâmetros:**
  - Path: `usuarioId` (uuid, obrigatório)
  - Query: `pagina` (int, opcional)
  - Query: `tamanho` (int, opcional)
- **Respostas:**
  - 200 OK

---

## [BUSCA]

### GET /api/v1/busca
- **Parâmetros:**
  - Query: `termo` (string, obrigatório)
- **Respostas:**
  - 200 OK

---

> **Observação:**
> - Todos os endpoints protegidos exigem o envio do token JWT no header Authorization: Bearer.
> - Parâmetros obrigatórios e opcionais foram validados conforme os DTOs e controllers reais.
> - Para detalhes de outros módulos, envie os controllers para validação.
