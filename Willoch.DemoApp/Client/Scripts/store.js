Store = {
    MaximumAge: 120/*minutes*/ * 60/*seconds per minute*/ * 1000/*milliseconds per second*/,//Milliseconds
    ReferralCodeKey: "b3eb2960281f2ecd20c9",
    GetReferralCode:()=>{
        let code = localStorage.getItem(Store.ReferralCodeKey);
        if(code == null || code.length == 0){
          return null;
        }
        return code;
    },
    SetReferralCode:(code)=>{
        let existing = Store.GetReferralCode();
        if(existing == null){
            localStorage.setItem(Store.ReferralCodeKey, code);
        }
    },
    getAssetsKey:(contractAddress)=>{
        if(contractAddress == null || App.Account == null){
            return null;
        }
        return "assets" + "-" + contractAddress.substr(5) + "-" + App.Account.substr(5);
    },
    loadAssets:(contractAddress)=>{
        let key = Store.getAssetsKey(contractAddress);
        return Store.loadItem(key);
    },
    storeAssets:(contractAddress, assets)=>{
        let key = Store.getAssetsKey(contractAddress);
        Store.storeItem(key, assets);
        
    },
    clearAssets:(contractAddress)=>{
        let key = Store.getAssetsKey(contractAddress);
        localStorage.removeItem(key);
    },
    storeItem:(key, item)=>{        
        item.TimeStamp = new Date().getTime();
        let serialized = JSON.stringify(item);
        localStorage.setItem(key, serialized);
    },
    clearAll:()=>{
        let refKey = Store.GetReferralCode();
        localStorage.clear();
        if(refKey != null && refKey.length > 0){
            Store.SetReferralCode(refKey);
        }        
    },
    clearItem:(key)=>{
        localStorage.removeItem(key);
    },
    loadItem:(key)=>{
        let item = Store.loadItemNoExpiration(key);
        if(item == null){return item;}
        {
            let now = new Date().getTime();
            let age = now - item.TimeStamp;
            if(isNaN(age) || age > Store.MaximumAge){
                item = null;
            }
        }        
        return item;
    },
    loadItemNoExpiration:(key)=>{
        let serialized = localStorage.getItem(key);
        let item = serialized == null ? null: JSON.parse(serialized);
        return item;
    }
};