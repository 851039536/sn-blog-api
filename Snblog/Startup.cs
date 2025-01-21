using System.Net.Http;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Snblog.Enties.Validator;
using Snblog.Jwt;
using Snblog.Service.pollys;
using Snblog.Service.Service.DataBases;
using Snblog.Util.Exceptions;
using SnBlogCore.Jwt;
using Swashbuckle.AspNetCore.SwaggerUI;
using ArticleService = Snblog.Service.Service.Articles.ArticleService;

namespace Snblog;

/// <summary>
/// Startup
/// </summary>
public class Startup
{
    private IConfiguration Configuration { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="configuration"></param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// 运行时将调用此方法。 使用此方法将服务添加到容器。
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services
            .AddControllers()
            .AddNewtonsoftJson(option =>
                //忽略循环引用
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            );

        // 添加 Health Checks 服务
        _ = services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database") // 添加自定义健康检查
            .AddCheck<ExternalServiceHealthCheck>("ExternalService"); // 添加另一个自定义健康检查
        #region 限流

        _ = services.AddRateLimiter(_ =>
            _.AddFixedWindowLimiter(
                policyName: "fixed",
                options =>
                {
                    options.PermitLimit = 3; //窗口阈值，即每个窗口时间范围内，最多允许的请求个数。这里指定为最多允许4个请求。该值必须 > 0
                    options.Window = TimeSpan.FromSeconds(10); // 窗口大小，即时间长度。该值必须 > TimeSpan.Zero
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; //排队请求的处理顺序。这里设置为优先处理先来的请求
                    options.QueueLimit = 2; //当窗口请求数达到最大时，后续请求会进入排队，用于设置队列的大小（即允许几个请求在里面排队等待）
                }
            )
        );

        #endregion

        #region MiniProfiler 性能分析

        _ = services.AddMiniProfiler(options => options.RouteBasePath = "/profiler");

        #endregion

        #region Swagger服务

        _ = services.AddSwaggerGen(c =>
        {
            //遍历版本信息
            typeof(ApiVersion)
                .GetEnumNames()
                .ToList()
                .ForEach(version =>
                {
                    c.SwaggerDoc(
                        version,
                        new OpenApiInfo
                        {
                            Title = "SN Blog API", //标题
                            Description = "EFCore数据操作 ASP.NET Core Web API",
                            TermsOfService = new Uri("https://example.com/terms"), //服务条款
                            Contact = new OpenApiContact
                            {
                                Name = "Kai OuYang", //联系人
                                Email = string.Empty, //邮箱
                                Url = new Uri("https://twitter.com/spboyer"), //网站
                            },
                            License = new OpenApiLicense
                            {
                                Name = "Use under LICX", //协议
                                Url = new Uri("https://example.com/license"), //协议地址
                            }
                        }
                    );
                });

            // 使用反射获取xml文件。并构造出文件的路径
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // 获取xml文件的路径
            string filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // 启用xml注释. 该方法第二个参数启用控制器的注释，默认为false.
            c.IncludeXmlComments(filePath, true);

            // 使用反射获取xml文件。并构造出文件的路径
            string xmlModel = Path.Combine("Snblog.Enties.xml");
            // 获取xml文件的路径
            string modelPath = Path.Combine(AppContext.BaseDirectory, xmlModel);
            c.IncludeXmlComments(modelPath, true);
            // 可以解决相同类名会报错的问题
            c.CustomSchemaIds(type => type.FullName);

            #region 配置Authorization

            //Bearer 的scheme定义
            var securityScheme = new OpenApiSecurityScheme()
            {
                Description = "JWT授权(数据将在请求头中进行传输) 参数结构: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                //参数添加在头部
                In = ParameterLocation.Header,
                //使用Authorize头部
                Type = SecuritySchemeType.Http,
                //内容为以 bearer开头
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            //把所有方法配置为增加bearer头部信息
            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                    },
                    Array.Empty<string>()
                }
            };

            //注册到swagger中
            c.AddSecurityDefinition("bearerAuth", securityScheme);
            c.AddSecurityRequirement(securityRequirement);

            #endregion
        });

        #endregion

        #region 数据库连接池

        _ = services.AddDbContext<SnblogContext>(options =>
            options.UseMySQL(Configuration.GetConnectionString("MysqlConnection") ?? string.Empty)
        );

        #endregion

        #region JWT身份授权

        _ = services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true, //是否验证Issuer
                    ValidIssuer = Configuration["Jwt:Issuer"], //发行人Issuer
                    ValidateAudience = true, //是否验证Audience
                    ValidAudience = Configuration["Jwt:Audience"], //订阅人Audience
                    ValidateIssuerSigningKey = true, //是否验证SecurityKey
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"])), //SecurityKey
                    ValidateLifetime = true, //是否验证失效时间
                    ClockSkew = TimeSpan.FromSeconds(30), //过期时间容错值，解决服务器端时间不同步问题（秒）
                    RequireExpirationTime = true,
                };
            });

        #region 配置动态授权服务

        //配置授权
        _ = services.AddAuthorization(JPermissions.ConfigureAuthorizationPolicies);
        // 注册动态授权处理程序为单例
        _ = services.AddSingleton<IAuthorizationHandler, DynamicAuthorizationHandler>();

        #endregion

        //将 JwtHelper 注册为单例模式
        _ = services.AddSingleton(new JwtHelper(Configuration));
        #endregion

        #region Cors跨域请求

        _ = services.AddCors(c =>
        {
            c.AddPolicy(
                "AllRequests",
                policy =>
                {
                    _ = policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        #endregion

        #region DI依赖注入配置

        //scrutor的方式
        _ = services.Scan(selector =>
            selector
                //加载ArticleService 类所在的程序集 , 将加载ArticleService所在的程序集中所有的类都注册进去
                .FromAssemblyOf<ArticleService>()
                // 过滤程序集中需要注册的类，只选择那些类名以"Service"结尾的类型
                .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service")))
                // 暴露匹配的接口，即注册服务为其实现的接口，且接口名称与服务类名称相同（除了后缀"Service"）
                .AsMatchingInterface()
                // 注释中提到了其他几种暴露服务的方式，它们分别是：
                // .AsImplementedInterfaces() // 将服务注册为它实现的所有接口
                // .As(t => t.GetInterfaces()) // 同样是将服务注册为它实现的所有接口，但提供了更多的灵活性
                // .AsSelf() // 将服务注册为它自己，通常在没有接口的情况下使用

                // 设置服务的生命周期为Scoped，意味着在同一个请求范围内，服务实例将被重用
                .WithScopedLifetime()
        );

        _ = services.AddScoped<DataBaseSql, DataBaseSql>();
        _ = services.AddScoped<ServiceHelper, ServiceHelper>();

        //IValidator
        _ = services.AddTransient<IValidator<Article>, ArticleValidator>();
        _ = services.AddTransient<IValidator<Snippet>, SnippetValidator>();
        _ = services.AddTransient<IValidator<UserTalk>, UserTalkValidator>();
        _ = services.AddTransient<IValidator<PhotoGallery>, PhotoGalleryValidator>();

        //整个应用程序生命周期以内只创建一个实例
        _ = services.AddSingleton<ICacheManager, CacheManager>();
        _ = services.AddSingleton<CacheUtils, CacheUtils>();
        _ = services.AddSingleton<HttpClient, HttpClient>();
        _ = services.AddSingleton<RetryPolicyService, RetryPolicyService>();

        #endregion

        #region 实体映射

        //自动化注册
        _ = services.AddAutoMapper(
            Assembly.Load("Snblog.Enties").GetTypes().Where(t => t.FullName != null && t.FullName.EndsWith("Mapper")).ToArray()
        );

        #endregion

        _ = services.AddControllers();
    }

    /// <summary>
    ///   运行时将调用此方法。 使用此方法来配置HTTP请求管道。
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            //对于开发模式，一旦报错就跳转到错误堆栈页面
            _ = app.UseDeveloperExceptionPage();
        }
        else
        {
            _ = app.UseExceptionMiddleware();
        }

        // 使用 Health Checks 中间件
        app.UseHealthChecks("/health");

        _ = app.UseRateLimiter();

        #region Swagger+性能分析（MiniProfiler）+自定义页面

        _ = app.UseMiniProfiler();

        //启用Swagger中间件,可以将Swagger的UI页面配置在Configure的开发环境之中
        _ = app.UseSwagger();

        //配置SwaggerUI
        _ = app.UseSwaggerUI(c =>
        {
            typeof(ApiVersion)
                .GetEnumNames()
                .ToList()
                .ForEach(version =>
                {
                    c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Snblog.index.html");
                    ////设置首页为Swagger
                    c.RoutePrefix = string.Empty;
                    //自定义页面 集成性能分析
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
                    ////设置为none可折叠所有方法
                    c.DocExpansion(DocExpansion.None);
                    ////设置为-1 可不显示models
                    // c.DefaultModelsExpandDepth(-1);
                });
        });

        #endregion

        _ = app.UseHttpsRedirection();

        _ = app.UseRouting();

        #region 开启Cors跨域请求中间件

        _ = app.UseCors("AllRequests");

        #endregion

        #region 启用jwt

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        #endregion

        _ = app.UseEndpoints(endpoints =>
        {
            _ = endpoints.MapControllers();
        });
    }
}
