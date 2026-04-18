using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecomendacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recomendacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_comum = table.Column<string>(type: "text", nullable: false),
                    url_imagem = table.Column<string>(type: "text", nullable: false),
                    justificativa = table.Column<string>(type: "text", nullable: false),
                    experiencia = table.Column<string>(type: "text", nullable: false),
                    iluminacao = table.Column<string>(type: "text", nullable: false),
                    regagem = table.Column<string>(type: "text", nullable: false),
                    seguranca = table.Column<string>(type: "text", nullable: false),
                    proposito = table.Column<string>(type: "text", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recomendacoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_recomendacao_usuario",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recomendacoes_UsuarioId",
                table: "recomendacoes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recomendacoes");
        }
    }
}
