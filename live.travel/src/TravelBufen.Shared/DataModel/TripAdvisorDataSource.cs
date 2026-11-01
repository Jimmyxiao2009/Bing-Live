using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

// “探索”部分的数据源，接 TripAdvisor Content API。
//
// 目前只做了 Location Search（拿到名称和地址）。评分、点评数和照片需要
// 再调 location/{id}/details 和 location/{id}/photos，见下面的 TODO。
// ApiKey 为空时整条链路会退回本地占位数据，方便先调 UI。

namespace TravelBufen.Data
{
    /// <summary>
    /// TripAdvisor 地点数据模型。
    ///
    /// 属性名刻意与模板自带的项模板（Standard310x260ItemTemplate、
    /// StandardTripleLineItemTemplate）保持一致，这样可以直接复用原生
    /// DataTemplate，不必另写一套。
    /// </summary>
    public class TripAdvisorPlace
    {
        public string LocationId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// 从 TripAdvisor Content API 读取地点列表。
    /// </summary>
    public sealed class TripAdvisorDataSource
    {
        // TODO: 在 TripAdvisor 开发者后台申请 Content API Key 后填入。
        // 注意 Content API 需要在后台把调用来源 IP 或 Referer 加入白名单。
        private const string ApiKey = "";

        private const string ApiBase = "https://api.content.tripadvisor.com/api/v1/";
        private const string PlaceholderImage = "ms-appx:///Assets/MediumGray.png";

        private static readonly ObservableCollection<TripAdvisorPlace> _places =
            new ObservableCollection<TripAdvisorPlace>();

        public static ObservableCollection<TripAdvisorPlace> Places
        {
            get { return _places; }
        }

        /// <summary>
        /// 按关键字搜索地点，结果写入 <see cref="Places"/> 并返回。
        /// </summary>
        public static async Task<IEnumerable<TripAdvisorPlace>> SearchAsync(string query)
        {
            IEnumerable<TripAdvisorPlace> results;

            if (String.IsNullOrEmpty(ApiKey))
            {
                results = GetPlaceholderPlaces();
            }
            else
            {
                results = await RequestSearchAsync(query);
            }

            _places.Clear();
            foreach (TripAdvisorPlace place in results)
            {
                _places.Add(place);
            }

            return _places;
        }

        /// <summary>
        /// 调 Location Search 接口。该接口只返回 location_id、name 和 address_obj，
        /// 不含评分和照片。
        /// </summary>
        private static async Task<IEnumerable<TripAdvisorPlace>> RequestSearchAsync(string query)
        {
            string url = ApiBase + "location/search"
                + "?key=" + ApiKey
                + "&searchQuery=" + Uri.EscapeDataString(query)
                + "&language=zh";

            var places = new List<TripAdvisorPlace>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new HttpMediaTypeWithQualityHeaderValue("application/json"));

                    string json = await client.GetStringAsync(new Uri(url));
                    JsonObject root = JsonObject.Parse(json);

                    if (!root.ContainsKey("data"))
                    {
                        return places;
                    }

                    foreach (JsonValue value in root["data"].GetArray())
                    {
                        JsonObject item = value.GetObject();

                        places.Add(new TripAdvisorPlace
                        {
                            LocationId = ReadString(item, "location_id"),
                            Title = ReadString(item, "name"),
                            Subtitle = ReadAddress(item),
                            Description = "查看详情",
                            ImagePath = PlaceholderImage
                        });
                    }
                }
            }
            catch (Exception)
            {
                // 网络不通或返回格式不对时退回占位数据，不让首页开天窗。
                return GetPlaceholderPlaces();
            }

            // TODO: 这里是没做完的另一半 ——
            //   1. 对每个 LocationId 调 location/{id}/details 取 rating 和 num_reviews，
            //      填进 Rating / ReviewCount，并把 Description 拼成“4.5 分 · 1234 条点评”。
            //   2. 调 location/{id}/photos 取首图 URL 替换 ImagePath。
            //   3. details 和 photos 都是一地一次请求，条数多时要限流（信号量或分批），
            //      不能对着列表直接 Parallel 发。
            return places;
        }

        /// <summary>
        /// JsonObject 的索引器碰到不存在的键会抛异常，这里统一兜一层。
        /// </summary>
        private static string ReadString(JsonObject item, string key)
        {
            if (item == null || !item.ContainsKey(key))
            {
                return String.Empty;
            }

            IJsonValue value = item[key];
            if (value.ValueType == JsonValueType.String)
            {
                return value.GetString();
            }
            if (value.ValueType == JsonValueType.Number)
            {
                return value.GetNumber().ToString();
            }

            return String.Empty;
        }

        private static string ReadAddress(JsonObject item)
        {
            if (item == null || !item.ContainsKey("address_obj"))
            {
                return String.Empty;
            }

            return ReadString(item["address_obj"].GetObject(), "address_string");
        }

        /// <summary>
        /// 没有 API Key 或请求失败时用的占位数据。
        /// </summary>
        private static IEnumerable<TripAdvisorPlace> GetPlaceholderPlaces()
        {
            return new List<TripAdvisorPlace>
            {
                new TripAdvisorPlace
                {
                    LocationId = "0",
                    Title = "隧道口观景台",
                    Subtitle = "Wawona Rd, Yosemite National Park, CA",
                    Description = "4.5 分 · 3,214 条点评",
                    ImagePath = PlaceholderImage,
                    Rating = 4.5,
                    ReviewCount = 3214
                },
                new TripAdvisorPlace
                {
                    LocationId = "1",
                    Title = "冰川点",
                    Subtitle = "Glacier Point Rd, Yosemite National Park, CA",
                    Description = "4.8 分 · 5,072 条点评",
                    ImagePath = PlaceholderImage,
                    Rating = 4.8,
                    ReviewCount = 5072
                },
                new TripAdvisorPlace
                {
                    LocationId = "2",
                    Title = "新娘面纱瀑布",
                    Subtitle = "Southside Dr, Yosemite National Park, CA",
                    Description = "4.3 分 · 1,896 条点评",
                    ImagePath = PlaceholderImage,
                    Rating = 4.3,
                    ReviewCount = 1896
                },
                new TripAdvisorPlace
                {
                    LocationId = "3",
                    Title = "马里波萨红杉林",
                    Subtitle = "Mariposa Grove Rd, Yosemite National Park, CA",
                    Description = "4.6 分 · 2,430 条点评",
                    ImagePath = PlaceholderImage,
                    Rating = 4.6,
                    ReviewCount = 2430
                }
            };
        }
    }
}
