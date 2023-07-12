using ModernMT.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;

namespace ModernMT
{
    // ReSharper disable once InconsistentNaming
    public class ModernMTService
    {
        private const string Platform = "modernmt-dotnet";
        private const string PlatformVersion = "1.4.0";

        private readonly ModernMTClient _client;
        public readonly MemoryServices Memories;

        private JwtBuilder _jwtBuilder;
        private long _batchPublicKeyTimestampSec;

        public ModernMTService(string apiKey, string platform = Platform, string platformVersion = PlatformVersion,
            long apiClient = 0)
        {
            _client = new ModernMTClient(apiKey, platform, platformVersion, apiClient);
            Memories = new MemoryServices(_client);
        }

        public ModernMTService(string apiKey, long apiClient) :
            this(apiKey, Platform, PlatformVersion, apiClient)
        {
            
        }

        #region Translation APIs

        public List<string> ListSupportedLanguages()
        {
            return _client.Send<List<string>>("get", "/translate/languages");
        }

        public DetectedLanguage DetectLanguage(string q, string format = null)
        {
            return DetectLanguage(new List<string> {q}, format)[0];
        }

        public List<DetectedLanguage> DetectLanguage(List<string> q, string format = null)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "q", q.ToArray() },
                { "format", format }
            };
                
            return _client.Send<List<DetectedLanguage>>("get", "/translate/detect", data);
        }
        
        public Translation Translate(string source, string target, string q, TranslateOptions options)
        {
            return TranslateWithKeys(source, target, new List<string>{ q }, null, null, options)[0];
        }
        
        public List<Translation> Translate(string source, string target, List<string> q, TranslateOptions options)
        {
            return TranslateWithKeys(source, target, q, null, null, options);
        }

        public Translation Translate(string source, string target, string q, long[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            return Translate(source, target, new List<string>{ q }, hints, contextVector, options)[0];
        }
        
        public List<Translation> Translate(string source, string target, List<string> q, long[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            string[] hintsArray = null;
            if (hints != null)
                hintsArray = hints.Select(el => el.ToString()).ToArray();
            
            return TranslateWithKeys(source, target, q, hintsArray, contextVector, options);
        }

        public Translation TranslateWithKeys(string source, string target, string q, string[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            return TranslateWithKeys(source, target, new List<string>{ q }, hints, contextVector, options)[0];
        }
        
        public List<Translation> TranslateWithKeys(string source, string target, List<string> q, string[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "source", source },
                { "target", target },
                { "q", q.ToArray() },
                { "hints", hints != null ? string.Join(",", hints) : null },
                { "context_vector", contextVector },
            };
            
            if (options != null)
            {
                data.Add("priority", options.Priority);
                data.Add("project_id", options.ProjectId);
                data.Add("multiline", options.Multiline);
                data.Add("timeout", options.Timeout);
                data.Add("format", options.Format);
                data.Add("alt_translations", options.AltTranslations);
                data.Add("session", options.Session);
            }
            
            return _client.Send<List<Translation>>("get", "/translate", data);
        }
        
        public bool BatchTranslate(string webhook, string source, string target, string q, TranslateOptions options)
        {
            return BatchTranslate(webhook, source, target, q, null, null, options);
        }
        
        public bool BatchTranslate(string webhook, string source, string target, List<string> q, TranslateOptions options)
        {
            return BatchTranslate(webhook, source, target, q, null, null, options);
        }
        
        public bool BatchTranslate(string webhook, string source, string target, string q, long[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            string[] hintsArray = null;
            if (hints != null)
                hintsArray = hints.Select(el => el.ToString()).ToArray();
            
            return BatchTranslateWithKeys(webhook, source, target, (object) q, hintsArray, contextVector, options);
        }
        
        public bool BatchTranslate(string webhook, string source, string target, List<string> q, long[] hints = null,
            string contextVector = null, TranslateOptions options = null)
        {
            string[] hintsArray = null;
            if (hints != null)
                hintsArray = hints.Select(el => el.ToString()).ToArray();
            
            return BatchTranslateWithKeys(webhook, source, target, (object) q, hintsArray, contextVector, options);
        }
        
        public bool BatchTranslateWithKeys(string webhook, string source, string target, string q,
            string[] hints = null, string contextVector = null, TranslateOptions options = null)
        {
            return BatchTranslateWithKeys(webhook, source, target, (object) q, hints, contextVector, options);
        }
        
        public bool BatchTranslateWithKeys(string webhook, string source, string target, List<string> q,
            string[] hints = null, string contextVector = null, TranslateOptions options = null)
        {
            return BatchTranslateWithKeys(webhook, source, target, (object) q, hints, contextVector, options);
        }
        
        public bool BatchTranslateWithKeys(string webhook, string source, string target, object q,
            string[] hints = null, string contextVector = null, TranslateOptions options = null)
        {
            var headers = new Dictionary<string, string>();
            var data = new Dictionary<string, dynamic>
            {
                { "webhook", webhook },
                { "source", source },
                { "target", target },
                { "q", q },
                { "hints", hints != null ? string.Join(",", hints) : null },
                { "context_vector", contextVector },
            };

            if (options != null)
            {
                data.Add("project_id", options.ProjectId);
                data.Add("multiline", options.Multiline);
                data.Add("format", options.Format);
                data.Add("alt_translations", options.AltTranslations);
                data.Add("session", options.Session);

                data.Add("metadata", options.Metadata);
                
                headers.Add("x-idempotency-key", options.IdempotencyKey);
            }

            var res = _client.Send<Dictionary<string, dynamic>>("post", "/translate/batch", data, null, headers);
            return res["enqueued"];
        }

        public BatchTranslation HandleCallback(dynamic body, string signature)
        {
            if (_jwtBuilder == null)
                RefreshJwtBuilder();

            if (DateTimeOffset.Now.ToUnixTimeSeconds() - _batchPublicKeyTimestampSec > 3600) // key is older than 1 hour
            {
                try
                {
                    RefreshJwtBuilder();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            try
            {
                // ReSharper disable once PossibleNullReferenceException
                _jwtBuilder.Decode(signature);
            }
            catch (SignatureVerificationException e)
            {
                throw new SignatureException(e);
            }

            JObject json;
            switch (body)
            {
                case string _:
                    json = JObject.Parse(body);
                    break;
                case JObject _:
                    json = body;
                    break;
                default:
                    json = JObject.FromObject(body);
                    break;
            }

            // ReSharper disable once PossibleNullReferenceException
            var resultJson = json["result"].ToObject<JObject>();
            
            // ReSharper disable once PossibleNullReferenceException
            dynamic metadata = null;
            if (json.TryGetValue("metadata", out var metadataJson))
                metadata = metadataJson.ToObject<dynamic>();
            
            // ReSharper disable once PossibleNullReferenceException
            var status = resultJson["status"].ToObject<int>();
            if (status >= 300 || status < 200)
            {
                // ReSharper disable once PossibleNullReferenceException
                var error = resultJson["error"].ToObject<dynamic>();
                throw new ModernMTException(status, (string) error.type, (string) error.message, metadata);
            }
            
            var data = resultJson["data"];
            if (data is JArray)
                return new BatchTranslation(data.ToObject<List<Translation>>(), metadata);
            else
                // ReSharper disable once PossibleNullReferenceException
                return new BatchTranslation(data.ToObject<Translation>(), metadata);
        }

        private void RefreshJwtBuilder()
        {
            var res = _client.Send<Dictionary<string, dynamic>>("get", "/translate/batch/key");
            string encodedPublicKey = res["publicKey"];
            
            var keyBytes = Convert.FromBase64String(encodedPublicKey);
            var keyString = System.Text.Encoding.UTF8.GetString(keyBytes);
            
            keyString = keyString
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "");
            
            var rsa = Utils.DecodeX509PublicKey(Convert.FromBase64String(keyString));
            
            _jwtBuilder = JwtBuilder.Create()
                .WithAlgorithm(new RS256Algorithm(rsa))
                .MustVerifySignature();

            _batchPublicKeyTimestampSec = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public string GetContextVector(string source, string target, string text, long[] hints = null, int limit = 0)
        {
            var res = GetContextVector(source, new List<string> { target }, text, hints, limit);
            return res.TryGetValue(target, out var re) ? re : null;
        }
        
        public Dictionary<string, string> GetContextVector(string source, List<string> targets, string text,
            long[] hints = null, int limit = 0)
        {
            string[] hintsArray = null;
            if (hints != null)
                hintsArray = hints.Select(el => el.ToString()).ToArray();

            return GetContextVectorByKeys(source, targets, text, hintsArray, limit);
        }

        public string GetContextVectorByKeys(string source, string target, string text, string[] hints = null, int limit = 0)
        {
            var res = GetContextVectorByKeys(source, new List<string> { target }, text, hints, limit);
            return res.TryGetValue(target, out var re) ? re : null;
        }
        
        public Dictionary<string, string> GetContextVectorByKeys(string source, List<string> targets, string text,
            string[] hints = null, int limit = 0)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "source", source },
                { "targets", targets.ToArray() },
                { "text", text },
                { "hints", hints != null ? string.Join(",", hints) : null },
                { "limit", limit }
            };
            
            var res = _client.Send<Dictionary<string, dynamic>>("get", "/context-vector", data);
            return ((JObject) res["vectors"]).ToObject<Dictionary<string, string>>();
        }
        
        public string GetContextVectorFromFile(string source, string target, string file, string compression)
        {
            var res = GetContextVectorFromFile(source, new List<string>{ target }, file, null, 0, compression);
            return res.TryGetValue(target, out var re) ? re : null;
        }

        public string GetContextVectorFromFile(string source, string target, string file, long[] hints = null,
            int limit = 0, string compression = null)
        {
            var res = GetContextVectorFromFile(source, new List<string>{ target }, file, hints, limit, compression);
            return res.TryGetValue(target, out var re) ? re : null;
        }
        
        public string GetContextVectorFromFile(string source, string target, FileStream file, string compression)
        {
            var res = GetContextVectorFromFile(source, new List<string>{ target }, file, null, 0, compression);
            return res.TryGetValue(target, out var re) ? re : null;
        }
        
        public string GetContextVectorFromFile(string source, string target, FileStream file, long[] hints = null,
            int limit = 0, string compression = null)
        {
            var res = GetContextVectorFromFile(source, new List<string>{ target }, file, hints, limit, compression);
            return res.TryGetValue(target, out var re) ? re : null;
        }
        
        public Dictionary<string, string> GetContextVectorFromFile(string source, List<string> targets, string file,
            string compression = null)
        {
            return GetContextVectorFromFile(source, targets, File.OpenRead(file), null, 0, compression);
        }
        
        public Dictionary<string, string> GetContextVectorFromFile(string source, List<string> targets, string file,
            long[] hints = null, int limit = 0, string compression = null)
        {
            return GetContextVectorFromFile(source, targets, File.OpenRead(file), hints, limit, compression);
        }
        
        public Dictionary<string, string> GetContextVectorFromFile(string source, List<string> targets, FileStream file,
            string compression)
        {
            return GetContextVectorFromFileByKeys(source, targets, file, null, 0, compression);
        }

        public Dictionary<string, string> GetContextVectorFromFile(string source, List<string> targets, FileStream file,
            long[] hints = null, int limit = 0, string compression = null)
        {
            string[] hintsArray = null;
            if (hints != null)
                hintsArray = hints.Select(el => el.ToString()).ToArray();

            return GetContextVectorFromFileByKeys(source, targets, file, hintsArray, limit, compression);
        }
        
        public string GetContextVectorFromFileByKeys(string source, string target, string file, string[] hints = null,
            int limit = 0, string compression = null)
        {
            var res = GetContextVectorFromFileByKeys(source, new List<string>{ target }, file, hints, limit, compression);
            return res.TryGetValue(target, out var re) ? re : null;
        }
        
        public string GetContextVectorFromFileByKeys(string source, string target, FileStream file, string[] hints = null,
            int limit = 0, string compression = null)
        {
            var res = GetContextVectorFromFileByKeys(source, new List<string>{ target }, file, hints, limit, compression);
            return res.TryGetValue(target, out var re) ? re : null;
        }
        
        public Dictionary<string, string> GetContextVectorFromFileByKeys(string source, List<string> targets, string file,
            string[] hints = null, int limit = 0, string compression = null)
        {
            return GetContextVectorFromFileByKeys(source, targets, File.OpenRead(file), hints, limit, compression);
        }

        public Dictionary<string, string> GetContextVectorFromFileByKeys(string source, List<string> targets, FileStream file,
            string[] hints = null, int limit = 0, string compression = null)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "source", source },
                { "targets", targets.ToArray() },
                { "compression", compression },
                { "limit", limit },
                { "hints", hints != null ? string.Join(",", hints) : null }
            };

            var files = new Dictionary<string, FileStream>
            {
                { "content", file }
            };

            var res = _client.Send<Dictionary<string, dynamic>>("get", "/context-vector", data, files);
            return ((JObject) res["vectors"]).ToObject<Dictionary<string, string>>();
        }

        public User Me()
        {
            return _client.Send<User>("get", "/users/me");
        }

        public QualityEstimation Qe(string source, string target, string sentence, string translation)
        {
            return Qe(source, target,
                new List<string> { sentence }, new List<string> { translation })[0];
        }
        
        public List<QualityEstimation> Qe(string source, string target,
            List<string> sentences, List<string> translations)
        {
            var data = new Dictionary<string, dynamic>
            {
                { "source", source },
                { "target", target },
                { "sentence", sentences.ToArray() },
                { "translation", translations.ToArray() }
            };

            return _client.Send<List<QualityEstimation>>("get", "/qe", data);
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
                return Get(id.ToString());
            }
            
            public Memory Get(string id)
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
                return Edit(id.ToString(), name, description);
            }
            
            public Memory Edit(string id, string name = null, string description = null)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "name", name },
                    { "description", description }
                };
            
                return _client.Send<Memory>("put", "/memories/" + id, data);
            }

            public Memory Delete(long id)
            {
                return Delete(id.ToString());
            }

            public Memory Delete(string id)
            {
                return _client.Send<Memory>("delete", "/memories/" + id);
            }

            public ImportJob Add(long id, string source, string target, string sentence, string translation,
                string tuid = null, string session = null)
            {
                return Add(id.ToString(), source, target, sentence, translation, tuid, session);
            }

            public ImportJob Add(string id, string source, string target, string sentence, string translation,
                string tuid = null, string session = null)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "source", source },
                    { "target", target },
                    { "sentence", sentence },
                    { "translation", translation },
                    { "tuid", tuid },
                    { "session", session }
                };
            
                return _client.Send<ImportJob>("post",  "/memories/" + id + "/content", data);
            }
            
            public ImportJob Replace(long id, string tuid, string source, string target, string sentence,
                string translation, string session = null)
            {
                return Replace(id.ToString(), tuid, source, target, sentence, translation, session);
            }
            
            public ImportJob Replace(string id, string tuid, string source, string target, string sentence,
                string translation, string session = null)
            {
                var data = new Dictionary<string, dynamic>
                {
                    { "tuid", tuid },
                    { "source", source },
                    { "target", target },
                    { "sentence", sentence },
                    { "translation", translation },
                    { "session", session }
                };
            
                return _client.Send<ImportJob>("put",  "/memories/" + id + "/content", data);
            }

            public ImportJob Import(long id, string tmx, string compression = null)
            {
                return Import(id, File.OpenRead(tmx), compression);
            }

            public ImportJob Import(long id, FileStream tmx, string compression = null)
            {
                return Import(id.ToString(), tmx, compression);
            }

            public ImportJob Import(string id, string tmx, string compression = null)
            {
                return Import(id, File.OpenRead(tmx), compression);
            }

            public ImportJob Import(string id, FileStream tmx, string compression = null)
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

        // ReSharper disable once InconsistentNaming
        internal class ModernMTClient
        {
            private readonly HttpClient _httpClient;
            
            internal ModernMTClient(string apiKey, string platform, string platformVersion, long apiClient)
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.modernmt.com")
                };
                _httpClient.DefaultRequestHeaders.Add("MMT-ApiKey", apiKey);
                _httpClient.DefaultRequestHeaders.Add("MMT-Platform", platform);
                _httpClient.DefaultRequestHeaders.Add("MMT-PlatformVersion", platformVersion);
                
                if (apiClient != 0)
                    _httpClient.DefaultRequestHeaders.Add("MMT-ApiClient", apiClient.ToString());
            } 
            
            internal dynamic Send<T>(string method, string path, Dictionary<string, dynamic> data = null,
                Dictionary<string, FileStream> files = null, Dictionary<string, string> headers = null)
            {
                data = data?
                    .Where(entry => entry.Value != null)
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
                
                files = files?
                    .Where(entry => entry.Value != null)
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
                
                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("X-HTTP-Method-Override", method);

                if (headers != null)
                {
                    foreach(var entry in headers)
                    {
                        request.Headers.Add(entry.Key, entry.Value);
                    }
                }

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
                
                // ReSharper disable once PossibleNullReferenceException
                var status = json["status"].ToObject<int>();
                if (status >= 300 || status < 200)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var error = json["error"].ToObject<dynamic>();
                    throw new ModernMTException(status, (string) error.type, (string) error.message);
                }

                // ReSharper disable once PossibleNullReferenceException
                return json["data"].ToObject(typeof(T));
            }
        }
        
        #endregion
    }
}
