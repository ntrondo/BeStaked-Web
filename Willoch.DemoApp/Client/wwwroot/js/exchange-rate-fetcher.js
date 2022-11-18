const cryptoCompareUrlTemplate = "https://min-api.cryptocompare.com/data/price";
export async function FetchRate(fromTicker, toTicker) {
    let url = cryptoCompareUrlTemplate + "?fsym=" + fromTicker + "&tsyms=" + toTicker;
    let response = await fetch(url);
    let text = await response.text();
    let parsed = JSON.parse(text);
    if (toTicker in parsed)
        return parsed[toTicker];
    else {
        console.log("Error fetching exchange rates.");
        console.log("fromTicker:" + fromTicker + ", toTicker:" + toTicker);
        console.log(parsed);
        return 0;
    }    
}