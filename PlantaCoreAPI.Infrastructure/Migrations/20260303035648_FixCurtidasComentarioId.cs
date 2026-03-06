using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCurtidasComentarioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_curtidas_comentarios_ComentarioId",
                table: "curtidas");

            migrationBuilder.DropIndex(
                name: "IX_curtidas_ComentarioId",
                table: "curtidas");

            migrationBuilder.DropIndex(
                name: "ix_curtidas_post_usuario",
                table: "curtidas");

            migrationBuilder.RenameColumn(
                name: "ComentarioId",
                table: "curtidas",
                newName: "comentario_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "post_id",
                table: "curtidas",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_comentario_usuario",
                table: "curtidas",
                columns: new[] { "comentario_id", "usuario_id" },
                unique: true,
                filter: "comentario_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_post_usuario",
                table: "curtidas",
                columns: new[] { "post_id", "usuario_id" },
                unique: true,
                filter: "post_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_curtidas_comentarios",
                table: "curtidas",
                column: "comentario_id",
                principalTable: "comentarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_curtidas_comentarios",
                table: "curtidas");

            migrationBuilder.DropIndex(
                name: "ix_curtidas_comentario_usuario",
                table: "curtidas");

            migrationBuilder.DropIndex(
                name: "ix_curtidas_post_usuario",
                table: "curtidas");

            migrationBuilder.RenameColumn(
                name: "comentario_id",
                table: "curtidas",
                newName: "ComentarioId");

            migrationBuilder.AlterColumn<Guid>(
                name: "post_id",
                table: "curtidas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_curtidas_ComentarioId",
                table: "curtidas",
                column: "ComentarioId");

            migrationBuilder.CreateIndex(
                name: "ix_curtidas_post_usuario",
                table: "curtidas",
                columns: new[] { "post_id", "usuario_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_curtidas_comentarios_ComentarioId",
                table: "curtidas",
                column: "ComentarioId",
                principalTable: "comentarios",
                principalColumn: "id");
        }
    }
}
