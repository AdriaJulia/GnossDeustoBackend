﻿// <auto-generated />
using System;
using API_CARGA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace API_CARGA.Migrations
{
    [DbContext(typeof(EntityContext))]
    partial class EntityContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("API_CARGA.Models.Entities.DiscoverItem", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("DiscoverRdf")
                        .HasColumnType("text");

                    b.Property<string>("DiscoverReport")
                        .HasColumnType("text");

                    b.Property<bool>("DissambiguationProcessed")
                        .HasColumnType("boolean");

                    b.Property<string>("Error")
                        .HasColumnType("text");

                    b.Property<string>("JobID")
                        .HasColumnType("text");

                    b.Property<bool>("Publish")
                        .HasColumnType("boolean");

                    b.Property<string>("Rdf")
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("DiscoverItem");
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.DiscoverItem+DiscoverDissambiguation", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DiscoverItemID")
                        .HasColumnType("uuid");

                    b.Property<string>("IDOrigin")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("DiscoverItemID");

                    b.ToTable("DiscoverDissambiguation");
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.DiscoverItem+DiscoverDissambiguation+DiscoverDissambiguationCandiate", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DiscoverDissambiguationID")
                        .HasColumnType("uuid");

                    b.Property<string>("IDCandidate")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float>("Score")
                        .HasColumnType("real");

                    b.HasKey("ID");

                    b.HasIndex("DiscoverDissambiguationID");

                    b.ToTable("DiscoverDissambiguationCandiate");
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.ProcessingJobState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("JobId")
                        .HasColumnType("text");

                    b.Property<string>("LastIdentifierOAIPMH")
                        .HasColumnType("text");

                    b.Property<int>("ProcessNumIdentifierOAIPMH")
                        .HasColumnType("integer");

                    b.Property<Guid>("RepositoryId")
                        .HasColumnType("uuid");

                    b.Property<int>("TotalNumIdentifierOAIPMH")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("JobId")
                        .IsUnique();

                    b.ToTable("ProcessingJobState");
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.RepositoryConfig", b =>
                {
                    b.Property<Guid>("RepositoryConfigID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OauthToken")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("RepositoryConfigID");

                    b.ToTable("RepositoryConfig");
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.RepositorySync", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("RepositoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("Set")
                        .HasColumnType("text");

                    b.Property<DateTime>("UltimaFechaDeSincronizacion")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("Sincronizacion_Repositorio");
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.ShapeConfig", b =>
                {
                    b.Property<Guid>("ShapeConfigID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("RepositoryID")
                        .HasColumnType("uuid");

                    b.Property<string>("Shape")
                        .HasColumnType("text");

                    b.HasKey("ShapeConfigID");

                    b.HasIndex("RepositoryID");

                    b.ToTable("ShapeConfig");
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.DiscoverItem+DiscoverDissambiguation", b =>
                {
                    b.HasOne("API_CARGA.Models.Entities.DiscoverItem", null)
                        .WithMany("DissambiguationProblems")
                        .HasForeignKey("DiscoverItemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.DiscoverItem+DiscoverDissambiguation+DiscoverDissambiguationCandiate", b =>
                {
                    b.HasOne("API_CARGA.Models.Entities.DiscoverItem+DiscoverDissambiguation", null)
                        .WithMany("DissambiguationCandiates")
                        .HasForeignKey("DiscoverDissambiguationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("API_CARGA.Models.Entities.ShapeConfig", b =>
                {
                    b.HasOne("API_CARGA.Models.Entities.RepositoryConfig", null)
                        .WithMany("ShapeConfig")
                        .HasForeignKey("RepositoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
