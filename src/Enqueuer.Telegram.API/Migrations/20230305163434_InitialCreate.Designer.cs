﻿// <auto-generated />
using System;
using Enqueuer.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Enqueuer.Telegram.API.Migrations
{
    [DbContext(typeof(EnqueuerContext))]
    [Migration("20230305163434_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Enqueuer.Persistence.Models.GroupChat", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.ToTable("GroupChats");
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.Queue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<long?>("CreatorId")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDynamic")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("CreatorId");

                    b.ToTable("Queues");
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.QueueMember", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<int>("QueueId")
                        .HasColumnType("int");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.HasKey("UserId", "QueueId");

                    b.HasIndex("QueueId");

                    b.ToTable("QueueMember");
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("LastName")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GroupChatUser", b =>
                {
                    b.Property<long>("GroupsId")
                        .HasColumnType("bigint");

                    b.Property<long>("MembersId")
                        .HasColumnType("bigint");

                    b.HasKey("GroupsId", "MembersId");

                    b.HasIndex("MembersId");

                    b.ToTable("GroupChatUser");
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.Queue", b =>
                {
                    b.HasOne("Enqueuer.Persistence.Models.GroupChat", "Chat")
                        .WithMany("Queues")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Enqueuer.Persistence.Models.User", "Creator")
                        .WithMany("CreatedQueues")
                        .HasForeignKey("CreatorId");

                    b.Navigation("Chat");

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.QueueMember", b =>
                {
                    b.HasOne("Enqueuer.Persistence.Models.Queue", "Queue")
                        .WithMany("Members")
                        .HasForeignKey("QueueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Enqueuer.Persistence.Models.User", "User")
                        .WithMany("ParticipatesIn")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Queue");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GroupChatUser", b =>
                {
                    b.HasOne("Enqueuer.Persistence.Models.GroupChat", null)
                        .WithMany()
                        .HasForeignKey("GroupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Enqueuer.Persistence.Models.User", null)
                        .WithMany()
                        .HasForeignKey("MembersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.GroupChat", b =>
                {
                    b.Navigation("Queues");
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.Queue", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("Enqueuer.Persistence.Models.User", b =>
                {
                    b.Navigation("CreatedQueues");

                    b.Navigation("ParticipatesIn");
                });
#pragma warning restore 612, 618
        }
    }
}
