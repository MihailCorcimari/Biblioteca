using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class StoreReaderProfileImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Readers");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfileImage",
                table: "Readers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageContentType",
                table: "Readers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Readers");

            migrationBuilder.DropColumn(
                name: "ProfileImageContentType",
                table: "Readers");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Readers",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }
    }
}
