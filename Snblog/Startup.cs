using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Snblog.Cache.Cache;
using Snblog.Enties.Validator;
using Snblog.IRepository;
using Snblog.IService;
using Snblog.IService.IReService;
using Snblog.Jwt;
using Snblog.Service;
using Snblog.Service.AngleSharp;
using Snblog.Service.ReService;
using Snblog.Service.Service;
using Snblog.Util.Exceptions;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Snblog
{
    public class Startup
    {

        #region �汾����ö��
        /// <summary>
        /// �汾����
        /// </summary>
        public enum ApiVersion
        {
            /// <summary>
            /// v1�汾
            /// </summary>
            V1 = 1,
            /// <summary>
            /// v2�汾
            /// </summary>
            V2 = 2,
            /// <summary>
            /// AngleSharp
            /// </summary>
            AngleSharp = 3
        }
        #endregion

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //����ʱ�����ô˷����� ʹ�ô˷�����������ӵ�������
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(option =>
              //����ѭ������
              option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
          );
            #region MiniProfiler ���ܷ���
            services.AddMiniProfiler(options =>
            options.RouteBasePath = "/profiler"
             );
            #endregion

            #region Swagger����
            services.AddSwaggerGen(c => {
                // ����ĵ���Ϣ
                //�����汾��Ϣ
                typeof(ApiVersion).GetEnumNames().ToList().ForEach(version => {
                    c.SwaggerDoc(version,new OpenApiInfo {
                        Title = "SN blog API", //����
                        Description = "EFCore���ݲ��� ASP.NET Core Web API", //����
                        TermsOfService = new Uri("https://example.com/terms"), //��������
                        Contact = new OpenApiContact {
                            Name = "kai ouyang", //��ϵ��
                            Email = string.Empty,  //����
                            Url = new Uri("https://twitter.com/spboyer"),//��վ
                        },
                        License = new OpenApiLicense {
                            Name = "Use under LICX", //Э��
                            Url = new Uri("https://example.com/license"), //Э���ַ
                        }
                    });
                });

                // ʹ�÷����ȡxml�ļ�����������ļ���·��
                var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlpath = Path.Combine(AppContext.BaseDirectory,xmlfile);
                // ����xmlע��. �÷����ڶ����������ÿ�������ע�ͣ�Ĭ��Ϊfalse.
                c.IncludeXmlComments(xmlpath,true);
                //Model Ҳ���ע��˵��
                var xmlpath1 = Path.Combine("Snblog.Enties.xml");
                var xmlpath2 = Path.Combine(AppContext.BaseDirectory,xmlpath1);
                c.IncludeXmlComments(xmlpath2,true);
                c.CustomSchemaIds(type => type.FullName);// ���Խ����ͬ�����ᱨ�������

                #region ����Authorization
                //Bearer ��scheme����
                var securityScheme = new OpenApiSecurityScheme() {
                    Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) �����ṹ: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    //���������ͷ��
                    In = ParameterLocation.Header,
                    //ʹ��Authorizeͷ��
                    Type = SecuritySchemeType.Http,
                    //����Ϊ�� bearer��ͷ
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                //�����з�������Ϊ����bearerͷ����Ϣ
                var securityRequirement = new OpenApiSecurityRequirement
              {
                    {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "bearerAuth"
                                }
                            },
                            Array.Empty<string>()
                    }
                };

                //ע�ᵽswagger��
                c.AddSecurityDefinition("bearerAuth",securityScheme);
                c.AddSecurityRequirement(securityRequirement);
                #endregion

            });

            #endregion

            #region ���ݿ����ӳ�

            services.AddDbContext<snblogContext>(
                options => options
                .UseMySQL(Configuration.GetConnectionString("DefaultConnection")
                ));
            #endregion

            #region JWT�����Ȩ
            services.ConfigureJwt(Configuration);
            //ע��JWT�����ļ�
            services.Configure<JwtConfig>(Configuration.GetSection("Authentication:JwtBearer"));
            #endregion

            #region Cors��������
            services.AddCors(c => {
                c.AddPolicy("AllRequests",policy => {
                    policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();

                });
            });
            #endregion

            #region DI����ע�����á�
            // ��ASP.NET Core�������õ�EF��Service ����Ҫע���Scoped
            services.AddScoped<IRepositoryFactory,RepositoryFactory>();//���͹���
            services.AddScoped<IConcardContext,snblogContext>();//db
            services.AddScoped<IArticleService,ArticleService>();//ioc
            services.AddScoped<ISnNavigationService,SnNavigationService>();
            services.AddScoped<IArticleTagService,ArticleTagService>();
            services.AddScoped<IArticleTypeService,ArticleTypeService>();
            services.AddScoped<IDiaryService,DiaryService>();
            services.AddScoped<IVideoService,VideoService>();
            services.AddScoped<ISnVideoTypeService,SnVideoTypeService>();
            services.AddScoped<ISnUserTalkService,SnUserTalkService>();
            services.AddScoped<IUserService,UserService>();
            services.AddScoped<IDiaryTypeService,DiaryTypeService>();
            services.AddScoped<ISnPictureService,SnPictureService>();
            services.AddScoped<ISnPictureTypeService,SnPictureTypeService>();
            services.AddScoped<ISnTalkService,SnTalkService>();
            services.AddScoped<ISnTalkTypeService,SnTalkTypeService>();
            services.AddScoped<ISnNavigationTypeService,SnNavigationTypeService>();
            services.AddScoped<ISnleaveService,SnleaveService>();
            services.AddScoped<ISnNavigationTypeService,SnNavigationTypeService>();
            services.AddScoped<IInterfaceService,InterfaceService>();
            services.AddScoped<ISnSetBlogService,SnSetBlogService>();
            services.AddScoped<ISnippetService,SnippetService>();
            services.AddScoped<ISnippetTagService,SnippetTagService>();
            services.AddScoped<ISnippetTypeService,SnippetTypeService>();
            services.AddScoped<ISnippetLabelService,SnippetLabelService>();
            services.AddScoped<IReSnArticleService,ReSnArticleService>();
            services.AddScoped<IReSnNavigationService,ReSnNavigationService>();
            services.AddScoped<HotNewsAngleSharp,HotNewsAngleSharp>();
            services.AddTransient<IValidator<Article>,ArticleValidator>();
            //����Ӧ�ó���������������ֻ����һ��ʵ�� 
            services.AddSingleton<ICacheManager,CacheManager>();
             services.AddSingleton<ICacheUtil,CacheUtil>();
              
            #endregion

            #region ʵ��ӳ��

            //services.AddAutoMapper(typeof(MappingProfile));

            //�Զ���ע��
            services.AddAutoMapper(
               Assembly.Load("Snblog.Enties").GetTypes()
                   .Where(t => t.FullName.EndsWith("Mapper"))
                   .ToArray()
           );
            #endregion

            services.AddControllers();

        }


        /// <summary>
        ///   ����ʱ�����ô˷����� ʹ�ô˷���������HTTP����ܵ���
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app,IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                //���ڿ���ģʽ��һ���������ת�������ջҳ��
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionMiddleware();
            }

            #region Swagger+���ܷ�����MiniProfiler��+�Զ���ҳ��

            //����UseMiniProfiler
            app.UseMiniProfiler();
            //���Խ�Swagger��UIҳ��������Configure�Ŀ�������֮��
            // ����Swagger�м��
            app.UseSwagger();

            //����SwaggerUI
            app.UseSwaggerUI(c => {
                typeof(ApiVersion).GetEnumNames().ToList().ForEach(version => {
                    c.IndexStream = () => GetType().GetTypeInfo()
                         .Assembly.GetManifestResourceStream("Snblog.index.html");
                    ////������ҳΪSwagger
                    c.RoutePrefix = string.Empty;
                    //�Զ���ҳ�� �������ܷ���
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json",version);
                    ////����Ϊnone���۵����з���
                    c.DocExpansion(DocExpansion.None);
                    ////����Ϊ-1 �ɲ���ʾmodels
                    // c.DefaultModelsExpandDepth(-1);
                });
            });
            #endregion
            app.UseHttpsRedirection();
            app.UseRouting();
            #region ����Cors���������м��
            app.UseCors("AllRequests");
            #endregion
            #region ����jwt
            app.UseAuthentication();
            app.UseAuthorization();
            #endregion
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
