using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantaCoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeysPostSaveShareViewActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_post_views_post_id",
                table: "post_views",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_shares_post_id",
                table: "post_shares",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_saves_post_id",
                table: "post_saves",
                column: "post_id");

            migrationBuilder.AddForeignKey(
                name: "fk_activitylog_usuario",
                table: "activity_logs",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_postsave_post",
                table: "post_saves",
                column: "post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_postsave_usuario",
                table: "post_saves",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_postshare_post",
                table: "post_shares",
                column: "post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_postshare_usuario",
                table: "post_shares",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_postview_post",
                table: "post_views",
                column: "post_id",
                principalTable: "posts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_postview_usuario",
                table: "post_views",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_activitylog_usuario",
                table: "activity_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_postsave_post",
                table: "post_saves");

            migrationBuilder.DropForeignKey(
                name: "fk_postsave_usuario",
                table: "post_saves");

            migrationBuilder.DropForeignKey(
                name: "fk_postshare_post",
                table: "post_shares");

            migrationBuilder.DropForeignKey(
                name: "fk_postshare_usuario",
                table: "post_shares");

            migrationBuilder.DropForeignKey(
                name: "fk_postview_post",
                table: "post_views");

            migrationBuilder.DropForeignKey(
                name: "fk_postview_usuario",
                table: "post_views");

            migrationBuilder.DropIndex(
                name: "IX_post_views_post_id",
                table: "post_views");

            migrationBuilder.DropIndex(
                name: "IX_post_shares_post_id",
                table: "post_shares");

            migrationBuilder.DropIndex(
                name: "IX_post_saves_post_id",
                table: "post_saves");
        }
    }
}
