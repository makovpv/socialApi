using socialApi.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;

namespace socialApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var mySetting = ConfigurationSettings.AppSettings;

            var vkProvider = new VKProvider(
                Convert.ToUInt64(mySetting["appId"]),
                mySetting["login"],
                mySetting["password"],
                new ConsoleLogger());

            //Console.WriteLine("get persons");
            //var persons = vkProvider.GetPersons(new List<string> {
            //    //"habr",
            //    "public31836774"
            //});

            var groups= vkProvider.GetGroupsInfo(LoadBaseGroups(mySetting["keyGroupsFile"]));

            
            //WriteOut(persons);
            //SaveToCSV(persons);

            Console.WriteLine("finished");
            Console.Read();

//            пользователь
//кол - во постов
//  и перепостов за все время



//кол - во лайков(или перепостов) постов
//кол - во лайков постов перепостов в разрезе групп

//пол
//возраст
        }

        private static List<string> LoadBaseGroups(string filePath)
        {
            Console.WriteLine("load groups");

            var groupFile = File.ReadAllLines(filePath);
            var groupList = new List<string>(groupFile);

            return groupList;
        }

        static void WriteOut(List<Person> persons)
        {
            foreach (var person in persons)
            {
                Console.WriteLine($"{person.Id} {person.FirstName} {person.LastName} {person.Gender} {person.BirthDate} {person.Religion}");
                foreach (var repost in person.GroupReposts)
                {
                    Console.WriteLine($"{repost.Key} {repost.Value} reposts");
                }
            }
        }

        static void SaveToCSV(List<Person> persons)
        {
            Console.WriteLine("Save");

            StringBuilder sb = new StringBuilder();
            StringBuilder sbReposts = new StringBuilder();

            sb.AppendLine("user Id;FirstName;LastName;Gender;Age;Religion");
            sbReposts.AppendLine("user Id;owner Id;reposts");

            foreach (var person in persons)
            {
                sb.AppendLine($"{person.Id};{person.FirstName};{person.LastName};{person.Gender};{person.Age};{person.Religion}");
                foreach (var repost in person.GroupReposts)
                {
                    sbReposts.AppendLine($"{person.Id};{repost.Key};{repost.Value}");
                }
            }

            File.WriteAllText(@"c:\projects\data\persons.csv", sb.ToString());
            File.WriteAllText(@"c:\projects\data\reposts.csv", sbReposts.ToString());
        }
    }
}
