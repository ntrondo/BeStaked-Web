namespace Willoch.DemoApp.Shared
{
    public record NetworkInfo(
        int Id, 
        string Type, 
        string Symbol, 
        string Environment, 
        ERC20Info StakeableContract,
        ContractInfo TransferableContract,
        string BrandAddress);
}
