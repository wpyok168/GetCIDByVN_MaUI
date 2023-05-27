using MVVM;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GetCIDByVN
{
    public class GetCID_VN_ViewMode:ViewModeBase
    {
        private string txtiid = string.Empty;
        private string txtcid = string.Empty;
        private RelayCommand txtiidClickCommad;
        private bool isEnable = true;


        public GetCID_VN_ViewMode()
        {
            TxtiidClickCommad= new RelayCommand(GetClipboard);
        }

        public string Txtiid { get => txtiid; set { txtiid = value; RaisePropertyChanged(); } }
        public string Txtcid { get => txtcid; set { txtcid = value; RaisePropertyChanged(); } }
        public bool IsEnable { get => isEnable; set { isEnable = value; RaisePropertyChanged(); } }

        public RelayCommand TxtiidClickCommad { get => txtiidClickCommad; set => txtiidClickCommad = value; }
        

        private async void GetClipboard(object obj)
        {
            
            if (!string.IsNullOrEmpty(Txtcid))
            {
                Txtcid = string.Empty;
            }
            string iid = (await Clipboard.GetTextAsync()).Replace("\\u00A0", "").Replace(" ", "").Replace("-", "").Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
            if (Regex.IsMatch(iid, "[\\d]{63}"))
            {
                Txtiid= Regex.Match(iid, "[\\d]{63}").Value;
                GetCID(Txtiid);
            }
            else if (Regex.IsMatch(iid, "[\\d]{54}"))
            {
                Txtiid = Regex.Match(iid, "[\\d]{54}").Value;
                GetCID(Txtiid);
            }
            else
            {
                Txtiid = "安装ID有误";
            }
        }
        
        private async void GetCID(string iid)
        {
            await Task.Factory.StartNew(async() => {

                Txtcid = "正在获取，请耐心等待下。。。。";
                IsEnable = false;
                string crtiid = string.Empty;
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.IV = Encoding.UTF8.GetBytes("7061759328313233");
                    aes.Key = Encoding.UTF8.GetBytes("ditObg4239Ajdk@d");
                    ICryptoTransform ct = aes.CreateEncryptor(aes.Key, aes.IV);
                    byte[] encrypted;
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, ct, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(iid);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                    crtiid = Convert.ToBase64String(encrypted);

                }
                string url1 = $"https://0xc004c008.com";
                string url2 = $"https://0xc004c008.com/ajax/get_cid?iids={System.Web.HttpUtility.UrlEncode(crtiid)}";
                CookieContainer cc = new CookieContainer();
                var ret = HttpClientGet(url1, cc, new Action<CookieContainer>((x) => cc = x));
                ret.Wait();
                ret = HttpClientGet(url2, cc, new Action<CookieContainer>((x) => cc = x));
                ret.Wait();

                string outhtml =ret.Result.ToString();
                string plaintext = string.Empty;
                if (!string.IsNullOrEmpty(outhtml))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.IV = Encoding.UTF8.GetBytes("7061759328313233");
                        aes.Key = Encoding.UTF8.GetBytes("ditObg4239Ajdk@d");
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(outhtml)))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    plaintext = srDecrypt.ReadToEnd();
                                }
                            }
                        }

                    }
                }
                Dictionary<string, object> dy = JsonSerializer.Deserialize<Dictionary<string, object>>(plaintext);
                //Successfully  result

                if (dy["result"].ToString().Equals("Successfully"))
                {
                    //confirmationid
                    Txtcid = dy["confirmationid"].ToString().Replace("-"," ");
                }
                else
                {
                    Txtcid = dy["short_result"].ToString();
                }
                IsEnable = true;
                string txt = "安装ID：" + this.Txtiid + "\r\n" + "确认ID：" + this.Txtcid;
                await Clipboard.SetTextAsync(txt);
                Page page = new Page();
                await page.DisplayAlert("获取结果提示", "已获取并复制到了剪切板","ok");
                // Invoke((Action)(() => { Clipboard.SetText("安装ID：" + this.textBox1.Text + "\r\n" + "确认ID：" + this.textBox2.Text); }));

                //this.label3.Invoke(new Action(() =>
                //{
                //    this.label3.Text = "已获取并复制到了剪切板";
                //}));

            });

        }
        
        private async Task<string> HttpClientGet (string url, CookieContainer cookieContainer, Action<CookieContainer> retCookieContainer)
        {
            string ret = string.Empty;
            Dictionary<string, string> cc = new Dictionary<string, string>()
            {
                {"Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
                //{"Accept-Encoding","gzip, deflate, br" },
                //{"ContentType","application/x-www-form-urlencoded;application/json; charset=UTF-8" },
                {"Accept-Language","zh-CN,zh;q=0.9,en;q=0.8,ru;q=0.7,de;q=0.6" },
                {"User-Agent","Mozilla/5.0 (Linux; Android 10; YAL-AL10 Build/HUAWEIYAL-AL10; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/89.0.4356.6 MQQBrowser/6.2 TBS/045434 Mobile Safari/537.36 V1_AND_SQ_8.5.0_1596_YYB_D QQ/8.5.0.5025 NetType/WIFI WebP/0.3.0 Pixel/1080 StatusBarHeight/108 SimpleUISwitch/0 QQTheme/999 InMagicWin/0" }
            };
            using (HttpClientHandler hch = new HttpClientHandler())
            {
                hch.AllowAutoRedirect = true;
                hch.AutomaticDecompression =  DecompressionMethods.None | DecompressionMethods.GZip |DecompressionMethods.Brotli;
                hch.UseCookies = true;
                hch.CookieContainer = cookieContainer;

                using (HttpClient client = new HttpClient(hch))
                {
                    foreach (var item in cc)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                    }
                    client.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "application/x-www-form-urlencoded;application/json; charset=UTF-8");

                    try
                    {
                        using (var result = await client.GetAsync(url))
                        {
                            if (result.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                //if (result.ToString().ToLower().Contains("gzip"))
                                //{
                                //    using (Stream hcs = await result.Content.ReadAsStreamAsync())
                                //    {
                                //        using (GZipStream gs = new GZipStream(hcs, CompressionMode.Decompress))
                                //        {
                                //            ret = new StreamReader(gs, Encoding.UTF8).ReadToEnd();
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    using (HttpContent content = result.Content)
                                //    {
                                //        ret = await content.ReadAsStringAsync();
                                //    }
                                //}
                                using (HttpContent content = result.Content)
                                {
                                    ret = await content.ReadAsStringAsync();
                                }

                            }
                            else if (result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                ret = "403";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ret = ex.Message;
                    }
                    retCookieContainer(cookieContainer);
                }
                
            }
            return ret;
            
        }
    }
}
