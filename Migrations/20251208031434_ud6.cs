using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TypeScore",
                table: "Criterias",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TypeScore",
                table: "Criterias",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");
        }
    }
}
