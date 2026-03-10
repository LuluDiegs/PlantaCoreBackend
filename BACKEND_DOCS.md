# 📘 Documentação Técnica Backend — PlantaCoreAPI

## 📑 Índice Completo

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

# Arquitetura

## Clean Architecture

```
PlantaCoreAPI.API
Controllers, Program.cs, Extensions
        ↓
PlantaCoreAPI.Application
DTOs, Interfaces, Utilidades
        ↓
PlantaCoreAPI.Infrastructure
EF Core, Repositórios, Serviços Externos
        ↓
PlantaCoreAPI.Domain
Entidades, Enums, Interfaces
```

## Princípios

- Separação de responsabilidades
- Inversão de controle com Dependency Injection
- Repository Pattern
- DTO Pattern
- Soft Delete
- Auditoria com DataCriacao e DataAtualizacao

---

# Estrutura Detalhada

## Domain (Núcleo de Negócio)

```
PlantaCoreAPI.Domain/

Entities/
Usuario.cs
Planta.cs
Post.cs
Comentario.cs
Curtida.cs
Notificacao.cs
TokenRefresh.cs

Enums/
TipoNotificacao.cs

Interfaces/
IRepositorio.cs
IRepositorioUsuario.cs
IRepositorioPlanta.cs
IRepositorioPost.cs
IRepositorioCurtida.cs
IRepositorioComentario.cs
IRepositorioNotificacao.cs
IRepositorioTokenRefresh.cs

Comuns/
PaginaResultado.cs
```

---

## Application (Camada de Negócio)

```
PlantaCoreAPI.Application/

DTOs/

Auth/
RegistroDTOEntrada.cs
LoginDTOEntrada.cs
LoginDTOSaida.cs

Planta/
PlantaDTOSaida.cs
AdicionarPlantaTrefleDTO.cs

Post/
PostDTOSaida.cs
CriarPostDTOEntrada.cs

Comentario/
ComentarioDTOSaida.cs
CriarComentarioDTOEntrada.cs

Usuario/
UsuarioDTOSaida.cs

Identificacao/
IdentificacaoDTOs.cs

Notificacao/
NotificacaoDTOSaida.cs
ListarNotificacoesDTOSaida.cs
LembreteCuidadoDTO.cs

Interfaces/

IAuthenticationService.cs
IUserService.cs
IPlantService.cs
IPostService.cs
INotificationService.cs
IPlantCareReminderService.cs
IJwtService.cs
IPasswordHashService.cs
IEmailService.cs
IFileStorageService.cs
IGeminiService.cs
IPlantNetService.cs
ITrefleService.cs

Comuns/

Resultado.cs
PaginaResultado.cs

Utils/

EmailTemplateGenerator.cs
PasswordValidator.cs
```

---

## Infrastructure (Implementações)

```
PlantaCoreAPI.Infrastructure/

Dados/
PlantaCoreDbContext.cs

Repositorios/
RepositorioUsuario.cs
RepositorioPlanta.cs
RepositorioPost.cs
RepositorioCurtida.cs
RepositorioComentario.cs
RepositorioNotificacao.cs
RepositorioTokenRefresh.cs

Services/

AuthenticationService.cs
UserService.cs
NotificationService.cs
PlantCareReminderService.cs
PlantCareReminderBackgroundService.cs
PasswordHashService.cs

Plant/

PlantService.cs
PlantService.Identificacao.cs
PlantService.Busca.cs
PlantService.Crud.cs
PlantService.GeminiParser.cs
PlantService.Mapper.cs
DadosPlantaEnriquecidos.cs

External/

JwtService.cs
EmailService.cs
PlantNetService.cs
TrefleService.cs
GeminiService.cs

Storage/

SupabaseFileStorageService.cs

Migrations/

InitialCreate.cs
AddPostsAndFollowers.cs
AddCurtidasAndComentarios.cs
AddNotificacoes.cs
```

---

## API (Camada de Apresentação)

```
PlantaCoreAPI.API/

Controllers/

AutenticacaoController.cs
UsuarioController.cs
PlantaController.cs
PostController.cs
NotificacaoController.cs
StorageController.cs
LembreteCuidadoController.cs

Extensions/

AuthExtensions.cs
DatabaseExtensions.cs
ServicesExtensions.cs
SwaggerExtensions.cs
LoggingExtensions.cs

Hubs/

NotificacaoHub.cs

Program.cs
appsettings.json
appsettings.Development.json
```

---

# Setup Completo

## Pré-requisitos

- .NET 8 SDK
- PostgreSQL 15+
- Supabase ou PostgreSQL local
- Git

---

## 1. Clonar repositório

