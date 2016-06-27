using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace ContractorsParser.Models
{
    // Класс, хранящий краткую информацию о контрагенте
    public class ContractorShortInfo
    {
        public Int32 Id { get; set; }
        public string Name { get; set; }
    }
    
    // Класс, реализующий сущность "Контрагент"
    public class Contractor : ContractorShortInfo
    {
        public Int64 Inn { get; set; }
        public string PaymentAccount { get; set; }

        public Contractor()
        {
        }

        public Contractor(string name, Int64 inn, string payment_account)
        {
            Name = name;
            Inn = inn;
            PaymentAccount = payment_account;
        }
    }

    // Класс-провайдер для работы с базой данных контрагентов
    public class ContractorRepository
    {
        private IDbConnection db;
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ContractorRepository()
        {
            db = new SqlConnection(connectionString);
        }

        public List<ContractorShortInfo> GetAll()
        {
            return db.Query<ContractorShortInfo>("SELECT id, name FROM contractors").ToList();
        }

        public Contractor Get(int id)
        {
            return db.Query<Contractor>("SELECT * FROM contractors WHERE id = @id", new { id }).FirstOrDefault();
        }

        public int Insert(Contractor contractor)
        {
            // Контрагент с необходимым inn уже может быть в базе.
            // Поэтому сначала пытаюсь его найти, и если он есть, то возвращаю id записи.
            // Если контрагента нет, то добавляю запись в базу и возвращаю id добавленной записи
            string query =
                "DECLARE @row_id INT;" +
                "SELECT @row_id = 0;" +
                "SELECT @row_id = id FROM contractors WHERE inn = @inn;" +
                "IF @row_id = 0 " +
                    "INSERT INTO contractors (name, inn, paymentAccount) OUTPUT INSERTED.id VALUES(@name, @inn, @paymentAccount) " +
                "ELSE " +
                    "SELECT @row_id";
            return db.Query<int>(query, contractor).FirstOrDefault();
        }

        public bool InsertList(ref List<Contractor> contractors)
        {
            for (int i = 0; i < contractors.Count; ++i)
            {
                try
                {
                    contractors[i].Id = Insert(contractors[i]);
                }
                catch
                {
                    return false;
                }
                
            }

            return true;
        }
    }
}