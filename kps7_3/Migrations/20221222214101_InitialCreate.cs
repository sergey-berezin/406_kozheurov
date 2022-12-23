using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WpfApp1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Detail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    emota = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "emo",
                columns: table => new
                {
                    imageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    hash = table.Column<string>(type: "TEXT", nullable: false),
                    image = table.Column<string>(type: "TEXT", nullable: false),
                    DetailsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emo", x => x.imageId);
                    table.ForeignKey(
                        name: "FK_emo_Detail_DetailsId",
                        column: x => x.DetailsId,
                        principalTable: "Detail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_emo_DetailsId",
                table: "emo",
                column: "DetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emo");

            migrationBuilder.DropTable(
                name: "Detail");
        }
    }
}
