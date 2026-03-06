# ?? Roadmap 15 Semanas — Pastas & Arquivos por Semana

## ?? Visão Geral

```
Semana 1-2:   Setup + Autenticação
Semana 3-4:   Plantas + IA
Semana 5-6:   Rede Social Básica
Semana 7-8:   Curtidas & Comentários
Semana 9-10:  Notificações + Seguir
Semana 11-12: Lembretes + Upload
Semana 13-14: Testes + Polimento
Semana 15:    WebSocket + Deploy
```

---

## ? SEMANA 1-2: Setup + Autenticação

**Status:** ? COMPLETO

### Backend — Arquivos para Subir

```
PlantaCoreAPI.Domain/
??? Entities/
?   ??? Usuario.cs
??? Enums/
?   ??? (vazio, sem enums)
??? Interfaces/
    ??? IRepositorio.cs
    ??? IRepositorioUsuario.cs

PlantaCoreAPI.Application/
??? DTOs/
?   ??? Auth/
?       ??? RegistroDTOEntrada.cs
?       ??? LoginDTOEntrada.cs
?       ??? LoginDTOSaida.cs
??? Interfaces/
?   ??? IAuthenticationService.cs
?   ??? IJwtService.cs
?   ??? IPasswordHashService.cs
?   ??? IEmailService.cs
??? Comuns/
    ??? Resultado.cs

PlantaCoreAPI.Infrastructure/
??? Dados/
?   ??? PlantaCoreDbContext.cs
??? Repositorios/
?   ??? RepositorioUsuario.cs
??? Services/
?   ??? AuthenticationService.cs
?   ??? PasswordHashService.cs
?   ??? External/
?       ??? JwtService.cs
?       ??? EmailService.cs
??? Migrations/
    ??? Initial/
    ?   ??? 20250303000000_InitialCreate.cs
    ?   ??? 20250303000000_InitialCreate.Designer.cs

PlantaCoreAPI.API/
??? Controllers/
?   ??? AutenticacaoController.cs
??? Extensions/
?   ??? AuthExtensions.cs
?   ??? DatabaseExtensions.cs
?   ??? ServicesExtensions.cs
?   ??? SwaggerExtensions.cs
?   ??? LoggingExtensions.cs
??? Program.cs
??? appsettings.json
```

### Frontend — Arquivos para Criar

```
src/
??? api/
?   ??? client.ts              (cliente HTTP com interceptores)
?   ??? auth.ts               (funções de autenticação)
??? pages/
?   ??? Registro.tsx          (página de registro)
?   ??? Login.tsx             (página de login)
??? types/
?   ??? auth.ts               (tipos TypeScript)
??? hooks/
    ??? useAuth.ts            (hook de autenticação)
```

### Deliverables

- ? API rodando em `http://localhost:5123`
- ? Endpoint: `POST /api/v1/autenticacao/registrar`
- ? Endpoint: `POST /api/v1/autenticacao/login`
- ? Frontend: Página de login/registro
- ? Tokens salvos em `localStorage`

---

## ?? SEMANA 3-4: Plantas + IA

**Status:** ? COMPLETO

### Backend — Arquivos para Adicionar

```
PlantaCoreAPI.Domain/
??? Entities/
?   ??? Planta.cs             (nova entidade)
??? Interfaces/
    ??? IRepositorioPlanta.cs (nova interface)

PlantaCoreAPI.Application/
??? DTOs/
?   ??? Planta/
?   ?   ??? PlantaDTOSaida.cs
?   ?   ??? AdicionarPlantaTrefleDTO.cs
?   ??? Identificacao/
?       ??? IdentificacaoDTOs.cs
??? Interfaces/
    ??? IPlantService.cs
    ??? IGeminiService.cs
    ??? IPlantNetService.cs
    ??? ITrefleService.cs
    ??? IFileStorageService.cs

PlantaCoreAPI.Infrastructure/
??? Repositorios/
?   ??? RepositorioPlanta.cs (nova)
??? Services/
?   ??? Plant/
?   ?   ??? PlantService.cs
?   ?   ??? PlantService.Identificacao.cs
?   ?   ??? PlantService.Busca.cs
?   ?   ??? PlantService.Crud.cs
?   ?   ??? PlantService.GeminiParser.cs
?   ?   ??? PlantService.Mapper.cs
?   ?   ??? DadosPlantaEnriquecidos.cs
?   ??? External/
?       ??? PlantNetService.cs (novo)
?       ??? TrefleService.cs (novo)
?       ??? GeminiService.cs (novo)
??? Storage/
    ??? SupabaseFileStorageService.cs (novo)

PlantaCoreAPI.API/
??? Controllers/
?   ??? PlantaController.cs (novo)
??? appsettings.json (adicionar chaves de API)
```

