using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluationScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EvaluationId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CriteriaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SelfScore = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ManagerScore = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationScores_Criterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "Criterias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationScores_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationScores_CriteriaId",
                table: "EvaluationScores",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationScores_EvaluationId",
                table: "EvaluationScores",
                column: "EvaluationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationScores");
        }
    }
}
