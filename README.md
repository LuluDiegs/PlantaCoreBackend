# ?? PlantaCoreAPI

**API REST para gerenciamento e compartilhamento de plantas com IA integrada.**

## ?? O que �?

Uma plataforma social para amantes de plantas com identifica��o autom�tica via IA, compartilhamento de cuidados, lembretes autom�ticos, reativa��o de conta e comunidade.

## ? Features Principais

| Feature | Status | Descri��o |
|---------|--------|-----------|
| **Identifica��o IA** | ? | Foto ? Planta (PlantNet + Trefle + Gemini) |
| **Rede Social** | ? | Posts, curtidas, coment�rios, seguir |
| **Notifica��es** | ? | Curtidas, coment�rios, novos seguidores |
| **Exclus�o de Conta** | ? | Cascata completa: plantas, posts, fotos, etc |
| **Reativa��o de Conta** | ? | Email + token + nova senha |
| **Lembretes** | ? | Autom�ticos 1x/dia com cuidados da planta |
| **Autentica��o** | ? | JWT com refresh tokens |
| **Upload** | ? | Fotos via Supabase Storage |
| **Email** | ? | Confirma��o, reset senha, reativa��o |

## ?? Quick Start

### Pr�-requisitos

- **.NET 8 SDK**
- **PostgreSQL** (ou Supabase)
- **Git**

### 1. Clonar

```bash
git clone seu-repositorio
cd PlantaCoreAPI
```

### 2. Configurar Secrets

```bash
dotnet user-secrets init --project PlantaCoreAPI.API

# Banco de dados
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "sua-connection-string" --project PlantaCoreAPI.API

# JWT
dotnet user-secrets set "Jwt:ChaveSecreta" "sua-chave-32-caracteres" --project PlantaCoreAPI.API

# Email
dotnet user-secrets set "Email:Email" "seu-email@gmail.com" --project PlantaCoreAPI.API
dotdate user-secrets set "Email:Senha" "sua-senha-app" --project PlantaCoreAPI.API

# APIs Externas
dotnet user-secrets set "Gemini:ChaveApi" "sua-chave-gemini" --project PlantaCoreAPI.API
dotnet user-secrets set "PlantNet:ChaveApi" "sua-chave-plantnet" --project PlantaCoreAPI.API
dotnet user-secrets set "Trefle:ChaveApi" "sua-chave-trefle" --project PlantaCoreAPI.API

# Supabase
dotnet user-secrets set "Supabase:Url" "https://seu-projeto.supabase.co" --project PlantaCoreAPI.API
dotnet user-secrets set "Supabase:ChavePublica" "sua-anon-key" --project PlantaCoreAPI.API
```

### 3. Restaurar e Migrar

```bash
dotnet restore
dotnet ef database update --project PlantaCoreAPI.Infrastructure --startup-project PlantaCoreAPI.API
```

### 4. Executar

```bash
cd PlantaCoreAPI.API
dotnet run --launch-profile https
```

**URLs:**
- API: `http://localhost:5123`
- Swagger: `http://localhost:5123/swagger`

## ?? Documenta��o

- **[BACKEND_DOCS.md](BACKEND_DOCS.md)** � Arquitetura, endpoints, servi�os, banco de dados
- **[DOCUMENTACAO_INDICE.md](DOCUMENTACAO_INDICE.md)** � �ndice completo de documenta��o

## ??? Arquitetura

```
Clean Architecture

API (Controllers) ? Application (DTOs, Interfaces)
  ?
Infrastructure (Servi�os, Reposit�rios, EF Core)
  ?
Domain (Entidades, Enums)
```

## ?? Principais Endpoints

### Autentica��o
```
POST   /api/v1/autenticacao/registrar
POST   /api/v1/autenticacao/login
POST   /api/v1/autenticacao/refresh-token
POST   /api/v1/autenticacao/logout
```

### Plantas
```
POST   /api/v1/planta/identificar              (foto)
POST   /api/v1/planta/buscar                   (cat�logo)
GET    /api/v1/planta/minhas-plantas           (paginado)
```

### Posts
```
POST   /api/v1/post
GET    /api/v1/post/feed                       (usu�rios seguidos)
GET    /api/v1/post/explorar                   (p�blicos)
POST   /api/v1/post/{id}/curtir
```

