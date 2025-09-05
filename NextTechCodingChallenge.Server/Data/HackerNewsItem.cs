using System.Text.Json;

namespace HackerNewsKevinLong.Server.Data
{
    public class HackerNewsItem
    {
        public int Id { get; set; }
        public bool Deleted { get; set; }
        public string Type { get; set; }
        public string By { get; set; }
        public long Time { get; set; }
        public string Text { get; set; }
        public bool Dead { get; set; }
        public int? Parent { get; set; }
        public List<int> Kids { get; set; }
        public string Url { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as HackerNewsItem;

            if (item == null)
            {
                return false;
            }

            return JsonSerializer.Serialize(this) == JsonSerializer.Serialize(item);
        }

    }
}
