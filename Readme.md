# 🌱 Documentação da API PlantaCore

## 📚 Endpoints

---

### 🔐 Autenticação

#### **POST `/api/v1/Autenticacao/registrar`**
Registra um novo usuário.

**Body:**
```json
{
  "nome": "string",
  "email": "string",
  "senha": "string"
}
```
**Respostas:**
- `200 OK`: Registro realizado com sucesso.
- `400 Bad Request`: Dados inválidos.

---

#### **POST `/api/v1/Autenticacao/login`**
Realiza o login de um usuário.

**Body:**
```json
{
  "email": "string",
  "senha": "string"
}
```
**Respostas:**
- `200 OK`: Login realizado com sucesso.
- `401 Unauthorized`: Credenciais inválidas.

---

#### **POST `/api/v1/Autenticacao/refresh-token`**
Atualiza o token de autenticação.

**Body:**
```json
{
  "tokenRefresh": "string"
}
```
**Respostas:**
- `200 OK`: Token atualizado com sucesso.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Autenticacao/logout`**
Realiza o logout do usuário.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Logout realizado com sucesso.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Autenticacao/confirmar-email`**
Confirma o email do usuário.

**Body:**
```json
{
  "email": "string",
  "codigoConfirmacao": "string"
}
```
**Respostas:**
- `200 OK`: Email confirmado com sucesso.
- `400 Bad Request`: Código inválido.

---

#### **POST `/api/v1/Autenticacao/resetar-senha`**
Envia um email para redefinir a senha.

**Body:**
```json
{
  "email": "string"
}
```
**Respostas:**
- `200 OK`: Email enviado com sucesso.

---

#### **POST `/api/v1/Autenticacao/nova-senha`**
Define uma nova senha para o usuário.

**Body:**
```json
{
  "email": "string",
  "codigoConfirmacao": "string",
  "novaSenha": "string"
}
```
**Respostas:**
- `200 OK`: Senha alterada com sucesso.
- `400 Bad Request`: Código inválido.

---

#### **POST `/api/v1/Autenticacao/trocar-senha`**
Troca a senha do usuário logado.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "senhaAtual": "string",
  "novaSenha": "string"
}
```
**Respostas:**
- `200 OK`: Senha alterada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Autenticacao/reenviar-confirmacao`**
Reenvia o email de confirmação.

**Body:**
```json
{
  "email": "string"
}
```
**Respostas:**
- `200 OK`: Email reenviado com sucesso.

---

### 📝 Posts

#### **POST `/api/v1/Post`**
Cria um novo post.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "conteudo": "string",
  "comunidadeId": "guid"
}
```
**Respostas:**
- `201 Created`: Post criado com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Post/{postId}`**
Atualiza um post existente.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "conteudo": "string"
}
```
**Respostas:**
- `200 OK`: Post atualizado com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Post/{postId}`**
Exclui um post.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Post excluído com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Post/{postId}`**
Obtém um post pelo ID.

**Respostas:**
- `200 OK`: Post encontrado.
- `404 Not Found`: Post não encontrado.

---

#### **GET `/api/v1/Post/feed`**
Obtém o feed do usuário.

**Headers:**  
`Authorization: Bearer <token>`

**Query Params:**
- `pagina`: int (default: 1)
- `tamanho`: int (default: 10)

**Respostas:**
- `200 OK`: Feed obtido com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Post/{postId}/curtir`**
Adiciona uma curtida a um post.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Curtida adicionada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Post/{postId}/curtida`**
Remove uma curtida de um post.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Curtida removida com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Post/comentario`**
Cria um comentário em um post.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "postId": "guid",
  "conteudo": "string"
}
```
**Respostas:**
- `201 Created`: Comentário criado com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Post/comentario/{comentarioId}`**
Atualiza um comentário.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "conteudo": "string"
}
```
**Respostas:**
- `200 OK`: Comentário atualizado com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Post/comentario/{comentarioId}`**
Exclui um comentário.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Comentário excluído com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Post/{postId}/comentarios`**
Lista os comentários de um post.

**Query Params:**
- `pagina`: int (default: 1)
- `tamanho`: int (default: 20)

**Respostas:**
- `200 OK`: Comentários listados com sucesso.
- `404 Not Found`: Post não encontrado.

---

#### **GET `/api/v1/Post/usuario/{usuarioId}`**
Lista os posts de um usuário.

**Query Params:**
- `pagina`: int (default: 1)
- `tamanho`: int (default: 10)

**Respostas:**
- `200 OK`: Posts listados com sucesso.
- `400 Bad Request`: Dados inválidos.

---

#### **GET `/api/v1/Post/explorar`**
Lista posts para explorar.

**Query Params:**
- `pagina`: int (default: 1)
- `tamanho`: int (default: 10)

**Respostas:**
- `200 OK`: Posts listados com sucesso.
- `400 Bad Request`: Dados inválidos.

---

#### **POST `/api/v1/Post/comentario/{comentarioId}/curtir`**
Adiciona uma curtida a um comentário.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Curtida adicionada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Post/comentario/{comentarioId}/curtida`**
Remove uma curtida de um comentário.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Curtida removida com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

