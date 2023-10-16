using System.Diagnostics.Metrics;
using System.Reflection;
using System.Reflection.Emit;
using Bogus;
using Bogus.DataSets;
using Npgsql;


class Person
{
    public Int32 Id { get; set; }
    public Guid TransportId { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public Int32 Age { get; set; }
    public string Phones { get; set; }
}

internal class PersonsGenerator
{
    public List<Person> persons = new List<Person>();
    Random random = new Random();

    PhoneNumbers phones = new PhoneNumbers();



    public PersonsGenerator(int count)
    {
        Faker name = new Faker();
        for (int i = 0; i < count; ++i)
        {
            persons.Add(new Person
            {
                Id = i,
                TransportId = Guid.NewGuid(),

                FirstName = name.Name.FirstName(),
                LastName = name.Name.LastName(),

                Age = name.Random.Number(10, 90),
                Phones = phones.PhoneNumber(),
            });
        }

    }
}
internal class ConsoleWriter<T> where T : class
{
    public ConsoleWriter(List<T> collection)
    {
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var o in collection)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                Console.WriteLine($"{properties[i].Name}:\t{properties[i].GetValue(o)}");

            }
            Console.WriteLine();
        }

    }
}
class Program
{
    static void Main(string[] args)
    {
        string connString = "Server=127.0.0.1;Port=5432;Database=csharpkt;User Id=postgres;Password=Pa$$w0rd";
        PersonsGenerator generator = new PersonsGenerator(10);
        using (NpgsqlConnection conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            string insertSql = $"INSERT INTO table1 (worker_id,new_id, First_Name, Second_Name, Age, phone) VALUES (@worker_id, @new_id, @First_Name, @Second_Name, @Age, @phone)";

            foreach (var person in generator.persons)
            {
                try
                {
                    if (person.Age > 14)
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@worker_id", person.Id);
                            cmd.Parameters.AddWithValue("@new_id", Guid.NewGuid());
                            cmd.Parameters.AddWithValue("@First_Name", person.FirstName);
                            cmd.Parameters.AddWithValue("@Second_Name", person.LastName);
                            cmd.Parameters.AddWithValue("@Age", person.Age);
                            cmd.Parameters.AddWithValue("@phone", person.Phones);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Age: {person.Age} for {person.FirstName} {person.LastName}. Skipping insertion.");
                    }
                }
                catch (WrongAge ex)
                {
                    Console.WriteLine("Возраст меньше 14 лет");
                }

            }
        }

        ConsoleWriter<Person> debuger = new ConsoleWriter<Person>(generator.persons);
        Console.WriteLine("Reflection is end. Push ENTER to continue...");

        Console.ReadKey();
    }
}
public class WrongAge : ApplicationException
{
    public WrongAge() { }
    public WrongAge(string message) : base(message) { }
    public WrongAge(string message, Exception ex) : base(message) { }
}