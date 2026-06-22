using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.MES
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Dictionary<string, Func<HttpListenerRequest, Task<string>>> _routeHandlers;
        private bool _isRunning;
        public bool IsRunning { get => _isRunning; }

        /// <summary>
        /// 初始化 HTTP 服务器，指定监听的 URL 前缀（如 http://localhost:8080/）
        /// </summary>
        public HttpServer(string urlPrefix)
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(urlPrefix);

                _routeHandlers = new Dictionary<string, Func<HttpListenerRequest, Task<string>>>();
            }
            catch (HttpListenerException ex)
            {
                //Log.Log.Error($"初始化失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public async Task StartAsync()
        {
            if (!_listener.IsListening)
            {
                try
                {
                    _listener.Start();
                    _isRunning = true;
                    Log.Log.LogMessage($"服务器已启动，监听地址：{string.Join(", ", _listener.Prefixes)}");

                    // 异步处理请求
                    while (_isRunning)
                    {
                        var context = await _listener.GetContextAsync();
                        _ = ProcessRequestAsync(context); // 不阻塞主循环
                    }
                }
                catch (HttpListenerException ex)
                {
                    Log.Log.LogException($"启动失败：{ex.Message}（可能需要管理员权限）", ex);
                }
            }
        }

        /// <summary>
        /// 注册路由处理函数（默认 GET 方法）
        /// </summary>
        public bool AddRoute(string path, Func<HttpListenerRequest, Task<string>> handler)
        {
            if (_routeHandlers != null)
                _routeHandlers[path] = handler;
            else
                return false;
            return true;
        }

        /// <summary>
        /// 处理单个 HTTP 请求
        /// </summary>
        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                //Log.Log.Info($"收到请求：{request.HttpMethod} {request.Url}");

                // 查找路由处理函数
                if (_routeHandlers.TryGetValue(request.Url.AbsolutePath, out var handler))
                {
                    string content = await handler(request);
                    await SendResponseAsync(response, 200, "application/json", content);
                }
                else
                {
                    await SendResponseAsync(response, 404, "text/plain", "404 Not Found");
                }
            }
            catch (Exception ex)
            {
                //Log.Log.Error($"处理请求异常：{ex.Message}", ex);
                await SendResponseAsync(response, 500, "text/plain", "500 Internal Server Error");
            }
            finally
            {
                response.Close();
            }
        }

        /// <summary>
        /// 发送 HTTP 响应
        /// </summary>
        private async Task SendResponseAsync(
            HttpListenerResponse response,
            int statusCode,
            string contentType,
            string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            response.StatusCode = statusCode;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public bool Stop()
        {
            try
            {
                if (_listener.IsListening)
                {
                    _isRunning = false;
                    _listener.Stop();
                    //Console.WriteLine("服务器已停止");
                }
                return true;
            }
            catch (Exception ex)
            {
                //Log.Log.Error($"停止服务器失败：{ex.Message}", ex);
                return false;
            }
        }

        public void Dispose() => Stop();
    }
}
