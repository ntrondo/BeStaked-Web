Numbers={
    round:(num, decimals)=>{
        let d = decimals.toString();
        if(num > 1e-6){
            return +(Math.round(num + ("e+" + d))  + ("e-"+d));
        }else{
            if(num == 0){return 0;}
            //#.#####e-P
            let splitString = num.toString().split("e-");
            let power = parseInt(Linq.Last(splitString));
            //3.333333185222229e-9
            let prefix = "0." + "0".repeat(power-1);
            if(decimals < power){return prefix.substr(0,decimals + 1);}
            let postfix = splitString[0].substr(0,1) + splitString[0].substr(2, decimals - power);
            return prefix + postfix;
        }        
    },
    format:(num, d)=>{
        if(typeof num === "string"){
            num = parseFloat(num);
        }
        if(num == undefined || num == null){
            num = 0;
        }
        if(d == undefined || d == null){
            d = 0;
        }
        if(num > 999999){  
            return Strings.scaleNumberToAppropriateUnit(num,Array(4).fill(1000),["","K", "M", "B", "T"], 3);      
            let count = 0;
            while(num > 999){                
                num = num / 1000;
                count++;
            }
        }
        let newNum = Numbers.round(num, d);
        if(newNum >= 1000){
            return newNum.toLocaleString();
        }
        return newNum.toString();
    },
    format2:(num, d, allowScientific)=>{
        if(allowScientific){
            let rounded = Number(Numbers.round(num, d));
            if(rounded == 0 && allowScientific){
                let exp = 1;
                while(num < 1){
                    num *= 10;
                    exp++;
                }
                return Numbers.format(num, 1) + "e-" + exp;
            }
        }        
        return Numbers.format(num,d);
    },
    average:(a,b)=>{
        if(typeof a == 'string'){
            a = BigInt(a);
        }
        if(typeof b == 'string'){
            b = BigInt(b);
        }
        if(typeof a == 'object' || typeof b == 'object'){
            if(!(a instanceof BigInt)){
                a = BigInt(a.toString());
            }
            if(!(b instanceof BigInt)){
                b = BigInt(b.toString());
            }
            
            let sum = a + b;
            let div = BigInt(2);
            let avg = sum / div;
            return avg;
        }    
        {
            let sum = a+b;
            let avg = sum/2;
            return avg;
        }  
    },
    toNumber:(o)=>{
        let n = Number(o);
        return n;
    }
};