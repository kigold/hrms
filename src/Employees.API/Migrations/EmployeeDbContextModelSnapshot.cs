﻿// <auto-generated />
using System;
using Employees.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Employees.API.Migrations
{
    [DbContext(typeof(EmployeeDbContext))]
    partial class EmployeeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Employees.API.Data.Models.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("LogoFileId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LogoFileId");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("Employees.API.Data.Models.Employee", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StaffId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("Employees.API.Data.Models.Qualification", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("DateReceived")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EducationLevel")
                        .HasColumnType("int");

                    b.Property<long>("EmployeeId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("MediaFileId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("QualificationType")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("MediaFileId");

                    b.ToTable("Qualifications");
                });

            modelBuilder.Entity("Shared.Data.MediaFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Mimetype")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Employees.API.Data.Models.Company", b =>
                {
                    b.HasOne("Shared.Data.MediaFile", "LogoFile")
                        .WithMany()
                        .HasForeignKey("LogoFileId");

                    b.Navigation("LogoFile");
                });

            modelBuilder.Entity("Employees.API.Data.Models.Employee", b =>
                {
                    b.HasOne("Employees.API.Data.Models.Company", "Company")
                        .WithMany("Employees")
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("Employees.API.Data.Models.Qualification", b =>
                {
                    b.HasOne("Employees.API.Data.Models.Employee", "Employee")
                        .WithMany("Qualifications")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Shared.Data.MediaFile", "MediaFile")
                        .WithMany()
                        .HasForeignKey("MediaFileId");

                    b.Navigation("Employee");

                    b.Navigation("MediaFile");
                });

            modelBuilder.Entity("Employees.API.Data.Models.Company", b =>
                {
                    b.Navigation("Employees");
                });

            modelBuilder.Entity("Employees.API.Data.Models.Employee", b =>
                {
                    b.Navigation("Qualifications");
                });
#pragma warning restore 612, 618
        }
    }
}