using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICTBroadcast
{
	class BroadcastApi
	{
        private String token = "api-token: you can get it from ICTBroadcast -> My Account -> Api Key";
        private String username = "username-from-ictbroadcast";
        private String password = "password";
        private String api_url = "http://ictbroadcast.server.ip.or.domain/rest";
        private String method_url = null;
        private Dictionary<String, String> parameters = new Dictionary<string, string>();
        private String attachmentName = null;
        private String attachmentFile = null;
        private ByteArrayContent attachmentData = null;

        public BroadcastApi(String method)
        {
            this.method_url = this.api_url + "/" + method;
        }

        public Task<String> post()
        {
            if (this.attachmentName != null) {
                MultipartFormDataContent form = new MultipartFormDataContent();
                foreach(KeyValuePair<String, String> parameter in this.parameters) {
                    form.Add(new StringContent(parameter.Value), parameter.Key);
                }
                form.Add(this.attachmentData, this.attachmentName, this.attachmentFile);
                return this.callApi(form);
            } else {
                FormUrlEncodedContent form = new FormUrlEncodedContent(this.parameters);
                return this.callApi(form);
            }
        }

        public void add_parameter(String name, String value)
        {
            this.parameters.Add(name, value);
        }

        public void add_dictionary(String name, Dictionary<String, String> data)
        {
            var data_json = this.FromDictionaryToJson(data);
            this.add_parameter(name, data_json);
        }

        public void set_attachment(String name, String fileFullPath)
        {
            FileInfo fi = new FileInfo(fileFullPath);
            String fileName = fi.Name;
            Byte[] fileContents = File.ReadAllBytes(fi.FullName);
            //this.attachmentData.Headers.ContentType = "application/pdf";

            this.attachmentName = name;
            this.attachmentFile = fileName;
            this.attachmentData = new ByteArrayContent(fileContents);
        }

        public async Task<string> callApi (HttpContent form)
        {
			HttpClient httpClient = new HttpClient();

            if (this.token != null) {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer", 
                    this.token
                );
            } else {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", 
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", this.username, this.password)
                        )
                    )
                );
            }

			HttpResponseMessage response = null;
            try{
                response = await httpClient.PostAsync(this.method_url, form);
            } catch (Exception ex) {
                if (response == null) { 
                    response = new HttpResponseMessage();
                }
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ReasonPhrase = string.Format("RestHttpClient.SendRequest failed: {0}", ex);
            }

			response.EnsureSuccessStatusCode();
			httpClient.Dispose();
			var WebAPIResult = response.Content.ReadAsStringAsync().Result;
            return WebAPIResult;
        }

        public String FromDictionaryToJson(Dictionary<String, String> dictionary)
        {
            var kvs = dictionary.Select(kvp => string.Format("\"{0}\":\"{1}\"", kvp.Key, string.Concat(",", kvp.Value)));
            return string.Concat("{", string.Join(",", kvs), "}");
        }

        public Dictionary<String, String> FromJsonToDictionary(String json)
        {
            String[] keyValueArray = json.Replace("{", string.Empty).Replace("}", string.Empty).Replace("\"", string.Empty).Split(',');
            return keyValueArray.ToDictionary(item => item.Split(':')[0], item => item.Split(':')[1]);
        }
	}
}
