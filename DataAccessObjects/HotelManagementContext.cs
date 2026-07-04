using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class HotelManagementContext : DbContext
{
    public HotelManagementContext()
    {
    }

    public HotelManagementContext(DbContextOptions<HotelManagementContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();

    public DbSet<CheckRecord> CheckRecords => Set<CheckRecord>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(DbContextFactory.GetConnectionString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles");
            builder.HasKey(role => role.RoleId);

            builder.Property(role => role.RoleId)
                .HasColumnName("role_id");

            builder.Property(role => role.Name)
                .HasColumnName("role_name")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.HasMany(role => role.Users)
                .WithOne(user => user.Role)
                .HasForeignKey(user => user.RoleId);
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(user => user.UserId);

            builder.Property(user => user.UserId)
                .HasColumnName("user_id");

            builder.Property(user => user.RoleId)
                .HasColumnName("role_id");

            builder.Property(user => user.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(user => user.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(user => user.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(user => user.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(user => user.PhoneNumber)
                .HasColumnName("phone_number")
                .HasMaxLength(20);

            builder.Property(user => user.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(user => user.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(user => user.UpdatedAt)
                .HasColumnName("updated_at");
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.ToTable("customers");
            builder.HasKey(c => c.CustomerId);
            builder.Property(c => c.CustomerId).HasColumnName("customer_id");
            builder.Property(c => c.FullName).HasColumnName("full_name").IsRequired();
            builder.Property(c => c.IdentityCard).HasColumnName("identity_card");
            builder.Property(c => c.PhoneNumber).HasColumnName("phone_number");
            builder.Property(c => c.Email).HasColumnName("email");
            builder.Property(c => c.Address).HasColumnName("address");
            builder.Property(c => c.CreatedAt).HasColumnName("created_at");
            builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        });

        // RoomType configuration
        modelBuilder.Entity<RoomType>(builder =>
        {
            builder.ToTable("room_types");
            builder.HasKey(rt => rt.RoomTypeId);
            builder.Property(rt => rt.RoomTypeId).HasColumnName("room_type_id");
            builder.Property(rt => rt.TypeName).HasColumnName("type_name").IsRequired();
            builder.Property(rt => rt.Description).HasColumnName("description");
            builder.Property(rt => rt.BasePrice).HasColumnName("base_price").HasPrecision(18, 2);
            builder.Property(rt => rt.Capacity).HasColumnName("capacity");
            builder.Property(rt => rt.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(rt => rt.CreatedAt).HasColumnName("created_at");
            builder.Property(rt => rt.UpdatedAt).HasColumnName("updated_at");
        });

        // Room configuration
        modelBuilder.Entity<Room>(builder =>
        {
            builder.ToTable("rooms");
            builder.HasKey(r => r.RoomId);
            builder.Property(r => r.RoomId).HasColumnName("room_id");
            builder.Property(r => r.RoomTypeId).HasColumnName("room_type_id");
            builder.Property(r => r.RoomNumber).HasColumnName("room_number").IsRequired();
            builder.Property(r => r.Floor).HasColumnName("floor");
            builder.Property(r => r.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(r => r.Note).HasColumnName("note");
            builder.Property(r => r.CreatedAt).HasColumnName("created_at");
            builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            builder.HasOne(r => r.RoomType);
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(builder =>
        {
            builder.ToTable("bookings");
            builder.HasKey(b => b.BookingId);
            builder.Property(b => b.BookingId).HasColumnName("booking_id");
            builder.Property(b => b.CustomerId).HasColumnName("customer_id");
            builder.Property(b => b.CreatedByUserId).HasColumnName("created_by_user_id");
            builder.Property(b => b.BookingDate).HasColumnName("booking_date");
            builder.Property(b => b.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(b => b.Note).HasColumnName("note");
            builder.Property(b => b.CreatedAt).HasColumnName("created_at");
            builder.Property(b => b.UpdatedAt).HasColumnName("updated_at");
        });

        // BookingDetail configuration
        modelBuilder.Entity<BookingDetail>(builder =>
        {
            builder.ToTable("booking_details");
            builder.HasKey(bd => bd.BookingDetailId);
            builder.Property(bd => bd.BookingDetailId).HasColumnName("booking_detail_id");
            builder.Property(bd => bd.BookingId).HasColumnName("booking_id");
            builder.Property(bd => bd.RoomId).HasColumnName("room_id");
            builder.Property(bd => bd.CheckInDate).HasColumnName("check_in_date");
            builder.Property(bd => bd.CheckOutDate).HasColumnName("check_out_date");
            builder.Property(bd => bd.RoomPrice).HasColumnName("room_price").HasPrecision(18, 2);
            builder.Property(bd => bd.NumberOfNights).HasColumnName("number_of_nights");
            builder.Property(bd => bd.RoomTotal).HasColumnName("room_total").HasPrecision(18, 2);
            builder.Property(bd => bd.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(bd => bd.Note).HasColumnName("note");
            builder.Property(bd => bd.CreatedAt).HasColumnName("created_at");
            builder.Property(bd => bd.UpdatedAt).HasColumnName("updated_at");
        });

        // CheckRecord configuration
        modelBuilder.Entity<CheckRecord>(builder =>
        {
            builder.ToTable("check_records");
            builder.HasKey(cr => cr.CheckRecordId);
            builder.Property(cr => cr.CheckRecordId).HasColumnName("check_record_id");
            builder.Property(cr => cr.BookingDetailId).HasColumnName("booking_detail_id");
            builder.Property(cr => cr.CheckInByUserId).HasColumnName("check_in_by_user_id");
            builder.Property(cr => cr.CheckOutByUserId).HasColumnName("check_out_by_user_id");
            builder.Property(cr => cr.ActualCheckInDate).HasColumnName("actual_check_in_date");
            builder.Property(cr => cr.ActualCheckOutDate).HasColumnName("actual_check_out_date");
            builder.Property(cr => cr.CheckInNote).HasColumnName("check_in_note");
            builder.Property(cr => cr.CheckOutNote).HasColumnName("check_out_note");
            builder.Property(cr => cr.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(cr => cr.CreatedAt).HasColumnName("created_at");
            builder.Property(cr => cr.UpdatedAt).HasColumnName("updated_at");
        });

        // Service configuration
        modelBuilder.Entity<Service>(builder =>
        {
            builder.ToTable("services");
            builder.HasKey(s => s.ServiceId);
            builder.Property(s => s.ServiceId).HasColumnName("service_id");
            builder.Property(s => s.ServiceName).HasColumnName("service_name").IsRequired();
            builder.Property(s => s.Category).HasColumnName("category").IsRequired();
            builder.Property(s => s.Price).HasColumnName("price").HasPrecision(18, 2);
            builder.Property(s => s.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(s => s.CreatedAt).HasColumnName("created_at");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        });

        // ServiceOrder configuration
        modelBuilder.Entity<ServiceOrder>(builder =>
        {
            builder.ToTable("service_orders");
            builder.HasKey(so => so.ServiceOrderId);
            builder.Property(so => so.ServiceOrderId).HasColumnName("service_order_id");
            builder.Property(so => so.BookingDetailId).HasColumnName("booking_detail_id");
            builder.Property(so => so.ServiceId).HasColumnName("service_id");
            builder.Property(so => so.CreatedByUserId).HasColumnName("created_by_user_id");
            builder.Property(so => so.Quantity).HasColumnName("quantity");
            builder.Property(so => so.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
            builder.Property(so => so.TotalPrice).HasColumnName("total_price").HasPrecision(18, 2);
            builder.Property(so => so.OrderDate).HasColumnName("order_date");
            builder.Property(so => so.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(so => so.Note).HasColumnName("note");
            builder.Property(so => so.CreatedAt).HasColumnName("created_at");
            builder.Property(so => so.UpdatedAt).HasColumnName("updated_at");
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(builder =>
        {
            builder.ToTable("invoices");
            builder.HasKey(i => i.InvoiceId);
            builder.Property(i => i.InvoiceId).HasColumnName("invoice_id");
            builder.Property(i => i.BookingId).HasColumnName("booking_id");
            builder.Property(i => i.CreatedByUserId).HasColumnName("created_by_user_id");
            builder.Property(i => i.RoomAmount).HasColumnName("room_amount").HasPrecision(18, 2);
            builder.Property(i => i.ServiceAmount).HasColumnName("service_amount").HasPrecision(18, 2);
            builder.Property(i => i.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2);
            builder.Property(i => i.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2);
            builder.Property(i => i.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
            builder.Property(i => i.PaidAmount).HasColumnName("paid_amount").HasPrecision(18, 2);
            builder.Property(i => i.RemainingAmount).HasColumnName("remaining_amount").HasPrecision(18, 2);
            builder.Property(i => i.CreateDate).HasColumnName("create_date");
            builder.Property(i => i.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(i => i.Note).HasColumnName("note");
            builder.Property(i => i.CreatedAt).HasColumnName("created_at");
            builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(builder =>
        {
            builder.ToTable("payments");
            builder.HasKey(p => p.PaymentId);
            builder.Property(p => p.PaymentId).HasColumnName("payment_id");
            builder.Property(p => p.InvoiceId).HasColumnName("invoice_id");
            builder.Property(p => p.ReceivedByUserId).HasColumnName("received_by_user_id");
            builder.Property(p => p.PaymentMethod).HasColumnName("payment_method").HasConversion<string>();
            builder.Property(p => p.Amount).HasColumnName("amount").HasPrecision(18, 2);
            builder.Property(p => p.PaymentDate).HasColumnName("payment_date");
            builder.Property(p => p.TransactionCode).HasColumnName("transaction_code");
            builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(p => p.Note).HasColumnName("note");
            builder.Property(p => p.CreatedAt).HasColumnName("created_at");
            builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
