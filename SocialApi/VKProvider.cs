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
                        result.Add(GetPersonWithReposts(user));
                    }
                }
            }

            return result;
        }

        public Person GetPerson(long userId)
        {
            return GetPersonWithReposts(api.Users.Get(userId, ProfileFields.BirthDate | ProfileFields.Sex | ProfileFields.StandInLife));
        }

        public Person GetPerson(string screenName)
        {
            return GetPersonWithReposts(api.Users.Get(screenName, ProfileFields.BirthDate | ProfileFields.Sex));
        }

        private Person GetPersonWithReposts(VkNet.Model.User user)
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

            try
            {
                var posts = GetaAllWallPosts(user.Id);

                foreach (var group in
                    posts.Where(p =>
                        p.CopyHistory.Count > 0
                        && p.Date.HasValue
                        && DateTime.Today.Subtract(p.Date.Value).TotalDays <= 365)

                        .GroupBy(p => p.CopyHistory.First().OwnerId)
                        .Select(group => new
                        {
                            OwnerId = -group.Key,
                            Count = group.Count()
                        }))
                {
                    person.GroupReposts.Add(new GroupCounter
                    {
                        GroupId = group.OwnerId,
                        CounterValue = group.Count
                    });
                }

                m_logger.Log($"{person.GroupReposts.Count} groups were found");
            }
            catch (Exception ex)
            {
                m_logger.Log($"ERROR: {ex.Message}");
            }

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


        private IEnumerable<VkNet.Model.Post> GetaAllWallPosts(long userId)
        {
            const int maxPortionSize = 100;

            List<VkNet.Model.Post> result = new List<VkNet.Model.Post>();

            VkNet.Model.WallGetObject wgo;
            ulong receivedPostCount = 0;

            do
            {
                wgo = api.Wall.Get(new WallGetParams
                {
                    OwnerId = userId,
                    Extended = true,
                    Offset = receivedPostCount,
                    Count = maxPortionSize
                });

                result.AddRange(wgo.WallPosts);

                receivedPostCount += Convert.ToUInt64(wgo.WallPosts.Count);
            }
            while (wgo.TotalCount > receivedPostCount);

            return result;

        }

        public List<Group> GetGroupsInfo(List<string> groupNames)
        {
            var groups = api.Groups.GetById(groupNames, null, GroupsFields.Activity);

            return
                groups.Select(g => new Group
                {
                    Id = g.Id,
                    Name = g.Name,
                    Activity = g.Activity
                }).ToList();

        }
    }
}
