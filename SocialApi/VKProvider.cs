using socialApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace socialApi
{
    public class VKProvider
    {
        private VkApi api = new VkApi();

        private ILogger m_logger;

        public VKProvider(ulong appId, string login, string pasword, ILogger logger)
        {
            m_logger = logger;

            api.Authorize(new ApiAuthParams
            {
                ApplicationId = appId,
                Login = login,
                Password = pasword,
                Settings = Settings.All
            });
        }

        public List<Person> GetPersons(List<string> groupNames)
        {
            var result = new List<Person>();

            foreach (var groupName in groupNames)
            {
                var group = api.Utils.ResolveScreenName(groupName);

                if (group != null && group.Id.HasValue)
                {
                    var users = api.Groups.GetMembers(new GroupsGetMembersParams
                    {
                        GroupId = group.Id.Value.ToString(),
                        //Fields = UsersFields.Sex | UsersFields.BirthDate,
                        Fields = UsersFields.All,
                        Count = 50
                    });

                    foreach (var user in users)
                    {
                        m_logger.Log(user.LastName);
                        result.Add(GetPerson(user));
                    }
                }
            }

            return result;
        }

        public Person GetPerson(long userId)
        {
            return GetPerson(api.Users.Get(userId, ProfileFields.BirthDate | ProfileFields.Sex | ProfileFields.StandInLife));
        }

        public Person GetPerson(string screenName)
        {
            return GetPerson(api.Users.Get(screenName, ProfileFields.BirthDate | ProfileFields.Sex));
        }

        private Person GetPerson(VkNet.Model.User user)
        {
            var person = new Person
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = (int)user.Sex,
                Religion = user.StandInLife?.Religion
            
            };

            if (DateTime.TryParse(user.BirthDate, out DateTime bdate))
                person.BirthDate = bdate;

            //var likes = api.Likes.GetList(new LikesGetListParams
            //{
            //    Type = LikeObjectType.Photo,
            //    ItemId = 342076961,
            //    OwnerId = user.Id,
            //    Filter = LikesFilter.Likes,
            //    Count = 50
            //});

            //var favs = api.Fave.GetPosts();

            try
            {
                var wall = api.Wall.Get(new WallGetParams
                {
                    OwnerId = user.Id,
                    Extended = true,
                    Count = 100
                });
            

            //var wall2 = api.Wall.Get(new WallGetParams
            //{
            //    OwnerId = user.Id,
            //    Offset = 101,
            //    Extended = true,
            //    Count = 100
            //});

                foreach (var group in
                    wall.WallPosts.Where(p =>
                        p.CopyHistory.Count > 0
                        && p.Date.HasValue
                        && DateTime.Today.Subtract(p.Date.Value).TotalDays <= 365)

                        .GroupBy(p => p.CopyHistory.First().OwnerId)
                        .Select(group => new
                        {
                            Metric = -group.Key,
                            Count = group.Count()
                        }))
                {
                    person.GroupReposts.Add(group.Metric, group.Count);
                }

            }
            catch { }
            //foreach ( var dfdf in wall.WallPosts)

            //var personGroups = api.Groups.Get(new GroupsGetParams
            //{
            //    UserId = user.Id,
            //    //Extended = true
            //});

            //var folowers = api.Users.GetFollowers(user.Id);

            //var subscriptions = api.Users.GetSubscriptions(user.Id);

            //var qq = api.Groups.GetById("flightradar24", GroupsFields.All).Activity;

            return person;
        }
    }
}
