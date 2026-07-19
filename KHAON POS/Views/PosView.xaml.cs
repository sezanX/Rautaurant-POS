using System;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantPOS.Views;

public partial class PosView : UserControl
{
    public static readonly DependencyProperty ColumnsCountProperty =
        DependencyProperty.Register(nameof(ColumnsCount), typeof(int), typeof(PosView), new PropertyMetadata(4));

    public int ColumnsCount
    {
        get => (int)GetValue(ColumnsCountProperty);
        set => SetValue(ColumnsCountProperty, value);
    }

    public PosView()
    {
        InitializeComponent();
        this.SizeChanged += PosView_SizeChanged;
    }

    private void PosView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateColumns();
    }

    private void UpdateColumns()
    {
        double availableWidth = this.ActualWidth - 350 - 44;
        if (availableWidth <= 0) return;

        int cols = (int)Math.Floor(availableWidth / 250);
        if (cols < 1) cols = 1;

        ColumnsCount = cols;
    }
}
