using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaLocalizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "localizacao",
                table: "plantas");

            migrationBuilder.AddColumn<bool>(
                name: "compartilhar_localizacao",
                table: "plantas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "latitude",
                table: "plantas",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "longitude",
                table: "plantas",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "compartilhar_localizacao",
                table: "plantas");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "plantas");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "plantas");

            migrationBuilder.AddColumn<string>(
                name: "localizacao",
                table: "plantas",
                type: "text",
                nullable: true);
        }
    }
}
