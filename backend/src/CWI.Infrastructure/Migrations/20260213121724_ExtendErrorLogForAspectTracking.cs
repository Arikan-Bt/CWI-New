using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CWI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendErrorLogForAspectTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "ErrorLogs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                table: "ErrorLogs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionType",
                table: "ErrorLogs",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MachineName",
                table: "ErrorLogs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParameterName",
                table: "ErrorLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestBodyMasked",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RequestContentLength",
                table: "ErrorLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestContentType",
                table: "ErrorLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHeaders",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestQuery",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestRouteValues",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "ErrorLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResolvedByUserId",
                table: "ErrorLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Target",
                table: "ErrorLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceId",
                table: "ErrorLogs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ErrorLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_IsResolved_OccurredAt",
                table: "ErrorLogs",
                columns: new[] { "IsResolved", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_OccurredAt",
                table: "ErrorLogs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_TraceId",
                table: "ErrorLogs",
                column: "TraceId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_UserId_OccurredAt",
                table: "ErrorLogs",
                columns: new[] { "UserId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_IsResolved_OccurredAt",
                table: "ErrorLogs");

            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_OccurredAt",
                table: "ErrorLogs");

            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_TraceId",
                table: "ErrorLogs");

            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_UserId_OccurredAt",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "ExceptionType",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "MachineName",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "ParameterName",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "RequestBodyMasked",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "RequestContentLength",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "RequestContentType",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "RequestHeaders",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "RequestQuery",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "RequestRouteValues",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "ResolvedByUserId",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "TraceId",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ErrorLogs");
        }
    }
}
