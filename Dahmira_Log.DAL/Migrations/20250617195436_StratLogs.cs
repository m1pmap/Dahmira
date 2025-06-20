using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dahmira_Log.DAL.Migrations
{
    /// <inheritdoc />
    public partial class StratLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lvl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryPoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ex_HResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ex_GetType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ex_Mesage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ex_InnerExceptionMesage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ex_StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Log");
        }
    }
}
