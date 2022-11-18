Src = {
    Network:{
        Id:0,
        Type:null
    },
    Sources: [
        {//Dev
            NetworkId:1111,
            NetworkType:"private",
            StakeableSymbol: "DEX",
            Environment: "dev",
            /** Contract ids can be omitted if they are specified in abi. Values here override values in abi */
            StakeableContractId:"0xE3F932E173127B1B873474Bd06AcBe058E652Eb1",
            TransferrableContractId:"0x857d7dE2AaFE19Ab87d2e0bba87977647ea48fC4",
            TransferrableMarketContractId:"0x035Bc1e3B033f92Adf5E2293600EeA06Afaeabf5",
            TransferrableMarketAbi:"/contracts/dev/NFTForStakeableMarketV1.json",
            StakeableAbi: "/contracts/dev/StakeableTokenMock.json",
            TransferrableAbi: "/contracts/dev/PortableStake.json",
            ExchangeAPI:"https://min-api.cryptocompare.com/data/price?fsym={fiatTicker}&tsyms=ETH,HEX",            
            BrandAddress:"0xAd7d009547272C30e9a84812Ca167B64671acD35",
            GasAPI:"https://www.gasnow.org/api/v3/gas/price",
            LaunchTime: new Date()      
        },
        {//Test
            NetworkId:3,
            NetworkType:"ropsten",
            StakeableSymbol: "TEX",
            Environment: "test",
            StakeableContractId:"0xf1633e8d441f6f5e953956e31923f98b53c9fd89",
            TransferrableContractId:"0x53F840d990d7c88b90A5496b7470Ee0b01083Ed8",
            StakeableAbi: "/contracts/test/stakeable.json",
            TransferrableAbi: "/contracts/test/PortableStake.json",
            ExchangeAPI:"https://min-api.cryptocompare.com/data/price?fsym={fiatTicker}&tsyms=ETH,HEX", 
            BrandAddress:"0xAd7d009547272C30e9a84812Ca167B64671acD35",   
            GasAPI:"https://www.gasnow.org/api/v3/gas/price"   ,
            LaunchTime: new Date(1626768697000)//2021-07-20T10:11:37
        },
        {//Prod
            NetworkId:1,
            NetworkType:"main",
            StakeableSymbol: "HEX",
            Environment: "prod",
            StakeableContractId: "0x2b591e99afe9f32eaa6214f7b7629768c40eeb39",
            TransferrableContractId:"0x22E1A96E3103AC7a900DF634d0E2696D05100856",
            StakeableAbi: "/contracts/prod/stakeable.json",
            TransferrableAbi: "/contracts/prod/PortableStake.json",
            BigPayDay:353,
            BigPayDayAmountPerShare: 3.6418255e-9,
            ExchangeAPI:"https://min-api.cryptocompare.com/data/price?fsym={fiatTicker}&tsyms=ETH,HEX", 
            BrandAddress:"0xAF83bb19c9AD4A4bD2Ae7eF6709ACa82c272f0FC",
            GasAPI:"https://www.gasnow.org/api/v3/gas/price",
            LaunchTime: new Date(1627288266000)//2021-07-26T10:31:06
        }
    ],
    SelectedFiat:{
        Ticker:"USD",
        Symbol:"$",
        Price:0,//Deprecated
        NativeTokenPrice:0,
        StakeableTokenPrice:0,
        Decimals:1
    },
    Source: null,
    initialize:async()=>{       
        await Src.loadNetwork();
        Src.Source = Linq.First(Src.Sources, (s)=>{return Src.Network.Id == s.NetworkId && Src.Network.Type==s.NetworkType;});
        if(Src.Source == null){
            console.log("src could not determine source");
            Stakeable.renderSymbols();
            return;
        }else{
            console.log("src environment:" + Src.Source.Environment);            
        }
        await Src.sourceChanged();
    },

    OnSourceChangedListeners:[],//[ctx, fn, args, persist]
    IsTruffleLoaded:false,
    sourceChanged:async()=>{
        console.log("Src.sourceChanged()");   
        Stakeable.dispose();
        TStake.dispose();  
        HTMLUtils.toggleClass("body", App.Connected, "is-loading-assets");
        if(!Src.IsTruffleLoaded){
            Src.IsTruffleLoaded = true;
            await App.loadScript("js/lib/truffle/contract/dist/truffle-contract.min.js");            
        }
        await Stakeable.initialize();
        App.ensureReferredAccountIsRegistered();
        await EXE.executeAll(Src.OnSourceChangedListeners);
    },
    OnStakeableInitializedListeners : [],
    stakeableInitiaized:async()=>{
        console.log("src stakeableInitialized");              
        await TStake.initialize();
        await EXE.executeAll(Src.OnStakeableInitializedListeners);
    },
    OnTStakeInitializedListeners : [],
    tstakeInitialized:async ()=>{
        console.log("src tstakeInitialized");
        await FStake.initialize();   
        await EXE.executeAll(Src.OnTStakeInitializedListeners);     
    },
    fstakeInitialized:async()=>{
        //console.log("src fstakeInitialized");
        await PStake.initialize();       
    },
    pstakeInitialized:async()=>{
        //console.log("src pstakeInitialized");
        await App.renderAssets();
        HTMLUtils.toggleClass("body", false, "is-loading-assets");
    },
    loadNetwork:async()=>{
        Src.Network = {Id:0,Type:null};
        if(App.Provider == null){return;}
        Src.Network.Id = await App.Provider.eth.net.getId();
        Src.Network.Type = await App.Provider.eth.net.getNetworkType();
        let knownTypes = ["development", "ropsten", "rinkeby","main"];
        await Linq.Foreach(knownTypes, (t)=>{ HTMLUtils.toggleClass("body", false, t); });
        switch(Src.Network.Type){
          case "private": $("body").addClass("development");break;
          case "": break;
          default:$("body").addClass(Src.Network.Type);
        }
      },      
      isOnNav2AssetsListenersCreated:false,
      onNav2Assets:async()=>{
        console.log("Src.onNav2Assets()");
        await Stakeable.renderSymbols();

        //Set up  listeners
        if(!Src.isOnNav2AssetsListenersCreated){
            Src.isOnNav2AssetsListenersCreated = true;
            //Reversed order as they are executed last first
            EXE.appendCall(Src.OnTStakeInitializedListeners, TStake, TStake.renderAssets,[], true);
            EXE.appendCall(Src.OnStakeableInitializedListeners, Stakeable, Stakeable.renderStakes,[], true);            
        }
        //Call functions to render stakes if assets are allready loaded
        let isStakeableAssetsLoaded = Stakeable.Assets != null && (Stakeable.Assets.Stakes.length > 0 || Stakeable.Assets.Liquid > 0);
        if(isStakeableAssetsLoaded){
            await Stakeable.renderStakes();
        }
        let isTStakeAssetsLoaded = TStake.Assets != null;
        if(isTStakeAssetsLoaded){
            await TStake.renderAssets();
        }
      },
      onNav2Index:async()=>{
        console.log("Src.onNav2Index()");
        await Stakeable.renderSymbols();
      },
      onNav2Stake:async()=>{
        console.log("Src.onNav2Stake()");
        await Stakeable.renderSymbols();        
        await TStake.maxDurationClicked();    
      },
      isOnNav2StatsListenersCreated:false,
      onNav2Stats:async()=>{
        console.log("Src.onNav2Stats()");
        await Stakeable.renderSymbols();
        //Set up  listeners
        if(!Src.isOnNav2StatsListenersCreated){
            Src.isOnNav2StatsListenersCreated = true;
            //Reversed order as they are executed last first
            EXE.appendCall(Src.OnTStakeInitializedListeners, TStake, TStake.renderStats,[], true);
            EXE.appendCall(Src.OnTStakeInitializedListeners, TStake, TStake.loadStats,[], true);            
        }
        //Call functions to render stakes if assets are allready loaded
        let isTStakeInitialized = TStake.Contract != null;
        if(isTStakeInitialized){
            await TStake.loadStats();
            await TStake.renderStats();
        }
      },
      onNav2Develop:async()=>{
        console.log("Src.onNav2Develop()");
      },
      onNav2Ropsten:async()=>{
        console.log("Src.onNav2Ropsten()");
      }
}