Deployment={
    deployTransferrableStake:async()=>{
        let source = Src.Source;
        if(source==null){
            console.log("Source is null. Cannot deploy.");
            return;
        }
        let contractAdress = source.TransferrableContractId;
        if(contractAdress != null && contractAdress.length > 0){
            console.log("The contract allready has a id. Will not deploy.");
            return;
        }
        let abiFile = Src.Source.TransferrableAbi;
        if(abiFile == null || abiFile.length == 0){
            console.log("ABI file is not specified. Cannot deploy.");
            return;
        }
        let contractDefinitionJSON = await HTMLUtils.getJSONParsed(Src.Source.TransferrableAbi);
        let abi = contractDefinitionJSON.abi;
        if(abi == null || abi == 0){
            console.log("ABI is not specified. Cannot deploy.");
            return;
        }
        let contract = new web3.eth.Contract(abi);
        let bytecode = contractDefinitionJSON.bytecode;
        if(bytecode == null || bytecode.length ==0){
            console.log("Bytecode is not specified. Cannot deploy.");
            return;
        }
        let arguments = [source.StakeableContractId, 1, 5555]; 
        await contract.deploy({data:bytecode, arguments:arguments});
    }
}