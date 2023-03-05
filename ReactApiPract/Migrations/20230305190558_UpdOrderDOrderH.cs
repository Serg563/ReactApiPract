using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactApiPract.Migrations
{
    /// <inheritdoc />
    public partial class UpdOrderDOrderH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_OrderHeaders_OrderHeaderId1",
                table: "OrderHeaders");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_OrderHeaderId1",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "OrderHeaderId1",
                table: "OrderHeaders");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderHeaderId",
                table: "OrderDetails",
                column: "OrderHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderHeaderId",
                table: "OrderDetails",
                column: "OrderHeaderId",
                principalTable: "OrderHeaders",
                principalColumn: "OrderHeaderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderHeaderId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_OrderHeaderId",
                table: "OrderDetails");

            migrationBuilder.AddColumn<int>(
                name: "OrderHeaderId1",
                table: "OrderHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_OrderHeaderId1",
                table: "OrderHeaders",
                column: "OrderHeaderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_OrderHeaders_OrderHeaderId1",
                table: "OrderHeaders",
                column: "OrderHeaderId1",
                principalTable: "OrderHeaders",
                principalColumn: "OrderHeaderId");
        }
    }
}
