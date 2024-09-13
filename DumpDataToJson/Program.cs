using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

public static class Program
{
    public static void Main()
    {
        List<NationalSupportRecord> records = new List<NationalSupportRecord>();
        string start_text = "Transactions in detail";
        string end_text = @"Page (\d+) of 12028";
        var regexTime = new Regex(@"\d{2}\/\d{2}\/\d{4}\d{4}\.\d{4}");
        using (PdfDocument document = PdfDocument.Open(@"mttq.pdf", new ParsingOptions
        {

        }))
        {
            foreach (Page page in document.GetPages())
            {
                int idxWord = 3;
                string text = page.Text;
                List<Word> words = page.GetWords().ToList();

                // SEARCH TABLE POSITION
                for (; idxWord < words.Count; idxWord++)
                {

                    if (words[idxWord].Text == "No" && words[idxWord - 1].Text == "Doc" && words[idxWord - 2].Text == "CT/")
                    {
                        break;
                    }
                }
                var reg = new Regex(end_text);
                var result = reg.Match(text);
                var indexEnd = result.Index;
                var indexStart = text.IndexOf(start_text) + start_text.Length;
                if (indexEnd == 0)
                {
                    indexEnd = text.Length;
                }
                text = text.Substring(indexStart, indexEnd - indexStart);
                var matchResult = regexTime.Matches(text);
                for (int idxRecord = 0; idxRecord < matchResult.Count; idxRecord++)
                {
                    var length = (idxRecord == matchResult.Count - 1 ? text.Length : matchResult[idxRecord + 1].Index) - matchResult[idxRecord].Index;
                    var supportRecordText = text.Substring(matchResult[idxRecord].Index, length);
                    var date = supportRecordText.Substring(0, 10);
                    // search through work to find price
                    while (true)
                    {
                        if (words[idxWord].Text == date && Regex.IsMatch(words[idxWord + 1].Text, @"(\d+\.)+\d+"))
                        {

                            break;
                        }
                        idxWord++;
                    }
                    var price = words[idxWord + 1].Text;

                    var transactionId = supportRecordText.Substring(10, 10);
                    var message = supportRecordText.Substring(20 + price.Length);
                    records.Add(new NationalSupportRecord
                    {
                        Amount = long.Parse(price.Replace(".", "")),
                        CreatedAt = DateTime.ParseExact(date,"dd/MM/yyyy", CultureInfo.InvariantCulture),
                        Message = message,
                        TransactionId = transactionId,
                    });
                    idxWord++;
                }


            }
        }

        File.WriteAllText("mttq.json", JsonSerializer.Serialize(records));
    }

    public class NationalSupportRecord
    {
        public DateTime CreatedAt { get; set; }

        public long Amount { get; set; }

        public string Message { get; set; }

        public string TransactionId { get; set; }
    }
}