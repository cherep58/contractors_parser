using ContractorsParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Internal;
using System.Web.Http;

namespace ContractorsParser.Controllers
{
    public class ContractorsController : ApiController
    {
        // GET /api/Contractors/
        public IEnumerable<ContractorShortInfo> GetAllContractors()
        {
            var result = new List<ContractorShortInfo>();
            try
            {
                var repository = new ContractorRepository();
                result = repository.GetAll();
            }
            catch
            {
                ThrowError("Ошибка при работе с базой");
            }

            return result;
        }

        // GET /api/Contractors/id
        public Contractor GetContractor(int id)
        {
            var result = new Contractor();
            try
            {
                var repository = new ContractorRepository();
                return repository.Get(id);
            }
            catch
            {
                ThrowError("Ошибка при работе с базой");
            }

            return result;
        }

        // POST /api/Contractors/
        public IEnumerable<Contractor> PostContractors()
        {
            List<Contractor> result = new List<Contractor>();

            System.Web.HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;
            for (int i = 0; i < files.Count; ++i)
            {
                System.Web.HttpPostedFile file = files[i];

                // выполняю парсинг файла
                var parser = new ImportFileParser(file.InputStream);
                if (parser.Parse() == false)
                {
                    ThrowError("Не удалось спрасить содержимое файла");
                }

                // выполяню парсинг контрагентов из документов
                var contractors_parser = new ContractorParser();
                if (contractors_parser.Parse(parser.Documents) == false)
                {
                    ThrowError("Не удалось спрасить данные о контрагентах");
                }

                // сохраняю контрагентов в базе
                var repository = new ContractorRepository();
                var contractors = contractors_parser.GetContractorsList();
                if (repository.InsertList(ref contractors) == false)
                {
                    ThrowError("Не удалось записать данные о контрагентах в базу");
                }

                result.AddRange(contractors);
            }

            return result;
        }

        [NonAction]
        private void ThrowError(string message)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(message)
            };
            throw new HttpResponseException(resp);
        }
    }
}
