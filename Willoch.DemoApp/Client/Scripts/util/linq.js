Linq = {
    Any:(array, discriminator)=>{
        return Linq.First(array, discriminator) != null;
    },
    Where:(array, discriminator)=>{
        let result = [];
        for(var i = 0; i < array.length; i++){
            if(discriminator(array[i])){
                result.push(array[i]);
            }
        }
        return result;
    },
    Select:(array, selector)=>{
        let values = [];
        for(var i = 0; i < array.length; i++){
            values.push(selector(array[i]));
        }
        return values;
    },
    Sum:(array, selector)=>{
        let result = 0;
        for(var i = 0; i < array.length; i++){
            result += selector(array[i]);
        }
        return result;
    },
    First:(array, discriminator)=>{
        if(discriminator == undefined){
            discriminator = (i)=>{return true;};
        }
        for(var i = 0; i < array.length; i++){
            if(discriminator(array[i])){
                return array[i];
            }
        }
        return null;
    },
    Last:(array, discriminator)=>{
        if(discriminator == undefined){
            discriminator = (i)=>{return true;};
        }
        for(var i = array.length - 1; i >= 0; i--){
            if(discriminator(array[i])){
                return array[i];
            }
        }
        return null;
    },
    Foreach:async(items, handler)=>{
        for(var i = 0; i < items.length; i++){
            await handler(items[i]);
        }
    },
    OrderBy:(items, comparator, ascending = true)=>{
        let sorted = items.sort(comparator);
        if(!ascending){
            sorted = sorted.reverse();
        }
        return sorted;
    },
    Min:(items, selector)=>{
        return Linq.Extreme(items, selector, false);
    },
    Max:(items, selector)=>{
        return Linq.Extreme(items, selector);
    },
    Extreme:(items, selector, max = true)=>{
        let values = Linq.Select(items, selector);
        let extreme = values.length == 0 ? null : values[0];
        for (let index = 1; index < values.length; index++) {
            const value = values[index];
            if((max && value > extreme) || (!max && value < extreme)){
                extreme = value;
            }
        }
        return extreme;
    }
}