using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLTours.Models;

public partial class QuanLyTourContext : DbContext
{
    public QuanLyTourContext()
    {
    }

    public QuanLyTourContext(DbContextOptions<QuanLyTourContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<DetailItinerary> DetailItineraries { get; set; }

    public virtual DbSet<Hotel> Hotels { get; set; }

    public virtual DbSet<Itinerary> Itineraries { get; set; }

    public virtual DbSet<ItineraryImage> ItineraryImages { get; set; }

    public virtual DbSet<Manage> Manages { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourDetail> TourDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }
    public object ResetPasswordViewModels { get; internal set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
         => optionsBuilder.UseSqlServer("Data Source=DESKTOP-UE3LPFV\\LOCAL;Initial Catalog=QLTour1;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__5DE3A5B1925070C7");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.BookingDate)
                .HasColumnType("datetime")
                .HasColumnName("booking_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total");
            entity.Property(e => e.TourId).HasColumnName("tour_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Tour).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK__Bookings__tour_i__4CA06362");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Bookings__user_i__4D94879B");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D54EE9B4E6B110DD");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contact");

            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Message)
                .HasMaxLength(200)
                .HasColumnName("message");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(200)
                .HasColumnName("subject");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<DetailItinerary>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__DetailIt__135C316DD49D7EE9");

            entity.ToTable("DetailItinerary");

            entity.HasOne(d => d.Itinerary).WithMany(p => p.DetailItineraries)
                .HasForeignKey(d => d.ItineraryId)
                .HasConstraintName("FK_DetailItinerary_Itinerary");
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.HotelId).HasName("PK__Hotels__45FE7E265D5B9B68");

            entity.Property(e => e.HotelId).HasColumnName("hotel_id");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.HotelName)
                .HasMaxLength(100)
                .HasColumnName("hotel_name");
        });

        modelBuilder.Entity<Itinerary>(entity =>
        {
            entity.HasKey(e => e.ItineraryId).HasName("PK__Itinerar__361216C6C8B33D29");

            entity.ToTable("Itinerary");

            entity.Property(e => e.TourId).HasColumnName("tour_id");

            entity.HasOne(d => d.Tour).WithMany(p => p.Itineraries)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK_Itinerary_Tours");
        });

        modelBuilder.Entity<ItineraryImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Itinerar__7516F70CAA019E0E");

            entity.Property(e => e.ImagePath).HasMaxLength(255);

            entity.HasOne(d => d.Itinerary).WithMany(p => p.ItineraryImages)
                .HasForeignKey(d => d.ItineraryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItineraryImages_Itinerary");
        });

        modelBuilder.Entity<Manage>(entity =>
        {
            entity.HasKey(e => e.IdMng);

            entity.ToTable("Manage");

            entity.Property(e => e.IdMng).HasColumnName("id_Mng");
            entity.Property(e => e.Password)
                .HasMaxLength(250)
                .HasColumnName("password");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK_Promotion");

            entity.Property(e => e.Code).HasMaxLength(200);
            entity.Property(e => e.Discount).HasColumnType("decimal(6, 3)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__60883D901486A816");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewText).HasColumnName("review_text");
            entity.Property(e => e.TourId).HasColumnName("tour_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Tour).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK__Reviews__tour_id__5070F446");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Reviews__user_id__5165187F");
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.TourId).HasName("PK__Tours__4B16B9E6CC2D7032");

            entity.Property(e => e.TourId).HasColumnName("tour_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.Img).HasColumnName("img");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.TourName)
                .HasMaxLength(100)
                .HasColumnName("tour_name");

            entity.HasOne(d => d.Category).WithMany(p => p.Tours)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Tours__category___5535A963");
        });

        modelBuilder.Entity<TourDetail>(entity =>
        {
            entity.HasKey(e => e.TourDetailId).HasName("PK__TourDeta__08A6F6FE03AEB1A1");

            entity.Property(e => e.TourDetailId).HasColumnName("tour_detail_id");
            entity.Property(e => e.HotelId).HasColumnName("hotel_id");
            entity.Property(e => e.TourId).HasColumnName("tour_id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Hotel).WithMany(p => p.TourDetails)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK__TourDetai__hotel__52593CB8");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourDetails)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK__TourDetai__tour___534D60F1");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TourDetails)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__TourDetai__vehic__5441852A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F79D1ABD3");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__F2947BC16F3CD1CC");

            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.VehicleName)
                .HasMaxLength(100)
                .HasColumnName("vehicle_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}