```bash
git clone seu-repositorio
cd PlantaCoreAPI
```

---

## 2. Inicializar User Secrets

```bash
dotnet user-secrets init --project PlantaCoreAPI.API
```

---

## 3. Configurar banco de dados

### Supabase

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
"Host=db.xxx.supabase.co;Username=postgres;Password=xxx;Database=postgres;Port=5432;SSL Mode=Require" \
--project PlantaCoreAPI.API
```

### PostgreSQL Local

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
"Host=localhost;Username=postgres;Password=sua-senha;Database=plantacore;Port=5432" \
--project PlantaCoreAPI.API
```

---

## 4. Configurar JWT

```bash
dotnet user-secrets set "Jwt:ChaveSecreta" "sua-chave-super-segura-com-32-caracteres" --project PlantaCoreAPI.API
dotnet user-secrets set "Jwt:Emissor" "PlantaCoreAPI" --project PlantaCoreAPI.API
dotnet user-secrets set "Jwt:Publico" "PlantaCoreAPI" --project PlantaCoreAPI.API
```

---

## 5. Configurar Email

```bash
dotnet user-secrets set "Email:Email" "seu-email@gmail.com" --project PlantaCoreAPI.API
dotnet user-secrets set "Email:Senha" "senha-de-app-do-gmail" --project PlantaCoreAPI.API
```

---

## 6. APIs externas

### PlantNet

```bash
dotnet user-secrets set "PlantNet:ChaveApi" "sua-chave-plantnet" --project PlantaCoreAPI.API
```

### Trefle

```bash
dotnet user-secrets set "Trefle:ChaveApi" "sua-chave-trefle" --project PlantaCoreAPI.API
```

### Gemini

```bash
dotnet user-secrets set "Gemini:ChaveApi" "sua-chave-gemini" --project PlantaCoreAPI.API
```

---

## 7. Supabase Storage

```bash
dotnet user-secrets set "Supabase:Url" "https://seu-projeto.supabase.co" --project PlantaCoreAPI.API
dotnet user-secrets set "Supabase:ChavePublica" "sua-anon-key" --project PlantaCoreAPI.API
```

---

## 8. Restaurar dependências

```bash
dotnet restore
```

---

## 9. Aplicar migrations

```bash
dotnet ef database update \
--project PlantaCoreAPI.Infrastructure \
--startup-project PlantaCoreAPI.API
```

---

## 10. Executar projeto

```bash
cd PlantaCoreAPI.API
dotnet run --launch-profile https
```

URLs:

```
https://localhost:7123
http://localhost:5123
http://localhost:5123/swagger
```

---

# Autenticação JWT

Fluxo:

```
Login
↓
Backend valida email e senha
↓
Gera AccessToken (15 min)
Gera RefreshToken (7 dias)
↓
Cliente salva tokens
↓
Requests usam Authorization Bearer
```

---

# Banco de Dados

## usuarios

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

---

## plantas

```sql
CREATE TABLE plantas (
 id UUID PRIMARY KEY,
 usuario_id UUID REFERENCES usuarios(id),
 nome_cientifico VARCHAR(255),
 nome_comum VARCHAR(255),
 familia VARCHAR(100),
 genero VARCHAR(100),
 toxica VARCHAR(20),
 requisitos_luz TEXT,
 requisitos_agua TEXT,
 requisitos_temperatura VARCHAR(100),
 cuidados TEXT,
 foto_planta VARCHAR(500),
 data_identificacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

# Lembretes Automáticos

Background Service executa diariamente às **08:00 UTC**.

```csharp
builder.Services.AddHostedService<PlantCareReminderBackgroundService>();
```

---

# Padrões de Código

## Resultado

```csharp
public record Resultado<T>(
 bool Sucesso,
 T? Dados,
 string? Mensagem,
 string[]? Erros
);
```

---

## Soft Delete

```csharp
public void Excluir()
{
 Ativo = false;
 DataExclusao = DateTime.UtcNow;
}
```

---

# Troubleshooting

## Erro conexão banco

```bash
dotnet user-secrets list --project PlantaCoreAPI.API
```

---

## Confiar certificado HTTPS

Windows

```bash
dotnet dev-certs https --trust
```

Linux

```bash
sudo dotnet dev-certs https --trust
```

Mac

```bash
dotnet dev-certs https --trust
```

---

# Status do Projeto

| Componente | Status |
|------------|--------|
| Autenticação | Completo |
| Plantas + IA | Completo |
| Rede Social | Completo |
| Notificações | Completo |
| Exclusão de Conta | Completo |
| Lembretes | Completo |
| Reativação de Conta | Completo |

---

Versão: 1.0  
Última atualização: 03/03/2025
