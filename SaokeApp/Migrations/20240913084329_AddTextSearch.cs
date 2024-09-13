using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace SaokeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlText = File.ReadAllText("./sql/generate_full_text_search_vietnamese.sql");
            migrationBuilder.Sql(sqlText);
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "DonateTracks",
                type: "tsvector",
                nullable: false)
                .Annotation("Npgsql:TsVectorConfig", "vietnamese")
                .Annotation("Npgsql:TsVectorProperties", new[] { "TransactionId", "Amount", "Message" });

            migrationBuilder.CreateIndex(
                name: "IX_DonateTracks_SearchVector",
                table: "DonateTracks",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DonateTracks_SearchVector",
                table: "DonateTracks");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "DonateTracks");
        }
    }
}
