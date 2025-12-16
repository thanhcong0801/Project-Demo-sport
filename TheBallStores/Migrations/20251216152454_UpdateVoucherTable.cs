using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheBallStores.Migrations
{
    public partial class UpdateVoucherTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chỉ giữ lại lệnh tạo bảng Voucher
            migrationBuilder.CreateTable(
                name: "Voucher",
                columns: table => new
                {
                    MaVoucher = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenChuongTrinh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LoaiGiamGia = table.Column<int>(type: "int", nullable: false),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    GiamToiDa = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    DonToiThieu = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime", nullable: false),
                    SoLuongConLai = table.Column<int>(type: "int", nullable: false),
                    KichHoat = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voucher", x => x.MaVoucher);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Chỉ xóa bảng Voucher khi revert
            migrationBuilder.DropTable(
                name: "Voucher");

            
        }
    }
}