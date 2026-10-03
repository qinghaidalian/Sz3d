using System.Globalization;
using Sz3d.App.Infrastructure;
using Sz3d.Core.Data;
using Sz3d.Core.Nodes;

namespace Sz3d.App.ViewModels;

/// <summary>端口 ViewModel：包装 Core 端口，提供画布锚点与属性面板编辑能力。</summary>
public sealed class PortViewModel : ObservableObject
{
    public Port Port { get; }
    public NodeViewModel Owner { get; }

    public PortViewModel(Port port, NodeViewModel owner)
    {
        Port = port;
        Owner = owner;
    }

    public bool IsInput => Port.IsInput;
    public int Index => Port.Index;
    public string Name => Port.Name;
    public DataTypeKind DataType => Port.DataType;

    private double _canvasX;
    public double CanvasX { get => _canvasX; set => Set(ref _canvasX, value); }

    private double _canvasY;
    public double CanvasY { get => _canvasY; set => Set(ref _canvasY, value); }

    /// <summary>端口在节点内的纵向中心偏移（用于模板布局）。</summary>
    public double CenterY => NodeLayout.PortCenterY(Index);

    /// <summary>端口项在节点内的顶部偏移（用于模板布局）。</summary>
    public double Top => CenterY - NodeLayout.PortRowHeight / 2.0;

    private bool _isConnected;
    public bool IsConnected { get => _isConnected; set => Set(ref _isConnected, value); }

    // —— 属性面板：可编辑参数 ——
    public bool IsBool => DataType == DataTypeKind.Bool;
    public bool IsEditableText => DataType is DataTypeKind.String or DataTypeKind.Number;
    public bool ShowTextBox => IsEditableText && !IsConnected;
    public bool ShowCheckBox => IsBool && !IsConnected;

    private string _valueText = string.Empty;
    /// <summary>最近一次执行后的取值（只读展示）。</summary>
    public string ValueText { get => _valueText; set => Set(ref _valueText, value); }

    private InputPort Input => (InputPort)Port;

    public string Text
    {
        get => Input.DefaultValue?.ToString() ?? string.Empty;
        set
        {
            if (DataType == DataTypeKind.Number)
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                {
                    Input.DefaultValue = d;
                    OnPropertyChanged();
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    Input.DefaultValue = 0.0;
                    OnPropertyChanged();
                }
            }
            else
            {
                Input.DefaultValue = value;
                OnPropertyChanged();
            }
        }
    }

    public bool BoolValue
    {
        get => Input.DefaultValue is bool b && b;
        set { Input.DefaultValue = value; OnPropertyChanged(); }
    }

    public void RefreshEditable()
    {
        OnPropertyChanged(nameof(ShowTextBox));
        OnPropertyChanged(nameof(ShowCheckBox));
    }
}
