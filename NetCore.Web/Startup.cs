using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.Services.Data;
using NetCore.Services.Interfaces;
using NetCore.Services.Svcs;
using NetCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            Common.SetDataProtection(services, @"C:\hwfile\vs2019\z0516\zxc\NetCore\DataProtector\", "NetCore", Enums.CryptoType.CngCbc);

            //의존성 주입을 사용하기 위해서 서비스로 등록
            //껍데기
            //IUser 인터페이스에 UserService 클래스 인스턴스 주입
            services.AddScoped<IUser, UserService>();

            //추가 14.
            services.AddHttpContextAccessor();

            //DBFirst DB접속정보만
            services.AddDbContext<DBFirstDbContext>(options =>
                options.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "DBFirstDBConnection")));

            //MVC 패턴을 사용하기 위해서 서비스로 등록
            services.AddMvc();
            
            //services.AddControllersWithViews(); 원래 있던거

            //신원보증과 승인권한 14
            services.AddAuthentication(defaultScheme: CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.AccessDeniedPath = "/Membership/Forbidden";
                        options.LoginPath = "/Membership/Login";
                    });

            services.AddAuthorization();            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            /*
            app.UseRouting(), app.UseAuthentication(), app.UseAuthorization(),
            app.UseSession(), app.UseEndpoints()
            이렇게 5개의 메서드는 반드시 순서를 지켜야 올바로 작동함.
            */


            // 아래의 app.UseEndpoints()메서드를 라우팅과 연결하기 위해 사용됨.
            app.UseRouting();

            //신원보증만 14.
            app.UseAuthentication();

            // 권한을 승인하기 위해 메서드가 추가됨.
            app.UseAuthorization();

            ////강의내용
            //세션 지정
            //System.InvalidOperationException:
            //'Session has not been configured for this application or request.'
            //app.UseSession();

            // .Net Core 2.1의 UseMvc()에서 다음과 같이 메서드명이 변경됨. 
            app.UseEndpoints(endpoints =>
            {
                // .Net Core 2.1의 UseMvc()에서 다음과 같이 메서드명이 변경됨.
                endpoints.MapControllerRoute(
                    name: "default",
                    // .Net Core 2.1의 template에서 다음과 같이 파라미터명이 변경됨.
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
