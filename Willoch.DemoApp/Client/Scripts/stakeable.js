Stakeable = {
    Contract: null,
    Decimals: null,
    CurrentDay: 0,
    ExchangeRatesSchemaVersion: 2,
    MaximumDuration: 5555,
    uint256Max: 5.78960446186581e+76,
    MaxAmount: 5.789e+68,
    NetworkFees: null,
    Assets: {
        Stakes: [],
        Liquid: 0
    },
    initialize: async () => {
        Stakeable.renderSymbols();
        //await Stakeable.loadContract();
        await Stakeable.loadExchangeRate();
        await Stakeable.loadCurrentDay();
        await Stakeable.loadAndRenderAssets1();
        await Src.stakeableInitiaized();
    },
    dispose:()=>{
        Stakeable.unRenderStakes();
        Stakeable.Contract = null;
        Stakeable.CurrentDay = 0;
        Stakeable.Globals = null;
        Stakeable.Assets = {
            Stakes: [],
            Liquid: 0
        };
    },
    loadCurrentDay: async () => {
        let key = "today" + Src.Source.NetworkId;
        let dayObject = Store.loadItem(key);
        if(dayObject==null){
            await Stakeable.loadContract();
            let day = (await Stakeable.Contract.currentDay()).toNumber();
            dayObject = {day:day};
            Store.storeItem(key, dayObject);
        }
        Stakeable.CurrentDay = dayObject.day;
        $(".stakeable.day.balance").text(Stakeable.CurrentDay);
    },
    jump: async () => {
        let valueStr = $("input#JumpDays").val();
        let daysToJump = parseInt(valueStr);
        if (daysToJump < 1 || daysToJump > 100) {
            return;
        }
        let targetDay = Stakeable.CurrentDay + daysToJump;
        console.log("stakeable setting current day:" + targetDay);
        await Stakeable.Contract.setCurrentDay(targetDay, App.Sender);
        await Stakeable.dailyUpdate();        
        await Stakeable.loadStakes();
    },
    dailyUpdate:async()=>{
        await Stakeable.Contract.dailyDataUpdate("0", App.Sender);
        await Stakeable.loadCurrentDay();
    },
    jumpDaysX2Clicked: () => {
        let value = $("input#JumpDays").val();
        value = value * 2;
        value = Math.min(100, value);
        $("input#JumpDays").val(value);
    },
    renderSymbols: () => {
        //console.log("Stakeable.renderSymbols()");
        let source = Src.Source == null ? Linq.Last(Src.Sources) : Src.Source;        
        $(".symbol.stakeable").text(source.StakeableSymbol);
        $(".symbol.fiat").text(Src.SelectedFiat.Symbol);
    },
    renderBalance: () => {
        //console.log("Stakeable.renderBalance()");        
        $(".balance.liquid.stakeable").text(Numbers.format(Stakeable.Assets.Liquid));
        $(".balance.legacy.staked.stakeable").text(Numbers.format(Stakeable.Assets.StakesValue));
    },
    renderStakes: () => {
        console.log("Stakeable.renderStakes()");
        const preSelector = "#legacyStakes";
        let i;
        let stakes = Stakeable.Assets.Stakes;
        let tableSelector = preSelector + " .is-stakes";
        let table = Linq.First($(tableSelector));
        if(table != null){
            Linq.Foreach(stakes, Stakeable.renderStake);
        }

        HTMLUtils.toggleClass(preSelector, stakes.length == 0, "is-empty");
        Stakeable.renderStakesTableTotals(preSelector, Stakeable.Assets.StakesValue);
        // $(".balance.legacy.staked.stakeable").text(Numbers.format(Stakeable.Assets.StakesValue));
        // $(".balance.legacy.staked.fiat").text(Numbers.format(Stakeable.Assets.StakesValue * Src.SelectedFiat.Price, Src.SelectedFiat.Decimals));
        let matureStakes = Linq.Where(Stakeable.Assets.Stakes, (s) => { return s.remaining <= 0; });
        $(".count.legacy.stakes.mature").text(matureStakes.length);
    },
    unRenderStakes:(tableSelector, balanceSelector)=>{
        if(tableSelector == null){
            tableSelector = "#legacyStakes .is-stakes";
        }
        let rowsSelector = ".is-stake:not(.template)";
        let table = $(tableSelector);
        if(table == 0 || table.length == 0){return;}
        let rows = table.find(rowsSelector);
        rows.remove();
        if(balanceSelector == null){
            balanceSelector = ".balance.legacy.staked";
        }
        $(balanceSelector).text(Numbers.format());
    },
    renderStake:(stake)=>{        
        let tableSelector = "#legacyStakes .is-stakes";
        Stakeable.renderStakeInTable(stake,tableSelector, "LS");
    },
    getStakeRowSelector:(tableSelector, stake)=>{
        let rowSelector = tableSelector + " [data-stake-id=\"" + stake.id.toString() + "\"]";
        return rowSelector;
    },
    renderStakeInTable:async(stake,tableSelector, namePrefix)=>{
        let rowSelector = Stakeable.getStakeRowSelector(tableSelector, stake);
        let rows = $(rowSelector);
        if (rows.length > 0) {return;}
        let templates = $(tableSelector + " .is-stake.template");
        if(templates.length < 2){return;}
        rows = Linq.Select(templates, (t)=>{return t.cloneNode(true);});
        await Linq.Foreach(rows, (r)=>{ r.setAttribute("data-stake-id", stake.id.toString())});
        for(let i = 0; i < templates.length;i++){
            templates[i].parentElement.appendChild(rows[i]);
        } 
        HTMLUtils.toggleClass(rowSelector, false, "template"); 
        //Render id
        $(rowSelector + " .stake-id").text(stake.id);

        //Render principal
        HTMLUtils.renderStakeable(rowSelector + " .principal", stake.amount);

        //Render duration
        $(rowSelector + " .stake.duration.days").text(stake.duration);
        $(rowSelector + " .duration.text").text(Time.getIntervalTextFromDays(stake.duration));

        //Render shares
        {
            let sharesText = Strings.scaleNumberToAppropriateUnit(stake.shares, Array(4).fill(1000), ["","K", "M", "B", "T"]);
            $(rowSelector + " .shares").text(sharesText + "Shares");
        }

        //Render stake value
        Stakeable.renderStakeableAndFiat(rowSelector, ".stake.value.book", stake.Valuation.BookValue);
        // HTMLUtils.renderStakeable(rowSelector + " .stake.stakeable.value.book", stake.Valuation.BookValue);
        // HTMLUtils.convertAndRenderFiat(rowSelector + " .stake.fiat.value.book", stake.Valuation.BookValue);

        Stakeable.renderStakeableAndFiat(rowSelector, ".stake.value.market", stake.Valuation.EMV1);
        // HTMLUtils.renderStakeable(rowSelector + " .stake.stakeable.value.market", stake.Valuation.EMV1);
        // HTMLUtils.convertAndRenderFiat(rowSelector + " .stake.fiat.value.market", stake.Valuation.EMV1);

        //Render daily interest
        {
            let dailyPerformance = 0;
            if(stake.remaining >= 0){
                dailyPerformance = await Valuation.getDailyPerformance(null, stake.shares);   
            }else{
                
            }                  
            //let elementSelector = ".stake.value.performance.daily";
            Stakeable.renderStakeableAndFiat(rowSelector, ".stake.value.performance.daily", dailyPerformance);
            // HTMLUtils.renderStakeable(rowSelector + " .stakeable" + elementSelector, dailyPerformance, 1);
            // HTMLUtils.convertAndRenderFiat(rowSelector + " .fiat" + elementSelector, dailyPerformance, 2);
        }
        
        //Render progress
        let lapsed = Math.max(0, Stakeable.CurrentDay - stake.start);
        let percentage = Math.min(100, Numbers.round(100 * lapsed / stake.duration, 0));        
        $(rowSelector + " .stake.lapsed.percentage").text(percentage);
        $(rowSelector + " .name").text(Valuation.calculateName(namePrefix, stake.start, stake.shares, stake.duration));
        $(rowSelector + " progress").val(percentage);
        $(rowSelector + " .stake.start.date").text(Time.formatDate(await Time.getDateFromDay(stake.start)));
        $(rowSelector + " .stake.end.date").text(Time.formatDate(await Time.getDateFromDay(stake.start + stake.duration)));
        {
            let age = Stakeable.CurrentDay - stake.start;
            let date = new Date();
            date.setDate(date.getDate() - age);
            let year = date.getFullYear();
            $(rowSelector + " .year").text(year);
        }
        
        //HTMLUtils.renderStakeable(rowSelector + " .accrued.value.stakeable", stake.Valuation.Accrued);
        // HTMLUtils.renderStakeable(rowSelector + " .remaining.value.stakeable", stake.Valuation.AdjRepCost);

      
        {
            let end = stake.start + stake.duration;
            let remaining = Math.max(0, end - Stakeable.CurrentDay);                    
            if(remaining > 0){
                let remainingText = Time.getIntervalTextFromDays(remaining);
                $(rowSelector + " .remaining.time").text(remainingText);
            }                    
        }
        //Set classes
        let isStarting = stake.start > Stakeable.CurrentDay;
        let isActive = !isStarting && stake.remaining > 0;
        let isMature = !isStarting && !isActive;
        
        HTMLUtils.toggleClass(rowSelector, isStarting, "is-starting");
        HTMLUtils.toggleClass(rowSelector, isActive, "is-active");
        HTMLUtils.toggleClass(rowSelector, isMature, "is-mature");
        $(rowSelector)
        
    },
    renderStakesTableTotals:async(preSelector, stakeableAmount)=>{
        Stakeable.renderStakeableAndFiat(preSelector, ".sum.staked", stakeableAmount);
        // HTMLUtils.renderStakeable(preSelector + " .sum.staked.stakeable", stakeableAmount);
        // HTMLUtils.convertAndRenderFiat(preSelector + " .sum.staked.fiat", stakeableAmount);
    },
    renderStakeableAndFiat:(preSelector, commonSlector, stakeableAmount)=>{
        HTMLUtils.renderStakeable(preSelector + " .stakeable" + commonSlector, stakeableAmount);
        HTMLUtils.convertAndRenderFiat(preSelector + " .fiat" + commonSlector, stakeableAmount);
    },
    loadContract: async () => {
        if(Stakeable.Contract != null){
            return;
        }
        let abi = await HTMLUtils.getJSONParsed(Src.Source.StakeableAbi);
        let tcontract = TruffleContract(abi);
        tcontract.setProvider(App.Provider.currentProvider);
        if ("StakeableContractId" in Src.Source) {
            Stakeable.Contract = await tcontract.at(Src.Source.StakeableContractId);
        } else {
            Stakeable.Contract = await tcontract.deployed();
        }
        //console.log("stakeable contract loaded");
    },
    loadExchangeRate: async () => {
        let key = "rates";
        let rates = Store.loadItem(key);
        if(rates == null || rates.SchemaVersion != Stakeable.ExchangeRatesSchemaVersion){
            let url = Src.Source.ExchangeAPI.replace("{fiatTicker}",Src.SelectedFiat.Ticker);
            rates = await HTMLUtils.getJSONParsed(url);  
            rates.SchemaVersion = Stakeable.ExchangeRatesSchemaVersion;   
            Store.storeItem(key, rates);
        }        
        Src.SelectedFiat.StakeableTokenPrice =  1 / rates[Src.Sources[2].StakeableSymbol];
        Src.SelectedFiat.Price = Src.SelectedFiat.StakeableTokenPrice;
        Src.SelectedFiat.NativeTokenPrice =  1 / rates.ETH;
    },
    loadNetworkFees:async()=>{
        const response = await HTMLUtils.getJSONParsed(Src.Source.GasAPI);
        Stakeable.NetworkFees = response.data;
    },
    calculateNetworkFees:()=>{

    },
    /**Loads assets from cache. Loads from chain if no cache. */
    loadAndRenderAssets1: async () => {
        await Stakeable.loadContract();
        let cached = Store.loadAssets(Stakeable.Contract.address);
        if (cached != null) {
            Stakeable.Assets = cached;
            Stakeable.renderBalance();
            console.log("stakeable loaded assets from local storage")
        } else {
            await Stakeable.loadAndRenderAssets2();
        }
    },
    /**Ignores cache. Loads from chain and saves to cache. */
    loadAndRenderAssets2: async () => {  
        await Stakeable.loadBalance(); 
        await Stakeable.loadStakes();    
        Stakeable.renderBalance();    
        Store.storeAssets(Stakeable.Contract.address, Stakeable.Assets);
    },

    loadAndRenderBalance: async () => {
        await Stakeable.loadBalance();
        Stakeable.renderBalance();
    },
    loadBalance: async () => {
        Stakeable.Assets.Liquid = 0;
        if (App.Account != null) {
            let balance = await Stakeable.loadBalanceFor(App.Account);
            Stakeable.Assets.Liquid = Math.floor(balance);
        }
    },
    loadBalanceFor:async(address)=>{
        await Stakeable.loadContract();
        let balanceBN = await Stakeable.Contract.balanceOf(address);
        let balance = await Stakeable.removeDecimals(balanceBN);
        return balance;
    },
    loadStakes: async () => {
        await Stakeable.loadGlobals();
        Stakeable.Assets.Stakes = [];
        Stakeable.Assets.StakesValue = 0;
        let count = 0;
        if (App.Account != null) {
            await Stakeable.loadContract();
            count = (await Stakeable.Contract.stakeCount(App.Account)).toNumber();
        }
        for (var i = 0; i < count; i++) {
            await Stakeable.loadStake(i);
        }
        Stakeable.Assets.Stakes = Linq.OrderBy(Stakeable.Assets.Stakes,(a,b)=>{ return a.remaining - b.remaining;}, true);
        Stakeable.Assets.StakesValue = Math.floor(Linq.Sum(Stakeable.Assets.Stakes, (s) => { return s.value; }));
        //console.log("stakes loaded");        
    },
    loadStakesForAddress:async(owner)=>{
        //Not in use
        if(owner == null){            
            if(TStake.Contract==null){
                await TStake.loadContract();
            }
            if(TStake.Contract==null){
                return;
            }            
            owner = TStake.Contract.address;
        }
        if(TStake.Contract != null && owner == TStake.Contract.address){
            console.log("Loading assets on stakeable managed by tstake.");
        }
        if(Stakeable.Contract == null){
            await Stakeable.loadContract();
        }        
        let assets = {Stakes:[]};
        let count = (await Stakeable.Contract.stakeCount(owner)).toNumber();
        console.log("Loading " + count + " stakes.");
        let stake;
        for (var i = 0; i < count; i++) {
            stake = await Stakeable.getStakeFor(i, owner);
            assets.Stakes.push(stake);
        }
        assets.StakesValue = Math.floor(Linq.Sum(Stakeable.Assets.Stakes, (s) => { return s.value; }));
        return assets;
    },
    loadStake: async (index) => {
        let stake = await Stakeable.getStakeFor(index, App.Account);
        Stakeable.Assets.Stakes.push(stake);
    },
    getStakeFor: async (index, owner) => {
        await Stakeable.loadContract();
        let stakeStore = await Stakeable.Contract.stakeLists(owner, index);
        let stake = {
            index: index,
            id: stakeStore.stakeId.toNumber(),
            shares: stakeStore.stakeShares.toNumber(),
            amount: await Stakeable.removeDecimals(stakeStore.stakedHearts),
            start: stakeStore.lockedDay.toNumber(),
            duration: stakeStore.stakedDays.toNumber()
        };
        stake.Valuation = await Valuation.CalculateValueExisting(stake.amount, stake.shares, stake.start, stake.duration);
        stake.value = stake.Valuation.BookValue;
        stake.remaining = stake.duration - Stakeable.CurrentDay + stake.start;
        return stake;
    },
    loadGlobals: async () => {
        if(Stakeable.Globals != null){return;}
        let key = "StakeableGlobals" + Src.Source.NetworkId;
        let cached = Store.loadItem(key);
        if(cached == null){
            
            /** uint256 internal constant SHARE_RATE_SCALE = 1e5; */
            let scale = 1e5; 
            /** uint256 newStakeShares = (newStakedHearts + bonusHearts) * SHARE_RATE_SCALE / g._shareRate; */  
            let glob = await Stakeable.Contract.globals();
            let rate = glob.shareRate.toNumber();
            
            let priceInHearts = rate / scale;
            let priceInHex = await Stakeable.removeDecimals(priceInHearts);
            cached = {
                //Shares per token
                SharePrice: priceInHex 
            };
            Store.storeItem(key, cached);
        }        
        //console.log(globals);
        Stakeable.Globals = cached;
    },
    getDailyData: async (day) => {
        //console.log("Probably moved to valuations. Remove this log statement if it is executetd.");
        let data = await Stakeable.Contract.dailyData(day);
        if(data != null && data.dayPayoutTotal == 0){
            data = null;
        }
        return data;
    },
    removeDecimals: async (value) => {
        if (Stakeable.Decimals == null) {
            await Stakeable.loadDecimals();
        }
        let valueStr = value.toString();
        let indexOfDecimalPoint = valueStr.indexOf(".");
        //Ensure decimal point
        if (indexOfDecimalPoint < 0) {
            valueStr = valueStr + ".0";
            indexOfDecimalPoint = valueStr.indexOf(".");
        }
        //Ensure digits before decimal point
        if (indexOfDecimalPoint < Stakeable.Decimals) {
            valueStr = "0".repeat(Stakeable.Decimals - indexOfDecimalPoint) + valueStr;
            indexOfDecimalPoint = valueStr.indexOf(".");
        }
        //Move decimal point
        {
            let newIndexOfdecimalPoint = indexOfDecimalPoint - Stakeable.Decimals;
            let indices = [newIndexOfdecimalPoint, Stakeable.Decimals, indexOfDecimalPoint + 1];
            let beforeDecimalPoint = valueStr.substr(0, newIndexOfdecimalPoint);
            let interDesimal = valueStr.substr(newIndexOfdecimalPoint, Stakeable.Decimals);
            let afterDecimalPoint = valueStr.substr(indexOfDecimalPoint + 1);
            valueStr = beforeDecimalPoint + "." + interDesimal + afterDecimalPoint;
        }

        let number = parseFloat(valueStr);
        return number;
    },
    addDecimals: async (value) => {
        if (Stakeable.Decimals == null) {
            await Stakeable.loadDecimals();
        }
        let valueStr = value.toString();
        //console.log("Adding decimals to:" + valueStr);
        let indexOfDecimalPoint = valueStr.indexOf(".");
        //Ensure decimal point
        if (indexOfDecimalPoint < 0) {
            valueStr = valueStr + ".0";
            indexOfDecimalPoint = valueStr.indexOf(".");
        }
        //Ensure digits after decimal point
        {
            let digitsAfter = valueStr.length - indexOfDecimalPoint - 1;
            if(digitsAfter < Stakeable.Decimals){
                valueStr += "0".repeat(Stakeable.Decimals - digitsAfter);
            }
        }
        //Move decimal point
        {
            let beforeDecimalPoint = valueStr.substr(0, indexOfDecimalPoint);
            let afterDecimalPoint = valueStr.substr(indexOfDecimalPoint + 1, Stakeable.Decimals);
            valueStr = beforeDecimalPoint  + afterDecimalPoint;
        }       
        console.log("Add decimals result:" + valueStr);
        let number = BigInt(valueStr);
        return number;
    },
    loadDecimals: async () => {
        if(Stakeable.Decimals == null){
            let decimalsBN = await Stakeable.Contract.decimals();
            Stakeable.Decimals = decimalsBN.toNumber();
        }        
    },
    mint: async () => {
        let amount = $("#MintStakeablefrm input[type='number']").val();
        let amountString = await Stakeable.addDecimals(amount);
        await Stakeable.Contract.mint(App.Account, amountString, App.Sender);
        console.log("stakeable minted " + amount);
        await Stakeable.unRenderReloadAndRenderAssets();
    },
    maxStakeAmountClicked: () => {
        $("#Stakefrm input.amount").val(Stakeable.Assets.Liquid);
    },
    maxStakeDurationClicked: () => {
        $("#Stakefrm input.duration").val(5555);
    },
    stake: async () => {
        let amount = $("#Stakefrm input.amount").val();
        let amountString = await Stakeable.addDecimals(amount);
        let duration = $("#Stakefrm input.duration").val();
        await Stakeable.Contract.stakeStart(amountString, duration.toString(), { from: App.Account });
        console.log("stakeable staked " + amount + " for " + duration + " days");
        await Stakeable.unRenderReloadAndRenderAssets();
    },
    endStake: async (stake) => {
        if(stake==null){
            stake = Linq.First(Stakeable.Assets.Stakes, (s) => { return s.remaining <= 0; });
        }        
        await Stakeable.Contract.stakeEnd(stake.index, stake.id, App.Sender);
        await Stakeable.unRenderReloadAndRenderAssets();
    },
    unRenderReloadAndRenderAssets:async()=>{
        await Stakeable.unRenderStakes();  
        Store.clearAssets(Stakeable.Contract.address);
        await Stakeable.loadAndRenderAssets1();
        App.renderAssets();
    },
    approve: async (amountString, spender) => {
        //let amountBN = await Stakeable.addDecimals(amount);
        await Stakeable.Contract.approve(spender, amountString, App.Sender);
    },
    calculateName: (start, shares, duration) => {
        let prefix = "LS";
        return Valuation.calculateName(prefix, start, shares, duration);
    },
    getStakeIdFromEvent:(e, stopPropagation = false)=>{
        let target = e.target;
        let stakeElement = $(target).closest(".is-stake");
        let stakeId = stakeElement.attr("data-stake-id");
        if(stopPropagation){
            e.stopPropagation();
        }
        console.log("Stakeable.getStakeIdFromEvent(), stakeId:" + stakeId);
        if(typeof stakeId == 'string'){
            stakeId = parseInt(stakeId);
        }
        return stakeId;
    },
    endStakeClicked:async(e)=>{
        let stakeId = Stakeable.getStakeIdFromEvent(e, true);           
        let stake = Linq.First(Stakeable.Assets.Stakes, (s) => { return s.id == stakeId; });
        if(stake == null){
            return;
        }
        await Stakeable.endStake(stake);
        await Stakeable.renderStakes();
    },
    getFormattedStakeText:(stake)=>{
        let values = [];
        values.push(["Details for stake with id:" + stake.id]);
        values.push(null);

        values.push(["Principal", stake.amount]);
        values.push(["Accrued", Numbers.format(stake.Valuation.Accrued)]);
        if(stake.Valuation.Pennalty > 0){
            values.push(["Pennalty", Numbers.format(stake.Valuation.Pennalty)]);
        }
        values.push(["Book value", Numbers.format(stake.Valuation.BookValue)]); 
        values.push(null);

        values.push(["First day", stake.start]);
        values.push(["Duration", stake.duration]);
        values.push(["Maturity day", stake.start + stake.duration]);
        //values.push(["Progress", ]);
        values.push(null);

        values.push(["Shares", Numbers.format(stake.shares)]);
        values.push(["Realised share price ", Numbers.round(stake.amount / stake.shares, 10), Src.Source.StakeableSymbol + "/Share"]);
        values.push(["Current share price  ", Numbers.round(Stakeable.Globals.SharePrice, 10), Src.Source.StakeableSymbol + "/Share"]);
                
        values.push(["Replacement cost        ", Numbers.format(stake.Valuation.RepCost)]);
        values.push(["Bonus adj. rep. cost    ", Numbers.format(stake.Valuation.BonAdjRepCost)/* , "Amount required to create stake with same shares" */]);
        values.push(["Remaining adj. rep. cost", Numbers.format(stake.Valuation.RemAdjRepCost)]);

        return Stakeable.getFormattedText(values);
    },
    getFormattedText:(values)=>{
        let text = "";
        for (let i = 0; i < values.length; i++) {
            if(i > 0){
                text += "\n";
            }            
            const value = values[i];
            if(value == null || value.length == 0){
                continue;
            }            
            text += value[0];
            if(value.length >= 2){
                text += ":" + "    " + value[1];
            }
            if(value.length >= 3){
                text += " " + value[2];
            }
            if(value.length >= 4){
                text += " " + value[3];
            }
        }
        return text;
    },
    onStakeClicked:async(e)=>{
        if(HTMLUtils.getSelectionHtml().length > 0){return;}
        let stakeId = Stakeable.getStakeIdFromEvent(e);           
        let stake = Linq.First(Stakeable.Assets.Stakes, (s) => { return s.id == stakeId; });
        if(stake == null){
            return;
        }
        await Stakeable.loadGlobals();
        let text = Stakeable.getFormattedStakeText(stake);
        alert(text);
    }
}