using ModernMT.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace ModernMT
{
    public class ModernMTService
    {
        private readonly ModernMTClient _client;
        public readonly MemoryServices Memories;

        public ModernMTService(string apiKey, string platform = "modernmt-dotnet", string platformVersion = "1.0.0")
        {
            _client = new ModernMTClient(apiKey, platform, platformVersion);
            Memories = new MemoryServices(_client);
        }
        
        #region Translation APIs

        public List<string> ListSupportedLanguages()
        {
            return _client.Send<List<string>>("get", "/translate/languages");
        }

        public Translation Translate(string source, string target, string q, long[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            return Translate(source, target, new List<string>{ q }, hints, contextVector, options)[0];
        }
        
        public List<Translation> Translate(string source, string target, List<string> q, long[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "source", source },
                { "target", target },
                { "q", q.ToArray() },
                { "hints", hints },
                { "context_vector", contextVector },
            };
            
            if (options != null)
            {
                data.Add("priority", options.Priority);
                data.Add("project_id", options.ProjectId);
                data.Add("multiline", options.Multiline);
                data.Add("timeout", options.Timeout);
            }
            
            return _client.Send<List<Translation>>("get", "/translate", data);
        }
        
        public string GetContextVector(string source, string targets, string text, long[] hints = null, int limit = 0)
        {
            var res = GetContextVector(source, new List<string> { targets }, text, hints, limit);
            return res.ContainsKey(targets) ? res[targets] : null;
        }
        
        public Dictionary<string, string> GetContextVector(string source, List<string> targets, string text,
            long[] hints = null, int limit = 0)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "source", source },
                { "targets", targets.ToArray() },
                { "text", text },
                { "hints", hints },
                { "limit", limit }
            };
            
            var res = _client.Send<Dictionary<string, dynamic>>("get", "/context-vector", data);
            return ((JObject) res["vectors"]).ToObject<Dictionary<string, string>>();
        }

        public string GetContextVectorFromFile(string source, string targets, string file, long[] hints = null,
            int limit = 0, string compression = null)
        {
            var res = GetContextVectorFromFile(source, new List<string>{ targets }, file, hints, limit, compression);
            return res.ContainsKey(targets) ? res[targets] : null;
        }
        
        public string GetContextVectorFromFile(string source, string targets, FileStream file, long[] hints = null,
            int limit = 0, string compression = null)
        {
            var res = GetContextVectorFromFile(source, new List<string>{ targets }, file, hints, limit, compression);
            return res.ContainsKey(targets) ? res[targets] : null;
        }
        
        public Dictionary<string, string> GetContextVectorFromFile(string source, List<string> targets, string file,
            long[] hints = null, int limit = 0, string compression = null)
        {
            return GetContextVectorFromFile(source, targets, File.OpenRead(file), hints, limit, compression);
        }

        public Dictionary<string, string> GetContextVectorFromFile(string source, List<string> targets, FileStream file,
            long[] hints = null, int limit = 0, string compression = null)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "source", source },
                { "targets", targets.ToArray() },
                { "compression", compression },
                { "limit", limit },
                { "hints", hints }
            };

            var files = new Dictionary<string, FileStream>
            {
                { "content", file }
            };

            var res = _client.Send<Dictionary<string, dynamic>>("get", "/context-vector", data, files);
            return ((JObject) res["vectors"]).ToObject<Dictionary<string, string>>();
        }

        #endregion

        #region Memory APIs
        
        public class MemoryServices
        {
            private readonly ModernMTClient _client;
            
            internal MemoryServices(ModernMTClient client)
            {
                _client = client;
            }
            
            public List<Memory> List()
            {
                return _client.Send<List<Memory>>("get", "/memories");
            }

            public Memory Get(long id)
            {
                return _client.Send<Memory>("get", "/memories/" + id);
            }

            public Memory Create(string name, string description = null, string externalId = null)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "name", name },
                    { "description", description },
                    { "external_id", externalId }
                };
            
                return _client.Send<Memory>("post", "/memories", data);
            }
            
            public Memory Edit(long id, string name = null, string description = null)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "name", name },
                    { "description", description}
                };
            
                return _client.Send<Memory>("put", "/memories/" + id, data);
            }

            public Memory Delete(long id)
            {
                return _client.Send<Memory>("delete", "/memories/" + id);
            }

            public ImportJob Add(long id, string source, string target, string sentence, string translation,
                string tuid = null)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "source", source },
                    { "target", target },
                    { "sentence", sentence },
                    { "translation", translation },
                    { "tuid", tuid }
                };
            
                return _client.Send<ImportJob>("post",  "/memories/" + id + "/content", data);
            }
            
            public ImportJob Replace(long id, string tuid, string source, string target, string sentence,
                string translation)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "tuid", tuid },
                    { "source", source },
                    { "target", target },
                    { "sentence", sentence },
                    { "translation", translation }
                };
            
                return _client.Send<ImportJob>("put",  "/memories/" + id + "/content", data);
            }

            public ImportJob Import(long id, string tmx, string compression = null)
            {
                return Import(id, File.OpenRead(tmx), compression);
            }

            public ImportJob Import(long id, FileStream tmx, string compression = null)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "compression", compression }
                };
                
                var files = new Dictionary<string, FileStream>
                {
                    { "tmx", tmx }
                };
            
                return _client.Send<ImportJob>("post", "/memories/" + id + "/content", data, files);
            }

            public ImportJob GetImportStatus(string uuid)
            {
                return _client.Send<ImportJob>("get", "import-jobs/" + uuid);
            }
        }
        
        #endregion

        #region Http Wrapper

        internal class ModernMTClient
        {
            private readonly HttpClient _httpClient;
            
            internal ModernMTClient(string apiKey, string platform, string platformVersion)
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.modernmt.com")
                };
                _httpClient.DefaultRequestHeaders.Add("MMT-ApiKey", apiKey);
                _httpClient.DefaultRequestHeaders.Add("MMT-Platform", platform);
                _httpClient.DefaultRequestHeaders.Add("MMT-PlatformVersion", platformVersion);
            } 
            
            internal dynamic Send<T>(string method, string path, Dictionary<string, dynamic> data = null,
                Dictionary<string, FileStream> files = null)
            {
                data = data?
                    .Where(entry => entry.Value != null)
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
                
                files = files?
                    .Where(entry => entry.Value != null)
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
                
                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("X-HTTP-Method-Override", method);

                if (files != null)
                {
                    var content = new MultipartFormDataContent();

                    if (data != null)
                    {
                        foreach (var entry in data)
                        {
                            var value = entry.Value is Array ? string.Join(",", entry.Value) : entry.Value;
                            content.Add(new StringContent(value.ToString(), System.Text.Encoding.UTF8), entry.Key);
                        }
                    }

                    foreach (var entry in files)
                        content.Add(new StreamContent(entry.Value), entry.Key, entry.Key);

                    request.Content = content;
                }
                else if (data != null)
                {
                    var jsonBody = JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                }

                var requestTask = _httpClient.SendAsync(request);
                requestTask.Wait();
                var contentTask = requestTask.Result.Content.ReadAsStringAsync();
                contentTask.Wait();
                var response = contentTask.Result;
                var json = JObject.Parse(response);
                
                var status = json["status"].ToObject<int>();
                if (status >= 300 || status < 200)
                {
                    var type = "UnknownException";
                    var message = "No details provided.";
                    
                    var error = json["error"].ToObject<dynamic>();
                    if (error == null) 
                        throw new ModernMTException(status, type, message);
                    
                    if (error.type != null) 
                        type = error.type;
                    if (error.message != null) 
                        message = error.message;

                    throw new ModernMTException(status, type, message);
                }

                var result = json["data"];
                return result.ToObject(typeof(T));
            }
        }
        
        #endregion
    }
}