### Notifica��es
```
GET    /api/v1/notificacao                     (todas + lembretes)
GET    /api/v1/notificacao/nao-lidas
PUT    /api/v1/notificacao/{id}/marcar-como-lida
DELETE /api/v1/notificacao/{id}                (deletar uma)
DELETE /api/v1/notificacao                     (deletar todas)
```

### Usu�rio
```
DELETE /api/v1/usuario/conta                   (deletar conta + cascata)
POST   /api/v1/usuario/reativar/solicitar      (reativa��o)
POST   /api/v1/usuario/reativar/confirmar      (confirmar reativa��o)
GET    /api/v1/usuario/perfil                  (dados do usu�rio)
PUT    /api/v1/usuario/nome                    (atualizar nome)
POST   /api/v1/usuario/foto-perfil             (upload de foto)
```

[Ver todos os endpoints ?](BACKEND_DOCS.md#endpoints-detalhados)

## ?? IA & Servi�os Externos

| Servi�o | Uso | API |
|---------|-----|-----|
| **PlantNet** | Identifica��o por foto | `my-api.plantnet.org` |
| **Trefle** | Dados bot�nicos | `trefle.io` |
| **Gemini 2.5** | Gera��o de cuidados | `google.com/generativeai` |
| **Supabase** | Storage de fotos | `supabase.co` |
| **Gmail** | Email transacional | `smtp.gmail.com` |

## ?? Lembretes Autom�ticos

- ? Executa **imediatamente** ao iniciar (localhost)
- ?? Executa **1x por dia �s 8:00 AM** (UTC)
- ?? Sem duplica��o
- ?? Logs completos

```
dotnet run
  ?
? Gerando lembretes AGORA...
  ?
? Lembretes criados
  ?
? Pr�ximo disparo: amanh� 08:00 AM
```

## ??? Banco de Dados

**PostgreSQL 15+** com 8 tabelas:

- `usuarios` (soft delete)
- `plantas`
- `posts` (soft delete)
- `comentarios` (soft delete)
- `curtidas`
- `notificacoes`
- `tokens_refresh`
- `seguidores` (N:N)

## ?? Autentica��o

- **JWT Bearer** com access + refresh tokens
- **Access Token:** 15 minutos
- **Refresh Token:** 7 dias
- **Senha:** Hash com bcrypt
- **Reativa��o:** Token �nico v�lido 1 hora

## ?? Stack T�cnico

| Camada | Tecnologia |
|--------|-----------|
| **Framework** | ASP.NET Core 8 |
| **ORM** | Entity Framework Core 8 |
| **Banco** | PostgreSQL 15+ |
| **Autentica��o** | JWT Bearer |
| **Storage** | Supabase Storage |
| **Email** | Gmail SMTP |
| **Testes** | xUnit |
| **Logging** | Serilog |

## ?? Testes

```bash
# Rodar testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

## ?? Padr�o de Resposta

Todos os endpoints retornam:

```json
{
  "sucesso": true,
  "dados": { /* payload */ },
  "mensagem": null,
  "erros": null
}
```

## ? Status

| Componente | Status |
|-----------|--------|
| Autentica��o | ? Completo |
| Plantas + IA | ? Completo |
| Rede Social | ? Completo |
| Notifica��es | ? Completo |
| Lembretes | ? Completo |
| Reativa��o de Conta | ? Completo |
| Testes | ?? 50% |
| WebSocket | ?? Planejado |

## ?? Deploy

### Op��es

- **Render.com** (recomendado)
- **Railway.app**
- **Azure App Service**
- **AWS Elastic Beanstalk**

### Passos B�sicos

1. Fazer push no Git
2. Conectar reposit�rio no Render/Railway
3. Configurar vari�veis de ambiente
4. Deploy autom�tico

## ?? Contribuindo

1. Fork o reposit�rio
2. Crie uma branch (`git checkout -b feature/nova-feature`)
3. Commit mudan�as (`git commit -am 'Adiciona nova feature'`)
4. Push (`git push origin feature/nova-feature`)
5. Abra um Pull Request

## ?? Suporte

- **Issues:** GitHub Issues
- **Documenta��o:** [BACKEND_DOCS.md](BACKEND_DOCS.md)

## ?? Licen�a

MIT License � Veja [LICENSE](LICENSE) para detalhes

---

**Desenvolvido com ?? para amantes de plantas e tecnologia.**

**Vers�o:** 1.0  
**Status:** ? Produ��o  
**�ltima atualiza��o:** 03/03/2025
