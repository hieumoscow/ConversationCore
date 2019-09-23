using BeautifulRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BeautifulRestApi
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            

        }

        public DbSet<ConversationEntity> Conversations { get; set; }


        public override int SaveChanges(){
            var ret = base.SaveChanges();
            return ret;
        }

        public DbSet<CommentEntity> Comments { get; set; }
    }
}
