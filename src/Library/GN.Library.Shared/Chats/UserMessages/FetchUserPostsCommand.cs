using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats.UserMessages
{
    public class FetchUserPostsCommand
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class FetchUserPostsResponse
    {
        public PostEntity[] Posts { get; set; }
    }
}
