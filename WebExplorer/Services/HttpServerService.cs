using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using WebExplorer.Controllers;
using WebExplorer.Services;

namespace WebExplorer.Services
{
    /// <summary>
    /// HTTP 服务器管理服务
    /// </summary>
    public class HttpServerService : IDisposable
    {
        private readonly LogService _logger;
        private WebApplication? _app;
        private CancellationTokenSource? _cts;
        private bool _disposed;
        private int _port = 8080;
        private Task? _runTask;

        public event Action<string>? OnServerStarted;
        public event Action<string>? OnServerStopped;
        public bool IsRunning => _app != null && _runTask?.IsCompleted == false;

        public HttpServerService(LogService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 启动 HTTP 服务器
        /// </summary>
        public async Task StartAsync(int port = 8080)
        {
            if (IsRunning)
            {
                _logger.Warn("服务器已在运行中");
                return;
            }

            _port = port;
            _cts = new CancellationTokenSource();

            try
            {
                var builder = WebApplication.CreateBuilder();
                ConfigureServices(builder, port, _logger);
                _app = builder.Build();
                ConfigurePipeline(_app);

                // 启动服务器并获取显示地址
                var address = await TryStartAndGetAddress(port);

                _logger.Info($"========================================");
                _logger.Info($"  服务器已启动");
                _logger.Info($"  访问地址: {address}");
                _logger.Info($"========================================");

                OnServerStarted?.Invoke(address);
            }
            catch (Exception ex)
            {
                _logger.Error($"启动服务器失败: {ex.Message}");
                throw; // 向上传播，让调用方（MainForm）能捕获并更新 UI
            }
        }

        /// <summary>
        /// 配置依赖注入、Kestrel、CORS
        /// </summary>
        private static void ConfigureServices(WebApplicationBuilder builder, int port, LogService logger)
        {
            builder.Services.AddControllers();
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });
            builder.Services.AddSingleton(logger);
            builder.Services.AddSingleton<FileService>();
            builder.Services.AddSingleton<UploadService>();
            builder.Services.AddSingleton<DownloadService>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any, port, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                });
                options.Limits.MaxRequestBodySize = long.MaxValue;
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
                options.Limits.MaxConcurrentConnections = 100;
                options.Limits.MaxConcurrentUpgradedConnections = 100;
                options.Limits.MinRequestBodyDataRate = null;
                options.Limits.MinResponseDataRate = null;
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
        }

        /// <summary>
        /// 配置中间件管道：响应压缩 → CORS → 日志 → 静态文件 → API 路由 → SPA fallback
        /// </summary>
        private void ConfigurePipeline(WebApplication app)
        {
            app.UseResponseCompression();
            app.UseCors();

            // 请求日志
            app.Use(async (context, next) =>
            {
                _logger.Debug($"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
                await next();
            });

            // 静态文件中间件 - 从嵌入资源提供
            ConfigureStaticFiles(app);

            // API 路由
            app.MapControllers();

            // SPA fallback - 所有未匹配路由返回 index.html
            app.MapFallback(async context =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                const string resourceName = "WebExplorer.wwwroot.index.html";

                await using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                    await stream.CopyToAsync(context.Response.Body);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not Found");
                }
            });
        }

        /// <summary>
        /// 异步启动服务器，等待启动完成，检查是否成功，返回可显示的访问地址
        /// </summary>
        private async Task<string> TryStartAndGetAddress(int port)
        {
            _runTask = RunAppAsync(_app!, _cts!.Token);

            // 等待服务器启动并检查是否成功
            await Task.Delay(500);

            if (_runTask.IsFaulted || _runTask.IsCanceled)
            {
                var ex = _runTask.Exception?.InnerException ?? new Exception("服务器启动失败");
                _app = null;
                _cts = null;
                _runTask = null;
                throw new InvalidOperationException($"端口 {port} 启动失败: {ex.Message}", ex);
            }

            var addresses = _app!.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
            var rawAddress = addresses?.FirstOrDefault(a => !a.Contains("[::]")) ?? $"http://localhost:{port}";
            return SettingsService.ResolveDisplayAddress(rawAddress, port);
        }

        private async Task RunAppAsync(WebApplication app, CancellationToken token)
        {
            try
            {
                await app.RunAsync(token);
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                _logger.Error($"服务器运行异常: {ex.Message}");
                throw; // 重新抛出，使 _runTask.IsFaulted = true，让 StartAsync 能检测到启动失败
            }
        }

        /// <summary>
        /// 停止 HTTP 服务器
        /// </summary>
        public async Task StopAsync()
        {
            if (!IsRunning)
            {
                _logger.Warn("服务器未在运行");
                return;
            }

            try
            {
                _cts?.Cancel();
                if (_runTask != null)
                {
                    await Task.WhenAny(_runTask, Task.Delay(5000));
                }

                _logger.Info("服务器已停止");
                OnServerStopped?.Invoke("服务器已停止");
            }
            catch (Exception ex)
            {
                _logger.Error($"停止服务器失败: {ex.Message}");
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _app = null;
                _runTask = null;
            }
        }

        /// <summary>
        /// 配置静态文件服务（从嵌入资源）
        /// </summary>
        private void ConfigureStaticFiles(WebApplication app)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith("WebExplorer.wwwroot.") && n != "WebExplorer.wwwroot.index.html")
                .ToList();

            app.MapGet("/static/{**path}", async context =>
            {
                var path = context.Request.Path.Value?.Replace("/static/", "") ?? "";
                var resourceName = $"WebExplorer.wwwroot.{path.Replace("/", ".")}";

                // 尝试精确匹配
                var stream = assembly.GetManifestResourceStream(resourceName);

                // 如果没找到，尝试其他格式
                if (stream == null)
                {
                    foreach (var name in resourceNames)
                    {
                        if (name.EndsWith(path.Replace("/", "."), StringComparison.OrdinalIgnoreCase))
                        {
                            stream = assembly.GetManifestResourceStream(name);
                            resourceName = name;
                            break;
                        }
                    }
                }

                if (stream == null)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                var contentType = GetContentType(resourceName);
                context.Response.ContentType = contentType;
                context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                await stream.CopyToAsync(context.Response.Body);
            });
        }

        /// <summary>
        /// 根据资源名称获取 Content-Type
        /// </summary>
        private static string GetContentType(string resourceName)
        {
            if (resourceName.EndsWith(".css")) return "text/css; charset=utf-8";
            if (resourceName.EndsWith(".js")) return "application/javascript; charset=utf-8";
            if (resourceName.EndsWith(".svg")) return "image/svg+xml";
            if (resourceName.EndsWith(".png")) return "image/png";
            if (resourceName.EndsWith(".jpg") || resourceName.EndsWith(".jpeg")) return "image/jpeg";
            if (resourceName.EndsWith(".ico")) return "image/x-icon";
            if (resourceName.EndsWith(".woff2")) return "font/woff2";
            if (resourceName.EndsWith(".woff")) return "font/woff";
            return "application/octet-stream";
        }

        /// <summary>
        /// 获取本机 IP 地址列表
        /// </summary>
        public static List<string> GetLocalIPs()
        {
            var ips = new List<string>();

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    ips.Add(ip.ToString());
                }
            }
            catch
            {
                ips.Add("127.0.0.1");
            }

            if (ips.Count == 0)
            {
                ips.Add("127.0.0.1");
            }

            return ips;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopAsync().Wait(3000);
                _disposed = true;
            }
        }
    }
}
