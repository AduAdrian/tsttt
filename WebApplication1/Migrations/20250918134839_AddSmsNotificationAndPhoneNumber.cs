using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsNotificationAndPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Clients",
                type: "TEXT",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SmsNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ScheduledFor = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpiryDateSnapshot = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsNotifications_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SmsNotifications_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_PhoneNumber",
                table: "Clients",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotification_Client_Type_ExpirySnapshot",
                table: "SmsNotifications",
                columns: new[] { "ClientId", "Type", "ExpiryDateSnapshot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_ClientId",
                table: "SmsNotifications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_CreatedAt",
                table: "SmsNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_CreatedByUserId",
                table: "SmsNotifications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_ScheduledFor",
                table: "SmsNotifications",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_Status",
                table: "SmsNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_Type",
                table: "SmsNotifications",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Clients_PhoneNumber",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Clients");
        }
    }
}
