using Microsoft.EntityFrameworkCore;
using NetCore.Data.Classes;
//using NetCore.Data.DataModels;
using NetCore.Data.ViewModels;
using NetCore.Services.Data;
using NetCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Services.Svcs
{
    public class UserService : IUser
    {
        //의존성 주입 추가
        private DBFirstDbContext _context;

        public UserService(DBFirstDbContext context)
        {
            _context = context;
        }

        #region private method
        private IEnumerable<User> GetUserInfos()
        {
            return _context.Users.ToList();
            //return new List<User>()
            //{
            //    new User()
            //    {
            //        UserId = "jadejs",
            //        UserName = "김정수",
            //        UserEmail = "jadejskim@gmail.com",
            //        Password = "123456"

            //    }
            //};
        }

        //메서드 추가 (EFCore FromSqlRaw() ),     데이터 단일건 
        //파라메타로 아이디와 비번을 직접 입력받는다. => 데이터 베이스에 있는 사용자 정보를 가져온다!
        private User GetUserInfo(string userId, string password)
        {
            User user;

            //1.Lambda 식
            //파라미터 값과 비교
            //FirstOrDefault(); 한건의 데이터만 가져온다
            //람다식을 통해서 Users테이블리스트 형태의 개체에 where절을 통해서 람다식으로 아이디와 비번을 비교해서 값을 도출
            //user = _context.Users.Where(u => u.UserId.Equals(userId) && u.Password.Equals(password)).FirstOrDefault();

            //2.FromSqlRaw
            //쿼리문을 직접 입력을 해서 가져올수 있다

            //2.1TABLE
            //user = _context.Users.FromSqlRaw("SELECT UserId, UserName, UserEmail, Password, IsMembershipWithdrawn, JoinedUtcDate From dbo.[User]")
            //                    .Where(u => u.UserId.Equals(userId) && u.Password.Equals(password))
            //                    .FirstOrDefault();            

            //2.2VIEW  Table이랑 비슷한테 From테이블쪽 [User] => uvwUser 수정
            //user = _context.Users.FromSqlRaw("SELECT UserId, UserName, UserEmail, Password, IsMembershipWithdrawn, JoinedUtcDate From dbo.uvwUser")
            //                    .Where(u => u.UserId.Equals(userId) && u.Password.Equals(password))
            //                    .FirstOrDefault();

            //2.3FUNCTION
            //FUNCTION와 STORED PROCEDURE은 파라미터 지정가능! where절을 따로 안쓴다(람다식으로 받는 것은 안한다) 
            //user = _context.Users.FromSqlInterpolated($"SELECT UserId, UserName, UserEmail, Password, IsMembershipWithdrawn, JoinedUtcDate FROM dbo.ufnUser({userId},{password})")
            //                     .FirstOrDefault();
            
            //2.4STORED PROCEDURE
            //FromSqlRaw()메서드 뒤에 .AsEnumerable() 메서드를 추가
            //파라메터 @p3 등 추가 가능
            //user = _context.Users.FromSqlRaw("dbo.uspCheckLoginByUserId @p0, @p1", new[] { userId, password })
            //                        .AsEnumerable().FirstOrDefault();       
            user = _context.Users.FromSqlInterpolated($"dbo.uspCheckLoginByUserId {userId}, {password}")
                                  .AsEnumerable().FirstOrDefault();

            //사용자 정보가 없을 경우(12.)
            //비밀번호가 틀려야 이 쪽으로 넘어온다
            if (user == null)
            {
                //접속실패횟수에 대한 증가
                int rowAffected;

                //SQL문 직접 작성 12.(1. 쿼리 문으로도)
                //ExecuteSqlInterpolated는 테이블리스트(Users)를 받아오는 것이 아닌
                //Database에서 직접 연결되는 메서드!
                //rowAffected = _context.Database.ExecuteSqlInterpolated($"Update dbo.[User] SET AccessFailedCount += 1 WHERE UserId={userId}");

                //STORED PROCEDURE 12.(2. 프로시져로)
                //rowAffected = _context.Database.ExecuteSqlRaw("dbo.FailedLoginByUserId @p0", parameters: new[] { userId });
                rowAffected = _context.Database.ExecuteSqlInterpolated($"dbo.FailedLoginByUserId {userId}");
            }

            return user;
        }

        //checkTheUserInfo()에 아이디와 비번 입력 받은 것을 GetUserInfos()의 리스트에서 
        //람다식으로 데이터가 있으면 T 없으면 F 나온다
        private bool checkTheUserInfo(string userId, string password)
        {
            //Any() : 리스트 형태에서만 사용가능
            //return GetUserInfos().Where(u => u.UserId.Equals(userId) && u.Password.Equals(password)).Any();
            return GetUserInfo(userId, password) != null ? true : false;
        }

        //추가 14.
        private User GetUserInfo(string userId)
        {
            return _context.Users.Where(u => u.UserId.Equals(userId)).FirstOrDefault();
        }

        private IEnumerable<UserRolesByUser> GetUserRolesByUserInfos(string userId)
        {
            var userRoleByUserInfos = _context.UserRolesByUsers.Where(uru => uru.UserId.Equals(userId)).ToList();

            foreach(var role in userRoleByUserInfos)
            {
                role.UserRole = GetUserRole(role.RoleId);
            }
            return userRoleByUserInfos.OrderByDescending(uru => uru.UserRole.RolePriority);
        }

        private UserRole GetUserRole(string roleId)
        {
            return _context.UserRoles.Where(ur => ur.RoleId.Equals(roleId)).FirstOrDefault();
        }
        #endregion

        bool IUser.MatchTheUserInfo(LoginInfo login)
        {
             return checkTheUserInfo(login.UserId, login.Password);
        }

        //명시적 14.
        User IUser.GetUserInfo(string userId)
        {
            return GetUserInfo(userId);
        }

        IEnumerable<UserRolesByUser> IUser.GetRolesOwnedByUser(string userId)
        {
            return GetUserRolesByUserInfos(userId);
        }
    }
}
