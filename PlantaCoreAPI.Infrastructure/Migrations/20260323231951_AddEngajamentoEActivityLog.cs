using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEngajamentoEActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categorias",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "Hashtags",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "PalavrasChave",
                table: "posts");

            migrationBuilder.AddColumn<Guid>(
                name: "ComentarioPaiId",
                table: "comentarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entidade_tipo = table.Column<string>(type: "text", nullable: true),
                    meta_dados = table.Column<string>(type: "text", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_saves",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_saves", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_shares",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_shares", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_views",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_views", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activitylog_usuario_id",
                table: "activity_logs",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_postsave_usuario_post",
                table: "post_saves",
                columns: new[] { "usuario_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_postshare_usuario_post",
                table: "post_shares",
                columns: new[] { "usuario_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_postview_usuario_post",
                table: "post_views",
                columns: new[] { "usuario_id", "post_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");

            migrationBuilder.DropTable(
                name: "post_saves");

            migrationBuilder.DropTable(
                name: "post_shares");

            migrationBuilder.DropTable(
                name: "post_views");

            migrationBuilder.DropColumn(
                name: "ComentarioPaiId",
                table: "comentarios");

            migrationBuilder.AddColumn<List<string>>(
                name: "Categorias",
                table: "posts",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "Hashtags",
                table: "posts",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "PalavrasChave",
                table: "posts",
                type: "text[]",
                nullable: false);
        }
    }
}
