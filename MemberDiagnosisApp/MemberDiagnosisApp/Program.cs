using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MemberDiagnosisApp;

class Program
{
    static string SetConfig()
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", false, true);

        IConfiguration config = builder.Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        return connectionString;

    }

    static List<MemberDiagnosisData> GetMemberDiagnosisData(int memberId, string connectionString)
    {
        List<MemberDiagnosisData> memberDiagnosisData = new List<MemberDiagnosisData>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            using (SqlCommand command = new SqlCommand("MemberDiagnosisStoredProc", conn))
            {
                try
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@MemberID", System.Data.SqlDbType.Int).Value = memberId;
                    conn.Open();
                    SqlDataReader rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        var memberDataRow = new MemberDiagnosisData();
                        memberDataRow.FirstName = rdr["FirstName"].ToString();
                        memberDataRow.LastName = rdr["LastName"].ToString();
                        memberDataRow.MostSevereDiagnosisId = string.IsNullOrEmpty(rdr["Most Severe Diagnosis Id"].ToString()) ? "NULL" : rdr["Most Severe Diagnosis Id"].ToString();
                        memberDataRow.MostSevereDiagnosisDescription = string.IsNullOrEmpty(rdr["Most Severe Diagnosis Description"].ToString()) ? "NULL" : rdr["Most Severe Diagnosis Description"].ToString();
                        memberDataRow.CategoryID = string.IsNullOrEmpty(rdr["CategoryID"].ToString()) ? "NULL": rdr["CategoryID"].ToString();

                        memberDataRow.CategoryDescription = string.IsNullOrEmpty(rdr["CategoryDescription"].ToString()) ? "NULL" : rdr["CategoryDescription"].ToString();

                        memberDataRow.CategoryScore = string.IsNullOrEmpty(rdr["CategoryScore"].ToString()) ? "NULL": rdr["CategoryScore"].ToString();

                        memberDataRow.IsMostSevereCategory = Convert.ToBoolean(rdr["IsMostSevereCategory"].ToString());
                        memberDiagnosisData.Add(memberDataRow);

                    }
                }
                catch (Exception ex)
                {
                    //log errors here
                }
            }
        }

        return memberDiagnosisData;
    }

    static void PrintMemberDiagnosisData(List<MemberDiagnosisData> memberData)
    {
        Console.WriteLine($"First Name: {memberData[0].FirstName}, Last Name: {memberData[0].LastName}");

        foreach(var row in memberData)
        {
            Console.WriteLine($"Most Severe Diagnosis ID: {row.MostSevereDiagnosisId}");
            Console.WriteLine($"Most Severe Diagnosis Description: {row.MostSevereDiagnosisDescription}");
            Console.WriteLine($"CategoryID: {row.CategoryID}");
            Console.WriteLine($"Category Description: {row.CategoryDescription}");
            Console.WriteLine($"CategoryScore: {row.CategoryScore}");
            Console.WriteLine($"Is Most Severe Category: {row.IsMostSevereCategory} -- {Convert.ToInt16(row.IsMostSevereCategory)}");
            Console.WriteLine("=================");

        }
    }
    static void Main(string[] args)
    {
        
        string SQLConnectionString = SetConfig();

        Console.WriteLine("Please input a Member ID");
        string userInput = Console.ReadLine();

        int memberId;


        while (!Int32.TryParse(userInput, out memberId))
        {
            Console.WriteLine("Not a valid MemberID. Please enter a numeric Member ID.");

            userInput = Console.ReadLine();
        }
           
        var results = GetMemberDiagnosisData(memberId, SQLConnectionString);
        PrintMemberDiagnosisData(results);

    }

    
}

class MemberDiagnosisData
{
    public int MemberId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MostSevereDiagnosisId { get; set; }
    public string MostSevereDiagnosisDescription { get; set; }
    public string CategoryID { get; set; }
    public string CategoryDescription { get; set; }
    public string CategoryScore { get; set; }
    public bool? IsMostSevereCategory { get; set; }
}

