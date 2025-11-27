using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TheBallStores.Models
{
    public partial class TheballStoreContext : DbContext
    {
        public TheballStoreContext()
        {
        }

        public TheballStoreContext(DbContextOptions<TheballStoreContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; } = null!;
        public virtual DbSet<DonHang> DonHangs { get; set; } = null!;
        public virtual DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public virtual DbSet<KichThuoc> KichThuocs { get; set; } = null!;
        public virtual DbSet<LoaiSanPham> LoaiSanPhams { get; set; } = null!;
        public virtual DbSet<SanPham> SanPhams { get; set; } = null!;
        public virtual DbSet<SanPhamChiTiet> SanPhamChiTiets { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.HasKey(e => e.MaCt)
                    .HasName("PK__ChiTietD__27258E7446AD14B2");

                entity.Property(e => e.MaCt).HasColumnName("MaCT");

                entity.Property(e => e.DonGia).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.MaChiTietNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaChiTiet)
                    .HasConstraintName("FK__ChiTietDo__MaChi__4BAC3F29");

                entity.HasOne(d => d.MaDonHangNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaDonHang)
                    .HasConstraintName("FK__ChiTietDo__MaDon__4AB81AF0");
            });

            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.HasKey(e => e.MaDonHang)
                    .HasName("PK__DonHangs__129584AD370F3EB8");

                entity.Property(e => e.DiaChiGiaoHang).HasMaxLength(200);

                entity.Property(e => e.MaKh).HasColumnName("MaKH");

                entity.Property(e => e.NgayDat)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SoDienThoaiNguoiNhan)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.TenNguoiNhan).HasMaxLength(100);

                entity.Property(e => e.TongTien).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TrangThai)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("(N'Mới đặt')");

                entity.HasOne(d => d.MaKhNavigation)
                    .WithMany(p => p.DonHangs)
                    .HasForeignKey(d => d.MaKh)
                    .HasConstraintName("FK__DonHangs__MaKH__47DBAE45");
            });

            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.HasKey(e => e.MaKh)
                    .HasName("PK__KhachHan__2725CF1E7C01B1ED");

                entity.Property(e => e.MaKh).HasColumnName("MaKH");

                entity.Property(e => e.DiaChi).HasMaxLength(200);

                entity.Property(e => e.DienThoai)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.HoTen).HasMaxLength(100);

                entity.Property(e => e.MatKhau).HasMaxLength(100);

                entity.Property(e => e.VaiTro)
                    .HasMaxLength(20)
                    .HasDefaultValueSql("('Customer')");
            });

            modelBuilder.Entity<KichThuoc>(entity =>
            {
                entity.HasKey(e => e.MaSize)
                    .HasName("PK__KichThuo__A787E7ED598484B8");

                entity.Property(e => e.TenSize).HasMaxLength(20);
            });

            modelBuilder.Entity<LoaiSanPham>(entity =>
            {
                entity.HasKey(e => e.MaLoai)
                    .HasName("PK__LoaiSanP__730A57599D0FD8CB");

                entity.Property(e => e.NhomLoai).HasMaxLength(50);

                entity.Property(e => e.TenLoai).HasMaxLength(100);
            });

            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.HasKey(e => e.MaSp)
                    .HasName("PK__SanPhams__2725081CE765653E");

                entity.Property(e => e.MaSp).HasColumnName("MaSP");

                entity.Property(e => e.AnhDaiDien).HasMaxLength(200);

                entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TenSp)
                    .HasMaxLength(200)
                    .HasColumnName("TenSP");

                entity.HasOne(d => d.MaLoaiNavigation)
                    .WithMany(p => p.SanPhams)
                    .HasForeignKey(d => d.MaLoai)
                    .HasConstraintName("FK__SanPhams__MaLoai__3B75D760");
            });

            modelBuilder.Entity<SanPhamChiTiet>(entity =>
            {
                entity.HasKey(e => e.MaChiTiet)
                    .HasName("PK__SanPhamC__CDF0A11446FCB7A6");

                entity.Property(e => e.MaSp).HasColumnName("MaSP");

                entity.Property(e => e.SoLuongTon).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.MaSizeNavigation)
                    .WithMany(p => p.SanPhamChiTiets)
                    .HasForeignKey(d => d.MaSize)
                    .HasConstraintName("FK__SanPhamCh__MaSiz__403A8C7D");

                entity.HasOne(d => d.MaSpNavigation)
                    .WithMany(p => p.SanPhamChiTiets)
                    .HasForeignKey(d => d.MaSp)
                    .HasConstraintName("FK__SanPhamChi__MaSP__3F466844");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
