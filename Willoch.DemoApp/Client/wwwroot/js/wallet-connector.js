const _globEth = "ethereum";
var service = null;

/** Captures dotnet object. Called by WalletConnectorService.cs. */
export function InitializeWalletConnector(obj) {
    //console.log('wallet-connector.js InitializeWalletConnector()');
    service = obj;
    window.walletConnector = this;
}
export function Listen() {
    //console.log('wallet-connector.js Listen()');
    if (_ethInWin()) {
        _unListen();
        ethereum.on("connect", _handleConnect);
        ethereum.on("disconnect", _handleDisconnect);
        ethereum.on("accountsChanged", _handleAccountsChanged);
        ethereum.on("chainChanged", _handleChainChanged);
        ethereum.on("message", _handleMessage);
        return "listening for events...";
    }
    return "could not listen. ethereum not in window.";
}
function _unListen() {   
    ethereum.removeListener("connect", _handleConnect);
    ethereum.removeListener("disconnect", _handleDisconnect);
    ethereum.removeListener("accountsChanged", _handleAccountsChanged);
    ethereum.removeListener("chainChanged", _handleChainChanged);
    ethereum.removeListener("message", _handleMessage);
}
async function _handleConnect(a,b,c) {
    await service.invokeMethodAsync("OnConnect", error);
}
async function _handleDisconnect(error) {
    await service.invokeMethodAsync("OnDisconnect", error);
}
async function _handleAccountsChanged(accounts) {    
    await service.invokeMethodAsync("OnAccountsChanged", accounts);
}
async function _handleChainChanged(chainId) {
    await service.invokeMethodAsync("OnChainChanged", chainId);
}
async function _handleMessage(message) {
    await service.invokeMethodAsync("OnMessage", message);
}
export function InvokeEnsureAssetsLoading() {
    service.invokeMethodAsync("EnsureAssetsLoading");
}
function _ethInWin(){
    return _globEth in window;
}
export function GetState(){
    if(_ethInWin() && "_state" in ethereum){
        return ethereum._state;
    }
    return null;
}
export function GetNetworkVersion() {
    let v = 0;
    if (_ethInWin()) {
        const ev = ethereum.networkVersion;
        if (ev != null) {
            v = parseInt(ev);
        }
    }
    return v;
}

export async function Enable(){
    if(_ethInWin())
        return ethereum.enable();
    return null;
}
export async function InvokeTestMethod() {
    return await service.invokeMethodAsync("TestMethod");
}
export async function idToToken(contract, id) {
    let value = await _queryContract(contract, "0xf5c6b96b", [id]);
    let values = _splitHexValues(value);
    return values;
}
export async function calculateReward(contract, principal, waitedDays, rewardStretching) {
    return _queryContract(contract, "0xcf42c95c", [principal, waitedDays, rewardStretching]);
}

export async function tokenByIndexERC721Enumerable(contract, index) {
    return _queryContract(contract, "0x4f6ccce7", [index]);
}
export async function tokenOfOwnerByIndexERC721Enumerable(contract, account, index) {
    return _queryContract(contract, "0x2f745c59", [account, index]);
}
export async function stakeLists(contract, account, index) {
    let value = await _queryContract(contract, "0x2607443b", [account, index]);
    let values = _splitHexValues(value);
    return values;
}
export async function stakeStart(contract, account, amount, days) {
    try {
        //Signature calculated using https://piyolab.github.io/playground/ethereum/getEncodedFunctionSignature/
        //with input stakeStart(uint256,uint256)
        return await _writeContract(contract, "0x52a438b8", account, [amount, days]);
    } catch (e) {
        if (e.code == 4001) {
            console.log(e.message);
            return;
        }
        console.log("stakeStart() error");
        console.log(e);
    }
}
export async function mint(contract, account, amount) {
          //Signature calculated using https://piyolab.github.io/playground/ethereum/getEncodedFunctionSignature/
        //with input mint(address,uint256)
    return await _writeContract(contract, "0x40c10f19", account, [account, amount]);
}
export async function getStakeIndex(contract, id) {
    return await _queryContract(contract, "0x32b5d2ff", [id]);
}
export async function stakeCount(contract, account) {
    return await _queryContract(contract, "0x33060d90", [account]);
}
export async function currentDay(contract) {
    return await _queryContract(contract, "0x5c9302c9", []);
}
export async function globals(contract) {
    let value = await _queryContract(contract, "0xc3124525", []);
    let values = _splitHexValues(value);
    return values;
}
export async function dailyData(contract, day) {
    let value = await _queryContract(contract, "0x90de6871", [day]);
    let values = _splitHexValues(value);
    values.length = 2;
    return values;
}
export async function balanceOfERC20(contract, account) {
    return await _queryContract(contract, "0x70a08231", [account]);
}
export async function allowanceOnERC20(contract, owner, spender) {
    //Signature calculated using https://piyolab.github.io/playground/ethereum/getEncodedFunctionSignature/
    //with input allowance(address,address)
    return await _queryContract(contract, "0xdd62ed3e", [owner, spender]);
}
export async function approveOnERC20(contract, owner, spender, amount) {
    try {
        //Signature calculated using https://piyolab.github.io/playground/ethereum/getEncodedFunctionSignature/
        //with input approve(address,uint256)
        return await _writeContract(contract, "0x095ea7b3", owner, [spender, amount]);
    } catch (e) {
        if (e.code == 4001) {
            console.log(e.message);
            return false;
        }
        console.log("approveOnERC20() error");
        console.log(e);        
    }
    return false;    
}
export async function balanceOfERC721(contract, account) {
    return await _queryContract(contract, "0x70a08231", [account]);
}
export async function getOwnerFeePermille(contract) {
   //Signature calculated using https://piyolab.github.io/playground/ethereum/getEncodedFunctionSignature/
   //OWNER_FEE_PERMILLE()
    let task = _queryContract(contract, "0xff95be27");
    let value = await task;
    return value;
}
async function _queryContract(contract, sign, args) {
    const data = _joinHexValues(sign, args);
    let k = {
        "method": "eth_call",
        "params": [
            {
                "to": contract,
                "data": data,
                gas: null,
                gasPrice: null
            },
            "latest"
        ]
    };
    const result = await ethereum.request(k);
    return result;
}
async function _writeContract(contract, sign, from, args) {
    const data = _joinHexValues(sign, args);
    let k = {
        "method":"eth_sendTransaction",
        "id": 1,
        "jsonrpc":"2.0",
        "params": [
            {
                "to": contract,
                "from":from,
                "data": data
            },
            "latest"
        ]
    };
    console.log(k);
    const result = await ethereum.request(k);
    return result;
}
function _joinHexValues(sign, args) {
    if (args == null)
        args = [];
    let result = sign;
    for (var i = 0; i < args.length; i++) {
        result += _normalizeArgument(args[i]);
    }
    return result;
}
const _hexPrefix = "0x";
const _argumentLength = 64;
function _normalizeArgument(argument) {
    if (argument.substring(0, 2) == _hexPrefix)
        argument = argument.substring(2);
    let prependZeroes = _argumentLength - argument.length;
    return "0".repeat(prependZeroes) + argument;
}
function _splitHexValues(normalized) {
    let values = [];
    let i = 2;
    while (i < normalized.length) {
        values.push(_hexPrefix + normalized.substr(i, _argumentLength));
        i += _argumentLength;
    }
    return values;
}