### 🌿 Plantas

#### **POST `/api/v1/Planta/identificar`**
Identifica uma planta a partir de uma imagem.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**  
Formulário multipart com um arquivo de imagem.

**Respostas:**
- `200 OK`: Planta identificada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Planta/buscar`**
Busca plantas.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "nomePlanta": "string",
  "pagina": int
}
```
**Respostas:**
- `200 OK`: Plantas encontradas.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.
- `404 Not Found`: Nenhuma planta encontrada.

---

#### **POST `/api/v1/Planta/buscar/adicionar`**
Busca e adiciona uma planta.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "plantaTrefleId": int,
  "nomeCientifico": "string",
  "urlImagem": "string"
}
```
**Respostas:**
- `200 OK`: Planta adicionada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Planta/minhas-plantas`**
Lista as plantas do usuário.

**Headers:**  
`Authorization: Bearer <token>`

**Query Params:**
- `pagina`: int (default: 1)
- `tamanho`: int (default: 10)

**Respostas:**
- `200 OK`: Plantas listadas com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Planta/{plantaId}`**
Obtém uma planta pelo ID.

**Respostas:**
- `200 OK`: Planta encontrada.
- `404 Not Found`: Planta não encontrada.

---

#### **DELETE `/api/v1/Planta/{plantaId}`**
Exclui uma planta.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Planta excluída com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Planta/{plantaId}/gerar-lembrete-cuidado`**
Gera um lembrete de cuidado para uma planta.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Lembrete gerado com sucesso.
- `401 Unauthorized`: Token inválido.

---

### 🔔 Notificações

#### **GET `/api/v1/Notificacao`**
Obtém todas as notificações do usuário.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Notificações obtidas com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Notificacao/nao-lidas`**
Obtém todas as notificações não lidas do usuário.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Notificações não lidas obtidas com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Notificacao/{notificacaoId}/marcar-como-lida`**
Marca uma notificação como lida.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Notificação marcada como lida.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Notificacao/marcar-todas-como-lidas`**
Marca todas as notificações como lidas.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Todas as notificações marcadas como lidas.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Notificacao/{notificacaoId}`**
Exclui uma notificação específica.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Notificação excluída com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Notificacao`**
Exclui todas as notificações do usuário.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Todas as notificações excluídas com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

### 👤 Usuário

#### **GET `/api/v1/Usuario/perfil`**
Obtém o perfil do usuário logado.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Perfil obtido com sucesso.
- `401 Unauthorized`: Token inválido.
- `404 Not Found`: Perfil não encontrado.

---

#### **GET `/api/v1/Usuario/perfil-publico/{usuarioId}`**
Obtém o perfil público de um usuário.

**Respostas:**
- `200 OK`: Perfil público obtido com sucesso.
- `404 Not Found`: Perfil não encontrado.

---

#### **PUT `/api/v1/Usuario/nome`**
Atualiza o nome do usuário logado.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "novoNome": "string"
}
```
**Respostas:**
- `200 OK`: Nome atualizado com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Usuario/biografia`**
Atualiza a biografia do usuário logado.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "biografia": "string"
}
```
**Respostas:**
- `200 OK`: Biografia atualizada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Usuario/foto-perfil`**
Atualiza a foto de perfil do usuário logado.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**  
Formulário multipart com um arquivo de imagem.

**Respostas:**
- `200 OK`: Foto de perfil atualizada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Usuario/conta`**
Exclui a conta do usuário logado.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Conta excluída com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

