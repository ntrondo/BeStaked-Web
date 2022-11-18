// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
App = {
  Brand: "BeStaked",//"WeStake",  
  Connected: null,
  Accounts: [],
  Account: null,
  load: async () => {
    console.log("App.load()");
    //$(".brand").text(App.Brand);
    App.captureReferralCode();
    App.redirectHTTPS();
    App.enableBulmaComponents();
    if(!("ethereum" in window))
      return;
    App.onFormModeChanged();
    await App.loadWeb3();
    await App.testWalletConnection();
    await Src.initialize();
  },
  captureReferralCode: async () => {
    //console.log("App.captureReferralCode()");
    let hash = window.location.hash;
    let l = 5;
    if (hash == null || hash.length <= l) { return; }
    let marker = "r=";
    if (hash.length < l + marker.length) { return; }
    let i = hash.indexOf(marker);
    if (i < 0) { return; }
    let code = hash.substr(i + marker.length, l);
    Store.SetReferralCode(code);
    window.location.hash = "";
  },
  ensureReferredAccountIsRegistered: async () => {
    if ("stakeReporter" in window) {//Register account            
      window.stakeReporter.registerReferredAccount();
    }
  },
  redirectHTTPS: () => {
    let marker = "://";
    let protocoll = Strings.getTextBeforeFirst(window.location.href, marker);
    let http = "http";
    let https = http + "s";
    if (protocoll.length == http.length) {
      let redirectTo = https + marker + Strings.getTextAfterFirst(window.location.href, marker);
      console.log("Redirecting to " + redirectTo);
      window.location.href = redirectTo;
    }
  },
  testWalletConnection: async () => {
    let connected = false;
    App.Account = null;
    if ("web3" in window) {
      let accounts = await App.Provider.eth.getAccounts();
      App.Accounts = accounts;
      connected = accounts.length > 0;
    }
    if (connected) {
      App.Account = App.Accounts[0];
    }

    let connectedChanged = App.Connected !== connected;
    App.Connected = connected;
    if (connectedChanged) {
      await App.onConnectedChange();
    }
  },
  listenForChanges: () => {
    App.Provider.currentProvider.on('networkChanged', function (networkId) {
      console.log('networkChanged', networkId);
      App.Connected = null;
      App.load();
    });
    App.Provider.currentProvider.on('accountsChanged', function (accounts) {
      console.log('accountsChanges', accounts);
      App.Connected = null;
      App.load();
    });
    App.listenForChanges = () => { };
  },
  onConnectedChange: async () => {
    HTMLUtils.toggleClass("body", App.Connected, "metamask-connected");
    App.Sender = App.Connected ? { from: App.Account } : null;
    // let className = "metamask-connected";
    // if (App.Connected) {
    //   App.Sender = { from: App.Account };
    //   $("body").addClass(className);
    // } else {
    //   App.Sender = null;
    //   $("body").removeClass(className);
    // }

    HTMLUtils.toggleClass("body", App.Provider == null, "metamask-not-detected");
    HTMLUtils.toggleClass("body", App.Provider != null, "metamask-detected");
    // className = "metamask-not-detected";
    // if (App.Provider == null) {
    //   $("body").addClass(className);
    // } else {
    //   $("body").removeClass(className);
    // }
    HTMLUtils.toggleClass("body", App.Provider != null && !App.Connected, "metamask-not-connected");
    // className = "metamask-not-connected";
    // if (App.Provider != null && !App.Connected) {
    //   $("body").addClass(className);
    // } else {
    //   $("body").removeClass(className);
    // }

    let singleAccountClassName = "single-address";
    //let multiAccountClassName = "multi-address";
    if (App.Connected && App.Accounts.length == 1) {
      $("body").addClass(singleAccountClassName);
    } else {
      $("body").removeClass(singleAccountClassName);
    }
    // if (App.Connected && App.Accounts.length > 1) {
    //   $("body").addClass(multiAccountClassName);
    // } else {
    //   $("body").removeClass(multiAccountClassName);
    // }
    for (var i = 0; App.Connected && i < App.Accounts.length; i++) {
      $(".address" + i + " .address").text(App.Accounts[i]);
    }
  },

  // https://medium.com/metamask/https-medium-com-metamask-breaking-change-injecting-web3-7722797916a8
  IsWeb3Loaded:false,
  IsDetectLoaded:false,
  loadWeb3: async () => {    
    if(!App.IsDetectLoaded){
      App.IsDetectLoaded = true;
      await App.loadScript("js/lib/metamask/detect-provider/dist/detect-provider.min.js");
    }
    
    let provider = await detectEthereumProvider();

    if (provider) {
      if(!App.IsWeb3Loaded){
        App.IsWeb3Loaded = true;
        await App.loadScript("js/lib/web3/web3.min.js");           
      }
      App.Provider = new Web3(provider);
      App.listenForChanges();
      //$("body").addClass("metamask-detected");
    } else {
      App.Provider = null;
      console.log("No ethereum provider detected");
      //$("body").addClass("metamask-not-detected");
    }
    HTMLUtils.toggleClass("body", provider == null, "metamask-not-detected");
    HTMLUtils.toggleClass("body", provider != null, "metamask-detected");
  },
  requestWalletConnection: async () => {
    try {
      // Request account access if needed
      if (!App.Connected) {
        await App.Provider.currentProvider.enable();
      }
      // Acccounts now exposed
      //web3.eth.sendTransaction({/* ... */})
    } catch (error) {
      // User denied account access...
    }
    await App.testWalletConnection();
  },
  onFormModeChanged: () => {
    let ctrl = $("#SimpleFormModecbx");
    if (ctrl.length == 0) { return; }
    let isSimple = ctrl[0].checked;
    HTMLUtils.toggleClass("form", !isSimple, "is-detailed");
    // let className = "is-detailed";
    // if (isSimple) {
    //   $("form").removeClass(className);
    // } else {
    //   $("form").addClass(className);
    // }
  },
  renderAssets: () => {
    //console.log("App.renderAssets()");
    let wsum = TStake.Assets.Liquid + TStake.Assets.Value + FStake.Assets.Value + PStake.Assets.Value;
    HTMLUtils.renderStakeable("#balancesRow .balance.wrapped.stakeable", wsum);
    //$(".balance.wrapped.stakeable").text(Numbers.format(wsum));
    let sum = Stakeable.Assets.Liquid + Stakeable.Assets.StakesValue + wsum;

    Stakeable.renderStakeableAndFiat("#balancesRow", ".balance.total", sum);
    // $(".balance.total.stakeable").text(Numbers.format(sum));
    // let f = Src.SelectedFiat;
    // sum = sum * f.Price;
    // $(".balance.total.fiat").text(Numbers.format(sum, f.Decimals));
  },
  reloadFromChainClicked: async () => {
    Store.clearAll();
    window.location.href = window.location.href;
    // await Stakeable.loadAndRenderAssets1();
    // await TStake.loadAssets1();
    // App.renderAssets();
  },
  collapseClicked: (event) => {
    let element = event.srcElement;
    let ancestor = Linq.First($(element).closest(".is-expanded"));
    App.toggleExpandCollapse(ancestor.parentElement, false);
  },
  expandClicked: (event) => {
    let element = event.srcElement;
    let ancestor = Linq.First($(element).closest(".is-collapsed"));
    App.toggleExpandCollapse(ancestor.parentElement, true);
  },
  toggleExpandCollapse: (decendant, expand) => {
    let classNameCollapsed = "is-collapsed";
    let classNameExpanded = "is-expanded";
    let classNameTo = expand ? classNameExpanded : classNameCollapsed;
    let classNameFrom = expand ? classNameCollapsed : classNameExpanded;
    let ancestors = $(decendant).closest("." + classNameFrom);
    let ancestor = Linq.First(ancestors);
    HTMLUtils.toggleClass(ancestor, false, classNameFrom);
    HTMLUtils.toggleClass(ancestor, true, classNameTo);
  },
  showTableClicked: (event) => {
    let element = event.srcElement;
    App.toggleTableTilesView(element, true);
  },
  showTilesClicked: (event) => {
    let element = event.srcElement;
    App.toggleTableTilesView(element, false);
  },
  toggleTableTilesView: (decendant, toList) => {
    let classNameTiles = "is-tiles-selected";
    let classNameList = "is-list-selected";
    let classNameFrom = toList ? classNameTiles : classNameList;
    let classNameTo = toList ? classNameList : classNameTiles;
    let ancestors = $(decendant).closest("." + classNameFrom);
    let ancestor = Linq.First(ancestors);
    HTMLUtils.toggleClass(ancestor, false, classNameFrom);
    HTMLUtils.toggleClass(ancestor, true, classNameTo);
  },
  enableBulmaComponents: async () => {
    // Check for click events on the navbar burger icon
    $(".navbar-burger").click(function () {
      // Toggle the "is-active" class on both the "navbar-burger" and the "navbar-menu"
      $(".navbar-burger").toggleClass("is-active");
      $(".navbar-menu").toggleClass("is-active");
    });

    // Check for click on card header icon
    $("button.card-header-icon, .button.card-header-icon").click((event, b) => {
      console.log(event);
      let button = event.target;
      let icon = $(button).find("i");
      $(icon).toggleClass("fa-angle-down");
      $(icon).toggleClass("fa-angle-up");
      let card = $(button).closest("div.card");
      let content = card.find("div.card-content");
      content.toggleClass("is-hidden");

      console.log(b);
    });
  },
  calculateNetworkFees: async () => {
    await Stakeable.loadNetworkFees();
    Stakeable.calculateNetworkFees();
    TStake.calculateNetworkFees();
  },
  loadScript: async (src) => {
    return new Promise(function (resolve, reject) {
      var s;
      s = document.createElement('script');
      s.src = src;
      s.onload = resolve;
      s.onerror = reject;
      document.head.appendChild(s);
    });
  }
};

