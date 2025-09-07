using System.Text.Json;

namespace HackerNewsKevinLong.Server.Data
{
    public class HackerNewsItem
    {
        public int Id { get; set; }
        public bool Deleted { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
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
