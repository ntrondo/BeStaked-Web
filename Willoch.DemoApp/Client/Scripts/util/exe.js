EXE={
    execute:async(def)=>{
        let ctx = def[0];
        let fn = def[1];
        let args = def[2];
        let result = fn.apply(ctx,args);
        if(result instanceof Promise){
            await result;
        }
    },
    appendCall:(calls, ctx, fn, args, persist)=>{
        calls.push(EXE.createCall(ctx, fn, args, persist));
    },
    executeOrAppend:async(calls, call, execute)=>{
        if(execute){
            await EXE.execute(call);
        }else{
            calls.push(call);
        }
    },
    createCall:(ctx, fn, args, persist)=>{
        return [ctx, fn, args, persist];
    },
    executeAll:async(calls)=>{
        let persistent = [];
        let call;
        while(calls.length > 0){
            call = calls.pop();
            await EXE.execute(call);
            if(call[3] == true){
                persistent.push(call);
            }
        }
        while(persistent.length > 0){
            calls.push(persistent.pop());
        }
    }
}