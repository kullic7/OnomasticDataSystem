using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnomasticsDataSystem.Core.Entity;

namespace OnomasticsDataSystem.Infrastructure.Data.Configurations
{
	public class SourceConfiguration : IEntityTypeConfiguration<Source>
	{
		public void Configure(EntityTypeBuilder<Source> builder)
		{
			builder.ToTable("source");

			builder.HasKey(s => s.Id);

			builder.Property(s => s.Url)
				.HasColumnName("url")
				.IsRequired();

			builder.Property(s => s.Name)
				.HasColumnName("name")
				.IsRequired();

			builder.Property(s => s.CreatedAt)
				.HasColumnName("created_at")
				.HasDefaultValueSql("now()");
		}
	}
}
