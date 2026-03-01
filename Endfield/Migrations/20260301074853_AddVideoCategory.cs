using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endfield.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "category_id",
                table: "bilibili_videos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "video_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_categories", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_bilibili_videos_category_id",
                table: "bilibili_videos",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_video_categories_code",
                table: "video_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_video_categories_sort_order",
                table: "video_categories",
                column: "sort_order");

            migrationBuilder.AddForeignKey(
                name: "FK_bilibili_videos_video_categories_category_id",
                table: "bilibili_videos",
                column: "category_id",
                principalTable: "video_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bilibili_videos_video_categories_category_id",
                table: "bilibili_videos");

            migrationBuilder.DropTable(
                name: "video_categories");

            migrationBuilder.DropIndex(
                name: "IX_bilibili_videos_category_id",
                table: "bilibili_videos");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "bilibili_videos");
        }
    }
}
