using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    // バナー広告取得（OG Metadata）
    public partial class MetamorphoseWindow
    {
        private readonly List<BannerCardState> _bannerCardStates = new();
        private bool _bannerLoading;

        private struct BannerCardState
        {
            public string url;
            public string title;
            public Texture2D image;
            public bool loaded;
            public bool failed;
        }

        #region Banner (OG Metadata)

        private void LoadBannerCards()
        {
            _bannerCardStates.Clear();
            _bannerLoading = false;

            var container = _root.Q<VisualElement>("banner-cards");
            if (container == null) return;
            container.Clear();

            var urls = _target.bannerAdUrls;
            if (urls == null || urls.Length == 0)
                urls = DefaultBannerUrls;

            container.style.display = DisplayStyle.Flex;

            foreach (var url in urls)
            {
                if (string.IsNullOrWhiteSpace(url)) continue;

                _bannerCardStates.Add(new BannerCardState { url = url });
                RenderBannerCard(container, _bannerCardStates.Count - 1);
            }

            FetchBannerMetadata();
        }

        private void RenderBannerCard(VisualElement container, int index)
        {
            var state = _bannerCardStates[index];

            var card = new VisualElement();
            card.AddToClassList("banner-card");

            var img = new Image();
            img.AddToClassList("banner-card-image");
            if (state.image != null)
                img.image = state.image;
            card.Add(img);

            var lbl = new Label();
            lbl.AddToClassList("banner-card-label");
            if (state.loaded)
                lbl.text = state.title;
            else if (state.failed)
                lbl.text = "Failed to load";
            else
                lbl.text = "Loading...";
            card.Add(lbl);

            var capturedUrl = state.url;
            card.AddManipulator(new Clickable(() => Application.OpenURL(capturedUrl)));

            container.Add(card);
        }

        private async void FetchBannerMetadata()
        {
            if (_bannerLoading) return;
            _bannerLoading = true;

            var container = _root.Q<VisualElement>("banner-cards");
            if (container == null) { _bannerLoading = false; return; }

            for (int i = 0; i < _bannerCardStates.Count; i++)
            {
                if (_bannerCardStates[i].loaded || _bannerCardStates[i].failed) continue;

                var url = _bannerCardStates[i].url;
                try
                {
                    var (title, imageUrl) = await FetchOgMetadataAsync(url);
                    Texture2D tex = null;

                    if (!string.IsNullOrEmpty(imageUrl))
                        tex = await DownloadImageAsync(imageUrl);

                    _bannerCardStates[i] = new BannerCardState
                    {
                        url = url,
                        title = title ?? url,
                        image = tex,
                        loaded = true,
                        failed = false,
                    };
                }
                catch
                {
                    _bannerCardStates[i] = new BannerCardState
                    {
                        url = url,
                        title = url,
                        loaded = false,
                        failed = true,
                    };
                }

                RefreshBannerCard(container, i);
            }

            _bannerLoading = false;
        }

        private void RefreshBannerCard(VisualElement container, int index)
        {
            if (index >= container.childCount) return;
            var card = container[index];
            if (card == null) return;

            var state = _bannerCardStates[index];

            var img = card.Q<Image>(className: "banner-card-image");
            if (img == null)
            {
                img = card.Query<Image>().First();
            }
            if (img != null && state.image != null)
                img.image = state.image;

            var lbl = card.Q<Label>(className: "banner-card-label");
            if (lbl == null)
            {
                lbl = card.Query<Label>().First();
            }
            if (lbl != null)
            {
                if (state.loaded)
                    lbl.text = state.title;
                else if (state.failed)
                    lbl.text = "Failed";
            }
        }

        private static async Task<(string title, string imageUrl)> FetchOgMetadataAsync(string url)
        {
            using var client = new HttpClient();
            client.Timeout = System.TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            var response = await client.GetStringAsync(url);
            var html = response;

            string title = ExtractMetaContent(html, "og:title")
                        ?? ExtractMetaContent(html, "twitter:title")
                        ?? ExtractTitleTag(html)
                        ?? url;

            string imageUrl = ExtractMetaContent(html, "og:image")
                           ?? ExtractMetaContent(html, "twitter:image")
                           ?? "";

            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("//"))
                imageUrl = "https:" + imageUrl;
            else if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("/"))
                imageUrl = new System.Uri(url).GetLeftPart(System.UriPartial.Scheme) + imageUrl;

            return (title, imageUrl);
        }

        private static async Task<Texture2D> DownloadImageAsync(string url)
        {
            using var client = new HttpClient();
            client.Timeout = System.TimeSpan.FromSeconds(10);

            var bytes = await client.GetByteArrayAsync(url);

            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(bytes))
            {
                Object.DestroyImmediate(tex);
                return null;
            }
            return tex;
        }

        private static string ExtractMetaContent(string html, string property)
        {
            var pattern = $@"<meta[^>]+(?:property|name)=[""']{Regex.Escape(property)}[""'][^>]+content=[""']([^""']+)[""']";
            var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                pattern = $@"<meta[^>]+content=[""']([^""']+)[""'][^>]+(?:property|name)=[""']{Regex.Escape(property)}[""']";
                match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            }
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string ExtractTitleTag(string html)
        {
            var match = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        #endregion
    }
}
