using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCore.Data.ViewModels;
using NetCore.Services.Interfaces;
using NetCore.Services.Svcs;
using NetCore.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCore.Controllers
{
    public class MembershipController : Controller
    {
        //의존성 주입 - 생성자 
        private IUser _user;
        private HttpContext _context;
        //생성자
        public MembershipController(IHttpContextAccessor accessor, IUser user)
        {
            _context = accessor.HttpContext;
            _user = user;
        }
        public IActionResult Index()
        {
            return View();
        }
        //Get방식으로 접근했을때 보여지는 view페이지를 위한 메서드 3.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /*아이디, 비번 입력해 로그인 눌리면 이곳으로 온다    3.
        위조방지 토큰을 통해 View로 받은 Post data가 유효한지 검증
        [HttpPost] 방식 Action메서드 일 때 지정 해 줘야한다 	
        */
        //Login메서드를 비동기로 14.
        [HttpPost("/{controller}/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(LoginInfo login)
        {
            string message = string.Empty;

            if (ModelState.IsValid)
            {

                //뷰모델
                //서비스 개념
                if (_user.MatchTheUserInfo(login))
                {
                    //신원보증과 승인권한
                    var userInfo = _user.GetUserInfo(login.UserId);
                    var roles = _user.GetRolesOwnedByUser(login.UserId);
                    var userTopRole = roles.FirstOrDefault();

                    var identity = new ClaimsIdentity(claims:new[]
                    {
                    new Claim(type:ClaimTypes.Name,
                              value:userInfo.UserName),
                    new Claim(type:ClaimTypes.Role,
                              value:userTopRole.RoleId + "|" + userTopRole.UserRole.RoleName + "|" + userTopRole.UserRole.RolePriority.ToString())
                    }, authenticationType:CookieAuthenticationDefaults.AuthenticationScheme);

                    await _context.SignInAsync(scheme:CookieAuthenticationDefaults.AuthenticationScheme,
                                               principal:new ClaimsPrincipal(identity: identity),
                                               properties:new AuthenticationProperties()
                                               { 
                                                   IsPersistent =login.RememberMe,
                                                   ExpiresUtc =login.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(30)
                                               });

                    

                    TempData["Message"] = "로그인이 성공적으로 이루어졌습니다.";

                    return RedirectToAction("Index", "Membership");
                }
                else
                {
                    message = "로그인되지 않았습니다.";
                }

            }
            else
            {
                message = "로그인정보를 올바르게 입력하세요";
            }
            ModelState.AddModelError(string.Empty, message);
            return View("Login", login);
        }

        //로그아웃 비동기 14.
        [HttpGet("/LogOut")]
        public async Task<IActionResult> LogOutAsync()
        {
            await _context.SignOutAsync(scheme: CookieAuthenticationDefaults.AuthenticationScheme);
            
            TempData["Message"] = "로그아웃이 성공적으로 이루어졌습니다. <br />웹사이트를 원활히 이용하시려면 로그인하세요.";

            return RedirectToAction("index", "Membership");
        }
    }
}