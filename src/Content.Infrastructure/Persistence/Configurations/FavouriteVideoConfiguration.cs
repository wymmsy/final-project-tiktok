using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokFeed.Content.Domain.Entities;

namespace TikTokFeed.Content.Infrastructure.Persistence.Configurations;

public class FavouriteVideoConfiguration : IEntityTypeConfiguration<FavouriteVideo>
{
    public void Configure(EntityTypeBuilder<FavouriteVideo> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(f => f.Id);
        builder.HasIndex(f => new { f.UserId, f.VideoId }).IsUnique();

        builder.Property(f => f.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
