using HackerNewsKevinLong.Server;
using HackerNewsKevinLong.Server.Data;
using HackerNewsKevinLong.Server.Services;
using HackerNewsKevinLong.Server.Wrappers;
using Moq;
using System.Text.Json;

namespace Testing
{
    public class HackerNewsItemServiceTest
    {
        private HackerNewsItemService _Service;
        private readonly Mock<HttpClientWrapper> _MockClient;
        private readonly Mock<IMemoryCacheWrapper> _MockCache;
        int MockMaximumId = 7;

        HackerNewsItem validItem1 = new HackerNewsItem()
        {
            Id = 1,
            Text = "Test Mock Item 1.",
            Type = "story",
            Url = "https://validurl.com"
        };

        HackerNewsItem validItem2_uppercase = new HackerNewsItem()
        {
            Id = 2,
            Text = "TEST MOCK ITEM 2 WITH ALL UPPERCASE.",
            Type = "story",
            Url = "https://validurl.com"
        };

        HackerNewsItem validItem3_lowercase = new HackerNewsItem()
        {
            Id = 3,
            Text = "test mock item 3 which is all lower case.",
            Url = "https://validurl.com",
            Type = "story"
        };

        HackerNewsItem validItem4_prepended = new HackerNewsItem()
        {
            Id = 4,
            Text = "!/dkgsokgdfs test mock item 4 which is prepended with nonsense.",
            Url = "https://validurl.com",
            Type = "story"
        };

        HackerNewsItem validItem5_missingTheWordItem = new HackerNewsItem()
        {
            Id = 5,
            Text = "Test Mock 5 which is missing a word which each other entry contains.",
            Url = "https://validurl.com",
            Type = "story"
        };

        HackerNewsItem invalidItem1_noUrl = new HackerNewsItem()
        {
            Id = 6,
            Text = "Test Mock Item 6 which has no url.",
            Type = "story"
        };

        HackerNewsItem invalidItem2_wrongType = new HackerNewsItem()
        {
            Id = 7,
            Text = "Test Mock Item 7 which is of the wrong type.",
            Url = "https://validurl.com",
            Type = "comment"
        };

        List<HackerNewsItem> mockItems = new List<HackerNewsItem>();

        public HackerNewsItemServiceTest()
        {
            _MockClient = new Mock<HttpClientWrapper>(new HttpClient());
            _MockCache = new Mock<IMemoryCacheWrapper>();

            mockItems.Add(validItem1);
            mockItems.Add(validItem2_uppercase);
            mockItems.Add(validItem3_lowercase);
            mockItems.Add(validItem4_prepended);
            mockItems.Add(validItem5_missingTheWordItem);
            mockItems.Add(invalidItem1_noUrl);
            mockItems.Add(invalidItem2_wrongType);

            foreach (HackerNewsItem item in mockItems)
            {
                SetupMockItem(item);
            }

            _MockClient.Setup(x => x.GetStringAsync(Constants.MaxIdUrl)).Returns(Task.FromResult(MockMaximumId.ToString()));

            _Service = new HackerNewsItemService(_MockClient.Object, _MockCache.Object);
        }

        private void SetupMockItem(HackerNewsItem item)
        {
            string mockUrl = String.Format(Constants.HackerNewsItemUrl, item.Id);

            string mockItemString = JsonSerializer.Serialize(item);

            _MockClient.Setup(x => x.GetStringAsync(mockUrl)).Returns(Task.FromResult(mockItemString));
        }

        [Fact]
        public async void SearchItems_BasicSuccess()
        {
            var latestItems = await _Service.SearchItems(MockMaximumId);

            Assert.Contains(validItem1, latestItems);
            Assert.Contains(validItem2_uppercase, latestItems);
            Assert.Contains(validItem3_lowercase, latestItems);
            Assert.Contains(validItem4_prepended, latestItems);
            Assert.Contains(validItem5_missingTheWordItem, latestItems);
            Assert.DoesNotContain(invalidItem1_noUrl, latestItems);
            Assert.DoesNotContain(invalidItem2_wrongType, latestItems);
        }

        [Fact]
        public async void SearchItems_UseStartIndex()
        {
            var latestItems = await _Service.SearchItems(MockMaximumId, 3);
            Assert.Contains(validItem1, latestItems);
            Assert.Contains(validItem2_uppercase, latestItems);
            Assert.Contains(validItem3_lowercase, latestItems);
            Assert.DoesNotContain(validItem4_prepended, latestItems);
            Assert.DoesNotContain(validItem5_missingTheWordItem, latestItems);
            Assert.DoesNotContain(invalidItem1_noUrl, latestItems);
            Assert.DoesNotContain(invalidItem2_wrongType, latestItems);
        }

        [Fact]
        public async void SearchItems_LimitedCount()
        {
            var latestItems = await _Service.SearchItems(2);
            Assert.DoesNotContain(validItem1, latestItems);
            Assert.DoesNotContain(validItem2_uppercase, latestItems);
            Assert.DoesNotContain(validItem3_lowercase, latestItems);
            Assert.Contains(validItem4_prepended, latestItems);
            Assert.Contains(validItem5_missingTheWordItem, latestItems);
            Assert.DoesNotContain(invalidItem1_noUrl, latestItems);
            Assert.DoesNotContain(invalidItem2_wrongType, latestItems);
        }

        [Fact]
        public async void SearchItems_FilterBySearchText()
        {
            var latestItems = await _Service.SearchItems(MockMaximumId, null, "item");
            Assert.Contains(validItem1, latestItems);
            Assert.Contains(validItem2_uppercase, latestItems);
            Assert.Contains(validItem3_lowercase, latestItems);
            Assert.Contains(validItem4_prepended, latestItems);
            Assert.DoesNotContain(validItem5_missingTheWordItem, latestItems);
            Assert.DoesNotContain(invalidItem1_noUrl, latestItems);
            Assert.DoesNotContain(invalidItem2_wrongType, latestItems);
        }

        [Fact]
        public async void SearchItems_FailWhenItemCountZeroOrBelow()
        {
            await Assert.ThrowsAnyAsync<Exception>(async () => { await _Service.SearchItems(0); });
            await Assert.ThrowsAnyAsync<Exception>(async () => { await _Service.SearchItems(-1); });
        }
    }
}