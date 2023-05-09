using Npgsql;
using Dapper;
using Newtonsoft.Json;

namespace name1
{
public class name2
{
    static async Task Main()
    {
        var connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=abcd1234;Port=5430;";
        var connection = new NpgsqlConnection(connectionString);

        Console.Write("How many users do you want to get(<=30): ");

        string? usersToGetString = Console.ReadLine();
        int usersToGet = UsersToGetFunction(usersToGetString);

        if (usersToGet != 0)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"https://randomuser.me/api/?inc=id,name,email,picture,nat=US&results={usersToGet}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("The request was unseccessful. Try again later");
                return;
            }

            var content = await response.Content.ReadAsStringAsync();

            
            RootObject? rootObject = JsonConvert.DeserializeObject<RootObject>(content);

            if (rootObject == null)
            {
                Console.WriteLine("The content is null.");
                return;
            }

            List<Result>? users = rootObject.results;

            connection.Open();

            foreach (Result user in users ?? Enumerable.Empty<Result>())
            {
                Guid tempUUID = GenerateUUID();
                HttpClient httpClient2 = new HttpClient();
                byte[] imageData = await httpClient2.GetByteArrayAsync($"{user?.picture?.medium}");
                
                connection.Execute($"INSERT INTO users (id, firstname, lastname, email, image) VALUES (@uuid, @firstname, @lastname, @email, @picture)",
                   new { uuid = tempUUID, firstname = user?.name?.first, lastname = user?.name?.last, email = user?.email, picture = imageData});
            }

            connection.Close();
        }

 
        return;
    }

    // Creates UUIDS
    private static Guid GenerateUUID()
    {
        return Guid.NewGuid();
    }

    // Transforms the usersToGet string to an integer.
    private static int UsersToGetFunction(string? inputString)
    {
        if (string.IsNullOrWhiteSpace(inputString))
        {
            Console.WriteLine("You will not get any users.");
            return 0;
        }

        int intValue;

        int counter = 4;
        while (!int.TryParse(inputString, out intValue))
        {

            if (counter == 0)
            {
                Console.WriteLine("You are out of attempts. You will not get any users.");
                return 0;
            }
            
            Console.Write($"You've not inputed a valid integer number. Please try again ({counter} attempts left): ");
            inputString = Console.ReadLine();

            counter--;
        }

        if (intValue < 0 )
        {
            Console.WriteLine("You've inputed a negative number. You will not get any users.");
            return 0;
        }

        if (intValue > 30)
        {
            Console.WriteLine("You've surpassed the maximum get users amount and thus you'll get 30.");
            return 30;
        }

        return intValue;
    }
    
}

public class RootObject
{
    public List<Result>? results { get; set; }
    public Info? info { get; set; }
}

public class Result
{
    public Name? name { get; set; }
    public string? email { get; set; }
    public Id? id { get; set; }
    public Picture? picture { get; set;}
}

public class Name
{
    public string? title { get; set; }
    public string? first { get; set; }
    public string? last { get; set; }
}

public class Id
{
    public string? name { get; set; }
    public string? value { get; set; }
}

public class Picture
{
    public string? large { get; set; }
    public string? medium { get; set; }
    public string? thumbnail { get; set; }
}

public class Info
{
    public string? seed { get; set; }
    public int results { get; set; }
    public int page { get; set; }
    public string? version { get; set; }
}


}

