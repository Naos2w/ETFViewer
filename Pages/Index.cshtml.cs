using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
namespace fubonTest.Pages;

public class EtfHeaders
{
    public string Header { get; set; }
}
public class ApiResponse
{
    public List<EtfData> msgArray { get; set; } // Data
    public string refURL { get; set; }          // referenceUrl
    public string userDelay { get; set; }       // dataUpdateInterval
    public string rtMessage { get; set; }       // retrunMessage
    public string rtCode { get; set; }          // retrunCode
}
public class EtfData
{//
    public string a { get; set; } // etfCode
    public string b { get; set; } // etfName
    public string c { get; set; } // totalIssuedUnits
    public string d { get; set; } // issuedUnitsChangeFromYesterday
    public string e { get; set; } // realTimeTradingPrice
    public string f { get; set; } // realTimeEstimatedNAV
    public string g { get; set; } // realTimePremiumDiscountRate
    public string h { get; set; } // openingReferenceNAV
    public string i { get; set; } // dataDate
    public string j { get; set; } // dataTime
    public string k { get; set; } // underlyingIndexOrProductType
    public string l { get; set; } // openingReferenceMarketPrice
    public string m { get; set; } // remarks
}

public class EtfViewModel
{
    public string CodeName { get; set; }
    public string PreviousNav { get; set; }
    public string EstimatedNav { get; set; }
    public string NavChange { get; set; }
    public string PreviousClose { get; set; }
    public string TransactionPrice { get; set; }
    public string PriceChange { get; set; }
    public string PremiumDiscount { get; set; }
    public string DateTime { get; set; }
}
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    public List<(string Text, int RowSpan)> EtfHeaders { get; set; } = new();
    public List<string> EtfLy2Headers { get; set; } = new();
    // public List<EtfData> EtfData { get; set; } = new();
    public List<EtfViewModel> EtfData { get; set; } = new();
    public async Task OnGetAsync()
    {
        _logger.LogInformation("OnGet Start");
        string json = await GetFubonDataAsync();

        var result = JsonSerializer.Deserialize<ApiResponse>(json);
        EtfData = result?.msgArray?.Select(etf =>
        {
            decimal f = 0, h = 1, l = 0, e = 0;
            decimal.TryParse(etf.f, out f);
            decimal.TryParse(etf.h, out h);
            decimal.TryParse(etf.l, out l);
            decimal.TryParse(etf.e, out e);
            var changeNav = h != 0 ? (f - h) / h : 0;
            var changePrice = l != 0 ? (e - l) / l : 0;
            var chaneDiscount = f != 0 ? (e - f) / e : 0;

            return new EtfViewModel
            {
                CodeName = etf.a + "<br />" + etf.b,
                PreviousNav = etf.h,
                EstimatedNav = etf.f,
                NavChange = changeNav.ToString("P2"),
                PreviousClose = etf.l,
                TransactionPrice = etf.e,
                PriceChange = changePrice.ToString("P2"),
                PremiumDiscount = chaneDiscount.ToString("P2"),
                DateTime = $"{etf.i.Substring(0, 4)}/{etf.i.Substring(4, 2)}/{etf.i.Substring(6, 2)} {etf.j}"
            };
        }).ToList() ?? new List<EtfViewModel>();

        if (EtfData.Count > 0)
        {
            EtfHeaders = new List<(string, int)>
            {
                ("ETF代號與名稱", 2),
                ("前一營業日淨值", 1),
                ("預估淨值", 1),
                ("漲跌幅", 1),
                ("昨收市價", 1),
                ("成交價", 1),
                ("漲跌幅", 1),
                ("即時折溢價幅", 1),
                ("資料日期與時間", 2)
            };
            EtfLy2Headers = new List<string>() { "A", "B", "(B-A) / A", "C", "D", "(D-C) / C", "(D-B) / B" };
        }
        _logger.LogInformation("OnGet End");
    }

    public async Task<string> GetFubonDataAsync()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var url = "https://websys.fsit.com.tw/FubonETF/TWSE/FubonRealtime.aspx?type=A0010";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return content; // 這裡回傳的是原始字串（通常是 JSON 或 HTML）
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFubonDataAsync Exception");
            throw ex;
        }
    }
}
