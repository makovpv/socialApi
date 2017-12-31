using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socialApi.Model
{
    public class Person
    {


        public long Id { get; set; }
        public int Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Religion { get; set; }

        public int LikeCount { get; set; }
        public int PostCount { get; set; }
        public int RepostCount { get; set; }
        
        public Dictionary<int, int> GroupLikes { get; set; }
        public Dictionary<int, int> GroupPosts { get; set; }
        public List<GroupCounter> GroupReposts { get; set; }

        public Person()
        {
            GroupReposts = new List<GroupCounter>();
        }

        public int? Age { get
            {
                return BirthDate.HasValue ? (int)DateTime.Today.Subtract(BirthDate.Value).TotalDays / 365 : (int?)null;
            }
        }
    }
}
