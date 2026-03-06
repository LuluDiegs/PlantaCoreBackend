using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
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
                name: "plantas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_cientifico = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    nome_comum = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    familia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    genero = table.Column<string>(type: "text", nullable: true),
                    especie = table.Column<string>(type: "text", nullable: true),
                    toxica = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    descricao_toxicidade = table.Column<string>(type: "text", nullable: true),
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
                name: "posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    planta_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                        name: "fk_posts_plantas",
                        column: x => x.planta_id,
                        principalTable: "plantas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_posts_usuarios",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
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
                    data_leitura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "curtidas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ComentarioId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curtidas", x => x.id);
                    table.ForeignKey(
                        name: "FK_curtidas_comentarios_ComentarioId",
                        column: x => x.ComentarioId,
                        principalTable: "comentarios",
                        principalColumn: "id");
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
                name: "ix_comentarios_post_id",
                table: "comentarios",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_comentarios_usuario_id",
                table: "comentarios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_curtidas_ComentarioId",
                table: "curtidas",
                column: "ComentarioId");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_post_id",
                table: "curtidas",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_post_usuario",
                table: "curtidas",
                columns: new[] { "post_id", "usuario_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_usuario_id",
                table: "curtidas",
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
                name: "ix_plantas_usuario_id",
                table: "plantas",
                column: "usuario_id");

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
                name: "ix_tokens_refresh_token",
                table: "tokens_refresh",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tokens_refresh_usuario_id",
                table: "tokens_refresh",
                column: "usuario_id");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "curtidas");

            migrationBuilder.DropTable(
                name: "notificacoes");

            migrationBuilder.DropTable(
                name: "seguidores");

            migrationBuilder.DropTable(
                name: "tokens_refresh");

            migrationBuilder.DropTable(
                name: "comentarios");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "plantas");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
