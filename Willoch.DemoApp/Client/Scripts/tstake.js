TStake = {
    GasEstimates:{
        mintReferred:350000,
        settle:150000,//Not certain
        approve:60000
    },
    AssetsSchemaVersion:3,
    Contract:null,
    Assets:null,
    Stats:null,
    MintingInfo:null,
    ContractFeePerMille:null,
    BrandFeePerMille:8,
    ReferralFeePermille:0,
    MinimumStakeAmount: 100,
    MinimumDuration:0,
    MaximumDuration:0,
    Allowance:0,
    initialize:async ()=>{
        await TStake.loadContract();
        await TStake.loadAssets1();       
        await Src.tstakeInitialized();            
    },
    dispose:()=>{
        TStake.unRenderStakes();
        TStake.Contract = null;
        TStake.Assets = null;
        TStake.Stats = null;
        TStake.MintingInfo = null;
        TStake.ContractFeePerMille = null;
        TStake.MinimumDuration = 0;
        TStake.MaximumDuration = 0;
        TStake.Allowance = 0;
    },
    loadAllowance:async()=>{
        let owner = App.Account;
        let spender = TStake.Contract.address;
        let allowanceBN = await Stakeable.Contract.allowance(owner,spender);
        TStake.Allowance = await Stakeable.removeDecimals(allowanceBN);
    },
    loadAssets1:async()=>{
       
        if(Src.Source.TStakeContractId){
            TStake.Assets = Store.loadAssets(Src.Source.TStakeContractId);
        }else if(TStake.Contract != null){
            TStake.Assets = Store.loadAssets(TStake.Contract.address);
        }        
        if(TStake.Assets == null || TStake.Assets.SchemaVersion != TStake.AssetsSchemaVersion){
            await TStake.loadAssets2();
        }
    },
    loadAssets2:async()=>{
        TStake.Assets = {TStakes:[], SchemaVersion:TStake.AssetsSchemaVersion, Liquid:0};
        let c = 0;
        if(App.Account != null && TStake.Contract != null){
            c = await TStake.Contract.balanceOf(App.Account);
        }
        for(let i=0;i<c;i++){
            //let id = await TStake.Contract.tokenOfOwnerByIndex(App.Account, i);
            const tStake = await TStake.loadTStake(App.Account, i);
            tStake.isOwnedByCurrentAccount = true;
            // let stakeIndex = await TStake.Contract.getStakeIndex(id);
            // let token = await TStake.Contract.idToToken(id);
            // console.log("Loading tstake. stakeId:" + token.stakeId + ", stakeIndex:" + stakeIndex + ".");
            // let stake = await Stakeable.getStakeFor(stakeIndex, TStake.Contract.address);            
            //TStake.Assets.TStakes.push({stake:stake, value:stake.value, id:token.tokenId.toNumber()});
            TStake.Assets.TStakes.push(tStake);
        }
        TStake.Assets.TStakes = Linq.OrderBy(TStake.Assets.TStakes,(a,b)=>{ return a.stake.remaining - b.stake.remaining;}, true);
        TStake.Assets.Value = Math.floor(Linq.Sum(TStake.Assets.TStakes, (s)=>{return s.value;}));        
        if(TStake.Contract != null && App.Account != null){
            //Load in-contract balance
            let redeemable = await TStake.Contract.redeemableFees(App.Account);
            TStake.Assets.Liquid = await Stakeable.removeDecimals(redeemable);
            //Persist
            Store.storeAssets(TStake.Contract.address, TStake.Assets);
        }        
    },
    loadTStake:async(owner, index)=>{
        let id;        
        if(owner==null){
            id = Number(await TStake.Contract.tokenByIndex(index));
        }else{
            id = Number(await TStake.Contract.tokenOfOwnerByIndex(owner, index));
        }
        let tStake = null;
        if(tStake == null && TStake.Assets != null){
            tStake = Linq.First(TStake.Assets.TStakes, (ts)=>{return ts.id == id;});
        }
        if(tStake == null && TStake.Stats != null){
            tStake = Linq.First(TStake.Stats.TStakes, (ts)=>{return ts.id == id;});
        }
        if(tStake==null){
            await TStake.loadContract();
            const stakeIndex = await TStake.Contract.getStakeIndex(id);
            const token = await TStake.Contract.idToToken(id);
            //console.log("Loading tstake. stakeId:" + token.stakeId + ", stakeIndex:" + stakeIndex + ".");
            let stake = await Stakeable.getStakeFor(stakeIndex, TStake.Contract.address);
            let stretch = token.rewardStretching.toNumber();
            let reward = TStake.calculateReward(stake.Valuation.BookValue, stretch, stake.remaining);
            tStake = {stake:stake, value:stake.value, id:token.tokenId.toNumber(), reward: reward, stretch:stretch};
        }        
        return tStake;
    },
    calculateReward:(amount, stretch, remaining)=>{
        let daysLate = remaining >= 0 ? 0 : -remaining;
        if(daysLate == 0){return 0;}
        if(daysLate >= stretch){return amount;}
        let exponent = stretch - daysLate;
        let divisor = Math.pow(2, exponent);
        let reward = amount / divisor;
        return reward;
    },
    loadStats:async()=>{
        if(TStake.Stats != null){return;}
        HTMLUtils.toggleClass("#transferrableStakes",true, "is-loading-stakes");
        TStake.Stats = {
            TStakes:[]
        }
        {
            //Load stakes
            let tc = Number(await TStake.Contract.totalSupply());
            for (let ti = 0; ti < tc; ti++) {
                const tStake = await TStake.loadTStake(null, ti);
                TStake.Stats.TStakes.push(tStake);
            }
            TStake.Stats.TStakes = Linq.OrderBy(TStake.Stats.TStakes,(a,b)=>{ return a.stake.remaining - b.stake.remaining;}, true);
            TStake.Stats.Value = Math.floor(Linq.Sum(TStake.Stats.TStakes, (s)=>{return s.value;}));
            //console.log("TStake.loadStats() " + TStake.Stats.TStakes.length + " stakes.");  
        }
        {
            //Load stakeable balance
            let stakeableBalance = await Stakeable.loadBalanceFor(TStake.Contract.address);
            let fiatBalance = stakeableBalance * Src.SelectedFiat.StakeablePrice;
            //console.log("Contract balance: " + stakeableBalance + " " + Src.Source.StakeableSymbol);
            TStake.Stats.StakeableBalance = stakeableBalance;
        }
        HTMLUtils.toggleClass("#transferrableStakes",false, "is-loading-stakes");
    },
    renderStats:async()=>{
        if(TStake.Stats == null){
            return;
        }
        await Stakeable.renderStakesTableTotals(".stats-section", TStake.Stats.Value);       
        await Linq.Foreach(TStake.Stats.TStakes,TStake.renderStake);

        //Render balance
        const preSelector = "#contractBalance";
        Stakeable.renderStakeableAndFiat(preSelector, ".amount",  TStake.Stats.StakeableBalance);
        // HTMLUtils.renderStakeable(preSelector + " .amount.stakeable", TStake.Stats.StakeableBalance);
        // HTMLUtils.convertAndRenderFiat(preSelector + " .amount.fiat", TStake.Stats.StakeableBalance);
        HTMLUtils.toggleClass(preSelector, TStake.Stats.StakeableBalance >= 1, "has-amount");        
    },
    renderAssets:async()=>{         
        console.log("TStake.renderAssets()");      
        await TStake.renderStakes();
        Stakeable.renderStakesTableTotals("#transferrableStakes", TStake.Assets.Value);

        const preSelector = "#inContractBalance";
        Stakeable.renderStakeableAndFiat(preSelector, ".amount",  TStake.Assets.Liquid);
        // HTMLUtils.renderStakeable(".stakeable.amount.in-contract", TStake.Assets.Liquid);
        // HTMLUtils.convertAndRenderFiat(".fiat.amount.in-contract", TStake.Assets.Liquid);
        HTMLUtils.toggleClass(preSelector, TStake.Assets.Liquid > 0, "has-amount");
    },
    renderStakes:async()=>{
         
        if(TStake.Assets == null){return;} 
        await Linq.Foreach(TStake.Assets.TStakes,TStake.renderStake);
    },
    unRenderStakes:()=>{
        let tableSelector = "#transferrableStakes .is-stakes";
        let balanceSelector = ".balance.transferrable.staked";
        Stakeable.unRenderStakes(tableSelector, balanceSelector);
    },
    renderStake:async(tStake)=>{
        let tableSelector = "#transferrableStakes .is-stakes";
        await Stakeable.renderStakeInTable(tStake.stake,tableSelector, "TS");
        let rowSelector = Stakeable.getStakeRowSelector(tableSelector, tStake.stake);
        if(tStake.stake.remaining <= 0){
            //Render reward
            Stakeable.renderStakeableAndFiat(rowSelector, ".amount.reward",  tStake.reward);        
            // HTMLUtils.renderStakeable(rowSelector + " .amount.stakeable.reward", tStake.reward);
            // HTMLUtils.convertAndRenderFiat(rowSelector + " .amount.fiat.reward", tStake.reward);
        }
        //Flag ownership
        HTMLUtils.toggleClass(rowSelector, tStake.isOwnedByCurrentAccount, "is-my-stake");
    },
    loadContract: async () => {
        if(Src.Source.TransferrableAbi == null){
            console.log("tstake abi not specified in source");
            return;
        }
        let abi = await HTMLUtils.getJSONParsed(Src.Source.TransferrableAbi);
        let tcontract = TruffleContract(abi);
        tcontract.setProvider(App.Provider.currentProvider);
        if("TransferrableContractId" in Src.Source && Src.Source.TransferrableContractId != null && Src.Source.TransferrableContractId.length > 0){
            TStake.Contract = await tcontract.at( Src.Source.TransferrableContractId);
        }else{
            console.log("tstake contract id not specified in source");
            return;
        }        
        //console.log("transferrable contract loaded");
    },
    loadDurationLimits:async()=>{
        if(TStake.MinimumDuration > 0 && TStake.MaximumDuration > 0){return;}
        TStake.MinimumDuration = 1;
        TStake.MaximumDuration = 5555;
        // TStake.MinimumDuration = (await TStake.Contract.MIN_STAKE_DAYS()).toNumber();
        // TStake.MaximumDuration = (await TStake.Contract.MAX_STAKE_DAYS()).toNumber();        
        $(".max.duration.portable").text(Numbers.format(TStake.MaximumDuration));        
    },
    loadContractFee:async()=>{
        if(TStake.ContractFeePerMille == null){
            await TStake.loadContract();
            if(TStake.Contract != null){
                TStake.ContractFeePerMille = (await TStake.Contract.OWNER_FEE_PERMILLE()).toNumber();
            }
        }        
    },
    maxDurationClicked:async()=>{
        let duration = TStake.getMintDuration();
        await TStake.loadDurationLimits();
        if(duration == TStake.MaximumDuration){return;}
        $("form input.duration").val(TStake.MaximumDuration);
        TStake.onDurationInput();
    },
    maxAmountClicked:async ()=>{        
        let incFees = Math.floor(Stakeable.Assets.Liquid);
        let exFees = await TStake.calculateAmountExcludingFees(incFees);
        $("form input.amount").val(Math.floor(exFees));
        TStake.onAmountInput();
    },
    getMintDuration:()=>{
        let valstr = $("form input.duration").val();
        if(valstr == ""){
            valstr = "0";
        }
        let val = parseInt(valstr);
        return val;
    },
    getMintValue:()=>{
        let valstr = $("form input.amount").val();
        if(valstr == ""){
            valstr = "0";
        }
        let val = parseInt(valstr);
        return val;
    },
    onDurationInput:async()=>{
        let d = TStake.getMintDuration();
        await TStake.loadDurationLimits();
        let form = "form#MintTransferrableStakeForm";
        HTMLUtils.toggleClass(form, d < TStake.MinimumDuration, "duration-too-short");
        HTMLUtils.toggleClass(form, d > TStake.MaximumDuration, "duration-too-long");
        HTMLUtils.toggleClass(form, d > 0, "has-duration");
        await TStake.calculateAndRenderName();
    },
    onAmountInput:async ()=>{
        let val = TStake.getMintValue();
        if(TStake.MintingInfo != null && TStake.MintingInfo.Principal == val){
            return;
        }
        TStake.MintingInfo = null;
        let isZero = val == 0;
        let isAboveMin = val >= TStake.MinimumStakeAmount;
        let isAboveMax = val > Stakeable.Assets.Liquid;
        let isAboveAllowance = false; 
        if(!isAboveMax && isAboveMin){
            //Calculate payable and check against balance
            let incFees = await TStake.calculateAmountIncludingFees(val);
            isAboveMax = incFees > Stakeable.Assets.Liquid;
            await TStake.checkAllowance(incFees);            
        }  
        
        let form = "form#MintTransferrableStakeForm";
        HTMLUtils.toggleClass(form, isAboveMax, "amount-too-high");
        HTMLUtils.toggleClass(form, !isZero && !isAboveMin, "amount-too-low");
        
        HTMLUtils.toggleClass(form, val > 0, "has-amount");
        
        if(isAboveMin && !isAboveMax){
            TStake.MintingInfo = {Principal:val};
            await TStake.calculateFees();       
        }
        TStake.renderPayable();
        TStake.renderFees();
        TStake.calculateAndRenderName();
        App.calculateNetworkFees();
    },
    calculateAndRenderName:async()=>{
        if(TStake.MintingInfo == null){
            $(".mint.name").text("''");
            HTMLUtils.renderStakeable(".mint.value",0);
            return;
        }
        let amount = TStake.MintingInfo.Principal;
        let duration = TStake.getMintDuration();
        let shares = await Valuation.calculateSharesOfNew(amount, duration);
        TStake.MintingInfo.Name = await TStake.calculateName(Stakeable.CurrentDay, shares, duration);
        $(".mint.name").text(TStake.MintingInfo.Name);
        let value = await Valuation.CalculateValueNew(amount, duration, shares);
        HTMLUtils.renderStakeable(".mint.value",value);
        //$(".mint.value").text(Numbers.format(TStake.MintingInfo.Principal, 0));
    },
    calculateName:async(start,shares, duration)=>{
        let prefix = "TS";
        return Valuation.calculateName(prefix, start, shares, duration);           
    },
    checkAllowance:async(amount)=>{
        if(amount == null || amount == undefined){
            amount = TStake.MintingInfo?.Payable?.Stakeable;
        }
        let isAboveAllowance = amount > TStake.Allowance;
        if(isAboveAllowance){
            await TStake.loadAllowance();
            isAboveAllowance = amount > TStake.Allowance; 
        }
        HTMLUtils.toggleClass("form#MintTransferrableStakeForm", isAboveAllowance, "allowance-too-low");
    },
    calculateAmountIncludingFees:async (exFees)=>{        
        await TStake.loadContractFee();
        if(TStake.ContractFeePerMille == null){
            return exFees;
        }
        let totalFeePerMille = TStake.ContractFeePerMille + TStake.BrandFeePerMille + TStake.ReferralFeePermille;        
        let invPerMille = 1000 - totalFeePerMille;
        let incFees = 1000 * exFees / invPerMille;
        await Stakeable.loadDecimals();
        incFees = Numbers.round(incFees, Stakeable.Decimals);
        return incFees;
    },    
    calculateAmountExcludingFees:async (incFees)=>{
        await TStake.loadContractFee();
        let totalFeePerMille = TStake.ContractFeePerMille + TStake.BrandFeePerMille + TStake.ReferralFeePermille; 
        let invPerMille = 1000 - totalFeePerMille;
        let exFees = incFees / 1000 * invPerMille;
        await Stakeable.loadDecimals();
        exFees = Numbers.round(exFees, Stakeable.Decimals);
        return exFees;
    },
    calculateFees:async ()=>{        
        let mi = TStake.MintingInfo;
        if(mi==null){return;}
        await Stakeable.loadDecimals();
        //Calculate amount including fees        
        let incFees = await TStake.calculateAmountIncludingFees(TStake.MintingInfo.Principal);
        mi.Payable = {
            Stakeable: incFees
        };
        //Record total fees
        let totalFees = Numbers.round(incFees - TStake.MintingInfo.Principal, Stakeable.Decimals);
        let totalFeePerMille = TStake.ContractFeePerMille + TStake.BrandFeePerMille + TStake.ReferralFeePermille;
        mi.Fees = {};
        mi.Fees.Total = {
            Percent: Numbers.round(totalFeePerMille/10,1),
            Stakeable: totalFees
        }
        //Split fees
        mi.Fees.Contract ={
            Percent: Numbers.round(TStake.ContractFeePerMille/10,1),
            Stakeable: Numbers.round(totalFees * TStake.ContractFeePerMille / totalFeePerMille, Stakeable.Decimals)
        };
        mi.Fees.Brand ={
            Percent: Numbers.round(TStake.BrandFeePerMille/10,1),
            Stakeable: Numbers.round(totalFees * TStake.BrandFeePerMille / totalFeePerMille, Stakeable.Decimals)
        };
        mi.Fees.Referral ={
            Percent: Numbers.round(TStake.ReferralFeePermille/10,1),
            Stakeable: Numbers.round(totalFees * TStake.ReferralFeePermille / totalFeePerMille, Stakeable.Decimals)
        };
        //Calculate fiat values
        let p = Src.SelectedFiat.Price;        
        mi.Payable.Fiat = p * mi.Payable.Stakeable;
        mi.Fees.Total.Fiat = p * mi.Fees.Total.Stakeable;
        mi.Fees.Brand.Fiat = p * mi.Fees.Brand.Stakeable;
        mi.Fees.Contract.Fiat = p * mi.Fees.Contract.Stakeable;
        mi.Fees.Referral.Fiat = p * mi.Fees.Referral.Stakeable;
    },
    calculateNetworkFees:()=>{
        let mi = TStake.MintingInfo;
        if(mi==null){return;}        
        
        let nf = Stakeable.NetworkFees;
        //console.log(nf);
        const nativeDecimalDivisor = 1e+18;
        let isRequiresApproval = true;
        if(isRequiresApproval){
            let approveFeeWei = nf.standard * TStake.GasEstimates.approve;
            let approveFeeNative = approveFeeWei/nativeDecimalDivisor;
            //console.log("approve fee native:" + approveFeeNative);
            let approveFeeNativeText = Numbers.format(approveFeeNative, 4);
            $(".fix-allowance-too-low .native.fee.network").text(approveFeeNativeText);
            let approveFeeFiat = approveFeeNative * Src.SelectedFiat.NativeTokenPrice;
            //console.log("approve fee fiat:" + approveFeeFiat);
            HTMLUtils.renderFiat(".fix-allowance-too-low .fiat.fee.network",approveFeeFiat);
        }
        {
            let mintFeeWei = nf.standard * TStake.GasEstimates.mintReferred;
            let mintFeeNative = mintFeeWei/nativeDecimalDivisor;
            //console.log("mint fee native:" + mintFeeNative);
            let mintFeeFiat = mintFeeNative * Src.SelectedFiat.NativeTokenPrice;
            //console.log("mint fee fiat:" + mintFeeFiat);            
            $(".mint-field .native.fee.network").text(Numbers.format(mintFeeNative, 4));
            HTMLUtils.renderFiat(".mint-field .fiat.fee.network",mintFeeFiat);
        }
    },
    renderPayable:()=>{
        let payable;
        if(TStake.MintingInfo == null){
            payable = {Stakeable:0,Fiat:0};
        }else{
            payable = TStake.MintingInfo.Payable;
        }
        let fd = Src.SelectedFiat.Decimals;
        HTMLUtils.renderStakeable(".stakeable.amount.payable.mint",payable.Stakeable);
        HTMLUtils.renderFiat(".fiat.amount.payable.mint",payable.Fiat);
    },
    renderFees:()=>{
        let fees;
        if(TStake.MintingInfo == null){
            let comp = {Percent:0,Stakeable:0,Fiat:0};
            fees = {Total:comp,Brand:comp,Referral:comp,Contract:comp};
        }else{
            fees = TStake.MintingInfo.Fees;
        }         
        $(".percentage.fee.total.mint").text(Numbers.format(fees.Total.Percent, 1));        
        HTMLUtils.renderStakeable(".stakeable.fee.total.mint", fees.Total.Stakeable);
        HTMLUtils.renderFiat(".fiat.fee.total.mint",fees.Total.Fiat);

        $(".percentage.fee.contract.mint").text(Numbers.format(fees.Contract.Percent, 1));
        HTMLUtils.renderStakeable(".stakeable.fee.contract.mint",fees.Contract.Stakeable);
        HTMLUtils.renderFiat(".fiat.fee.contract.mint",fees.Contract.Fiat);

        $(".percentage.fee.brand.mint").text(Numbers.format(fees.Brand.Percent, 1));
        HTMLUtils.renderStakeable(".stakeable.fee.brand.mint", fees.Brand.Stakeable);
        HTMLUtils.renderFiat(".fiat.fee.brand.mint", fees.Brand.Fiat);

        $(".percentage.fee.referral.mint").text(Numbers.format(fees.Referral.Percent, 1));
        HTMLUtils.renderStakeable(".stakeable.fee.referral.mint", fees.Referral.Stakeable);
        HTMLUtils.renderFiat(".fiat.fee.referral.mint", fees.Referral.Fiat);
    },
    allowExactClicked:async()=>{
        let amountBN = await Stakeable.addDecimals(TStake.MintingInfo.Payable.Stakeable);
        let amountString = amountBN.toString();
        await TStake.approve(amountString);       
    },
    allowUnlimitedClicked:async()=>{
        let amountString = BigInt(Stakeable.MaxAmount).toString();
        await TStake.approve(amountString);  
    },
    approve:async(amountString)=>{
        $(".fix-allowance-too-low progress").removeAttr("value");
        try{
            await Stakeable.approve(amountString, TStake.Contract.address);
            $(".fix-allowance-too-low progress").attr("value", "100");
        }catch{
            $(".fix-allowance-too-low progress").attr("value", "0");
        }        
        await TStake.checkAllowance();
    },
    mintClicked:async()=>{
        
        let mi = TStake.MintingInfo;
        let fees = mi.Fees;
        let amount = mi.Payable.Stakeable;
        let duration = TStake.getMintDuration();    
        let totalExternalFee = fees.Total.Stakeable - fees.Contract.Stakeable;

        let amountString = (await Stakeable.addDecimals(amount)).toString();
        let feeString = (await Stakeable.addDecimals(totalExternalFee)).toString();
      
        let result = null;
        try{
            $("progress.mint-button").removeAttr("value");
            let gasParams = {
                //gasLimit:TStake.GasEstimates.mintReferred,
                //maxPriorityFeePerGas:1
            };
            let tParams = {};
            Object.assign(tParams, App.Sender);
            Object.assign(tParams, gasParams);
            console.log(tParams);
            result = await TStake.Contract.mintReferred(amountString, duration, Src.Source.BrandAddress, feeString, tParams);
            // result = {
            //     tx: "0xc3f84079b546baa3feb1e30c726426f9e427cedee390bb1e2150e013116da6d4", receipt:{
            //     blockHash: "0xf6a6f3edd2f2b3eb2960281f2ecd20c9ea6f4654de7ccbb765b298b205004e31",
            //     blockNumber: 10756797,
            //     contractAddress: null,
            //     cumulativeGasUsed: 6276838,
            //     effectiveGasPrice: "0x77359405",
            //     from: "0xad7d009547272c30e9a84812ca167b64671acd35",
            //     gasUsed: 390576,
            //     logs: [],
            //     logsBloom: "0x00000000000000000400000000000000000000000000000000000000000000000000000040000000000000008000004000000000000000000000000000200000000010020000400800001008000800000000000000000000000000000000000001000000020010000000000000000800000000000000000000000010000000000001000000000000000000000800001000000900000000000004000000000000020000000004200000000000000000190000000000000000000000000000000000000002000000000000000000000000000000000000000000000000000020080010000000040000004000000000000000000000000000000020000000000000",
            //     rawLogs: [],
            //     status: true,
            //     to: "0x53f840d990d7c88b90a5496b7470ee0b01083ed8",
            //     transactionHash: "0xc3f84079b546baa3feb1e30c726426f9e427cedee390bb1e2150e013116da6d4",
            //     transactionIndex: 24,
            //     type: "0x0"
            // }
            $("progress.mint-button").attr("value", "100");
        }catch(err){
            console.log(err);
            $("progress.mint-button").attr("value", "0");
            return;
        }        
        //Record referral fee collected  
        // if("stakeReporter" in window){//Record stake before minting
        //     window.stakeReporter.reportStaked(App.Account, mi, result);
        // }
        
        
        await TStake.unRenderReloadAndRenderAssets();
        TStake.loadAllowance();
    },
    unRenderReloadAndRenderAssets:async()=>{
        await TStake.unRenderStakes();
        await TStake.loadAssets2();
        if(TStake.Stats != null){
            TStake.Stats = null;
            await TStake.loadStats();
            await TStake.renderStats();
        }
        await Stakeable.unRenderReloadAndRenderAssets();
        await TStake.renderStakes();
    
    },
    getTokenIdFromEvent:(e, stopPropagation = false)=>{
        let stakeId = Stakeable.getStakeIdFromEvent(e, stopPropagation); 
        let sentinel = (ts)=>{return ts.stake.id == stakeId;};       
        let tStake = Linq.First(TStake.Assets.TStakes, sentinel);
        if(tStake == null && TStake.Stats != null){
            tStake = Linq.First(TStake.Stats.TStakes, sentinel)
        }
        return tStake.id;
    },
    transferClicked:async(e)=>{
        let tokenId = TStake.getTokenIdFromEvent(e, true);
        console.log("TStake.transferClicked(), tokenId:" + tokenId);
        let toAddress = prompt("Send to address:", "0x");
        if(toAddress==null){
            return;
        }
        let isValidAddress = Web3.utils.isAddress(toAddress);
        if(!isValidAddress){
            alert("Invalid adress. Transfer aborted.");
            return;
        }
        await TStake.Contract.transferFrom(App.Account, toAddress, tokenId, App.Sender);
        await TStake.unRenderReloadAndRenderAssets();
    },
    settleClicked:async(e)=>{
        let tokenId = TStake.getTokenIdFromEvent(e, true);
        if(tokenId == null){
            return;
        }
        console.log("TStake.settleClicked(), tokenId:" + tokenId);
        let stakeIndex = await TStake.Contract.getStakeIndex(tokenId);
        if(stakeIndex == null || Number(stakeIndex)<0){
            alert("Could not get stake index for token with id: " + tokenId);
            return;
        }
        await TStake.Contract.settle(tokenId, stakeIndex, App.Sender);
        await TStake.unRenderReloadAndRenderAssets();
    },
    onStakeClicked:async(e)=>{
        if(HTMLUtils.getSelectionHtml().length > 0){return;}
        let tokenId = TStake.getTokenIdFromEvent(e);
        let sentinel = (ts)=>{return ts.id == tokenId;};
        let tStake = Linq.First(TStake.Assets.TStakes, sentinel);        
        if(tStake == null && TStake.Stats != null){
            tStake = Linq.First(TStake.Stats.TStakes, sentinel);  
        }
        if(tStake == null){
            return;
        }
        await Stakeable.loadGlobals();
        let text = Stakeable.getFormattedStakeText(tStake.stake);
        alert(text);
    },
    onRewardClicked:(e)=>{
        if(HTMLUtils.getSelectionHtml().length > 0){return;}
        let tokenId = TStake.getTokenIdFromEvent(e, true);
        let discriminator = (ts)=>{return ts.id == tokenId;};
        let tStake = Linq.First(TStake.Assets.TStakes, discriminator);        
        if(tStake == null && TStake.Stats != null){
            tStake = Linq.First(TStake.Stats.TStakes, discriminator);  
        }
        if(tStake == null){
            return;
        }
        let text = TStake.getFormattedRewardText(tStake);
        alert(text);
    },
    getFormattedRewardText:(tStake)=>{
        let values = [["Reward table for stake id: " + tStake.stake.id], ["The reward doubles every day for " + tStake.stretch + " days."]]; 
        values.push(["The reward is payed to the address that settles the stake."]);    
        values.push(null);   
        const bv = tStake.stake.Valuation.BookValue;
        const rem = tStake.stake.remaining;
        const n = tStake.stretch + rem;
        var date = new Date();
        if(n < 0){
            values.push(null);
            values.push(["This stake has matured passed its reward stretching. The reward is 100% and the stake has no value to its owner."]);
        }
        for (let i = 0; i < n; i++) {
            date.setDate(date.getDate() + 1);
            const amount = TStake.calculateReward(bv, tStake.stretch, rem - i);
            const fraction = amount/bv;
            const value = [];
            value.push(Time.formatDate(date) + " ");
            value.push(Numbers.format2(amount, 6, true) + " " + Src.Source.StakeableSymbol + "  ");
            value.push(Numbers.format(Src.SelectedFiat.Price * amount, 2) + " " + Src.SelectedFiat.Symbol  + "  ");
            value.push(Numbers.format(fraction * 100, 4) + " %");
            values.push(value);  
        }
        return Stakeable.getFormattedText(values);
    },
    onWithdrawClicked:async()=>{
        await TStake.Contract.redeemFees(App.Sender);
        await TStake.unRenderReloadAndRenderAssets();
    }
};