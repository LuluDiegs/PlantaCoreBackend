# ?? Documentação Técnica Backend — PlantaCoreAPI

## ?? Índice Completo

1. [Arquitetura](#arquitetura)
2. [Estrutura Detalhada](#estrutura-detalhada)
3. [Setup Completo](#setup-completo)
4. [Autenticação](#autenticação-jwt)
5. [Endpoints Detalhados](#endpoints-detalhados)
6. [Banco de Dados](#banco-de-dados)
7. [Serviços Externos](#serviços-externos)
8. [Lembretes Automáticos](#lembretes-automáticos)
9. [Exclusão & Reativação de Conta](#exclusão--reativação-de-conta)
10. [Padrões de Código](#padrões-de-código)
11. [Troubleshooting](#troubleshooting)

---

## Arquitetura

### Clean Architecture

```
???????????????????????????????????????
?     PlantaCoreAPI.API               ?
?  Controllers, Program.cs, Extensions?
???????????????????????????????????????
              ?
???????????????????????????????????????
?  PlantaCoreAPI.Application          ?
?  DTOs, Interfaces, Utilidades       ?
???????????????????????????????????????
              ?
???????????????????????????????????????
?  PlantaCoreAPI.Infrastructure       ?
?  EF Core, Repos, Serviços Externos  ?
???????????????????????????????????????
              ?
???????????????????????????????????????
?  PlantaCoreAPI.Domain               ?
?  Entidades, Enums, Interfaces       ?
???????????????????????????????????????
```

### Princípios

- **Separação de Responsabilidades:** Cada layer tem um propósito
- **Inversão de Controle:** DI em todos os serviços
- **Repository Pattern:** Abstração de acesso a dados
- **DTO Pattern:** Input/Output separados
- **Soft Delete:** Nunca deletar, apenas marcar como inativo
- **Auditory:** Rastrear `DataCriacao` e `DataAtualizacao`

---

## Estrutura Detalhada

### Domain (Núcleo de Negócio)

```
PlantaCoreAPI.Domain/
??? Entities/                          # Modelos de domínio
?   ??? Usuario.cs                    # Usuário (email, senha, perfil)
?   ??? Planta.cs                     # Planta identificada
?   ??? Post.cs                       # Post sobre planta
?   ??? Comentario.cs                 # Comentário em post
?   ??? Curtida.cs                    # Curtida (Post/Comentário)
?   ??? Notificacao.cs                # Notificação
?   ??? TokenRefresh.cs               # Token de refresh
?
??? Enums/
?   ??? TipoNotificacao.cs            # Curtida, Comentario, NovoSeguidor, LembreteCuidado
?
??? Interfaces/                        # Contratos de repositórios
?   ??? IRepositorio.cs               # Interface genérica
?   ??? IRepositorioUsuario.cs
?   ??? IRepositorioPlanta.cs
?   ??? IRepositorioPost.cs
?   ??? IRepositorioCurtida.cs
?   ??? IRepositorioComentario.cs
?   ??? IRepositorioNotificacao.cs
?   ??? IRepositorioTokenRefresh.cs
?
??? Comuns/                            # Domínio comum
    ??? PaginaResultado.cs            # Paginação (itens, total, páginas, etc)
```

### Application (Camada de Negócio)

```
PlantaCoreAPI.Application/
??? DTOs/                              # Data Transfer Objects
?   ??? Auth/
?   ?   ??? RegistroDTOEntrada.cs     # { nome, email, senha }
?   ?   ??? LoginDTOEntrada.cs        # { email, senha }
?   ?   ??? LoginDTOSaida.cs          # { usuarioId, token, tokenRefresh }
?   ?
?   ??? Planta/
?   ?   ??? PlantaDTOSaida.cs         # { id, nomeCientifico, cuidados, ... }
?   ?   ??? AdicionarPlantaTrefleDTO.cs
?   ?
?   ??? Post/
?   ?   ??? PostDTOSaida.cs           # { id, conteudo, usuarioId, ... }
?   ?   ??? CriarPostDTOEntrada.cs    # { plantaId, conteudo }
?   ?
?   ??? Comentario/
?   ?   ??? ComentarioDTOSaida.cs
?   ?   ??? CriarComentarioDTOEntrada.cs
?   ?
?   ??? Usuario/
?   ?   ??? UsuarioDTOSaida.cs        # { id, nome, biografia, ... }
?   ?
?   ??? Identificacao/
?   ?   ??? IdentificacaoDTOs.cs      # DTOs internos para IA
?   ?
?   ??? Notificacao/
?       ??? NotificacaoDTOSaida.cs
?       ??? ListarNotificacoesDTOSaida.cs
?       ??? LembreteCuidadoDTO.cs
?
??? Interfaces/                        # Contratos de serviços
?   ??? IAuthenticationService.cs     # Registrar, login, logout
?   ??? IUserService.cs               # Perfil, follow, unfollow
?   ??? IPlantService.cs              # Identificar, buscar, CRUD
?   ??? IPostService.cs               # CRUD posts, feed
?   ??? INotificationService.cs       # Obter, marcar como lida
?   ??? IPlantCareReminderService.cs  # Gerar lembretes
?   ??? IJwtService.cs                # Gerar/validar JWT
?   ??? IPasswordHashService.cs       # Hash de senha
?   ??? IEmailService.cs              # Enviar emails
?   ??? IFileStorageService.cs        # Upload/download arquivos
?   ??? IGeminiService.cs             # Chamadas API Gemini
?   ??? IPlantNetService.cs           # Chamadas API PlantNet
?   ??? ITrefleService.cs             # Chamadas API Trefle
?
??? Comuns/
?   ??? Resultado.cs                  # { sucesso, dados, mensagem, erros }
?   ??? PaginaResultado.cs            # Paginação
?
??? Utils/
    ??? EmailTemplateGenerator.cs     # Templates HTML para email
    ??? PasswordValidator.cs          # Validações de senha
```

### Infrastructure (Implementações)

```
PlantaCoreAPI.Infrastructure/
??? Dados/
?   ??? PlantaCoreDbContext.cs        # DbContext EF Core
?       ??? DbSet<Usuario>
?       ??? DbSet<Planta>
?       ??? DbSet<Post>
?       ??? DbSet<Comentario>
?       ??? DbSet<Curtida>
?       ??? DbSet<Notificacao>
?       ??? DbSet<TokenRefresh>
?       ??? (configurações de modelo)
?
??? Repositorios/                      # Implementações de IRepositorio
?   ??? RepositorioUsuario.cs
?   ??? RepositorioPlanta.cs
?   ??? RepositorioPost.cs
?   ??? RepositorioCurtida.cs
?   ??? RepositorioComentario.cs
?   ??? RepositorioNotificacao.cs
?   ??? RepositorioTokenRefresh.cs
?
??? Services/
?   ??? AuthenticationService.cs      # Registrar, login, logout
?   ??? UserService.cs                # Serviços de usuário
?   ??? NotificationService.cs        # Gerar notificações
?   ??? PlantCareReminderService.cs   # Gerar lembretes
?   ??? PlantCareReminderBackgroundService.cs  # Job de lembretes
?   ??? PasswordHashService.cs        # Hashing com bcrypt
?   ?
?   ??? Plant/                         # Serviço de plantas (parcial)
?   ?   ??? PlantService.cs           # Orquestrador
?   ?   ??? PlantService.Identificacao.cs
?   ?   ??? PlantService.Busca.cs
?   ?   ??? PlantService.Crud.cs
?   ?   ??? PlantService.GeminiParser.cs
?   ?   ??? PlantService.Mapper.cs
?   ?   ??? DadosPlantaEnriquecidos.cs
?   ?
?   ??? External/                     # Integrações com APIs
?       ??? JwtService.cs             # Gerar/validar JWT
?       ??? EmailService.cs           # Enviar via Gmail SMTP
?       ??? PlantNetService.cs        # API PlantNet
?       ??? TrefleService.cs          # API Trefle
?       ??? GeminiService.cs          # API Gemini IA
?
??? Storage/
?   ??? SupabaseFileStorageService.cs # Upload para Supabase
?
??? Migrations/                        # EF Core migrations
    ??? 20250303000000_InitialCreate.cs
    ??? 20250305000000_AddPostsAndFollowers.cs
    ??? 20250310000000_AddCurtidasAndComentarios.cs
    ??? 20250312000000_AddNotificacoes.cs
```

### API (Camada de Apresentação)

```
PlantaCoreAPI.API/
??? Controllers/                       # Endpoints REST
?   ??? AutenticacaoController.cs     # /api/v1/autenticacao
?   ??? UsuarioController.cs          # /api/v1/usuario
?   ??? PlantaController.cs           # /api/v1/planta
?   ??? PostController.cs             # /api/v1/post
?   ??? NotificacaoController.cs      # /api/v1/notificacao
?   ??? StorageController.cs          # /api/v1/armazenamento
?   ??? LembreteCuidadoController.cs  # /api/v1/lembretes (opcional)
?
??? Extensions/                        # Extensões de configuração
?   ??? AuthExtensions.cs             # JWT, autenticação
?   ??? DatabaseExtensions.cs         # EF Core, migrations
?   ??? ServicesExtensions.cs         # DI de serviços
?   ??? SwaggerExtensions.cs          # OpenAPI/Swagger
?   ??? LoggingExtensions.cs          # Serilog
?
??? Hubs/                              # SignalR (Semana 15)
?   ??? NotificacaoHub.cs             # Notificações em tempo real
?
??? Program.cs                         # Configuração principal
??? appsettings.json                  # Configurações padrão
??? appsettings.Development.json      # Dev-specific
??? Properties/
    ??? launchSettings.json           # URLs de execução
```

---

## Setup Completo

### Pré-requisitos

```
- .NET 8 SDK
- PostgreSQL 15+
- Supabase (ou PostgreSQL local)
- Git
```

### 1. Clonar Repositório

```bash
git clone seu-repo
cd PlantaCoreAPI
```

### 2. Inicializar User Secrets

```bash
dotnet user-secrets init --project PlantaCoreAPI.API
```

### 3. Configurar Conexão com Banco

```bash
# Supabase (recomendado)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=db.xxx.supabase.co;Username=postgres;Password=xxx;Database=postgres;Port=5432;SSL Mode=Require" \
  --project PlantaCoreAPI.API

# Ou PostgreSQL local
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Username=postgres;Password=sua-senha;Database=plantacore;Port=5432" \
  --project PlantaCoreAPI.API
```

### 4. Configurar JWT

```bash
# Gerar chave segura (32+ caracteres)
dotnet user-secrets set "Jwt:ChaveSecreta" "sua-chave-super-segura-com-32-caracteres-ou-mais" --project PlantaCoreAPI.API
dotnet user-secrets set "Jwt:Emissor" "PlantaCoreAPI" --project PlantaCoreAPI.API
dotnet user-secrets set "Jwt:Publico" "PlantaCoreAPI" --project PlantaCoreAPI.API
```

### 5. Configurar Email (Gmail)

```bash
# Criar senha de app em: https://myaccount.google.com/apppasswords
dotnet user-secrets set "Email:Email" "seu-email@gmail.com" --project PlantaCoreAPI.API
dotnet user-secrets set "Email:Senha" "sua-senha-app-gmail" --project PlantaCoreAPI.API
```

### 6. Configurar APIs Externas

```bash
# PlantNet: https://my-api.plantnet.org/
dotnessubuser-secrets set "PlantNet:ChaveApi" "sua-chave-plantnet" --project PlantaCoreAPI.API

# Trefle: https://trefle.io/
dotnet user-secrets set "Trefle:ChaveApi" "sua-chave-trefle" --project PlantaCoreAPI.API

# Gemini: https://ai.google.dev/
dotnet user-secrets set "Gemini:ChaveApi" "sua-chave-gemini" --project PlantaCoreAPI.API
```

### 7. Configurar Supabase

```bash
# Obter em: https://app.supabase.com/
dotnet user-secrets set "Supabase:Url" "https://seu-projeto.supabase.co" --project PlantaCoreAPI.API
dotnet user-secrets set "Supabase:ChavePublica" "sua-anon-key" --project PlantaCoreAPI.API
```

### 8. Restaurar Dependências

```bash
dotnet restore
```

### 9. Aplicar Migrations

```bash
dotnet ef database update \
  --project PlantaCoreAPI.Infrastructure \
  --startup-project PlantaCoreAPI.API
```

### 10. Executar

```bash
cd PlantaCoreAPI.API
dotnet run --launch-profile https
```

**URLs:**
- HTTPS: `https://localhost:7123`
- HTTP: `http://localhost:5123`
- Swagger: `http://localhost:5123/swagger`

---

## Autenticação JWT

### Fluxo

```
1. Usuario faz POST /autenticacao/login
2. Backend valida email/senha
3. Backend gera 2 tokens:
   - Access Token (15 min)
   - Refresh Token (7 dias)
4. Cliente armazena em localStorage
5. Cada request inclui: Authorization: Bearer {token}
6. Se expirar ? cliente usa refresh para novo access token
```

### Implementação

**AuthenticationService.cs:**

```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly IRepositorioUsuario _repo;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IEmailService _emailService;

    public async Task<LoginDTOSaida> LoginAsync(LoginDTOEntrada dados)
    {
        // 1. Validar email/senha
        var usuario = await _repo.ObterPorEmailAsync(dados.Email);
        if (usuario == null || !_passwordHashService.Verificar(dados.Senha, usuario.SenhaHash))
            throw new DomainException("Email ou senha inválidos");

        // 2. Gerar tokens
        var accessToken = _jwtService.GerarAccessToken(usuario.Id);
        var refreshToken = _jwtService.GerarRefreshToken(usuario.Id);

        // 3. Salvar refresh token
        var tokenRefresh = new TokenRefresh
        {
            UsuarioId = usuario.Id,
            Token = refreshToken,
            DataExpiracao = DateTime.UtcNow.AddDays(7),
        };
        await _repo.AdicionarTokenRefreshAsync(tokenRefresh);

        return new LoginDTOSaida
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            TokenAcesso = accessToken,
            TokenRefresh = refreshToken,
        };
    }
}
```

**JwtService.cs:**

```csharp
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public string GerarAccessToken(Guid usuarioId)
    {
        var chaveSecreta = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:ChaveSecreta"])
        );

        var claims = new []
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
        };

        var credenciais = new SigningCredentials(chaveSecreta, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Emissor"],
            audience: _config["Jwt:Publico"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credenciais
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## Endpoints Detalhados

### Autenticação

#### Registrar

```
POST /api/v1/autenticacao/registrar

Request:
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "senha": "SenhaForte123!@",
  "confirmacaoSenha": "SenhaForte123!@"
}

Response (201):
{
  "sucesso": true,
  "dados": {
    "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
    "nome": "João Silva",
    "email": "joao@email.com",
    "tokenAcesso": "eyJhbGciOiJIUzI1NiIsInR5...",
    "tokenRefresh": "eyJhbGciOiJIUzI1NiIsInR5..."
  }
}

Validações:
- Email: válido, único
- Senha: min 8 chars, 1 maiúscula, 1 número, 1 especial
- Nome: min 3 chars
```

#### Login

```
POST /api/v1/autenticacao/login

Request:
{
  "email": "joao@email.com",
  "senha": "SenhaForte123!@"
}

Response (200):
{
  "sucesso": true,
  "dados": {
    "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
    "nome": "João Silva",
    "email": "joao@email.com",
    "tokenAcesso": "eyJhbGciOiJIUzI1NiIsInR5...",
    "tokenRefresh": "eyJhbGciOiJIUzI1NiIsInR5..."
  }
}

Erros:
- 400: Email ou senha inválidos
```

### Plantas

#### Identificar Planta

```
POST /api/v1/planta/identificar
Content-Type: multipart/form-data
?? Requer autenticação

Request:
- foto: [arquivo JPEG/PNG/GIF]

Response (201):
{
  "sucesso": true,
  "dados": {
    "id": "uuid",
    "nomeCientifico": "Monstera deliciosa",
    "nomeComum": "Costela-de-adão",
    "familia": "Araceae",
    "toxica": "Sim",
    "descricaoToxicidade": "Moderadamente tóxica para cães e gatos",
    "requisitosLuz": "Luz indireta brilhante",
    "requisitosAgua": "Solo úmido mas bem drenado",
    "requisitosTemperatura": "18-27°C",
    "cuidados": "Regar 1-2x por semana",
    "fotoPlanta": "https://url-da-foto.jpg"
  }
}

Processo:
1. Frontend envia foto
2. Backend tenta PlantNet API
3. Se PlantNet não conseguir:
   - Tenta Trefle API
   - Se Trefle também falhar, usa Gemini
4. Dados são persistidos
```

### Posts

#### Criar Post

```
POST /api/v1/post
?? Requer autenticação

Request:
{
  "plantaId": "uuid-da-planta",
  "conteudo": "Minha planta está crescendo muito bem!"
}

Response (201):
{
  "sucesso": true,
  "dados": {
    "id": "uuid",
    "plantaId": "uuid-da-planta",
    "usuarioId": "uuid-do-usuario",
    "nomeUsuario": "João Silva",
    "conteudo": "Minha planta está crescendo muito bem!",
    "totalCurtidas": 0,
    "totalComentarios": 0,
    "curtiuUsuario": false,
    "dataCriacao": "2025-03-03T10:30:00Z"
  }
}
```

#### Obter Feed

```
GET /api/v1/post/feed?pagina=1&tamanho=10
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "dados": {
    "itens": [
      {
        "id": "uuid",
        "plantaId": "uuid",
        "usuarioId": "uuid",
        "nomeUsuario": "João",
        "conteudo": "Minha planta está linda!",
        "totalCurtidas": 5,
        "totalComentarios": 2,
        "curtiuUsuario": false,
        "dataCriacao": "2025-03-03T10:30:00Z"
      }
    ],
    "pagina": 1,
    "tamanhoPagina": 10,
    "total": 42,
    "totalPaginas": 5,
    "temProxima": true,
    "temAnterior": false
  }
}

Lógica:
- Retorna posts de usuários que você segue
- Ordenado por data descrescente
- Paginação: página começa em 1
```

### Notificações

#### Obter Notificações

```
GET /api/v1/notificacao
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "dados": {
    "notificacoesSociais": [
      {
        "id": "uuid",
        "tipo": "Curtida",
        "mensagem": "Maria curtiu seu post",
        "lida": false,
        "dataCriacao": "2025-03-03T10:30:00Z",
        "usuarioOrigemNome": "Maria",
        "postId": "uuid-do-post"
      }
    ],
    "lembretes": [
      {
        "id": "uuid",
        "plantaId": "uuid-da-planta",
        "nomePlanta": "Rosa",
        "lida": false,
        "dataCriacao": "2025-03-03T08:00:00Z",
        "detalhes": {
          "rega": "Manter solo úmido",
          "luz": "Sol pleno",
          "temperatura": "15-25°C",
          "cuidados": "Podar regularmente"
        }
      }
    ],
    "totalNaoLidas": 3
  }
}

#### Marcar Notificação como Lida

```
PUT /api/v1/notificacao/{notificacaoId}/marcar-como-lida
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "mensagem": "Notificação marcada como lida"
}
```

#### Marcar Todas como Lidas

```
PUT /api/v1/notificacao/marcar-todas-como-lidas
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "mensagem": "Todas as notificações marcadas como lidas"
}
```

#### Deletar Notificação

```
DELETE /api/v1/notificacao/{notificacaoId}
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "mensagem": "Notificação deletada com sucesso"
}

Erros:
- 400: Notificação não encontrada
- 400: Você não tem permissão para deletar esta notificação
```

#### Deletar Todas as Notificações

```
DELETE /api/v1/notificacao
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "mensagem": "Todas as notificações foram deletadas"
}
```

### Usuário

#### Obter Perfil

```
GET /api/v1/usuario/perfil
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "dados": {
    "id": "uuid",
    "nome": "João Silva",
    "email": "joao@email.com",
    "biografia": "Amante de plantas",
    "fotoPerfil": "https://url-da-foto.jpg",
    "totalSeguidores": 42,
    "totalSeguindo": 15,
    "totalPlantas": 8,
    "totalPosts": 23,
    "totalCurtidasRecebidas": 156,
    "dataCriacao": "2025-03-03T10:30:00Z"
  }
}
```

#### Deletar Conta (Exclusão Completa)

```
DELETE /api/v1/usuario/conta
?? Requer autenticação

Response (200):
{
  "sucesso": true,
  "mensagem": "Conta e todos os dados associados foram deletados com sucesso"
}

?? AVISO: Esta ação é IRREVERSÍVEL e deleta:
- Foto de perfil
- Todas as plantas
- Todos os posts
- Todos os comentários
- Todas as curtidas
- Todas as notificações
- Relacionamentos de seguidores
- Tokens de sessão
- Fotos no bucket Supabase
```

### Reativação de Conta

#### Solicitar Reativação (Email + Token)

```
POST /api/v1/usuario/reativar/solicitar
?? Sem autenticação obrigatória

Request:
{
  "email": "usuario@email.com"
}

Response (200):
{
  "sucesso": true,
  "mensagem": "Email de reativação enviado com sucesso. Verifique sua caixa de entrada."
}

Processo:
1. Sistema gera token único (UUID)
2. Token válido por 1 hora
3. Email HTML enviado com link seguro
4. Link contém: email + token
```

#### Verificar Token

```
POST /api/v1/usuario/reativar/verificar-token
?? Sem autenticação

Request:
{
  "email": "usuario@email.com",
  "token": "token-do-email"
}

Response (200):
{
  "sucesso": true,
  "mensagem": "Token válido"
}

Erros:
- 400: "Token inválido"
- 400: "Token expirou"
- 400: "A conta já está ativa"
```

#### Reativar com Nova Senha

```
POST /api/v1/usuario/reativar/confirmar
?? Sem autenticação

Request:
{
  "email": "usuario@email.com",
  "token": "token-do-email",
  "novaSenha": "NovaSenha123!@"
}

Response (200):
{
  "sucesso": true,
  "mensagem": "Conta reativada com sucesso! Sua senha foi atualizada. Você pode fazer login agora."
}

Validações:
- Senha: mín 8 chars, maiúscula, minúscula, número, especial
- Token: válido e não expirado
- Email: conta deve estar inativa

Efeitos:
- ? Conta reativada (Ativo = true)
- ? Senha atualizada
- ? DataExclusao limpa
- ? Dados restaurados (plantas, posts, etc)
- ? Email de confirmação enviado
```

#### Reset de Senha (Sem Token)

```
POST /api/v1/usuario/reativar/resetar-senha
?? Sem autenticação

Request:
{
  "email": "usuario@email.com",
  "novaSenha": "NovaSenha123!@"
}

Response (200):
{
  "sucesso": true,
  "mensagem": "Senha resetada com sucesso!"
}

Validações:
- Email: deve existir
- Senha: mín 8 chars, complexidade obrigatória

Efeitos:
- ? Senha atualizada
- ? Se inativa, reativa automaticamente
```

---

## Exclusão & Reativação de Conta

### Fluxo de Exclusão

```
1. Usuário faz DELETE /usuario/conta
   ?
2. Sistema marca como inativo (Ativo = false)
   ?
3. Sistema registra DataExclusao = NOW
   ?
4. Cascata de deleção:
   - Deletar fotos (Supabase)
   - Deletar plantas
   - Deletar posts
   - Deletar comentários
   - Deletar curtidas
   - Limpar seguidores
   ?
5. ? Conta removida (mas dados persistem 30 dias)
```

### Fluxo de Reativação

```
1. Usuário clica em "Reativar Conta"
   ?
2. Entra email em: /usuario/reativar/solicitar
   ?
3. Sistema gera token único (1h validade)
   ?
4. Email enviado com link:
   https://frontend.com/reativar?email=xxx&token=yyy
   ?
5. Usuário clica link e insere nova senha
   ?
6. Frontend envia: POST /usuario/reativar/confirmar
   ?
7. Sistema valida token + email + senha
   ?
8. ? Conta reativada:
   - Ativo = true
   - DataExclusao = null
   - Senha atualizada
   - Dados restaurados
   ?
9. Email de confirmação enviado
   ?
10. Usuário faz login com nova senha
```

### Segurança de Reativação

| Aspecto | Implementação |
|---------|---|
| **Token** | UUID único por tentativa, válido 1 hora |
| **Email** | Verificação de existência obrigatória |
| **Senha** | Validação força: 8+ chars, complexidade |
| **Expiração** | Validação automática de data/hora |
| **Estado** | Verifica se conta está realmente inativa |
| **Hash** | bcrypt em todas as senhas |
| **Email HTML** | Templates profissionais, sem scripts |

### Arquivos de Código

**Interface:**
```
PlantaCoreAPI.Application/Interfaces/IAccountReactivationService.cs
```

**Implementação:**
```
PlantaCoreAPI.Infrastructure/Services/AccountReactivationService.cs
```

**DTOs:**
```
PlantaCoreAPI.Application/DTOs/Usuario/ReativacaoDTOs.cs
- SolicitarReativacaoDTOEntrada
- ReativarComTokenDTOEntrada
- VerificarTokenReativacaoDTOEntrada
- ResetarSenhaSemTokenDTOEntrada
```

**Endpoints:**
```
PlantaCoreAPI.API/Controllers/UsuarioController.cs
- POST /reativar/solicitar
- POST /reativar/verificar-token
- POST /reativar/confirmar
- POST /reativar/resetar-senha
```

### Documentação Completa

Para documentação completa sobre reativação:
?? [ACCOUNT_REACTIVATION_INDEX.md](ACCOUNT_REACTIVATION_INDEX.md)

Documentos inclusos:
- [ACCOUNT_REACTIVATION_QUICK_REFERENCE.md](ACCOUNT_REACTIVATION_QUICK_REFERENCE.md) — Referência rápida
- [ACCOUNT_REACTIVATION_GUIDE.md](ACCOUNT_REACTIVATION_GUIDE.md) — Guia técnico
- [ACCOUNT_REACTIVATION_PRACTICAL_EXAMPLE.md](ACCOUNT_REACTIVATION_PRACTICAL_EXAMPLE.md) — Passo a passo
- [ACCOUNT_REACTIVATION_TESTS.http](ACCOUNT_REACTIVATION_TESTS.http) — Testes HTTP

---

## Banco de Dados

### Diagrama de Relacionamento

```
usuarios
??? plantas (1:N)
??? posts (1:N)
??? comentarios (1:N)
??? curtidas (1:N)
??? notificacoes (1:N)
??? seguidores (N:N - auto-relacionamento)
??? tokens_refresh (1:N)

plantas
??? posts (1:N)

posts
??? comentarios (1:N)
??? curtidas (1:N)
??? notificacoes (1:N)

comentarios
??? curtidas (1:N)
```

### Tabelas Principais

#### usuarios

```sql
CREATE TABLE usuarios (
  id UUID PRIMARY KEY,
  nome VARCHAR(255) NOT NULL,
  email VARCHAR(255) UNIQUE NOT NULL,
  senha_hash VARCHAR(255) NOT NULL,
  biografia TEXT,
  foto_perfil VARCHAR(500),
  email_confirmado BOOLEAN DEFAULT FALSE,
  ativo BOOLEAN DEFAULT TRUE,
  data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  data_atualizacao TIMESTAMP,
  data_exclusao TIMESTAMP
);
```

**Soft Delete:** `ativo` e `data_exclusao`

#### plantas

```sql
CREATE TABLE plantas (
  id UUID PRIMARY KEY,
  usuario_id UUID REFERENCES usuarios(id),
  nome_cientifico VARCHAR(255),
  nome_comum VARCHAR(255),
  familia VARCHAR(100),
  genero VARCHAR(100),
  toxica VARCHAR(20),
  toxica_animais VARCHAR(20),
  toxica_criancas VARCHAR(20),
  requisitos_luz TEXT,
  requisitos_agua TEXT,
  requisitos_temperatura VARCHAR(100),
  cuidados TEXT,
  foto_planta VARCHAR(500),
  data_identificacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  data_atualizacao TIMESTAMP
);
```

#### posts

```sql
CREATE TABLE posts (
  id UUID PRIMARY KEY,
  usuario_id UUID REFERENCES usuarios(id),
  planta_id UUID REFERENCES plantas(id),
  conteudo TEXT NOT NULL,
  ativo BOOLEAN DEFAULT TRUE,
  data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  data_atualizacao TIMESTAMP,
  data_exclusao TIMESTAMP
);
```

#### notificacoes

```sql
CREATE TABLE notificacoes (
  id UUID PRIMARY KEY,
  usuario_id UUID REFERENCES usuarios(id),
  tipo VARCHAR(50) NOT NULL,
  mensagem TEXT NOT NULL,
  usuario_origem_id UUID REFERENCES usuarios(id),
  post_id UUID REFERENCES posts(id),
  planta_id UUID REFERENCES plantas(id),
  lida BOOLEAN DEFAULT FALSE,
  data_criacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  data_leitura TIMESTAMP,
  data_delecao TIMESTAMP
);
```

**Tipos:** `Curtida`, `Comentario`, `NovoSeguidor`, `LembreteCuidado`  
**Soft Delete:** `data_delecao` marca deleção lógica

---

## Serviços Externos

### PlantNet — Identificação

- **URL:** `https://my-api.plantnet.org/v2/identify`
- **Autenticação:** API Key como query param
- **Timeout:** 30s
- **Fallback:** Se falhar, tenta Trefle

```csharp
public async Task<PlantNetResponse> IdentificarAsync(Stream fotoStream)
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    using var form = new MultipartFormDataContent();
    
    form.Add(new StreamContent(fotoStream), "images", "foto.jpg");
    form.Add(new StringContent("all"), "include-related-images");

    var response = await client.PostAsync(
        $"{_baseUrl}?api-key={_apiKey}",
        form
    );

    return await response.Content.ReadAsAsync<PlantNetResponse>();
}
```

### Trefle — Dados Botânicos

- **URL:** `https://trefle.io/api/v1/plants/search`
- **Retorna:** Nome comum, foto, família, gênero
- **Paginação:** 20 resultados por page

```csharp
public async Task<TrefleResponse> BuscarAsync(string nomePlanta)
{
    var query = $"?q={nomePlanta}&token={_apiKey}&page=0&limit=20";
    var response = await _client.GetAsync($"{_baseUrl}{query}");
    return await response.Content.ReadAsAsync<TrefleResponse>();
}
```

### Gemini 2.5 Flash — IA

- **Modelo:** `gemini-2.5-flash`
- **Uso:** Enriquecer dados, gerar cuidados
- **2 Chamadas:** 1ª para geração, 2ª para validação

### Supabase Storage

- **Bucket:** `fotos`
- **Pastas:** `perfil-{usuarioId}`, `planta-{usuarioId}`
- **URLs públicas:** `https://projeto.supabase.co/storage/v1/object/public/fotos/{arquivo}`

---

## Lembretes Automáticos

### BackgroundService

**PlantCareReminderBackgroundService.cs:**

```csharp
public class PlantCareReminderBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1ª execução: AGORA (para testar em localhost)
        // Próximas: 08:00 AM UTC todos os dias
    }
}
```

**Registrado em Program.cs:**

```csharp
builder.Services.AddHostedService<PlantCareReminderBackgroundService>();
```

### Comportamento

- ? Executa imediatamente ao iniciar
- ? Depois, executa todos os dias às 8:00 AM UTC
- ? Sem duplicação (máximo 1 por planta/dia)
- ? Logs completos

---

## Padrões de Código

### Resultado<T>

```csharp
public record Resultado<T>(
    bool Sucesso,
    T? Dados = default,
    string? Mensagem = null,
    string[]? Erros = null
);

// Uso:
return Resultado<Usuario>.Ok(usuario);
return Resultado<Usuario>.Erro("Email já existe");
```

### DTO Pattern

```csharp
// Entrada
public record RegistroDTOEntrada(
    string Nome,
    string Email,
    string Senha,
    string ConfirmacaoSenha
);

// Saída
public record LoginDTOSaida(
    Guid UsuarioId,
    string Nome,
    string TokenAcesso,
    string TokenRefresh
);
```

### Soft Delete

```csharp
public class Post
{
    public bool Ativo { get; set; } = true;
    public DateTime? DataExclusao { get; set; }

    public void Excluir()
    {
        Ativo = false;
        DataExclusao = DateTime.UtcNow;
    }
}

// Query: sempre filtrar por Ativo
var posts = _db.Posts.Where(p => p.Ativo).ToList();
```

### Repository Pattern

```csharp
public interface IRepositorioPlanta : IRepositorio<Planta>
{
    Task<List<Planta>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<Planta?> ObterPorIdComDetalhesAsync(Guid id);
}

public class RepositorioPlanta : IRepositorioPlanta
{
    private readonly PlantaCoreDbContext _db;

    public async Task<List<Planta>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _db.Plantas
            .Where(p => p.UsuarioId == usuarioId && p.Ativo)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }
}
```

---

## Troubleshooting

### "Connection refused" ao Banco

```bash
# Verificar connection string
dotnet user-secrets list --project PlantaCoreAPI.API | grep Connection

# Testar conectividade (Supabase)
psql -h seu-host.supabase.co -U postgres -W

# Local
psql -h localhost -U postgres -W
```

### "The certificate is not trusted"

```bash
# Windows
dotnet dev-certs https --trust

# Linux
sudo dotnet dev-certs https --trust

# macOS
dotnet dev-certs https --trust
```

### Migrations com erro

```bash
# Ver migrations aplicadas
dotnet ef migrations list --project PlantaCoreAPI.Infrastructure

# Remover última migration
dotnet ef migrations remove --project PlantaCoreAPI.Infrastructure

# Limpar e recriar
dotnet ef database drop --project PlantaCoreAPI.Infrastructure
dotnet ef database update --project PlantaCoreAPI.Infrastructure
```

### JWT Token Expirado

**Problema:** 401 Unauthorized

**Solução:** Cliente deve usar refresh token

```csharp
POST /api/v1/autenticacao/refresh-token
{
  "tokenRefresh": "seu-refresh-token"
}
```

### Swagger não aparece

```bash
dotnet clean
dotnet restore
dotnet build
dotnet run --launch-profile https
# Acessar: https://localhost:7123/swagger
```

---

**Status:** ? Pronto para Produção  
**Versão:** 1.0  
**Última atualização:** 03/03/2025

## ?? Status

| Componente | Status |
|-----------|--------|
| Autenticação | ? Completo |
| Plantas + IA | ? Completo |
| Rede Social | ? Completo |
| Notificações | ? Completo |
| Deleção de Notificações | ? Completo |
| Exclusão de Conta (Cascata) | ? Completo |
| Lembretes | ? Completo |
| Reativação de Conta | ? Completo |
| Testes | ?? 50% |
| WebSocket | ?? Planejado |
