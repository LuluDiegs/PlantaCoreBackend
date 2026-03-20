using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriasHashtagsPalavrasChave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Categorias",
                table: "posts",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>()); // Adicionado valor padrão

            migrationBuilder.AddColumn<List<string>>(
                name: "Hashtags",
                table: "posts",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>()); // Adicionado valor padrão

            migrationBuilder.AddColumn<List<string>>(
                name: "PalavrasChave",
                table: "posts",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>()); // Adicionado valor padrão
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
