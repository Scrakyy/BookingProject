using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FernandoTan_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Bookings");
        }
    }
}
