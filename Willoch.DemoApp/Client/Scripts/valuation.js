Valuation = {
    DailyData:{},
    CalculateValueNew:async(amount, duration, shares = 0)=>{
        //Replacement cost
        await Stakeable.loadGlobals();
        if(duration == Stakeable.MaximumDuration){
            return amount;
        }
        if(shares == 0){
            shares = await Valuation.calculateSharesOfNew(amount, duration);
        }
        let start = Stakeable.CurrentDay + 1;
        let response = await Valuation.CalculateValueExisting(amount, shares, start, duration);
        return response.EMV1;
    },
    //principal:staked amount large units
    CalculateValueExisting:async(principal,shares, start, duration)=>{
        let response = {
            Accrued:0,
            Pennalty:0,
            RepCost:0,
            AdjRepCost:0,
            BookValue:0,
            EMV1:0//Estimated market value 1
        };
        //Replacement cost means cost to create stake with same amount of shares using maximum duration.
        //Adjusted replacement cost is multiplied by remaining duration divided by maximum duration.
        await Stakeable.loadGlobals();
        response.RepCost = shares * Stakeable.Globals.SharePrice;
        response.BonAdjRepCost = response.RepCost - await Valuation.calculateBonusAmountFromTotal(response.RepCost,duration); 
        let remainingDuration = duration - (Stakeable.CurrentDay - start) - 1;
        response.RemAdjRepCost = remainingDuration > 0 ? (remainingDuration * response.BonAdjRepCost /  Stakeable.MaximumDuration) : 0; 
            
        
        //Aditionaly value of accrued performance
        response.Accrued = await Valuation.calculateAccruedPerformanceFromDuration(shares, start,duration);

        //Apply late stake pennalty
        response.Pennalty = await Valuation.calculateLateStakePennaltyFromDuration(principal + response.Accrued, start, duration);        
        
        let profit = response.Accrued - response.Pennalty;
        response.BookValue = Math.max(0,  principal + profit);
        response.EMV1 = remainingDuration > 0 ? Math.max(0, profit + response.RemAdjRepCost) : response.BookValue;        
        //console.log("Stake valued at " + response.EMV1);
        return response;
    },
    calculateSharesOfNew:async(amount,duration)=>{
        let bonusAmount = await Valuation.calculateBonusAmount(amount, duration);
        let totalAmount = amount + bonusAmount;
        await Stakeable.loadGlobals();
        let shares = totalAmount / Stakeable.Globals.SharePrice;
        return shares;
    },
    applyLateStakePennalty:async(amount, lastDay)=>{
        await Stakeable.loadCurrentDay();
        if(Stakeable.CurrentDay <= lastDay){
            return amount;
        }
        let lateDays = Stakeable.CurrentDay - lastDay;
        let lateWeeks = Math.floor(lateDays / 7);
        if(lateWeeks >= 50){
            return 0;
        }
        let f = 1 - (lateWeeks/50);
        console.log("late end-stake factor: " + f.toFixed(2));
        return amount * f;
    },
    calculateLateStakePennaltyFromDuration:async(amount, firstDay, duration)=>{
        let lastDay = firstDay + duration;
        await Stakeable.loadCurrentDay();
        let currentDay = Stakeable.CurrentDay;
        let lateDays = currentDay - lastDay;
        return Valuation.calculateLateStakePennalty(amount, lateDays);
    },
    calculateLateStakePennalty:(amount, lateDays)=>{
        let lateWeeks = Math.floor(lateDays / 7) - 2;
        if(lateWeeks < 1){return 0;}
        let maxWeeks = 50;
        if(lateWeeks >= maxWeeks){return amount;}
        let f = lateWeeks / maxWeeks;
        return amount * f;
    },
    calculatePrincipalOfNew:async(shares, duration)=>{
        if(duration < 1){return 0;}
        await Stakeable.loadGlobals();
        let totalAmount = shares * Stakeable.Globals.SharePrice;
        let bonusAmount = await Valuation.calculateBonusAmountFromTotal(totalAmount,duration);
        return totalAmount - bonusAmount;
    },
    calculateBonusAmountFromTotal:async(totalAmount,duration)=>{
        let maxIterations = 10;
        let i=0;
        let factor = 1/2;
        let maxDiff = 1;
        let amount = totalAmount * factor;
        let bonus = Valuation.calculateBonusAmount(amount, duration);
        let diff = totalAmount - amount - bonus;
        while(Math.abs(diff) > maxDiff && i < maxIterations){
            //If diff is positive, the amount was too small and must be increased
            //If diff is negative, the amount was too large and must be decreased
            
            let newAmount = amount + diff * factor;
            amount = newAmount;
            bonus = Valuation.calculateBonusAmount(amount, duration);
            diff = totalAmount - amount - bonus;
            i++;
        }
        return bonus;
    },
    calculateBonusAmount:(amount, duration)=>{       
        //Source: _stakeStartBonusHearts
        let LPB_BONUS_PERCENT = 20;
        let LPB_BONUS_MAX_PERCENT = 200;
        let LPB = 364 * 100 / LPB_BONUS_PERCENT;
        let cappedExtraDays = Math.min(duration-1, LPB * LPB_BONUS_MAX_PERCENT / 100);        
        
        let BPB_MAX_HEX = 150 * 1e6;
        let cappedStakedHex = amount <= BPB_MAX_HEX ? amount : BPB_MAX_HEX;
        
        let BPB_BONUS_PERCENT = 10;
        let BPB = BPB_MAX_HEX * 100 / BPB_BONUS_PERCENT;
        let bonusHex = cappedExtraDays * BPB + cappedStakedHex * LPB;
        bonusHex = amount * bonusHex / (LPB * BPB);

        return bonusHex;
    },
    calculateAccruedPerformanceFromDuration:async(shares, firstDay, duration)=>{
        let lastDay = firstDay + duration;
        return Valuation.calculateAccruedPerformance(shares, firstDay, lastDay);
    },
    calculateAccruedPerformance:async(shares, firstDay, lastDay)=>{  
        return Valuation.estimateAccruedPerformanceLinear(shares, firstDay, lastDay);              
        //return Valuation.measureAccruedPerformance(shares, firstDay, lastDay);
    },
    measureAccruedPerformance:async(shares, firstDay, lastDay)=>{
        //Very slow
        let accrued = 0;
        await Stakeable.loadCurrentDay();
        lastDay = Math.min(lastDay,Stakeable.CurrentDay);
        let data;
        let i = firstDay;
        //let duration = lastDay - firstDay + 1;
        for(;i<=lastDay;i++){
            data = await Valuation.getDailyData(i);            
            let amount = data.dayPayoutTotal * (shares/data.dayStakeSharesTotal);
            accrued += amount;
        }
        return accrued;
    },
    estimateAccruedPerformanceLinear:async(shares, firstDay, lastDay)=>{
        lastDay = Math.min(Stakeable.CurrentDay - 1, lastDay);
        let duration = lastDay - firstDay + 1;
        if(duration <= 0){return 0;}    
        let includesBPD = false;
        if("BigPayDay" in Src.Source){
            let bpd = Src.Source.BigPayDay;
            includesBPD = firstDay <= bpd && lastDay >= bpd;  
        }
        let firstData = await Valuation.getDailyData(firstDay);
        let lastData = await Valuation.getDailyData(lastDay);
        if(firstData == null | lastData == null){
            if(lastData == null){
                lastData = firstData;
            }
            if(firstData == null){
                firstData = lastData;
            }            
        }
        let avgDayPayoutTotal = 0;
        let avgDayStakeSharesTotal = 0;
        if(firstData != null){
            let innerAvgDayPayoutTotal = Numbers.average(firstData.dayPayoutTotal, lastData.dayPayoutTotal);
            avgDayPayoutTotal = await Stakeable.removeDecimals(innerAvgDayPayoutTotal);
            //avgDayPayoutTotal = await Stakeable.removeDecimals((BigInt(firstData.dayPayoutTotal) + BigInt(lastData.dayPayoutTotal))/BigInt(2));
            let innerAvgDayStakeSharesTotal = Numbers.average(firstData.dayStakeSharesTotal, lastData.dayStakeSharesTotal);// (BigInt(firstData.dayStakeSharesTotal) + BigInt(lastData.dayStakeSharesTotal))/BigInt(2);
            avgDayStakeSharesTotal = Numbers.toNumber(innerAvgDayStakeSharesTotal);
        }
        let avgDayAmountPerShare = avgDayStakeSharesTotal == 0 ? 0 : avgDayPayoutTotal / avgDayStakeSharesTotal;
        let estimatedAccruedPerformance = shares * avgDayAmountPerShare * duration;
        if(includesBPD){
            let bpd = shares * Src.Source.BigPayDayAmountPerShare; 
            //console.log("BPD:" + bpd);
            estimatedAccruedPerformance += bpd;
        }
        return estimatedAccruedPerformance;
    },
    printDailyTotalPayout:async(day)=>{
        let data = await Valuation.getDailyData(day);
        let totalBN = data.dayPayoutTotal;
        console.log(totalBN.toString());
    },
    getDailyData:async(day)=>{
        let data=Valuation.DailyData[day]; 
        if(data == null || data == undefined){
            data = await Stakeable.getDailyData(day);   
            Valuation.DailyData[day] = data;
        }
        return data;
    },
    getDailyPerformance:async(day,shares)=>{
        await Stakeable.loadCurrentDay();
        let data = null;
        if(day == null || day < 0 || day >= Stakeable.CurrentDay){
            day = Stakeable.CurrentDay;
            //Search back 3 days to find a day with data
            while(data==null && day > Stakeable.CurrentDay - 5){
                day--;
                data = await Valuation.getDailyData(day);
            }
        }else{
            data = await Valuation.getDailyData(day);
        }   
        let dayStakeSharesTotal = data == null ? 0 : Number(data.dayStakeSharesTotal);
        if(dayStakeSharesTotal==0){return 0;}        
        let fraction = shares / Number(data.dayStakeSharesTotal);
        if(fraction == Infinity){return 0;}
        let totalPayout = await Stakeable.removeDecimals(data.dayPayoutTotal);
        let performance = totalPayout * fraction;
        //let performance2 = await Stakeable.removeDecimals(performance);
        return performance;
    },
    calculateName:(prefix, start,shares, duration)=>{
        let yearString;
        {
            let age = Stakeable.CurrentDay - start;
            let date = new Date();
            date.setDate(date.getDate() - age);
            let year = date.getFullYear();
            yearString = year.toString().substr(2);
        }
        let durationString;
        {
            let years = Math.floor(duration / 365);
            durationString = Strings.prefixUntilLength(years.toString(), "0", 4);
            durationString = durationString.substr(2);
        }
        let sharesString;
        {            
            sharesString = Strings.scaleNumberToAppropriateUnit(shares,Array(4).fill(1000), ["","K", "M", "B", "T"]); 
            sharesString = sharesString.replace(" ","");           
            sharesString = Strings.prefixUntilLength(sharesString, "0", 4);
        }
        let dash = "-";
        return prefix + dash + yearString + dash + durationString + dash + sharesString;        
    },
};