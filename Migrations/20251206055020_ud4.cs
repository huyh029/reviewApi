using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reviewApi.Migrations
{
    /// <inheritdoc />
    public partial class ud4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CriteriaSetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ManagerId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PeriodMonth = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PeriodYear = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_CriteriaSets_CriteriaSetId",
                        column: x => x.CriteriaSetId,
                        principalTable: "CriteriaSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EvaluationId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Message = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ReplyToChatId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    SenderId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationChats_EvaluationChats_ReplyToChatId",
                        column: x => x.ReplyToChatId,
                        principalTable: "EvaluationChats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvaluationChats_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationChats_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationChatFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EvaluationChatId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FilePath = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    FileName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationChatFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationChatFiles_EvaluationChats_EvaluationChatId",
                        column: x => x.EvaluationChatId,
                        principalTable: "EvaluationChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChatFiles_EvaluationChatId",
                table: "EvaluationChatFiles",
                column: "EvaluationChatId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChats_EvaluationId",
                table: "EvaluationChats",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChats_ReplyToChatId",
                table: "EvaluationChats",
                column: "ReplyToChatId",
                unique: true,
                filter: "\"ReplyToChatId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationChats_SenderId",
                table: "EvaluationChats",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_CriteriaSetId",
                table: "Evaluations",
                column: "CriteriaSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ManagerId",
                table: "Evaluations",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_UserId",
                table: "Evaluations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationChatFiles");

            migrationBuilder.DropTable(
                name: "EvaluationChats");

            migrationBuilder.DropTable(
                name: "Evaluations");
        }
    }
}
