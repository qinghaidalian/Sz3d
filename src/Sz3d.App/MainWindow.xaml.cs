using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Sz3d.App.Infrastructure;
using Sz3d.App.ViewModels;
using Sz3d.Core.Nodes;

namespace Sz3d.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly ModelVisual3D _cloudRoot = new();

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel(new WpfImageLoader());
        DataContext = _vm;
        _vm.CloudChanged += RebuildCloudView;
        Viewport3D.Children.Add(_cloudRoot);
    }

    private void RebuildCloudView()
    {
        _cloudRoot.Children.Clear();
        foreach (var cluster in _vm.CloudClusters)
        {
            _cloudRoot.Children.Add(new PointsVisual3D
            {
                Points = cluster.Points,
                Color = cluster.Color,
                Size = 3,
            });
        }
    }

    private void OnLibraryItemMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is NodeDescriptor desc)
        {
            DragDrop.DoDragDrop(fe, new DataObject(typeof(NodeDescriptor), desc), DragDropEffects.Copy);
            e.Handled = true;
        }
    }

    private void OnAbout(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(this,
            "Sz3D 视觉节点式编程平台（原型）\n\n" +
            "· 节点式 2D/3D 视觉算子编排\n" +
            "· 拖拽算子、连线构建项目流程\n" +
            "· 图漾 RVS 风格深色界面\n\n" +
            "技术栈：.NET 8 + WPF + HelixToolkit",
            "关于 Sz3D", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnFitView(object sender, RoutedEventArgs e)
    {
        Viewport3D.ZoomExtents(300);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && (_vm.SelectedNode != null || _vm.SelectedConnection != null))
        {
            _vm.DeleteSelection();
            e.Handled = true;
        }
    }
}
