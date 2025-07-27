using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<AdminStaff> AdminStaff { get; set; }
        public DbSet<PetOwner> PetOwners { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Veterinarian> Veterinarians { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<AnimalSpecies> AnimalSpecies { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<MedicalConsultation> MedicalConsultations { get; set; }
        public DbSet<VitalSign> VitalSigns { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<VaccinationHistory> VaccinationHistories { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación AppUser -> UserRole
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación 1:1 AppUser -> AdminStaff
            modelBuilder.Entity<AdminStaff>()
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<AdminStaff>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Datos iniciales
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { RoleId = 1, RoleName = "Admin", Description = "Administrador del sistema", IsActive = true },
                new UserRole { RoleId = 2, RoleName = "Veterinarian", Description = "Veterinario", IsActive = true },
                new UserRole { RoleId = 3, RoleName = "PetOwner", Description = "Dueño de mascota", IsActive = true }
            );

            // Índices únicos
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Relación 1:1 AppUser -> PetOwner
            modelBuilder.Entity<PetOwner>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<PetOwner>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict); // No Action es una alternativa

            // Índices para PetOwner (opcional)
            modelBuilder.Entity<PetOwner>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Relación 1:1 AppUser -> Veterinarian
            modelBuilder.Entity<Veterinarian>()
                .HasOne(v => v.User)
                .WithOne()
                .HasForeignKey<Veterinarian>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Veterinarian -> Specialty
            modelBuilder.Entity<Veterinarian>()
                .HasOne(v => v.Specialty)
                .WithMany(s => s.Veterinarians)
                .HasForeignKey(v => v.SpecialtyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Datos iniciales de especialidades
            modelBuilder.Entity<Specialty>().HasData(
                new Specialty { SpecialtyId = 1, SpecialtyName = "Cirugía General", Description = "Cirugías generales en animales", IsActive = true },
                new Specialty { SpecialtyId = 2, SpecialtyName = "Dermatología", Description = "Especialidad en problemas de piel", IsActive = true },
                new Specialty { SpecialtyId = 3, SpecialtyName = "Cardiología", Description = "Especialidad en corazón y sistema circulatorio", IsActive = true },
                new Specialty { SpecialtyId = 4, SpecialtyName = "Oftalmología", Description = "Especialidad en ojos y visión", IsActive = true },
                new Specialty { SpecialtyId = 5, SpecialtyName = "Odontología", Description = "Especialidad en salud dental", IsActive = true }
            );

            // Relación PetOwner -> Pet (1:N)
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.PetOwner)
                .WithMany(po => po.Pets)
                .HasForeignKey(p => p.PetOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación AnimalSpecies -> Pet (1:N)
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Species)
                .WithMany(s => s.Pets)
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Breed -> Pet (1:N)
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Breed)
                .WithMany(b => b.Pets)
                .HasForeignKey(p => p.BreedId)
                .OnDelete(DeleteBehavior.SetNull);

            // Datos iniciales para especies
            modelBuilder.Entity<AnimalSpecies>().HasData(
                new AnimalSpecies { SpeciesId = 1, SpeciesName = "Perro", Description = "Canino", IsActive = true },
                new AnimalSpecies { SpeciesId = 2, SpeciesName = "Gato", Description = "Felino", IsActive = true },
                new AnimalSpecies { SpeciesId = 3, SpeciesName = "Conejo", Description = "Lagomorfo", IsActive = true }
            );

            // Datos iniciales para razas (opcional)
            modelBuilder.Entity<Breed>().HasData(
                new Breed { BreedId = 1, SpeciesId = 1, BreedName = "Labrador Retriever", Description = "Perro grande y amigable", IsActive = true },
                new Breed { BreedId = 2, SpeciesId = 1, BreedName = "Pastor Alemán", Description = "Perro de trabajo inteligente", IsActive = true },
                new Breed { BreedId = 3, SpeciesId = 2, BreedName = "Siamés", Description = "Gato vocal y sociable", IsActive = true }
            );

            modelBuilder.Entity<Medication>(entity =>
            {
                entity.HasIndex(m => m.MedicationName).IsUnique();
                entity.Property(m => m.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.HasIndex(mr => mr.RecordNumber).IsUnique();

                entity.HasOne(mr => mr.Pet)
                      .WithMany(p => p.MedicalRecords)
                      .HasForeignKey(mr => mr.PetId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(mr => mr.CreationDate)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<MedicalConsultation>(entity =>
            {
                entity.HasOne(c => c.MedicalRecord)
                      .WithMany(mr => mr.MedicalConsultations)
                      .HasForeignKey(c => c.MedicalRecordId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Cambia esta configuración:
                entity.HasOne(c => c.AttendingVeterinarian)  // Usa el nuevo nombre
                      .WithMany(v => v.MedicalConsultations)
                      .HasForeignKey(c => c.VeterinarianId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(c => c.ConsultationDate)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<VaccinationHistory>(entity =>
            {
                entity.HasOne(v => v.Pet)
                    .WithMany(p => p.VaccinationHistories)
                    .HasForeignKey(v => v.PetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Vaccine)
                    .WithMany(v => v.VaccinationHistories)
                    .HasForeignKey(v => v.VaccineId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Veterinarian)
                    .WithMany(v => v.VaccinationHistories)
                    .HasForeignKey(v => v.VeterinarianId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Pet)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Veterinarian)
                    .WithMany(v => v.Appointments)
                    .HasForeignKey(a => a.VeterinarianId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.AppointmentDate)
                    .IsRequired();

                entity.Property(a => a.AppointmentType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(a => a.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Scheduled");
            });
        }
    }
}