### Frontend — Arquivos para Adicionar

```
src/
??? api/
?   ??? planta.ts             (funções de plantas)
??? pages/
?   ??? IdentificarPlanta.tsx (identificação por foto)
?   ??? MinhasPlantas.tsx     (listar plantas)
?   ??? DetalhePlanta.tsx     (detalhe de uma planta)
??? components/
?   ??? UploadFoto.tsx        (upload de arquivo)
?   ??? PlantaCard.tsx        (card de planta)
??? types/
    ??? planta.ts             (tipos TypeScript)
```

### Deliverables

- ? Endpoint: `POST /api/v1/planta/identificar`
- ? Endpoint: `POST /api/v1/planta/buscar`
- ? Endpoint: `GET /api/v1/planta/minhas-plantas`
- ? Frontend: Página de identificação
- ? Integração com PlantNet + Trefle + Gemini
- ? Upload para Supabase Storage

---

## ?? SEMANA 5-6: Rede Social Básica

**Status:** ? COMPLETO

### Backend — Arquivos para Adicionar

```
PlantaCoreAPI.Domain/
??? Entities/
?   ??? Post.cs               (nova)
?   ??? Seguidor.cs           (novo - N:N)
??? Interfaces/
    ??? IRepositorioPost.cs   (nova)
    ??? IRepositorioSeguidor.cs (nova)

PlantaCoreAPI.Application/
??? DTOs/
?   ??? Post/
?   ?   ??? PostDTOSaida.cs
?   ?   ??? CriarPostDTOEntrada.cs
?   ??? Usuario/
?       ??? UsuarioDTOSaida.cs
??? Interfaces/
    ??? IPostService.cs       (novo)
    ??? IUserService.cs       (novo)
    ??? IPlantCareReminderService.cs (novo)

PlantaCoreAPI.Infrastructure/
??? Repositorios/
?   ??? RepositorioPost.cs    (novo)
?   ??? RepositorioSeguidor.cs (novo)
??? Services/
?   ??? PostService.cs        (novo)
?   ??? UserService.cs        (novo)
?   ??? PlantCareReminderService.cs (novo)
??? Migrations/
    ??? 20250305000000_AddPostsAndFollowers.cs

PlantaCoreAPI.API/
??? Controllers/
?   ??? PostController.cs     (novo)
?   ??? UsuarioController.cs  (novo)
??? (atualizar Program.cs)
```

### Frontend — Arquivos para Adicionar

```
src/
??? api/
?   ??? post.ts               (funções de posts)
?   ??? usuario.ts            (funções de usuário)
??? pages/
?   ??? Feed.tsx              (feed social)
?   ??? Perfil.tsx            (perfil do usuário)
?   ??? CriarPost.tsx         (criar post)
?   ??? Explorar.tsx          (explorar posts)
??? components/
?   ??? PostCard.tsx
?   ??? UsuarioCard.tsx
??? types/
    ??? post.ts
    ??? usuario.ts
```

### Deliverables

- ? Endpoint: `POST /api/v1/post`
- ? Endpoint: `GET /api/v1/post/feed`
- ? Endpoint: `GET /api/v1/post/explorar`
- ? Endpoint: `POST /api/v1/usuario/seguir/{usuarioId}`
- ? Frontend: Feed social com posts