### 🌐 Comunidades

#### **POST `/api/v1/Comunidade`**
Cria uma nova comunidade.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "nome": "string",
  "descricao": "string",
  "privada": true
}
```
**Respostas:**
- `201 Created`: Comunidade criada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Comunidade/{comunidadeId}`**
Obtém os detalhes de uma comunidade pelo ID.

**Respostas:**
- `200 OK`: Comunidade encontrada.
- `404 Not Found`: Comunidade não encontrada.

---

#### **PUT `/api/v1/Comunidade/{comunidadeId}`**
Atualiza os detalhes de uma comunidade.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "nome": "string",
  "descricao": "string",
  "privada": true
}
```
**Respostas:**
- `200 OK`: Comunidade atualizada com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Comunidade/{comunidadeId}`**
Exclui uma comunidade.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Comunidade excluída com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Comunidade/{comunidadeId}/participar`**
Solicita participação em uma comunidade privada ou entra em uma comunidade pública.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Solicitação enviada ou participação confirmada.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Comunidade/{comunidadeId}/sair`**
Sai de uma comunidade.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Saída confirmada.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Comunidade/{comunidadeId}/membros`**
Lista os membros de uma comunidade.

**Headers:**  
`Authorization: Bearer <token>`

**Query Params:**
- `pagina`: int (default: 1)
- `tamanho`: int (default: 10)

**Respostas:**
- `200 OK`: Membros listados com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Comunidade/explorar`**
Lista comunidades para explorar.

**Query Params:**
- `pagina`: int (default: 1)
- `tamanho`: int (default: 10)

**Respostas:**
- `200 OK`: Comunidades listadas com sucesso.
- `400 Bad Request`: Dados inválidos.

---

### 📅 Eventos

#### **GET `/api/v1/Evento`**
Obtém todos os eventos.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Lista de eventos obtida com sucesso.
- `401 Unauthorized`: Token inválido.

---

#### **GET `/api/v1/Evento/{id}`**
Obtém um evento pelo ID.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Evento encontrado.
- `404 Not Found`: Evento não encontrado.
- `401 Unauthorized`: Token inválido.

---

#### **POST `/api/v1/Evento`**
Cria um novo evento.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "titulo": "string",
  "descricao": "string",
  "data": "datetime",
  "local": "string"
}
```

**Respostas:**
- `200 OK`: Evento criado com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Evento`**
Atualiza um evento existente.

**Headers:**  
`Authorization: Bearer <token>`

**Body:**
```json
{
  "id": "guid",
  "titulo": "string",
  "descricao": "string",
  "data": "datetime",
  "local": "string"
}
```

**Respostas:**
- `200 OK`: Evento atualizado com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **DELETE `/api/v1/Evento/{eventoId}`**
Remove um evento.

**Headers:**  
`Authorization: Bearer <token>`

**Respostas:**
- `200 OK`: Evento removido com sucesso.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Evento/marcar-participacao`**
Marca participação em um evento.

**Headers:**  
`Authorization: Bearer <token>`

**Query Params:**
- `eventoId`: guid

**Respostas:**
- `200 OK`: Participação confirmada.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---

#### **PUT `/api/v1/Evento/desmarcar-participacao`**
Remove participação em um evento.

**Headers:**  
`Authorization: Bearer <token>`

**Query Params:**
- `eventoId`: guid

**Respostas:**
- `200 OK`: Participação removida.
- `400 Bad Request`: Dados inválidos.
- `401 Unauthorized`: Token inválido.

---