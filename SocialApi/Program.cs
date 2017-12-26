using socialApi.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace socialApi
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.Configuration.AppSettingsReader
            var mySetting = ConfigurationSettings.AppSettings;

            var vkProvider = new VKProvider(
                Convert.ToUInt64(mySetting["appId"]),
                mySetting["login"],
                mySetting["password"],
                new ConsoleLogger());

            //var persons = new List<Person> { vkProvider.GetPerson(463871143) }; // me

            //var persons = new List<Person> { vkProvider.GetPerson("caterina_03") };

            //var persons = new List<Person> { vkProvider.GetPerson("makovsv") }; // bro

            //1.Ёбаный пиздец http://vk.com/public12382740
            //2.Смейся до слёз :D https://vk.com/public26419239
            //3.Чёткие приколы https://vk.com/public31836774

            var persons = vkProvider.GetPersons(new List<string> {
                //"habr",
                "public31836774"
            });

            Console.WriteLine("Save");
            //WriteOut(persons);
            SaveToCSV(persons);

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

            System.IO.File.WriteAllText(@"c:\projects\data\persons.csv", sb.ToString());
            System.IO.File.WriteAllText(@"c:\projects\data\reposts.csv", sbReposts.ToString());
        }
    }
}