---

## ?? SEMANA 7-8: Curtidas & Comentários

**Status:** ? COMPLETO

### Backend — Arquivos para Adicionar

```
PlantaCoreAPI.Domain/
??? Entities/
?   ??? Curtida.cs            (nova)
?   ??? Comentario.cs         (nova)
??? Interfaces/
    ??? IRepositorioCurtida.cs (nova)
    ??? IRepositorioComentario.cs (nova)

PlantaCoreAPI.Application/
??? DTOs/
?   ??? Comentario/
?       ??? ComentarioDTOSaida.cs
?       ??? CriarComentarioDTOEntrada.cs
??? Interfaces/
    ??? (atualizações em IPostService.cs)

PlantaCoreAPI.Infrastructure/
??? Repositorios/
?   ??? RepositorioCurtida.cs (novo)
?   ??? RepositorioComentario.cs (novo)
??? Migrations/
    ??? 20250310000000_AddCurtidasAndComentarios.cs
```

### Frontend — Arquivos para Adicionar

```
src/
??? api/
?   ??? curtida.ts            (funções de curtidas)
?   ??? comentario.ts         (funções de comentários)
??? components/
?   ??? ComentarioLista.tsx
?   ??? ComentarioForm.tsx
?   ??? BotaoCurtida.tsx
??? types/
    ??? curtida.ts
    ??? comentario.ts
```

### Deliverables

- ? Endpoint: `POST /api/v1/post/{id}/curtir`
- ? Endpoint: `DELETE /api/v1/post/{id}/curtida`
- ? Endpoint: `POST /api/v1/post/comentario`
- ? Endpoint: `GET /api/v1/post/{postId}/comentarios`
- ? Frontend: Curtir posts e comentar

---

## ?? SEMANA 9-10: Notificações + Seguir

**Status:** ? COMPLETO

### Backend — Arquivos para Adicionar

```
PlantaCoreAPI.Domain/
??? Entities/
?   ??? Notificacao.cs        (nova)
??? Enums/
?   ??? TipoNotificacao.cs    (nova)
??? Interfaces/
    ??? IRepositorioNotificacao.cs (nova)

PlantaCoreAPI.Application/
??? DTOs/
?   ??? Notificacao/
?       ??? NotificacaoDTOSaida.cs
?       ??? ListarNotificacoesDTOSaida.cs
?       ??? LembreteCuidadoDTO.cs
??? Interfaces/
    ??? INotificationService.cs (novo)

PlantaCoreAPI.Infrastructure/
??? Repositorios/
?   ??? RepositorioNotificacao.cs (novo)
??? Services/
?   ??? NotificationService.cs (novo)
??? Migrations/
    ??? 20250312000000_AddNotificacoes.cs
```

### Frontend — Arquivos para Adicionar

```
src/
??? api/
?   ??? notificacao.ts        (funções de notificações)
??? components/
?   ??? NotificacaoBadge.tsx
?   ??? NotificacaoLista.tsx
?   ??? LembreteCard.tsx
??? hooks/
?   ??? useNotificacoes.ts
??? types/
    ??? notificacao.ts
```

### Deliverables

- ? Endpoint: `GET /api/v1/notificacao`
- ? Endpoint: `GET /api/v1/notificacao/nao-lidas`
- ? Endpoint: `PUT /api/v1/notificacao/{id}/marcar-como-lida`
- ? Endpoint: `DELETE /api/v1/notificacao/{id}` (deletar uma)
- ? Endpoint: `DELETE /api/v1/notificacao` (deletar todas)
- ? Frontend: Bell icon com notificações
- ? Polling a cada 30 segundos

---

## ?? SEMANA 11-12: Lembretes + Upload

**Status:** ? COMPLETO

### Backend — Arquivos para Adicionar

