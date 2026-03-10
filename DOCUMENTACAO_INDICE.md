# PlantaCoreAPI

**API REST para gerenciamento e compartilhamento de plantas com IA integrada.**

## O que é?

Uma plataforma social para amantes de plantas com identificação automática via IA, compartilhamento de cuidados, lembretes automáticos, reativação de conta e comunidade.

## Features Principais

| Feature | Status | Descrição |
|---------|--------|-----------|
| **Identificação IA** | - | Foto → Planta (PlantNet + Trefle + Gemini) |
| **Rede Social** | - | Posts, curtidas, comentários, seguir |
| **Notificações** | - | Curtidas, comentários, novos seguidores |
| **Exclusão de Conta** | - | Cascata completa: plantas, posts, fotos, etc |
| **Reativação de Conta** | - | Email + token + nova senha |
| **Lembretes** | - | Automáticos 1x/dia com cuidados da planta |
| **Autenticação** | - | JWT com refresh tokens |
| **Upload** | - | Fotos via Supabase Storage |
| **Email** | - | Confirmação, reset senha, reativação |

## Quick Start

### Pré-requisitos

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
dotnet user-secrets set "Email:Senha" "sua-senha-app" --project PlantaCoreAPI.API

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

## Documentação

- **[BACKEND_DOCS.md](BACKEND_DOCS.md)** – Arquitetura, endpoints, serviços, banco de dados
- **[DOCUMENTACAO_INDICE.md](DOCUMENTACAO_INDICE.md)** – Índice completo de documentação

## Arquitetura

```
Clean Architecture

API (Controllers) → Application (DTOs, Interfaces)
  →
Infrastructure (Serviços, Repositórios, EF Core)
  →
Domain (Entidades, Enums)
```

## Principais Endpoints

### Autenticação
```
POST   /api/v1/autenticacao/registrar
POST   /api/v1/autenticacao/login
POST   /api/v1/autenticacao/refresh-token
POST   /api/v1/autenticacao/logout
```

### Plantas
```
POST   /api/v1/planta/identificar
POST   /api/v1/planta/buscar
GET    /api/v1/planta/minhas-plantas
```

### Posts
```
POST   /api/v1/post
GET    /api/v1/post/feed
GET    /api/v1/post/explorar
POST   /api/v1/post/{id}/curtir
```

### Notificações
```
GET    /api/v1/notificacao
GET    /api/v1/notificacao/nao-lidas
PUT    /api/v1/notificacao/{id}/marcar-como-lida
DELETE /api/v1/notificacao/{id}
DELETE /api/v1/notificacao
```

### Usuário
```
DELETE /api/v1/usuario/conta
POST   /api/v1/usuario/reativar/solicitar
POST   /api/v1/usuario/reativar/confirmar
GET    /api/v1/usuario/perfil
PUT    /api/v1/usuario/nome
POST   /api/v1/usuario/foto-perfil
```

[Ver todos os endpoints](BACKEND_DOCS.md#endpoints-detalhados)

## IA & Serviços Externos

| Serviço | Uso | API |
|---------|-----|-----|
| **PlantNet** | Identificação por foto | `my-api.plantnet.org` |
| **Trefle** | Dados botânicos | `trefle.io` |
| **Gemini 2.5** | Geração de cuidados | `google.com/generativeai` |
| **Supabase** | Storage de fotos | `supabase.co` |
| **Gmail** | Email transacional | `smtp.gmail.com` |

## Lembretes Automáticos

- Executa **imediatamente** ao iniciar (localhost)
- Executa **1x por dia às 8:00 AM** (UTC)
- Sem duplicação
- Logs completos

```
dotnet run
  → Gerando lembretes AGORA...
  → Lembretes criados
  → Próximo disparo: amanhã 08:00 AM
```

## Banco de Dados

**PostgreSQL 15+** com 8 tabelas:

- `usuarios` (soft delete)
- `plantas`
- `posts` (soft delete)
- `comentarios` (soft delete)
- `curtidas`
- `notificacoes`
- `tokens_refresh`
- `seguidores` (N:N)

## Autenticação

- **JWT Bearer** com access + refresh tokens
- **Access Token:** 15 minutos
- **Refresh Token:** 7 dias
- **Senha:** Hash com bcrypt
- **Reativação:** Token único válido 1 hora

## Stack Técnico

| Camada | Tecnologia |
|--------|-----------|
| **Framework** | ASP.NET Core 8 |
| **ORM** | Entity Framework Core 8 |
| **Banco** | PostgreSQL 15+ |
| **Autenticação** | JWT Bearer |
| **Storage** | Supabase Storage |
| **Email** | Gmail SMTP |
| **Testes** | xUnit |
| **Logging** | Serilog |

## Testes

```bash
dotnet test
dotnet test /p:CollectCoverage=true
```

## Padrão de Resposta

```json
{
  "sucesso": true,
  "dados": { /* payload */ },
  "mensagem": null,
  "erros": null
}
```

## Status

| Componente | Status |
|-----------|--------|
| Autenticação | Completo |
| Plantas + IA | Completo |
| Rede Social | Completo |
| Notificações | Completo |
| Lembretes | Completo |
| Reativação de Conta | Completo |

### Passos Básicos

1. Fazer push no Git
2. Conectar repositório no Render/Railway
3. Configurar variáveis de ambiente
4. Deploy automático

## Contribuindo

1. Fork o repositório
2. Crie uma branch (`git checkout -b feature/nova-feature`)
3. Commit mudanças (`git commit -am 'Adiciona nova feature'`)
4. Push (`git push origin feature/nova-feature`)
5. Abra um Pull Request

## Suporte

- **Issues:** GitHub Issues
- **Documentação:** [BACKEND_DOCS.md](BACKEND_DOCS.md)

## Licença

MIT License – Veja [LICENSE](LICENSE) para detalhes

---

**Desenvolvido para amantes de plantas e tecnologia.**

**Versão:** 1.0  
**Status:** Produção  
**Última atualização:** 03/03/2025
