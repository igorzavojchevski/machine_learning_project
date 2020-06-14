using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using ML.Core;
using ML.Domain.DataModels;
using ML.Infrastructure.DependecyResolution;
using ML.Utils.Extensions.Base;

namespace ML.Web
{
    public class Startup
    {
        private readonly string _tensorFlowModelFilePath;

        private readonly ITransformer _mlnetModel;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;



            //
            //change this part for multiple instances due to multiple models
            //Configure the ML.NET model for the pre-trained TensorFlow model.
            _tensorFlowModelFilePath = BaseExtensions.GetPath(Configuration["MLModel:TensorFlowModelFilePath"], Configuration.GetValue<bool>("MLModel:IsAbsolute"));
            TensorFlowModelConfigurator tensorFlowModelConfigurator = new TensorFlowModelConfigurator(_tensorFlowModelFilePath);
            _mlnetModel = tensorFlowModelConfigurator.Model;
            //
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //TO-DO - Not needed
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent 
                //for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc(opt=>opt.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddInternalServices();
            services.AddExternalService(_mlnetModel); //change this to instance pipelines for multiple models

            //services.AddSingleton<IConfiguration>(Configuration);

            //// Register the PredictionEnginePool as a service in the IoC container for DI.
            //services.AddPredictionEnginePool<ImageInputData, ImageLabelPredictions>();
            //services
            //    .AddOptions<PredictionEnginePoolOptions<ImageInputData, ImageLabelPredictions>>()
            //    .Configure(options => { options.ModelLoader = new InMemoryModelLoader(_mlnetModel); });
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseRouting();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