```
PlantaCoreAPI.Application/
??? Interfaces/
    ??? IPlantCareReminderService.cs

PlantaCoreAPI.Infrastructure/
??? Services/
?   ??? PlantCareReminderService.cs
?   ??? PlantCareReminderBackgroundService.cs (novo)
??? Migrations/
    ??? (nenhuma — usa tabela de notificações)

PlantaCoreAPI.API/
??? Controllers/
?   ??? StorageController.cs  (novo)
?   ??? LembreteCuidadoController.cs (novo - opcional)
??? Program.cs                (registrar BackgroundService)
??? appsettings.json          (Supabase config)
```

### Frontend — Arquivos para Adicionar

```
src/
??? api/
?   ??? upload.ts             (funções de upload)
?   ??? lembretes.ts          (funções de lembretes)
??? components/
?   ??? UploadFotoPerfil.tsx
?   ??? UploadFotoPlanta.tsx
?   ??? LembretesList.tsx
??? types/
    ??? upload.ts
```

### Deliverables

- ? Endpoint: `POST /api/v1/usuario/foto-perfil`
- ? Endpoint: `POST /api/v1/planta/identificar` (com upload)
- ? Endpoint: `GET /api/v1/notificacao` (com lembretes estruturados)
- ? BackgroundService: Gera lembretes 1x/dia
- ? Frontend: Upload de fotos

---

## ?? SEMANA 12 (ADICIONADO): Reativação de Conta

**Status:** ? COMPLETO

### Backend — Arquivos Adicionados

```
PlantaCoreAPI.Application/
??? Interfaces/
?   ??? IAccountReactivationService.cs (novo)
??? DTOs/
?   ??? Usuario/
?       ??? ReativacaoDTOs.cs (novo)
?           ??? SolicitarReativacaoDTOEntrada
?           ??? ReativarComTokenDTOEntrada
?           ??? VerificarTokenReativacaoDTOEntrada
?           ??? ResetarSenhaSemTokenDTOEntrada

PlantaCoreAPI.Infrastructure/
??? Services/
    ??? AccountReactivationService.cs (novo)
        ??? SolicitarReativacaoAsync()
        ??? ReativarComTokenAsync()
        ??? VerificarTokenReativacaoAsync()
        ??? ResetarSenhaSemTokenAsync()

PlantaCoreAPI.Domain/
??? Entities/
    ??? Usuario.cs (ATUALIZADO)
        ??? + método Reativar()

PlantaCoreAPI.API/
??? Controllers/
?   ??? UsuarioController.cs (ATUALIZADO)
?       ??? + POST /reativar/solicitar
?       ??? + POST /reativar/verificar-token
?       ??? + POST /reativar/confirmar
?       ??? + POST /reativar/resetar-senha
??? Extensions/
    ??? ServicesExtensions.cs (ATUALIZADO)
        ??? + DI para IAccountReactivationService
```

### Frontend — Páginas para Criar

```
src/
??? pages/
?   ??? ReativarConta/
?       ??? SolicitarReativacao.tsx (solicitação)
?       ??? VerificaToken.tsx (verificação)
?       ??? ReativarComNovaSenha.tsx (confirmação)
??? api/
?   ??? reativacao.ts (funções)
??? types/
    ??? reativacao.ts (tipos)
```

### Deliverables

- ? Endpoint: `POST /api/v1/usuario/reativar/solicitar`
- ? Endpoint: `POST /api/v1/usuario/reativar/verificar-token`
- ? Endpoint: `POST /api/v1/usuario/reativar/confirmar`
- ? Endpoint: `POST /api/v1/usuario/reativar/resetar-senha`
- ? Email HTML com token único (válido 1 hora)
- ? Validação de senha forte obrigatória
- ? Dados restaurados ao reativar
- ? Documentação completa (10 arquivos)

### Endpoints Documentados

```
POST /api/v1/usuario/reativar/solicitar
  ? Email com token (1h)

POST /api/v1/usuario/reativar/verificar-token
  ? Valida token antes de usar

POST /api/v1/usuario/reativar/confirmar
  ? Reativa + nova senha + confirmação

POST /api/v1/usuario/reativar/resetar-senha
  ? Reset sem token (emergência)
```

---

## ? SEMANA 13-14: Testes + Polimento

