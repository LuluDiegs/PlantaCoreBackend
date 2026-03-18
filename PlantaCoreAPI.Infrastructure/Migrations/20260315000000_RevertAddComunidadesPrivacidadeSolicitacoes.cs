using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevertAddComunidadesPrivacidadeSolicitacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Revert changes made by AddComunidadesPrivacidadeSolicitacoes
            migrationBuilder.DropForeignKey(
                name: "fk_posts_comunidades",
                table: "posts");

            migrationBuilder.DropForeignKey(
                name: "fk_posts_plantas",
                table: "posts");

            migrationBuilder.DropTable(
                name: "membros_comunidade");

            migrationBuilder.DropTable(
                name: "solicitacoes_seguir");

            migrationBuilder.DropTable(
                name: "comunidades");

            migrationBuilder.DropIndex(
                name: "ix_posts_comunidade_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "perfil_privado",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "comunidade_id",
                table: "posts");

            migrationBuilder.AlterColumn<Guid>(
                name: "planta_id",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_posts_plantas",
                table: "posts",
                column: "planta_id",
                principalTable: "plantas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reapply changes made by AddComunidadesPrivacidadeSolicitacoes
            migrationBuilder.AddColumn<bool>(
                name: "perfil_privado",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "planta_id",
                table: "posts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "comunidade_id",
                table: "posts",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_posts_comunidade_id",
                table: "posts",
                column: "comunidade_id");

            migrationBuilder.CreateIndex(
                name: "IX_comunidades_criador_id",
                table: "comunidades",
                column: "criador_id");

            migrationBuilder.CreateIndex(
                name: "ix_comunidades_nome",
                table: "comunidades",
                column: "nome");

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
                name: "IX_solicitacoes_seguir_alvo_id",
                table: "solicitacoes_seguir",
                column: "alvo_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_seguir_par",
                table: "solicitacoes_seguir",
                columns: new[] { "solicitante_id", "alvo_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_posts_comunidades",
                table: "posts",
                column: "comunidade_id",
                principalTable: "comunidades",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_posts_plantas",
                table: "posts",
                column: "planta_id",
                principalTable: "plantas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
