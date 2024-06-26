﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TextCommandFramework;

#nullable disable

namespace TextCommandFramework.Migrations
{
    [DbContext(typeof(BotContext))]
    [Migration("20240325175618_ItemSelect")]
    partial class ItemSelect
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("TextCommandFramework.Models.Profile", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("CDamage")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CExpGain")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CHP")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Damage")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("DiscordId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Experience")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Fight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Hp")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Inventory")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemSelected")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Money")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProfileId")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserListId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserListId");

                    b.ToTable("Profile");
                });

            modelBuilder.Entity("TextCommandFramework.Models.UserList", b =>
                {
                    b.Property<string>("UserListId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserListId");

                    b.ToTable("List");
                });

            modelBuilder.Entity("TextCommandFramework.Models.Profile", b =>
                {
                    b.HasOne("TextCommandFramework.Models.UserList", null)
                        .WithMany("Profiles")
                        .HasForeignKey("UserListId");
                });

            modelBuilder.Entity("TextCommandFramework.Models.UserList", b =>
                {
                    b.Navigation("Profiles");
                });
#pragma warning restore 612, 618
        }
    }
}
