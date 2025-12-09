using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriteriaSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CriteriaSetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ShortName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Min = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Max = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classifications_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Criterias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CriteriaSetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MaxScore = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TypeScore = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    parentId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criterias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Criterias_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Criterias_Criterias_parentId",
                        column: x => x.parentId,
                        principalTable: "Criterias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteriaSetObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CriteriaSetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EvaluationObjectCode = table.Column<string>(type: "NVARCHAR2(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSetObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaSetObjects_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CriteriaSetObjects_EvaluationObjects_EvaluationObjectCode",
                        column: x => x.EvaluationObjectCode,
                        principalTable: "EvaluationObjects",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteriaSetPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CriteriaSetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    month = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    year = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaSetPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaSetPeriods_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classifications_CriteriaSetId",
                table: "Classifications",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Criterias_CriteriaSetId",
                table: "Criterias",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Criterias_parentId",
                table: "Criterias",
                column: "parentId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetObjects_CriteriaSetId",
                table: "CriteriaSetObjects",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetObjects_EvaluationObjectCode",
                table: "CriteriaSetObjects",
                column: "EvaluationObjectCode");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaSetPeriods_CriteriaSetId",
                table: "CriteriaSetPeriods",
                column: "CriteriaSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Classifications");

            migrationBuilder.DropTable(
                name: "Criterias");

            migrationBuilder.DropTable(
                name: "CriteriaSetObjects");

            migrationBuilder.DropTable(
                name: "CriteriaSetPeriods");

            migrationBuilder.DropTable(
                name: "CriteriaSets");
        }
    }
}
