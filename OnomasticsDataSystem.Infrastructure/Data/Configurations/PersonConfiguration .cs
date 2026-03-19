using OnomasticsDataSystem.Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnomasticsDataSystem.Infrastructure.Data.Configurations
{
	public class PersonConfiguration : IEntityTypeConfiguration<Person>
	{
		public void Configure(EntityTypeBuilder<Person> builder)
		{
			builder.ToTable("person");

			builder.HasKey(p => p.Id);

			builder.Property(p => p.Id)
				.HasColumnName("id");

			builder.Property(p => p.FirstName)
				.HasColumnName("first_name")
				.IsRequired();

			builder.Property(p => p.LastName)
				.HasColumnName("last_name")
				.IsRequired();

			builder.Property(p => p.FirstNameNormalized)
				.HasColumnName("first_name_norm");

			builder.Property(p => p.LastNameNormalized)
				.HasColumnName("last_name_norm");

			builder.Property(p => p.CreatedAt)
				.HasColumnName("created_at")
				.HasDefaultValueSql("now()");
			builder.Property(p => p.BirthYear).HasColumnName("birth_year");
			builder.Property(p => p.BirthCity).HasColumnName("birth_city");
			builder.Property(p => p.BirthCityNormalized).HasColumnName("birth_city_norm");
			builder.Property(p => p.SourceId)
					.HasColumnName("source_id");

			builder.HasOne(p => p.Source)
				   .WithMany(s => s.Persons)
				   .HasForeignKey(p => p.SourceId)
				   .OnDelete(DeleteBehavior.Restrict);
			builder.HasIndex(p => p.FirstNameNormalized);

			builder.HasIndex(p => p.LastNameNormalized);

			builder.HasIndex(p => p.BirthCityNormalized);

			builder.HasIndex(p => p.BirthYear);
			builder.HasIndex(p => new
			{
				p.FirstNameNormalized,
				p.LastNameNormalized,
				p.BirthYear,
				p.BirthCity
			}).IsUnique();
		}
	}
}
