# Demo website:
This site was demo at [AnalyticBaoSo3 website](https://analytic-quyengop-bao-so-3.pearl2201.com/).
# Setup
- Follow this guide to setup vietnamese full text search in postgresql: https://gist.github.com/anhtran/de0691f848e115d841822baa6ee9f693
- Replace postgresql connection string at SaokeApp\appsettings.json
- Run follow command to setup postgresql
```
    cd SaokeApp
    dotnet ef database update
```
- Run DumpDataToJson program to dump DumpDataToJson.mttq.pdf raw file to json
- Copy json file at DumpDataToJson\bin\Debug\net8.0\mttq.json to SaokeApp\mttq.json 
# Preview
![Search feature](./images/preview.png "Search feature")
![Donate distribution](./images/donate_distribution.png "Donate distribution")
![Donate by date](./images/donate_by_date.png "Donate by date")