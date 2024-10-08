﻿// <auto-generated />
using System;
using BackendChat.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BackendChat.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240823170002_Add tables for chat implementation")]
    partial class Addtablesforchatimplementation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BackendChat.Models.AppUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DOB")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("EmailConfirmationToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailConfirmed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ProfilePictureUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Nickname")
                        .IsUnique();

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("BackendChat.Models.Chat", b =>
                {
                    b.Property<int>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ChatId"));

                    b.Property<string>("ChatName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ChatTypeId")
                        .HasColumnType("int");

                    b.Property<int?>("CreatedByUserId")
                        .HasColumnType("int");

                    b.HasKey("ChatId");

                    b.HasIndex("ChatTypeId");

                    b.HasIndex("CreatedByUserId");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("BackendChat.Models.ChatMessage", b =>
                {
                    b.Property<int>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageId"));

                    b.Property<int>("ChatId")
                        .HasColumnType("int");

                    b.Property<string>("MediaUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("MessageId");

                    b.HasIndex("ChatId");

                    b.HasIndex("UserId");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("BackendChat.Models.ChatParticipant", b =>
                {
                    b.Property<int>("ChatId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("ChatId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ChatParticipants");
                });

            modelBuilder.Entity("BackendChat.Models.ChatType", b =>
                {
                    b.Property<int>("ChatTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ChatTypeId"));

                    b.Property<string>("ChatTypeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ChatTypeId");

                    b.ToTable("ChatTypes");
                });

            modelBuilder.Entity("BackendChat.Models.Chat", b =>
                {
                    b.HasOne("BackendChat.Models.ChatType", "ChatType")
                        .WithMany("Chats")
                        .HasForeignKey("ChatTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BackendChat.Models.AppUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedByUserId");

                    b.Navigation("ChatType");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("BackendChat.Models.ChatMessage", b =>
                {
                    b.HasOne("BackendChat.Models.Chat", "Chat")
                        .WithMany("ChatMessages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BackendChat.Models.AppUser", "User")
                        .WithMany("ChatMessages")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BackendChat.Models.ChatParticipant", b =>
                {
                    b.HasOne("BackendChat.Models.Chat", "Chat")
                        .WithMany("ChatParticipants")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BackendChat.Models.AppUser", "User")
                        .WithMany("ChatParticipants")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BackendChat.Models.AppUser", b =>
                {
                    b.Navigation("ChatMessages");

                    b.Navigation("ChatParticipants");
                });

            modelBuilder.Entity("BackendChat.Models.Chat", b =>
                {
                    b.Navigation("ChatMessages");

                    b.Navigation("ChatParticipants");
                });

            modelBuilder.Entity("BackendChat.Models.ChatType", b =>
                {
                    b.Navigation("Chats");
                });
#pragma warning restore 612, 618
        }
    }
}
