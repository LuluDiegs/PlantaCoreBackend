using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    senha_hash = table.Column<string>(type: "text", nullable: false),
                    biografia = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    foto_perfil = table.Column<string>(type: "text", nullable: true),
                    perfil_privado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    email_confirmado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    token_confirmacao_email = table.Column<string>(type: "text", nullable: true),
                    token_resetar_senha = table.Column<string>(type: "text", nullable: true),
                    data_token_resetar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_exclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comunidades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    criador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    foto_comunidade = table.Column<string>(type: "text", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ativa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comunidades", x => x.id);
                    table.ForeignKey(
                        name: "fk_comunidades_criador",
                        column: x => x.criador_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plantas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_cientifico = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    nome_comum = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    familia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    genero = table.Column<string>(type: "text", nullable: true),
                    toxica = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    descricao_toxicidade = table.Column<string>(type: "text", nullable: true),
                    toxica_animais = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    descricao_toxicidade_animais = table.Column<string>(type: "text", nullable: true),
                    toxica_criancas = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    descricao_toxicidade_criancas = table.Column<string>(type: "text", nullable: true),
                    requisitos_luz = table.Column<string>(type: "text", nullable: true),
                    requisitos_agua = table.Column<string>(type: "text", nullable: true),
                    requisitos_temperatura = table.Column<string>(type: "text", nullable: true),
                    cuidados = table.Column<string>(type: "text", nullable: true),
                    foto_url = table.Column<string>(type: "text", nullable: true),
                    dados_plantnet = table.Column<string>(type: "text", nullable: true),
                    data_identificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plantas", x => x.id);
                    table.ForeignKey(
                        name: "fk_plantas_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seguidores",
                columns: table => new
                {
                    seguidor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seguido_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seguidores", x => new { x.seguidor_id, x.seguido_id });
                    table.ForeignKey(
                        name: "FK_seguidores_usuarios_seguido_id",
                        column: x => x.seguido_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_seguidores_usuarios_seguidor_id",
                        column: x => x.seguidor_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes_seguir",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alvo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_solicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    pendente = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitacoes_seguir", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitacoes_seguir_alvo",
                        column: x => x.alvo_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_solicitacoes_seguir_solicitante",
                        column: x => x.solicitante_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens_refresh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    data_expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    revogado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_revogacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens_refresh", x => x.id);
                    table.ForeignKey(
                        name: "fk_tokens_refresh_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "membros_comunidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comunidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    eh_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membros_comunidade", x => x.id);
                    table.ForeignKey(
                        name: "fk_membros_comunidade",
                        column: x => x.comunidade_id,
                        principalTable: "comunidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_membros_comunidade_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    planta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comunidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conteudo = table.Column<string>(type: "text", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_exclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    pontuacao_total = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                    table.ForeignKey(
                        name: "fk_posts_comunidades",
                        column: x => x.comunidade_id,
                        principalTable: "comunidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_posts_plantas",
                        column: x => x.planta_id,
                        principalTable: "plantas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_posts_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comentarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conteudo = table.Column<string>(type: "text", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_exclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    pontuacao_total = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comentarios", x => x.id);
                    table.ForeignKey(
                        name: "fk_comentarios_posts",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_comentarios_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hashtags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashtags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hashtags_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notificacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    planta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: false),
                    lida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_leitura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_delecao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_notificacoes_plantas",
                        column: x => x.planta_id,
                        principalTable: "plantas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notificacoes_posts",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notificacoes_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notificacoes_usuarios_origem",
                        column: x => x.usuario_origem_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PalavrasChave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Palavra = table.Column<string>(type: "text", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalavrasChave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalavrasChave_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "curtidas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comentario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curtidas", x => x.id);
                    table.ForeignKey(
                        name: "fk_curtidas_comentarios",
                        column: x => x.comentario_id,
                        principalTable: "comentarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_curtidas_posts",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_curtidas_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_PostId",
                table: "Categorias",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "ix_comentarios_post_id",
                table: "comentarios",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_comentarios_usuario_id",
                table: "comentarios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_comunidades_criador_id",
                table: "comunidades",
                column: "criador_id");

            migrationBuilder.CreateIndex(
                name: "ix_comunidades_nome",
                table: "comunidades",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_comentario_usuario",
                table: "curtidas",
                columns: new[] { "comentario_id", "usuario_id" },
                unique: true,
                filter: "comentario_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_post_id",
                table: "curtidas",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_post_usuario",
                table: "curtidas",
                columns: new[] { "post_id", "usuario_id" },
                unique: true,
                filter: "post_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_usuario_id",
                table: "curtidas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_PostId",
                table: "Hashtags",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "ix_membros_comunidade_unico",
                table: "membros_comunidade",
                columns: new[] { "comunidade_id", "usuario_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_membros_comunidade_usuario_id",
                table: "membros_comunidade",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_planta_id",
                table: "notificacoes",
                column: "planta_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_post_id",
                table: "notificacoes",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_usuario_id",
                table: "notificacoes",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_usuario_origem_id",
                table: "notificacoes",
                column: "usuario_origem_id");

            migrationBuilder.CreateIndex(
                name: "IX_PalavrasChave_PostId",
                table: "PalavrasChave",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "ix_plantas_usuario_id",
                table: "plantas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_comunidade_id",
                table: "posts",
                column: "comunidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_planta_id",
                table: "posts",
                column: "planta_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_usuario_id",
                table: "posts",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_seguidores_seguido_id",
                table: "seguidores",
                column: "seguido_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_seguir_alvo_id",
                table: "solicitacoes_seguir",
                column: "alvo_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_seguir_par",
                table: "solicitacoes_seguir",
                columns: new[] { "solicitante_id", "alvo_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tokens_refresh_token",
                table: "tokens_refresh",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tokens_refresh_usuario_id",
                table: "tokens_refresh",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "curtidas");

            migrationBuilder.DropTable(
                name: "Hashtags");

            migrationBuilder.DropTable(
                name: "membros_comunidade");

            migrationBuilder.DropTable(
                name: "notificacoes");

            migrationBuilder.DropTable(
                name: "PalavrasChave");

            migrationBuilder.DropTable(
                name: "seguidores");

            migrationBuilder.DropTable(
                name: "solicitacoes_seguir");

            migrationBuilder.DropTable(
                name: "tokens_refresh");

            migrationBuilder.DropTable(
                name: "comentarios");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "comunidades");

            migrationBuilder.DropTable(
                name: "plantas");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
