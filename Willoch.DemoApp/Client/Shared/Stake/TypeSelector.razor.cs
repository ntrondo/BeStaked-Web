using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Pages;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public interface IStakeTypeProvider
    {
        StakeType Value { get; }
        event EventHandler OnValueChanged;
        StakeType[] Values { get; }
    }

    public interface ITypeSelectorModel :IStakeTypeProvider{
        string Label { get; }
              
        void SetValue(StakeType value);        
    }
    internal class TypeSelectorModel : ITypeSelectorModel
    {
        public string Label => "Type of stake";

        public StakeType[] Values { get; } = (StakeType[])Enum.GetValues(typeof(StakeType));

        private StakeType _value = StakeType.Legacy;
        public StakeType Value { get => _value; }

        public void SetValue(StakeType value)
        {
            if(this._value != value)
            {
                this._value = value;
                this.OnValueChanged?.Invoke(this, null);
            }
        }
        public event EventHandler OnValueChanged;
        internal TypeSelectorModel(StakeType value = StakeType.Legacy)
        {
            this._value = value;
        }
        
    }
    public partial class TypeSelector:ComponentBase
    {
        [Parameter, EditorRequired]
        public ITypeSelectorModel Model { get; set; }
        [Inject]
        private ILogger<TypeSelector> logger { get; set; }
        private static readonly IConvert<StakeType, string> StakeTypeToStringConverter = new StakeTypeToSingularDisplayStringConverter();
        private void OnStakeTypeChanged(ChangeEventArgs e)
        {
            this.Model.SetValue(Enum.Parse<StakeType>(e.Value.ToString()));
        }
        protected override Task OnInitializedAsync()
        {
            this.Model.OnValueChanged += Model_OnValueChanged;
            return base.OnInitializedAsync();
        }

        private void Model_OnValueChanged(object sender, EventArgs e)
        {
            logger.Log(LogLevel.Information, "Model_OnValueChanged() " + Model.Value);
        }
    }
}
