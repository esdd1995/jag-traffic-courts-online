﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TrafficCourts.Workflow.Service.Sagas;

#nullable disable

namespace TrafficCourts.Workflow.Service.Migrations.VerifyEmailAddressStateMigrations
{
    [DbContext(typeof(VerifyEmailAddressStateDbContext))]
    [Migration("20240423152745_allow-null")]
    partial class allownull
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TrafficCourts.Workflow.Service.Sagas.VerifyEmailAddressState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasMaxLength(64)
                        .HasColumnType("uuid");

                    b.Property<string>("CurrentState")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<long?>("DisputeId")
                        .HasColumnType("bigint");

                    b.Property<string>("EmailAddress")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("IsUpdateEmailVerification")
                        .HasColumnType("boolean");

                    b.Property<string>("TicketNumber")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<Guid>("Token")
                        .HasColumnType("uuid");

                    b.Property<bool>("Verified")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("VerifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("CorrelationId");

                    b.ToTable("VerifyEmailAddressState");
                });
#pragma warning restore 612, 618
        }
    }
}
