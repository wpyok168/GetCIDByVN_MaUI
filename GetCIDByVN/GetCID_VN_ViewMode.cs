using MVVM;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetCIDByVN
{
    public class GetCID_VN_ViewMode:ViewModeBase
    {
        private static CookieContainer cc = new CookieContainer();
        private string txtiid = string.Empty;
        private string txtcid = string.Empty;
        private RelayCommand txtiidClickCommad;
        private bool isEnable = true;
        private string  imagesource = string.Empty;
        private RelayCommand imageClickCommad;
        private string txtVerify = string.Empty;
        private RelayCommand verifyClickCommad;
        private RelayCommand insertTxtCommand;
        private int entryCP = 0;
        private string statusVerify = "首次启动需要进行验证";


        public GetCID_VN_ViewMode()
        {
            TxtiidClickCommad = new RelayCommand(GetClipboard);
            ImageClickCommad = new RelayCommand(GetVerifyImage);
            InsertTxtCommand = new RelayCommand(InsertTxt);
            VerifyClickCommad = new RelayCommand(SummitVerify);
        }

        public string Txtiid { get => txtiid; set { txtiid = value; RaisePropertyChanged(); } }
        public string Txtcid { get => txtcid; set { txtcid = value; RaisePropertyChanged(); } }
        public bool IsEnable { get => isEnable; set { isEnable = value; RaisePropertyChanged(); } }

        public RelayCommand TxtiidClickCommad { get => txtiidClickCommad; set => txtiidClickCommad = value; }
        public string Imagesource { get => imagesource; set { imagesource = value; RaisePropertyChanged(); }}

        public RelayCommand ImageClickCommad { get => imageClickCommad; set => imageClickCommad = value; }
        public string TxtVerify { get => txtVerify; set { txtVerify = value; RaisePropertyChanged(); } }

        public RelayCommand VerifyClickCommad { get => verifyClickCommad; set => verifyClickCommad = value; }
        public RelayCommand InsertTxtCommand { get => insertTxtCommand; set => insertTxtCommand = value; }
        public int EntryCP { get => entryCP; set { entryCP = value; RaisePropertyChanged(); } }

        public string StatusVerify { get => statusVerify; set { statusVerify = value; RaisePropertyChanged(); } }

        private async void SummitVerify(object obj)
        {
            await Task.Run(async () => {
                string codeAes = AesEncrypt(TxtVerify);
                string code = System.Web.HttpUtility.UrlEncode(codeAes);
                string url = $"https://0xc004c008.com/ajax/cidms_verify_captcha?code={code}";
                var ret = await HttpClientGet(url, cc, new Action<CookieContainer>((x) => cc = x));
                string outhtml = ret.ToString();
                if (!string.IsNullOrEmpty(outhtml))
                {
                    string status = AesDecrypt(outhtml);
                    if (status.Equals("1"))
                    {
                        StatusVerify = "验证成功";
                    }
                    else 
                    {
                        TxtVerify = string.Empty;
                        StatusVerify = "无效验证码，请重新验证";
                    }
                }
                else
                {
                    //IsEnable = true;
                    return;
                }
            });
        }

        private async void InsertTxt(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }

                await Task.Run(() => {
                    object[] objects = obj as object[];

                    Entry entry = objects[0] as Entry;
                    Label lbl = objects[1] as Label;
                    entry.Unfocus();
                    int index = entry.CursorPosition;
                    Debug.Print("index：" + index.ToString());
                    TxtVerify = TxtVerify.Insert(index, lbl.Text);
                    //TxtVerify = TxtVerify.Insert(index, lbl.Text);
                    EntryCP = index + 1;
                    entry.CursorPosition = EntryCP;
                    //Debug.Print("EntryCP：" + EntryCP.ToString());
                });
            }
            catch (Exception ex)
            {

            }
            
        }

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
                crtiid = AesEncrypt(iid);
                string url1 = $"https://0xc004c008.com";
                //string url2 = $"https://0xc004c008.com/ajax/cidms_api?iids={System.Web.HttpUtility.UrlEncode(iid)}&username=trogiup24h&password=PHO";
                string url2 = $"https://0xc004c008.com/ajax/get_cid?iids={System.Web.HttpUtility.UrlEncode(crtiid)}";
                Task<string> ret;
                if (cc.Count == 0)
                {
                    ret = HttpClientGet(url1, cc, new Action<CookieContainer>((x) => cc = x));
                    ret.Wait();
                }
                
                ret = HttpClientGet(url2, cc, new Action<CookieContainer>((x) => cc = x));
                ret.Wait();

                string outhtml =ret.Result.ToString();
                string plaintext = string.Empty;
                if (!string.IsNullOrEmpty(outhtml))
                {
                    plaintext = AesDecrypt(outhtml);
                    //"For security purposes, Please verify the code first!"
                    if (plaintext.Contains("Please verify the code first"))
                    {
                        StatusVerify = "请先进行验证后再获取确认ID";
                        GetVerifyImage(null);
                        IsEnable = true;
                        return;
                    }
                }
                else
                {
                    //IsEnable = true;
                    GetCID(iid);
                    return;
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

        public  async void GetVerifyImage(object obj)
        {
            await  Task.Run(async () => {
                TxtVerify = string.Empty;
                string url1 = $"https://0xc004c008.com";
                string url2 = "https://0xc004c008.com/ajax/cidms_refresh_captcha";
                Task<string> ret;
                if (cc.Count == 0)
                {
                    ret = HttpClientGet(url1, cc, new Action<CookieContainer>((x) => cc = x));
                    ret.Wait();
                }
                var outhtml = await HttpClientGet(url2, cc, new Action<CookieContainer>(x => cc = x));
                
                string imaestr = "data:image/png;base64," + AesDecrypt(outhtml.ToString());
                Imagesource = imaestr;
            });
            
            //Imagesource = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAHgAAAAoCAYAAAA16j4lAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAQkSURBVHhe7Zk7SyNRFIDd3+D+gc2Wa7lgKWm2iwumcrvAFqKioI0gwUYxKm4aQQQN8dEk4AMJEosgvpNCxCZbbOwEZX/Bdmdzk5m5507OvCdjMrnFB5J7JnOTz/O4k75/f15AEl6k4JAjBYccKTjkSMEhRwoOOVJwyJGCQ44UHHK6XPAjZGMx+PKZZnT9kbimt+j6DL6YouViell0RwuuzXyEzEA/ZGYOyXWN3DIXGstCjcjsXpX8boI1eQOTdSFUTAUe4lYxTWrrE7TIqyyMapInIHslXtcLeBacin4lXzfnEEoDqrxv8HBtERNPw9+WdRWcrYREnN1TRXGtB3AtmIlVodZNuU7DkZU8HGNaoouwoApslGeTdSnYPp4E5yeV7O2HUp5YtxvDsMzQNgs+HoNP0Q8tRJJ5Oj5g3kVwu/rvQo6IQf8A/g5aZcgk+ki5KpHoGJTIa4PDF8HOJPPeepSuEOt1bJdn3H+X4aJlHWUvue6e2uaQochSkouPJNZM/0G98rq9BanvqSbDOajq1j0NWa4Ea6XXaLjC2WsUo2LSf4UJ2iC7XZOH+WhTIp2lPLsj0SHI3OrX/aAMJ8OKWAO5jMAFW5Zn1HsdnX+1/urgDFzeg8SPERisk8g6KN+3axBXBNshvlmm38ct90XYQXJPTokYBd8E25OMspOSh0uzRe9lCOffGP+bY3X2PYclRXCD2T14JuN0vKdgLHd4C27uiRiEJ8EMZ4LR2VYvWJBrPjljsTQOH2qgTB5cPadjMEhwu3usCCrLNuQyfBVsLRk/4OAZysu2tVxxcML4O0SZw3swY/7YeN2/HlyFm3FFrkVZxngWzHAiWS9TxGqoaj/P2WmlZE/DQZmOYahTtP6ze4W6V4PTnCY3NV6EVyqGwBfBDNsbrUNJNjwytYvCitZ7+YD1CAezSqlWBOs/V5Dw/TovzSo2BZegkPgJvxhzeXgjY+gvg4rrBC5XVZEj5L47CbfZy7AQ/AR3c4rYBhvwm4zjUBtkULFBQu3JK9R9vELdp4Ei2G7vVTEWXMnDrio2XaJjDCA3qIO6zi3U+/sFdb+gEPbiojwzfOvBFMIGO5jGfvFRqc5SofXzmIJ+dPDz6MT2x/sv/bTKDGPBZxuoNDvPYj36LzVo+F7wILUCly2vdZZgYcBy2H8ZhGA0UOkonOlj3UNJcAv1/sboJ+VWua4Etw08QXvOYL3c+lCFM9ljFncK/KyLWD0XXu8cwegBh7ceLE7MPFvtHZG6DXxMUs+8dh9yBE11URFcZ2e7SsYYoQl+209qcnf3n3iQh2la4hPCDwzOyrQiGGWvLkuxeD97sMQZOItTi/Z/nVIEozKMsxRnb4jKc3firhc3BVMihWNSEu4q4oWS7qC1RLcg5XYz2pBFnn9lWe56kGBJ+HiB/8SWnGz0BBSvAAAAAElFTkSuQmCC";
        }

        /// <summary>
        /// Aes解密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string AesDecrypt(string content)
        {
            string outstr =string.Empty;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Encoding.UTF8.GetBytes("ditObg4239Ajdk@d");
                aes.IV = Encoding.UTF8.GetBytes("7061759328313233");
                ICryptoTransform cy = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(content)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, cy, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            outstr = sr.ReadToEnd();
                        }
                    }
                }
            }
            return outstr;
        }
        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string AesEncrypt(string content) 
        {
            string outstr = string.Empty;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Encoding.UTF8.GetBytes("ditObg4239Ajdk@d");
                aes.IV = Encoding.UTF8.GetBytes("7061759328313233");
                ICryptoTransform cy = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] encryptbyte;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, cy, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sr = new StreamWriter(cs))
                        {
                            sr.Write(content); 
                        }
                        encryptbyte = ms.ToArray();
                    }
                }
                outstr = Convert.ToBase64String(encryptbyte);
            }
            return outstr;
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
                    client.Timeout = TimeSpan.FromSeconds(420);
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
