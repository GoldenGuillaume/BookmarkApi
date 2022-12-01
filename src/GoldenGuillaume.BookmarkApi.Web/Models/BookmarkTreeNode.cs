namespace GoldenGuillaume.BookmarkApi.Web.Models
{
    public class BookmarkTreeNode
    {
        public string Id { get; set; } = null!;

        public BookmarkTreeNode[]? Children { get; set; }

        public long? DateAdded { get; set; }

        public long? DateGroupModified { get; set; }

        public int? Index { get; set; }

        public string? ParentId { get; set; }

        public string Title { get; set; } = null!;

        public string? Url { get; set; }

        public int CountChildrenElements()
        {
            int counter = 0;
            CountChildrenNodes(this, ref counter);
            return counter;
        }

        private void CountChildrenNodes(BookmarkTreeNode node, ref int count)
        {
            foreach (BookmarkTreeNode child in node.Children ?? Enumerable.Empty<BookmarkTreeNode>())
            {
                count++;
                CountChildrenNodes(child, ref count);
            }
        }
    }
}
