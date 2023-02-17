using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace 금기
{
    public static class Extensions
    {
        public static void Increment<T>(this IDictionary<T, int> dict, T key)
        {
            int count;
            dict.TryGetValue(key, out count);
            dict[key] = count + 1;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            Console.WriteLine("#fetch dak.gg");
            Console.Write("유저> ");
            string _ = Console.ReadLine();
            string name = _.Split('#')[0];
            string tag = _.Split('#')[1];
            Console.Title = _;
            string puuid = parsePuuid(JObject.Parse(getPuuid(name, tag)));
            int n = 1;
            while (true)
            {
                JObject obj = matchData(puuid, n);
                if (obj == null) break;
                for (int i = 0; i < obj["matches"].Count(); i++)
                {
                    var tmpUrlData = obj["matches"][i]["matchInfo"]["matchId"];
                    var url = $"https://dak.gg/valorant/_next/data/0UwWEikFYni1rYUhT_nKN/ko/profile/{name}-{tag}/match/{tmpUrlData}.json";
                    using (WebResponse res = WebRequest.Create(url).GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            var tmp = JObject.Parse(sr.ReadToEnd())["pageProps"]["matchDetail"]["players"];
                            for (int j = 0; j < tmp.Count(); j++)
                            {
                                var fullName = tmp[j]["gameName"].ToString() + "#" + tmp[j]["tagLine"].ToString();
                                if (fullName != _)
                                {
                                    dic.Increment(fullName);
                                }
                            }
                        }
                    }
                }
                n++;
            }
            dic.ToList().ForEach(x =>
            {
                if (x.Value <= 1)
                {
                    dic.Remove(x.Key);
                }
            });
            var desc = dic.OrderByDescending(x => x.Value);
            Console.Clear();
            foreach (var d in desc)
            {
                Console.WriteLine("-----------------");
                Console.WriteLine($"|\t{d.Value}\t| --- {d.Key}");
            }
            Console.WriteLine("-----------------");
            Console.ReadLine();
        }

        static void refresh(string who)
        {
            string name = who.Split('#')[0];
            string tag = who.Split('#')[1];
            string puuid = parsePuuid(JObject.Parse(getPuuid(name, tag)));

            Console.Clear();
            Console.WriteLine("refreshing..");
            WebRequest request = WebRequest.Create($"https://val.dakgg.io/api/v1/rpc/account-sync/by-puuid/{puuid}");
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            response.Close();
            Console.WriteLine("waiting..");
            Thread.Sleep(1000);
            Process.Start($"https://dak.gg/valorant/profile/{name}-{tag}");
        }

        static JObject matchData(string puuid, int page = 1)
        {
            string matchURL = $"https://val.dakgg.io/api/v1/accounts/{puuid}/matches?page={page}";
            Console.WriteLine($"puuid: {puuid} | page: {page}");
            using (WebResponse res = WebRequest.Create(matchURL).GetResponse())
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    var result = JObject.Parse(sr.ReadToEnd());
                    if (result["matches"].ToString() == "[]")
                    {
                        return null;
                    }
                    else
                    {
                        return result;
                    }
                }
            }
        }

        static string parsePuuid(JObject obj)
        {
            return obj["pageProps"]["account"]["account"]["puuid"].ToString();
        }

        static string getPuuid(string name, string tag)
        {
            string url = $"https://dak.gg/valorant/_next/data/0UwWEikFYni1rYUhT_nKN/ko/profile/{name}-{tag}.json?name={name}-{tag}";
            using (WebResponse res = WebRequest.Create(url).GetResponse())
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
