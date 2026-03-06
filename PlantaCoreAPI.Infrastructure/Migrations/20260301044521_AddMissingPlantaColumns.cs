using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    public partial class AddMissingPlantaColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "toxica_animais",
                table: "plantas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "descricao_toxicidade_animais",
                table: "plantas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "toxica_criancas",
                table: "plantas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "descricao_toxicidade_criancas",
                table: "plantas",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "toxica_animais",
                table: "plantas");

            migrationBuilder.DropColumn(
                name: "descricao_toxicidade_animais",
                table: "plantas");

            migrationBuilder.DropColumn(
                name: "toxica_criancas",
                table: "plantas");

            migrationBuilder.DropColumn(
                name: "descricao_toxicidade_criancas",
                table: "plantas");
        }
    }
}
