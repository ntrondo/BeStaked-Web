Time = {
    getIntervalTextFromDays:(days)=>{
        return Strings.scaleNumberToAppropriateUnit(days, [30, 12],["days", "months", "years"]);        
    },
    getDateFromDay:async(day)=>{
        await Stakeable.loadCurrentDay();
        let daysAgo = Stakeable.CurrentDay - day;
        let date = new Date();
        date.setDate(date.getDate() - daysAgo);
        return date;
    },
    formatDate:(date)=>{
        return date.toLocaleDateString();
    }
};