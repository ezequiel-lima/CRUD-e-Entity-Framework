using Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace Blog.Data.Mappings
{
    internal class PostMap : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Post");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            builder.Property(x => x.LastUpdateDate)
                .IsRequired()
                .HasColumnName("LastUpdateDate")
                .HasColumnType("SMALLDATETIME")
                .HasDefaultValueSql("GETDATE()");
            //.HasDefaultValue(DateTime.Now.ToUniversalTime());

            //Índices
            builder
                .HasIndex(x => x.Slug, "IX_Post_Slug")
                .IsUnique();

            // Relacionamentos
            // 1 x N
            builder
                .HasOne(x => x.Author) // Um Autor tem muitos Posts
                .WithMany(x => x.Posts)
                .HasConstraintName("FK_Post_Author")
                .OnDelete(DeleteBehavior.Cascade); // Quando deletado algum post vai deletar o author também 

            builder
                .HasOne(x => x.Category) 
                .WithMany(x => x.Posts)
                .HasConstraintName("FK_Post_Category")
                .OnDelete(DeleteBehavior.Cascade);

            // N x N
            builder
                .HasMany(x => x.Tags) // Tem muitas tags com muitos posts
                .WithMany(x => x.Posts)
                .UsingEntity<Dictionary<string, object>>( // gera uma entidade virtual, baseada em um dicionario que recebe dois valores uma string e um objeto
                    "PostTag", // essa é a string recebida
                    post => post.HasOne<Tag>() // post tem uma tag
                        .WithMany() // essa tag tem varios post
                        .HasForeignKey("PostId")
                        .HasConstraintName("FK_PostTag_PostId")
                        .OnDelete(DeleteBehavior.Cascade),
                    tag => tag.HasOne<Post>()
                        .WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK_PostTag_TagId")
                        .OnDelete(DeleteBehavior.Cascade)); // esse é o objeto recebido desde "post =>..."
        }
    }
}