**Status:** ?? EM PROGRESSO

### Backend — Arquivos para Adicionar

```
PlantaCoreAPI.Domain.Tests/
??? Entities/
?   ??? UsuarioTests.cs       (já existe)
?   ??? PlantaTests.cs
?   ??? PostTests.cs
?   ??? NotificacaoTests.cs
??? PlantaCoreAPI.Domain.Tests.csproj

PlantaCoreAPI.Application.Tests/
??? Servicos/
?   ??? ServicoAutenticacaoTests.cs (já existe)
?   ??? ServicoPlantTests.cs
?   ??? ServicoPostTests.cs
?   ??? ServicoNotificacaoTests.cs
??? PlantaCoreAPI.Application.Tests.csproj
```

### Frontend — Arquivos para Adicionar

```
src/
??? __tests__/
?   ??? pages/
?   ?   ??? Login.test.tsx
?   ?   ??? Registro.test.tsx
?   ?   ??? Feed.test.tsx
?   ?   ??? Perfil.test.tsx
?   ??? components/
?   ?   ??? PostCard.test.tsx
?   ?   ??? ComentarioForm.test.tsx
?   ?   ??? NotificacaoBadge.test.tsx
?   ??? api/
?       ??? auth.test.ts
?       ??? post.test.ts
?       ??? notificacao.test.ts
??? setupTests.ts
??? vitest.config.ts
```

### Deliverables

- ? Tests: 70%+ cobertura
- ? Documentação: Swagger atualizado
- ? Logging: Serilog estruturado
- ? Performance: N+1 queries otimizadas
- ? Security: Rate limiting

---

## ?? SEMANA 15: WebSocket + Deploy

**Status:** ?? PLANEJADO

### Backend — Arquivos para Adicionar

```
PlantaCoreAPI.API/
??? Hubs/
?   ??? NotificacaoHub.cs     (novo)
??? Extensions/
?   ??? SignalRExtensions.cs  (novo)
??? Program.cs                (registrar SignalR)
```

### Frontend — Arquivos para Adicionar

```
src/
??? hooks/
?   ??? useWebSocket.ts       (novo - SignalR)
??? services/
?   ??? signalR.ts            (novo - conexão)
??? (atualizar useNotificacoes.ts para usar WebSocket)
```

### Deliverables

- ? Hub: Notificações em tempo real
- ? Frontend: Cliente SignalR
- ? Deploy: Produção (Render/Railway)
- ? Health checks: Status da API
- ? Monitoramento: Logs em produção

---

## ?? Resumo por Semana

| Semana | Feature | Backend | Frontend | Status |
|--------|---------|---------|----------|--------|
| 1-2 | Autenticação | ? 8 arquivos | ?? 4 arquivos | ? |
| 3-4 | Plantas + IA | ? 15 arquivos | ?? 5 arquivos | ? |
| 5-6 | Rede Social | ? 10 arquivos | ?? 5 arquivos | ? |
| 7-8 | Curtidas | ? 6 arquivos | ?? 4 arquivos | ? |
| 9-10 | Notificações | ? 9 arquivos | ?? 5 arquivos | ? |
| 11-12 | Lembretes | ? 5 arquivos | ?? 4 arquivos | ? |
| **12** | **Reativação** | **? 5 arquivos** | **?? 3 páginas** | **?** |
| 13-14 | Testes | ?? 8 arquivos | ?? 8 arquivos | ?? |
| 15 | WebSocket | ?? 2 arquivos | ?? 3 arquivos | ?? |

---

## ?? Próximos Passos

**Agora (Semana 12):**
- ? Backend: 46 arquivos completos
- ?? Frontend: Comece pela autenticação

**Semana 13:**
- [ ] Adicionar testes backend
- [ ] Testes frontend

**Semana 14:**
- [ ] Performance tuning
- [ ] Documentação final

**Semana 15:**
- [ ] WebSocket
- [ ] Deploy produção

---

**Status:** ? Roadmap Completo  
**Última atualização:** 03/03/2025
