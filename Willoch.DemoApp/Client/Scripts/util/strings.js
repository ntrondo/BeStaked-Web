Strings = {
    Empty:"",
    prefixUntilLength:(text, prefix, targetLength)=>{
        while(text.length < targetLength && prefix.length > 0){
            text = prefix + text;
        }
        return text;
    },
    scaleNumberToAppropriateUnit:(value, divisors, units, decimals)=>{
        if(decimals==undefined){
            decimals = 0;
        }
        let i;
        for(i = 0; i< divisors.length;i++){
            if(value < divisors[i]){
                break;
            }
            value = value/divisors[i];
        }
        value = Numbers.round(value, decimals);
        return value.toString() + " " + units[i];
    },
    getTextBeforeFirst:(text, markers)=>{
        if(text == null){
            return null;            
        }
        if(markers.length == 0){
            return text;
        }
        if(typeof markers == 'string'){
            markers = [markers];
        }
        let length = Linq.Min(markers, (m)=>{return text.indexOf(m);});
        if(length < 0){ return Strings.Empty;}
        return text.substr(0, length);
    },
    getTextAfterFirst:(text, markers)=>{
        let selector = (m)=>{return text.indexOf(m);};
        let indices = Linq.Select(markers, selector);
        indices = Linq.Where(indices, (i)=>{return i >= 0;});
        let minValue = Linq.Min(indices, (i)=>{return i;});
        let marker = Linq.First(markers, (m)=>{return selector(m) == minValue;});
        return text.substr(minValue + marker.length);
    },
    
};