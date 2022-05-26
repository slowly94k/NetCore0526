using Microsoft.EntityFrameworkCore;
using NetCore.Data.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Services.Data
{
    //상속
    //DBFirstDbContext : 자식클래스
    //DbContext : 부모클래스
    public class DBFirstDbContext : DbContext
    {
        //생성자 상솝
        public DBFirstDbContext(DbContextOptions<DBFirstDbContext> options) : base(options)
        {

        }

        //DB테이블 리스트 지정 (DBFirstDBEM.Context에서 복사해옴)
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserRolesByUser> UserRolesByUsers { get; set; }

        //메서드 상속,  부모클래스 DbContext에서 OnModelCreating 메서드가 virtual키워드로 지정되어 있어야만 상속 가능
        //OnModelCreating가 DbContext에 있는데, 그것을 상속 받아온다
        //메서드에는 override를 사용
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //DB 테이블이름 (단수로)변경, 매핑을 연결해서 "//DB테이블 리스트 지정"의 리스트들을 사용할 수 있도록
            modelBuilder.Entity<User>().ToTable(name: "User");
            modelBuilder.Entity<UserRole>().ToTable(name: "UserRole");
            modelBuilder.Entity<UserRolesByUser>().ToTable(name: "UserRolesByUser");

            //복합키 지정
            modelBuilder.Entity<UserRolesByUser>().HasKey(c => new { c.UserId, c.RoleId });
        }

    }
}
