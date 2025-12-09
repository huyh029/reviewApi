using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Criterias_Criterias_parentId",
                table: "Criterias");

            migrationBuilder.AlterColumn<int>(
                name: "parentId",
                table: "Criterias",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AddForeignKey(
                name: "FK_Criterias_Criterias_parentId",
                table: "Criterias",
                column: "parentId",
                principalTable: "Criterias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Criterias_Criterias_parentId",
                table: "Criterias");

            migrationBuilder.AlterColumn<int>(
                name: "parentId",
                table: "Criterias",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Criterias_Criterias_parentId",
                table: "Criterias",
                column: "parentId",
                principalTable: "Criterias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
