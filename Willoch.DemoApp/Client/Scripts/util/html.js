HTMLUtils = {
    toggleClass:(selector,flag,className)=>{
        if(flag){
            $(selector).addClass(className);
        }else{
            $(selector).removeClass(className);
        }
    },
    renderStakeable:(selector, amount, d = 0)=>{
        if(amount > 100){
            d = 0;
        }
        let text = Numbers.format(amount,d);
        $(selector).text(text);
    },
    renderFiat:(selector, amount, d = -1)=>{
        let sf = Src.SelectedFiat;
        if(sf==null){
            console.log("selected fiat is null");
            return;
        }
        if(d < 0){
            d = sf.Decimals;
            if(amount > 100){d=0;}
        }        
        let text = Numbers.format(amount,d);
        $(selector).text(text);
    },
    convertAndRenderFiat:(selector, stakeable)=>{
        let sf = Src.SelectedFiat;
        if(sf==null){
            console.log("selected fiat is null");
            return;
        }
        let p = sf.Price;        
        let a = stakeable * p;
        HTMLUtils.renderFiat(selector, a);        
    },
    getJSON:async(url)=>{
        let response = await fetch(url);
        let text = await response.text();
        return text;
    },
    getJSONParsed:async(url)=>{
        let text = await HTMLUtils.getJSON(url);
        let parsed = JSON.parse(text);
        return parsed;
    },
    /** Source: https://stackoverflow.com/questions/7570810/is-it-possible-to-distinguish-between-click-and-selection */
    getSelectionHtml:()=> {        
        var sel, html = "";
        if (window.getSelection) {
            sel = window.getSelection();
            if (sel.rangeCount) {
                var frag = sel.getRangeAt(0).cloneContents();
                var el = document.createElement("div");
                el.appendChild(frag);
                html = el.innerHTML;
            }
        } else if (document.selection && document.selection.type == "Text") {
            html = document.selection.createRange().htmlText;
        }
        return html;
    }
};