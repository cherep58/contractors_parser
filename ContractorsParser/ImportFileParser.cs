using ContractorsParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace ContractorsParser
{
    public class ParserConstants
    {
        public const string kDocumentStart = "СекцияДокумент";
        public const string kDocumentEnd = "КонецДокумента";
        public const string kFileEnd = "КонецФайла";

        public const string kPayerNameField = "Плательщик1";
        public const string kPayerInnField = "ПлательщикИНН";
        public const string kPayerPaymentAccountField = "ПлательщикРасчСчет";

        public const string kReceiverNameField = "Получатель1";
        public const string kReceiverInnField = "ПолучательИНН";
        public const string kReceiverPaymentAccountField = "ПолучательРасчСчет";
    }
    
    // Класс, реализующий сущность "платежный документ"
    public class PaymentDocument
    {
        // Название документа
        public string Name { get; set; }
        
        // список служебных строк и их значений, указанных в документе
        public Dictionary<string, string> Values { get; set; }

        public PaymentDocument(string DocumentName)
        {
            Values = new Dictionary<string, string>();
            Name = DocumentName;
        }

        // Функция выполняет парсинг документа.
        // Функция парсит все служебные строки до строки "КонецДокумента"
        public bool Parse(StreamReader stream)
        {
            while (!stream.EndOfStream)
            {
                var line = stream.ReadLine().Split(new Char[]{'='}, 2);
                if (line[0] == ParserConstants.kDocumentEnd)
                {
                    // Достигнут конец документа
                    return true;
                }
                
                Values[line[0]] = line[1].Trim();
            }

            // Конец документ не найден. Ошибка парсинга
            Clear();
            return false;
        }

        public void Clear()
        {
            Name = "";
            Values.Clear();
        }
    }

    // Класс, позволяющий спарсить список контрагентов из платежного документа
    public class ContractorParser
    {
        // Храню список контрагентов в словаре, где ключем будет являтся ИНН контрагента.
        // Данный формат хранения позволит избежать создание дубликатов
        private Dictionary<Int64, Contractor> Contractors { set; get; }

        public ContractorParser()
        {
            Contractors = new Dictionary<Int64, Contractor>();
        }

        public List<Contractor> GetContractorsList()
        {
            return Contractors.Values.ToList();
        }

        // Функция выполняет парсинг контрагента из документа.
        // Функция принимает имена полей, в которых хранятся необходимые данные
        private bool Parse(PaymentDocument document, string FieldName, string FieldInn, string FieldPaymentAccount)
        {
            if (!document.Values.ContainsKey(FieldName) ||
                !document.Values.ContainsKey(FieldInn) ||
                !document.Values.ContainsKey(FieldPaymentAccount))
            {
                return false;
            }

            Int64 inn;
            try {
                inn = Convert.ToInt64(document.Values[FieldInn]);
            }
            catch {
                return false;
            }

            if (Contractors.ContainsKey(inn))
            {
                return true;
            }

            Contractors[inn] = new Contractor(
                document.Values[FieldName],
                inn,
                document.Values[FieldPaymentAccount]);
            return true;
        }

        // Функция выполняет парсинг контрагентов из платежного документа
        public bool Parse(PaymentDocument document)
        {
            // выполняю парсинг контрагента из поля "Плательщик"
            if (Parse(
                    document, 
                    ParserConstants.kPayerNameField, 
                    ParserConstants.kPayerInnField, 
                    ParserConstants.kPayerPaymentAccountField) == false)
            {
                return false;
            }

            // выполняю парсинг контрагента из поля "Получатель"
            return Parse(
                document, 
                ParserConstants.kReceiverNameField, 
                ParserConstants.kReceiverInnField, 
                ParserConstants.kReceiverPaymentAccountField);
        }

        // Функция выполняет парсинг контрагентов из списка платежных документов
        public bool Parse(List<PaymentDocument> documents)
        {
            foreach (var document in documents)
            {
                if (Parse(document) == false)
                {
                    Contractors.Clear();
                    return false;
                }
            }

            return true;
        }
    }
    
    // Класс, выполняющий парсинг импортируемого файла.
    // В связи с заданием, парсинг служебной информации файла не реализован.
    public class ImportFileParser
    {
        private StreamReader stream;

        public List<PaymentDocument> Documents { get; set; }

        public ImportFileParser(Stream input_stream)
        {
            stream = new StreamReader(input_stream, System.Text.Encoding.Default, true);
            Documents = new List<PaymentDocument>();
        }

        // Функция выполняет парсинг всех платежных документов из файла
        public bool Parse()
        {
            while (!stream.EndOfStream)
            {
                var LineParts = stream.ReadLine().Split(new Char[] { '=' }, 2);
                if (LineParts[0] == ParserConstants.kDocumentStart)
                {
                    // начало документа
                    var document = new PaymentDocument(LineParts[1]);
                    if (document.Parse(stream) == false)
                    {
                        // ошибка при парсинге документ
                        Documents.Clear();
                        return false;
                    }
                    Documents.Add(document);
                }
                else if (LineParts[0] == ParserConstants.kFileEnd)
                {
                    // Файл закончился
                    return true;
                }
            }

            return false;
        }
    }
}